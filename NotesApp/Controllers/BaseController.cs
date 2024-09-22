using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NotesApp.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (User.Identity != null && User.Identity.Name != null)
            {
                string email = User.Identity.Name;
                string username = email.Split('@')[0];
                ViewData["Username"] = username;
            }
        }
    }
}
