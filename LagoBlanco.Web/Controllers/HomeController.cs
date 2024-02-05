using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Web.Models;
using LagoBlanco.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LagoBlanco.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }




        public IActionResult Index()
        {
            HomeVM homeVM = new() {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties:"amenities"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Today)
            }; 

            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            Thread.Sleep(1000);
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "amenities"); 
            foreach (var villa in villaList) {
                if (villa.Id % 2 == 0) { villa.IsAvailable = false;}
            }
            HomeVM homeVM = new() {VillaList=villaList,  Nights=nights, CheckInDate=checkInDate};
            return PartialView("_VillaList", homeVM);
        }



        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
