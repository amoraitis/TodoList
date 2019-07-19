using Amoraitis.TodoList.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Microsoft.AspNetCore.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string EmailConfirmationLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return urlHelper.Action(new UrlActionContext {
                Action = nameof(AccountController.ConfirmEmail),
                Controller = "Account",
                Values = new { userId, code },
                Protocol = scheme
            });
        }

        public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
        {
            return urlHelper.Action(new UrlActionContext {
                Action = nameof(AccountController.ResetPassword),
                Controller = "Account",
                Values = new { userId, code },
                Protocol = scheme
            });
        }
    }
}
