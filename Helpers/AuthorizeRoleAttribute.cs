using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AuthorizeRoleAttribute : ActionFilterAttribute
{
    private readonly string _role;

    public AuthorizeRoleAttribute(string role)
    {
        _role = role;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var http = context.HttpContext;

        string? role = http.Session.GetString("Role");

        if (string.IsNullOrEmpty(role) || role != _role)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}
