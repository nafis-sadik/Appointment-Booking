using Microsoft.AspNetCore.Http;
using RedBook.Core.Constants;
using System.Security.Claims;

namespace RedBook.Core.Models
{
    public class RequestingUser
    {
        private readonly HttpContext _httpContext;
        private readonly ClaimsPrincipal _contextReference;

        internal RequestingUser(ClaimsPrincipal contextReference, HttpContext httpContext)
        {
            _httpContext = httpContext;
            _contextReference = contextReference;
        }

        public int UserId
        {
            get
            {
                string? _userId = _contextReference.Claims.FirstOrDefault(x => x.Type.Equals("UserId", StringComparison.InvariantCultureIgnoreCase))?.Value;
                if (!string.IsNullOrWhiteSpace(_userId) && int.TryParse(_userId, out int userId))
                    return userId;
                else
                    return 1;
            }
        }

        public string RequestOrigin
        {
            get
            {
                return _httpContext.Request.Headers["Origin"].ToString();
            }
        }
    }
}
