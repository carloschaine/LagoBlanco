using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Domain.Entities;
using LagoBlanco.Infrastructure.Data;
using LagoBlanco.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LagoBlanco.Web.Controllers
{
    [Authorize(Roles = SD.Role_Admin) ]
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork _repo;
        public AmenityController(IUnitOfWork repo)
        {
            _repo = repo;
        }


        public IActionResult Index()
        {
            var amenties = _repo.Amenity.GetAll(null,"Villa");
            return View(amenties);
        }

        public IActionResult Create() 
        {
            AmenityVM amenityVM = new() {
                VillaList = _repo.Villa.GetAll()
                                       .Select(u => new SelectListItem 
                                       { Text = u.Name, Value = u.Id.ToString() }),
                Amenity = new()
            };
            return View( amenityVM);  
        }


        [HttpPost]
        public IActionResult Create(AmenityVM obj )
        {//Pasado desde el submit de Create. El Form pasa automatic. el Model.  
            
            if (ModelState.IsValid) {
                _repo.Amenity.Add(obj.Amenity);
                _repo.Amenity.Save();
                TempData["success"] = "The Amenity has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            //---
            obj.VillaList = _repo.Villa.GetAll()
                                       .Select(u => new SelectListItem 
                                        { Text = u.Name, Value = u.Id.ToString() }); 
            return View(obj);
        }


        public IActionResult Update(int amenityId)
        {
            AmenityVM amenityVm = new() {
                VillaList = _repo.Villa.GetAll()
                                       .Select(u => new SelectListItem 
                                       { Text = u.Name, Value = u.Id.ToString() }),
                Amenity = _repo.Amenity.Get(vn=>vn.Id==amenityId)
            };
                       
            if (amenityVm.Amenity is null)  RedirectToAction("Error", "Home");
            return View(amenityVm);
        }


        [HttpPost]
        public IActionResult Update(AmenityVM  amenityVM)
        {

            if (ModelState.IsValid) {
                _repo.Amenity.Update(amenityVM.Amenity);
                _repo.Amenity.Save();
                TempData["success"] = "The Amenity has been modified successfully.";
                return RedirectToAction(nameof(Index));
            }            
            //---
            amenityVM.VillaList = _repo.Villa.GetAll()
                                             .Select(u => new SelectListItem 
                                              { Text = u.Name, Value = u.Id.ToString() });
            return View(amenityVM);
        }


        public IActionResult Delete(int amenityId)
        {
            AmenityVM amenityVm = new() {
                VillaList = _repo.Villa.GetAll()
                                       .Select(u => new SelectListItem 
                                       { Text = u.Name, Value = u.Id.ToString() }),
                Amenity = _repo.Amenity.Get(vn => vn.Id == amenityId)
            };

            if (amenityVm.Amenity is null) RedirectToAction("Error", "Home");
            return View(amenityVm);
        }

        [HttpPost]
        public IActionResult Delete(AmenityVM amenityVM)
        {
            Amenity? objFromDb = _repo.Amenity.Get(u => u.Id == amenityVM.Amenity.Id); 

            if (objFromDb is not null) {
                _repo.Amenity.Remove(objFromDb);
                _repo.Amenity.Save();
                TempData["success"] = "The Amenity has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            //---
            amenityVM.VillaList = _repo.Villa.GetAll()
                                             .Select(u => new SelectListItem 
                                             { Text = u.Name, Value = u.Id.ToString() });
            return View(amenityVM);
        }        
    }
}
