using System.Security.Claims;

namespace RedBook.Core.Security
{
    public interface IClaimsPrincipalAccessor
    {
        ClaimsPrincipal GetCurrentPrincipal();
    }
}
