using AutoMapper;

namespace RedBook.Core.AutoMapper
{
    public class AutoMapperObjectMapper(IMapper mapper) : IObjectMapper
    {
        private readonly IMapper mapper = mapper;

        public TDestination Map<TDestination>(object source)
        {
            return mapper.Map<TDestination>(source);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return mapper.Map(source, destination);
        }
    }
}
