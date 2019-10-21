using System.Web.Mvc;
using WebMatrix.WebData;

namespace MvcEmpty.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        [Authorize] 
        public ActionResult Private()
        {

            ViewBag.Message = WebSecurity.CurrentUserName;

            return View();
        }

    }
}