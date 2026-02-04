using AnglingClubShared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Diagnostics;
using System.Linq;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // We're checking here to see if the route has been decorated with an [AllowAnonymous] attribute. If it has, we skip authorization
        // for the route. Doing this allows us to apply the [Authorize] attribute by default in the startup using:
        //
        // services.AddControllers().AddMvcOptions(x => x.Filters.Add(new AuthorizeAttribute()))
        //
        if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            var hasAllowAnonymousAttribute = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true)
                .Any(a => a.GetType() == typeof(AllowAnonymousAttribute));
            if (hasAllowAnonymousAttribute)
            {
                return;
            }
        }

        var member = (Member)context.HttpContext.Items["User"];
        if (member == null)
        {
            var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

            // Token present but expired/invalid
            var authError = context.HttpContext.Items["AuthError"] as string;

            // Return 401 for missing or invalid/expired token
            context.Result = new UnauthorizedObjectResult(new
            {
                type = "https://httpstatuses.com/401",
                title = "Unauthorized",
                status = 401,
                traceId,
                authError = authError // optional, safe: "expired_token"/"invalid_token"
            });
            // not logged in
            //            context.Result = new JsonResult(new { message = "Member not found or membership has expired" }) { StatusCode = StatusCodes.Status401Unauthorized };
            //context.Result = new JsonResult(new { message = context.HttpContext.Items["UserError"] }) { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}
