using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Application.Services.Interface;
using LagoBlanco.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace LagoBlanco.Web.Controllers
{
    [Authorize(Roles = SD.Role_Admin) ]
    public class AmenityController : Controller
    {
        private readonly IAmenityService _amenityService;
        private readonly IVillaService _villaService;
        public AmenityController(IAmenityService amenityService,
                                 IVillaService villaService)
        {
            _amenityService = amenityService;
            _villaService = villaService;
        }


        public IActionResult Index()
        {
            var amenties = _amenityService.GetAllAmenities();
            return View(amenties);
        }

        public IActionResult Create() 
        {
            AmenityVM amenityVM = new() {
                VillaList = _villaService.GetAllVillas()
                                         .Select(u => new SelectListItem 
                                            { Text = u.Name, Value = u.Id.ToString() }),
                Amenity = new()
            };
            return View(amenityVM);  
        }


        [HttpPost]
        public IActionResult Create(AmenityVM obj )
        {//Pasado desde el submit de Create. El Form pasa automatic. el Model.  
            
            if (ModelState.IsValid) {
                //---
                _amenityService.CreateAmenity(obj.Amenity); 
                //---
                TempData["success"] = "The Amenity has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            //---
            obj.VillaList = _villaService.GetAllVillas()
                                         .Select(u => new SelectListItem 
                                         { Text = u.Name, Value = u.Id.ToString() }); 
            return View(obj);
        }


        public IActionResult Update(int amenityId)
        {
            AmenityVM amenityVm = new() {
                VillaList = _villaService.GetAllVillas()
                                         .Select(u => new SelectListItem 
                                           { Text = u.Name, Value = u.Id.ToString() }),
                Amenity = _amenityService.GetAmenityById(amenityId)
            };
                       
            if (amenityVm.Amenity is null)  RedirectToAction("Error", "Home");
            return View(amenityVm);
        }


        [HttpPost]
        public IActionResult Update(AmenityVM  amenityVM)
        {

            if (ModelState.IsValid) {
                //---
                _amenityService.UpdateAmenity(amenityVM.Amenity);
                //---
                TempData["success"] = "The Amenity has been modified successfully.";
                return RedirectToAction(nameof(Index));
            }            
            //---
            amenityVM.VillaList = _villaService.GetAllVillas()
                                         .Select(u => new SelectListItem 
                                         { Text = u.Name, Value = u.Id.ToString() });
            return View(amenityVM);
        }


        public IActionResult Delete(int amenityId)
        {
            AmenityVM amenityVm = new() {
                VillaList = _villaService.GetAllVillas()
                                         .Select(u => new SelectListItem 
                                         { Text = u.Name, Value = u.Id.ToString()}),
                Amenity = _amenityService.GetAmenityById(amenityId)
            };

            if (amenityVm.Amenity is null) RedirectToAction("Error", "Home");
            return View(amenityVm);
        }

        [HttpPost]
        public IActionResult Delete(AmenityVM amenityVM)
        {
            //---
            bool deletedAmenity = _amenityService.DeleteAmenity(amenityVM.Amenity.Id); 
            //---
            if (deletedAmenity) {                
                TempData["success"] = "The Amenity has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            //---
            amenityVM.VillaList = _villaService.GetAllVillas()
                                               .Select(u => new SelectListItem 
                                               { Text = u.Name, Value = u.Id.ToString() });
            return View(amenityVM);
        }        
    }
}
