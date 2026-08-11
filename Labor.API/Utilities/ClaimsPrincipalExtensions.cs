using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace Labor.API.Utilities
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetCurrentUserId(this ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated!=true) 
                return null;

            var id=user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(id, out var userId) ? userId : null;

        }
    }
}
