using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Security;

namespace web.Controllers
{
    [RoutePrefix("~/"), Authorize]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Censors()
        {
            return View();
        }

        public ActionResult People()
        {
            return View();
        }

        public ActionResult Settings()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        public ActionResult Users()
        {
            return View();
        }
    }
}