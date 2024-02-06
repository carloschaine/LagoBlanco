using Microsoft.AspNetCore.Mvc;

namespace LagoBlanco.Web.Controllers
{
    public class DashBoardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
