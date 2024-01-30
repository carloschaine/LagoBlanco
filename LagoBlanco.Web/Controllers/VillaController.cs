using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Domain.Entities;
using LagoBlanco.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LagoBlanco.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _repo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaController(IUnitOfWork repo,
                               IWebHostEnvironment webHostEnvironment)
        {   _repo = repo;
            _webHostEnvironment = webHostEnvironment; //path donde guardar images
        }

        public IActionResult Index()
        {
            var villas = _repo.Villa.GetAll();
            return View(villas);
        }


        public IActionResult Create() 
        { 
            return View();  
        }

        [HttpPost]
        public IActionResult Create(Villa obj )
        {//Pasado desde el submit de Create. El Form pasa automatic. el Model.  
            if (obj.Name==obj.Description) {
                ModelState.AddModelError("name", "La description no puede ser igual al nombre");}

            if (ModelState.IsValid) { 

                //---Creo Archivo JPG
                if(obj.Image is not null) {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath,//hasta wwwroot
                                                    @"Images\VillaImage");
                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    obj.Image.CopyTo(fileStream);
                    obj.ImageUrl = @"\Images\VillaImage\" + fileName;
                }
                else {
                    obj.ImageUrl = @"\images\placeholder.png"; 
                }

                _repo.Villa.Add(obj);
                _repo.Villa.Save();
                TempData["success"] = "The villa has been created successfully.";
                return RedirectToAction(nameof(Index));// RedirectToAction("Index","Villa");
            }
            return View();
        }


        public IActionResult Update(int villaId)
        {
            Villa? obj = _repo.Villa.Get(v=>v.Id==villaId);
            if (obj is null)  RedirectToAction("Error", "Home");

            return View(obj);
        }


        [HttpPost]
        public IActionResult Update(Villa obj)
        {

            if (ModelState.IsValid && obj.Id > 0) {
                //---Creo Archivo JPG
                if (obj.Image is not null) {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"Images\VillaImage");
                    //---
                    if (!string.IsNullOrEmpty(obj.ImageUrl)) {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                    }
                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    obj.Image.CopyTo(fileStream);
                    obj.ImageUrl = @"\Images\VillaImage\" + fileName;
                }
                else {
                    obj.ImageUrl = @"\images\placeholder.png";
                }

                _repo.Villa.Update(obj);
                _repo.Villa.Save();
                TempData["success"] = "The villa has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa has not updated.";
            return View();
        }

        public IActionResult Delete(int villaId)
        {
            Villa? obj = _repo.Villa.Get(v => v.Id == villaId);
            if (obj is null) RedirectToAction("Error", "Home");

            return View(obj);
        }


        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
            Villa? objDb = _repo.Villa.Get(v => v.Id == obj.Id);

            if (objDb is not null) {
                if (!string.IsNullOrEmpty(obj.ImageUrl)) {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                }
                _repo.Villa.Remove(objDb);
                _repo.Villa.Save();
                TempData["success"] = "The villa has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa has not Deleted.";
            return View();
        }
    }
}
