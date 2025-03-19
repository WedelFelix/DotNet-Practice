using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Movies.Api.Auth;

public class ApiKeyAuthFilter(IConfiguration configuration) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.Request.Headers.TryGetValue(
                AuthConstants.ApiKeyHeaderName, out var extractedApiKey))
            context.Result = new UnauthorizedObjectResult("API key is missing");

        var apiKey = configuration["ApiKey"];
        if (apiKey != extractedApiKey)
            context.Result = new UnauthorizedObjectResult("API key is missing");
    }
}