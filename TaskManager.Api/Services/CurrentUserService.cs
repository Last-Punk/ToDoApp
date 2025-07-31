using System.Security.Claims;
using TaskManager.Domain.Services;

namespace TaskManager.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string GetUserId()
    {
        var context = httpContextAccessor.HttpContext;
        return context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}