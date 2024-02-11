using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Application.Services.Interface;
using LagoBlanco.Domain.Entities;
using LagoBlanco.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace LagoBlanco.Web.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class VillaNumberController : Controller
    {
        private readonly IVillaNumberService _villaNumberService;
        private readonly IVillaService _villaService;
        public VillaNumberController(IVillaNumberService villaNumberService,
                                     IVillaService villaService)
        {
            _villaNumberService = villaNumberService;
            _villaService = villaService;
        }


        public IActionResult Index()
        {
            var villaNumbers = _villaNumberService.GetAllVillaNumbers();
            return View(villaNumbers);
        }


        public IActionResult Create() 
        {
            VillaNumberVM villaNumberVm = new VillaNumberVM() {
                VillaList = _villaService.GetAllVillas()
                                       .Select(u => new SelectListItem 
                                       { Text = u.Name, Value = u.Id.ToString() }),
                VillaNumber = new VillaNumber()
            };
            return View(villaNumberVm);  
        }


        [HttpPost]
        public IActionResult Create(VillaNumberVM obj )
        {//Pasado desde el submit de Create. El Form pasa automatic. el Model.  
            bool roomExist = _villaNumberService.CheckVillaNumberExists(obj.VillaNumber.Villa_Number);
            if (ModelState.IsValid && !roomExist) {
                //---
                _villaNumberService.CreateVillaNumber(obj.VillaNumber);
                //---
                TempData["success"] = "The villaNumber has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            if (roomExist) TempData["error"] = "Habitación ya existe.";
            //---
            obj.VillaList = _villaService.GetAllVillas()
                                       .Select(u => new SelectListItem 
                                        { Text = u.Name, Value = u.Id.ToString() }); 
            return View(obj);
        }


        public IActionResult Update(int villaNumberId)
        {
            VillaNumberVM villaNumberVm = new VillaNumberVM() {
                VillaList = _villaService.GetAllVillas()
                                       .Select(u => new SelectListItem 
                                       { Text = u.Name, Value = u.Id.ToString() }),
                VillaNumber = _villaNumberService.GetVillaNumberById(villaNumberId)
            };
                       
            if (villaNumberVm.VillaNumber is null)  RedirectToAction("Error", "Home");
            return View(villaNumberVm);
        }


        [HttpPost]
        public IActionResult Update(VillaNumberVM  villaNumberVM)
        {

            if (ModelState.IsValid) {
                //---
                _villaNumberService.UpdateVillaNumber(villaNumberVM.VillaNumber); 
                //---
                TempData["success"] = "The villaNumber has been modified successfully.";
                return RedirectToAction(nameof(Index));
            }            
            //---
            villaNumberVM.VillaList = _villaService.GetAllVillas()
                                                   .Select(u => new SelectListItem 
                                                      { Text = u.Name, Value = u.Id.ToString() });
            return View(villaNumberVM);
        }


        public IActionResult Delete(int villaNumberId)
        {
            VillaNumberVM villaNumberVm = new VillaNumberVM() {
                VillaList = _villaService.GetAllVillas()
                                         .Select(u => new SelectListItem 
                                            { Text = u.Name, Value = u.Id.ToString() }),
                VillaNumber = _villaNumberService.GetVillaNumberById(villaNumberId)
            };

            if (villaNumberVm.VillaNumber is null) RedirectToAction("Error", "Home");
            return View(villaNumberVm);
        }

        [HttpPost]
        public IActionResult Delete(VillaNumberVM villaNumberVM)
        {
            VillaNumber? objFromDb = _villaNumberService.GetVillaNumberById(villaNumberVM.VillaNumber.Villa_Number);
            if (objFromDb  is not null) {
                //---
                _villaNumberService.DeleteVillaNumber(objFromDb.Villa_Number);
                //---
                TempData["success"] = "The villaNumber has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            //---
            villaNumberVM.VillaList = _villaService.GetAllVillas()
                                                   .Select(u => new SelectListItem 
                                                      { Text = u.Name, Value = u.Id.ToString() });
            return View(villaNumberVM);
        }        
    }
}
