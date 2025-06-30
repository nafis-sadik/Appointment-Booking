using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RedBook.Core.AutoMapper;
using RedBook.Core.Models;
using RedBook.Core.Security;
using RedBook.Core.UnitOfWork;

namespace RedBook.Core.Domain
{
    public class ServiceBase
    {
        private IHttpContextAccessor? _httpContextAccessor;
        public HttpContext? HttpContextAccessor
        {
            get
            {
                if (_httpContextAccessor != null)
                    return _httpContextAccessor.HttpContext;
                return null;
            }
        }

        private IUnitOfWorkManager _unitOfWorkManager;
        public IUnitOfWorkManager UnitOfWorkManager
        {
            get
            {
                if (_unitOfWorkManager == null)
                    throw new ArgumentException("UnitOfWorkManager must be assigned before used.");

                return _unitOfWorkManager;
            }
            private set { _unitOfWorkManager = value; }
        }

        public ILogger _logger;
        public ILogger Logger { get { return _logger; } }

        public IObjectMapper _mapper;
        public IObjectMapper Mapper { get { return _mapper; } }

        private RequestingUser _userInfo;
        public RequestingUser User { get { return _userInfo; } }

        public ServiceBase(ILogger<ServiceBase> logger, IObjectMapper mapper, IClaimsPrincipalAccessor accessor, IUnitOfWorkManager unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _mapper = mapper;
            _unitOfWorkManager = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _userInfo = new RequestingUser(accessor.GetCurrentPrincipal(), httpContextAccessor.HttpContext);
        }
    }
}
