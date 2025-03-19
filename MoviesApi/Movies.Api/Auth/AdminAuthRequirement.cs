using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Movies.Api.Auth;

public class AdminAuthRequirement(string apiKey) : IAuthorizationHandler, IAuthorizationRequirement
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.HasClaim(AuthConstants.AdminUserClaimName, "true"))
        {
            context.Succeed(this);
            return Task.CompletedTask;
        }

        if (context.Resource is not HttpContext httpContext) return Task.CompletedTask;

        if (!httpContext.Request.Headers.TryGetValue(
                AuthConstants.ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (apiKey != extractedApiKey)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var identity = (ClaimsIdentity)httpContext.User.Identity!;
        identity.AddClaim(new Claim("userid",
            Guid.Parse("").ToString())); // inject some GUID as the user id for the given api key
        context.Succeed(this);
        return Task.CompletedTask;
    }
}