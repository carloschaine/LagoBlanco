using LagoBlanco.Application.Services.Interface;
using LagoBlanco.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace LagoBlanco.Web.Controllers
{
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;
        public VillaController(IVillaService villaService)
        {
            _villaService = villaService;
        }


        public IActionResult Index()
        {
            var villas = _villaService.GetAllVillas();
            return View(villas);
        }


        public IActionResult Create() 
        { 
            return View();  
        }


        public IActionResult Update(int villaId)
        {   //---
            Villa? obj = _villaService.GetVillaById(villaId);
            //---
            if (obj is null)  RedirectToAction("Error", "Home");
            return View(obj);
        }

        public IActionResult Delete(int villaId)
        {   //---
            Villa? obj = _villaService.GetVillaById(villaId);
            //---
            if (obj is null) RedirectToAction("Error", "Home");
            return View(obj);
        }




        [HttpPost]
        public IActionResult Create(Villa obj )
        {   //Pasado desde el submit de Create. El Form pasa automatic. el Model.  
            if (obj.Name==obj.Description) ModelState.AddModelError("name", "Description debe ser <> al nombre");
            if (ModelState.IsValid) {
                //---
                _villaService.CreateVilla(obj); 
                //---
                TempData["success"] = "The villa has been created successfully.";
                return RedirectToAction(nameof(Index));// RedirectToAction("Index","Villa");
            }
            return View();
        }


        [HttpPost]
        public IActionResult Update(Villa obj)
        {

            if (ModelState.IsValid && obj.Id > 0) {
                //---
                _villaService.UpdateVilla(obj);
                //---
                TempData["success"] = "The villa has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa has not updated.";
            return View();
        }


        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
            //---
            bool deletedVilla = _villaService.DeleteVilla(obj.Id);  
            //---
            if (deletedVilla) {
                TempData["success"] = "The villa has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }            
            TempData["error"] = "The villa has not Deleted.";
            return View();
        }
    }
}
