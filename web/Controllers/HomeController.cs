using System.Web.Mvc;

namespace web.Controllers
{
    [RoutePrefix("~/")]
    public class HomeController : Controller
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
    }
}