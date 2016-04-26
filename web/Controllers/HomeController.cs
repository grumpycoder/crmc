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

        [Route(Name = "censors", Order = 1)]
        public ActionResult Censors()
        {
            return View();
        }

        public ActionResult People()
        {
            return View();
        }
    }
}