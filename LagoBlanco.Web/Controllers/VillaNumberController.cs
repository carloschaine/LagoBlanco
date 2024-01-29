using LagoBlanco.Domain.Entities;
using LagoBlanco.Infrastructure.Data;
using LagoBlanco.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LagoBlanco.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly AppDbContext _db;
        public VillaNumberController(AppDbContext context)
        {
            _db = context;
        }


        public IActionResult Index()
        {
            var villaNumbers = _db.VillaNumbers
                                  .Include(v=>v.Villa).ToList();

            return View(villaNumbers);
        }

        public IActionResult Create() 
        {
            VillaNumberVM villaNumberVm = new VillaNumberVM() {
                VillaList = _db.Villas.ToList().Select(u => new SelectListItem 
                                                { Text = u.Name, Value = u.Id.ToString() }),
                VillaNumber = new VillaNumber()
            };
            return View(villaNumberVm);  
        }


        [HttpPost]
        public IActionResult Create(VillaNumberVM obj )
        {//Pasado desde el submit de Create. El Form pasa automatic. el Model.  

            //ModelState.Remove("Villa"); 
            bool roomExist = _db.VillaNumbers.Any(vn => vn.Villa_Number == obj.VillaNumber.Villa_Number);

            if (ModelState.IsValid && !roomExist) { 
                _db.VillaNumbers.Add(obj.VillaNumber);
                _db.SaveChanges();
                TempData["success"] = "The villaNumber has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            if (roomExist) TempData["error"] = "Habitación ya existe.";
            //---
            obj.VillaList = _db.Villas.ToList().Select(u => new SelectListItem 
                                                    { Text = u.Name, Value = u.Id.ToString() }); 
            return View(obj);
        }


        public IActionResult Update(int villaNumberId)
        {
            VillaNumberVM villaNumberVm = new VillaNumberVM() {
                VillaList = _db.Villas.ToList().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() }),
                VillaNumber = _db.VillaNumbers.FirstOrDefault(vn=>vn.Villa_Number==villaNumberId)
            };
                       
            if (villaNumberVm.VillaNumber is null)  RedirectToAction("Error", "Home");
            return View(villaNumberVm);
        }


        [HttpPost]
        public IActionResult Update(VillaNumberVM  villaNumberVM)
        {

            if (ModelState.IsValid) {
                _db.VillaNumbers.Update(villaNumberVM.VillaNumber);
                _db.SaveChanges();
                TempData["success"] = "The villaNumber has been modified successfully.";
                return RedirectToAction(nameof(Index));
            }            
            //---
            villaNumberVM.VillaList = _db.Villas.ToList().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
            return View(villaNumberVM);
        }


        public IActionResult Delete(int villaNumberId)
        {
            VillaNumberVM villaNumberVm = new VillaNumberVM() {
                VillaList = _db.Villas.ToList().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() }),
                VillaNumber = _db.VillaNumbers.FirstOrDefault(vn => vn.Villa_Number == villaNumberId)
            };

            if (villaNumberVm.VillaNumber is null) RedirectToAction("Error", "Home");
            return View(villaNumberVm);
        }

        [HttpPost]
        public IActionResult Delete(VillaNumberVM villaNumberVM)
        {
            if (ModelState.IsValid) {
                _db.VillaNumbers.Remove(villaNumberVM.VillaNumber);
                _db.SaveChanges();
                TempData["success"] = "The villaNumber has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            //---
            villaNumberVM.VillaList = _db.Villas.ToList().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
            return View(villaNumberVM);
        }
        
    }
}
