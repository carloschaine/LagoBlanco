using LagoBlanco.Application.Services.Interface;
using LagoBlanco.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LagoBlanco.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IVillaService _villaService;
        public HomeController(IVillaService villaService )
        {
            _villaService = villaService;
        }


        public IActionResult Index()
        {
            HomeVM homeVM = new() {
                VillaList = _villaService.GetAllVillas(),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Today)
            }; 
            return View(homeVM);
        }


        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            Thread.Sleep(1000);
            //---
            HomeVM homeVM = new() {
                VillaList = _villaService.GetVillasAvailabilityByDate(nights,checkInDate),  
                Nights=nights, 
                CheckInDate=checkInDate};
            //---
            return PartialView("_VillaList", homeVM);
        }


        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
