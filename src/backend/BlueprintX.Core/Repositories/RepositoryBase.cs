using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using CaseConverter;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace RedBook.Core.Repositories
{
    public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<TEntity> _dbSet;

        public RepositoryBase(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        public virtual IQueryable<TEntity> TrackableQuery() => _dbSet.AsTracking().AsQueryable();
        public virtual IQueryable<TEntity> UnTrackableQuery() => _dbSet.AsNoTracking().AsQueryable();

        // Create
        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            EntityEntry<TEntity> entityEntry = await _dbSet.AddAsync(entity);
            if (entityEntry.State == EntityState.Added) return entityEntry.Entity;
            else throw new InvalidOperationException($"Failed to insert {entity}");
        }

        //public async Task BulkInsertAsync(IEnumerable<TEntity> entities) => await _dbSet.AddRangeAsync(entities);
        public async Task<IEnumerable<TEntity>> BulkInsertAsync(IEnumerable<TEntity> entities, bool isRaw = false)
        {
            if (entities == null || !entities.Any())
                return new List<TEntity>();

            Type entityType = typeof(TEntity);
            var entityTypeMetadata = _dbContext.Model.FindEntityType(entityType);
            string? tableName = entityTypeMetadata?.GetTableName();
            string? schemaName = entityTypeMetadata?.GetSchema();

            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Table name not found!");

            var primaryKey = entityTypeMetadata?.FindPrimaryKey();
            if (primaryKey == null)
                throw new ArgumentException($"Unable to locate primary key for type {entityType.Name}");

            PropertyInfo? primaryKeyProperty = primaryKey.Properties.Single().PropertyInfo;
            if (primaryKeyProperty == null)
                throw new ArgumentException($"Unable to locate primary key for type {entityType.Name}");
            string primaryKeyName = primaryKeyProperty.Name;
            
            // Build the columns and values for the INSERT statement 
            List<PropertyInfo> properties = entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !p.GetGetMethod().IsVirtual 
                    && p.Name != primaryKeyName 
                    && !Attribute.IsDefined(p, typeof(ForeignKeyAttribute)))
                .ToList();

            return await BulkInsertEFAsync(entities, properties, primaryKeyProperty, tableName, schemaName);
        }

        private async Task<IEnumerable<TEntity>> BulkInsertEFAsync(
            IEnumerable<TEntity> entities,
            List<PropertyInfo> columnList,
            PropertyInfo primaryKeyColumnInfo,
            string tableName,
            string schemaName
        )
        {
            // Build the columns and values for the INSERT statement
            string columns = string.Join(", ", columnList.Select(p => $"[{p.Name}]"));
            StringBuilder values = new StringBuilder();
            List<SqlParameter> parameters = new List<SqlParameter>();

            int parameterIndex = 0;
            foreach (var entity in entities)
            {
                var valueList = new List<string>();
                foreach (var property in columnList)
                {
                    var parameterName = $"@p{parameterIndex++}";
                    var value = property.GetValue(entity);
                    valueList.Add(parameterName);
                    parameters.Add(new SqlParameter(parameterName, value ?? DBNull.Value));
                }

                values.AppendLine($"({string.Join(", ", valueList)}),");
            }

            // Generate the temporary table name dynamically
            string tempTableName = $"Inserted_{tableName}_{Guid.NewGuid():N}";

            // Get the list of properties to insert (excluding primary key and navigation properties)
            Type entityType = typeof(TEntity);
            string primaryKeyName = primaryKeyColumnInfo.Name;
            List<PropertyInfo> properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    !p.GetGetMethod().IsVirtual &&
                    p.Name != primaryKeyName &&
                    !Attribute.IsDefined(p, typeof(ForeignKeyAttribute)))
                .ToList();

            // Create the temporary table to hold the inserted IDs
            Type primaryType = typeof(TEntity);
            Type primaryKeyType = primaryKeyColumnInfo.PropertyType;
            string primaryKeySqlType = FormatValueForSql(primaryKeyType);
            string createTempTableSql = $@"CREATE TABLE {tempTableName} ([{primaryKeyName}] {primaryKeySqlType});";
            await _dbContext.Database.ExecuteSqlRawAsync(createTempTableSql);

            // Build the INSERT statement with OUTPUT INTO the temp table
            string insertSql = $@"
                INSERT INTO {(string.IsNullOrEmpty(schemaName) ? "" : $"[{schemaName}].")}[{tableName}] ({columns})
                OUTPUT INSERTED.[{primaryKeyName}] INTO {tempTableName} ([{primaryKeyName}])
                VALUES {values.ToString().TrimEnd(',', '\r', '\n')};";

            // Execute the INSERT command
            await _dbContext.Database.ExecuteSqlRawAsync(insertSql, parameters.ToArray());

            // Retrieve the inserted IDs
            string selectInsertedIdsSql = $@"SELECT [{primaryKeyName}] FROM {tempTableName};";
            var insertedIds = new List<object>();

            await using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = selectInsertedIdsSql;
                command.CommandType = CommandType.Text;

                // Ensure the connection is open
                if (command.Connection.State != ConnectionState.Open)
                    await command.Connection.OpenAsync();

                // Assign the transaction if one exists
                var currentTransaction = _dbContext.Database.CurrentTransaction;
                if (currentTransaction != null)
                {
                    command.Transaction = currentTransaction.GetDbTransaction();
                }

                await using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var insertedId = Convert.ChangeType(reader[primaryKeyName], primaryKeyType);
                        insertedIds.Add(insertedId);
                    }
                }
            }

            // Drop the temporary table
            string dropTempTableSql = $@"DROP TABLE {tempTableName};";
            await _dbContext.Database.ExecuteSqlRawAsync(dropTempTableSql);

            // Assign the IDs to the entities and attach them to the context
            var entitiesList = entities.ToList();
            for (int i = 0; i < insertedIds.Count; i++)
            {
                primaryKeyColumnInfo.SetValue(entitiesList[i], insertedIds[i]);
                _dbContext.Attach(entitiesList[i]);
                _dbContext.Entry(entitiesList[i]).State = EntityState.Unchanged;
            }

            return entitiesList;
        }

        private string FormatValueForSql(object value){
            if (value == null) return "NULL"; 
            if (value is string || value is DateTime) return $"'{value.ToString()?.Replace("'", "''")}'"; 
            if (value is bool) return (bool)value ? "1" : "0"; 
            if (Nullable.GetUnderlyingType(value.GetType()) != null) return value?.ToString(); 
            string valueTypeStr = value.ToString();
            return valueTypeStr == "System.Int32"? "INT" : valueTypeStr;
        }

        // Read Async
        public TEntity? Get(int id) => _dbSet.Find(id);
        public TEntity? Get(string id) => _dbSet.Find(id);
        public TEntity? Get(object id) => _dbSet.Find(id);
        public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> query) => UnTrackableQuery().Where(query).ToList();

        // Read
        public async Task<TEntity?> GetAsync(int id) => await _dbSet.FindAsync(id);
        public async Task<TEntity?> GetAsync(string id) => await _dbSet.FindAsync(id);
        public async Task<TEntity?> GetAsync(object id) => await _dbSet.FindAsync(id);
        public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> query) => await UnTrackableQuery().Where(query).ToListAsync();

        // Helper method
        private object ParseJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    {
                        // Adjust based on your needs
                        string elementValue = element.ToString();
                        if (elementValue.Contains("."))
                            return element.GetDecimal();
                        else
                            return element.GetInt32();
                    }
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Object:
                    var nestedDictionary = new Dictionary<string, object>();
                    foreach (var nestedProperty in element.EnumerateObject())
                    {
                        nestedDictionary[nestedProperty.Name] = ParseJsonElement(nestedProperty.Value);
                    }
                    return nestedDictionary;
                default:
                    return null; // Handle other value kinds as needed
            }
        }
        // Update
        public virtual TEntity Update(TEntity entity) => _dbSet.Update(entity).Entity;

        public void ColumnUpdate(object pk, Dictionary<string, object> keyValuePairs)
        {
            // Generate entity object
            TEntity? entity = Activator.CreateInstance(typeof(TEntity)) as TEntity;
            if (entity == null) throw new ArgumentNullException($"Unable to create entity of type {nameof(entity)}");

            // Get primary key column
            PropertyInfo? primaryKeyColumnInfo = _dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties.Single().PropertyInfo;
            if (primaryKeyColumnInfo == null) throw new ArgumentException($"Unable to locate primary key for type {nameof(entity)}");

            // Set primary key value
            if (primaryKeyColumnInfo.PropertyType == pk.GetType())
                primaryKeyColumnInfo.SetValue(entity, pk);
            else
                throw new ArgumentException($"Cannot set value of type {pk.GetType()} for the primary key {primaryKeyColumnInfo.Name} of type {primaryKeyColumnInfo.PropertyType}");

            // Update entity entries and mark as modified for update operation
            foreach (KeyValuePair<string, object> property in keyValuePairs)
            {
                string propertyName = property.Key.ToPascalCase();
                PropertyEntry? targetProperty = _dbContext.Entry(entity).Properties.FirstOrDefault(prop => prop.Metadata.Name == propertyName);
                if (targetProperty == null) continue;
                PropertyInfo? targetPropertyInfo = targetProperty.Metadata.PropertyInfo;
                if (targetPropertyInfo == null) throw new ArgumentException($"Unable to locate property {property.Key} in type {typeof(TEntity)}");

                if (targetPropertyInfo.Name != primaryKeyColumnInfo.Name)
                {
                    if (targetPropertyInfo.PropertyType.IsAssignableFrom(property.Value?.GetType()))
                        targetProperty.CurrentValue = property.Value;
                    else if (property.Value != null && property.Value.GetType() == typeof(JsonElement))
                        targetProperty.CurrentValue = ParseJsonElement((JsonElement)property.Value);
                    else
                        throw new ArgumentException($"The type of the provided value does not match the type of the property. Property: {property.Key.ToString()}, Provided Type: {property.GetType().ToString()}, Expected Type: {targetProperty.CurrentValue?.GetType()}");

                    targetProperty.IsModified = true;
                }
            }
        }

        // Delete
        public void Delete(TEntity entity) => _dbSet.Remove(entity);

        public async Task DeleteAsync(int id)
        {
            var targetRow = await GetAsync(id);
            if (targetRow != null)
                _dbSet.Remove(targetRow);
            else
                throw new ArgumentException($"Object with identifier {id} was not found");
        }

        public async Task DeleteAsync(string id)
        {
            var targetRow = await GetAsync(id);
            if (targetRow != null)
                _dbSet.Remove(targetRow);
            else
                throw new ArgumentException($"Object with identifier {id} was not found");
        }

        public async Task<int> DeleteAsync(string propertyName, string symbol, string value)
        {
            // Get the entity type and its SQL table name
            IEntityType? entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
            var tableName = entityType?.GetTableName();

            // Build the SQL command
            string sqlCommand = $"DELETE FROM {tableName} WHERE {propertyName} {symbol} {value}";

            // Execute the SQL command
            return await _dbContext.Database.ExecuteSqlRawAsync(sqlCommand);
        }

        // Utilities
        public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();

        public async Task Dispose() => await _dbContext.DisposeAsync();

        public void DetachAllEntities()
        {
            IEnumerable<EntityEntry> changedEntriesCopy = _dbContext.ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified
                        || x.State == EntityState.Added
                        || x.State == EntityState.Deleted);
            foreach (var entity in changedEntriesCopy)
            {
                entity.State = EntityState.Detached;
            }
        }
    }
}
