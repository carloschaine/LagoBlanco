using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Application.Common.Utility;
using LagoBlanco.Application.Services.Interface;
using LagoBlanco.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Services.Implementation
{
    public class VillaService : IVillaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VillaService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment; //path donde guardar images
        }


        public IEnumerable<Villa> GetAllVillas()
        {
            return _unitOfWork.Villa.GetAll(includeProperties:"amenities");
        }
        public Villa GetVillaById(int villaId)
        {
            return _unitOfWork.Villa.Get(v => v.Id == villaId, "amenities");
        }



        public void CreateVilla(Villa villa)
        {            
            //---Creo Archivo JPG
            if (villa.Image is not null) {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"Images\VillaImage");
                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                villa.Image.CopyTo(fileStream);
                villa.ImageUrl = @"\Images\VillaImage\" + fileName;
            }
            else {
                villa.ImageUrl = @"\images\placeholder.png";
            }

            _unitOfWork.Villa.Add(villa);
            _unitOfWork.Villa.Save();
        }



        public void UpdateVilla(Villa villa)
        {
            //---Creo Archivo JPG
            if (villa.Image is not null) {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"Images\VillaImage");
                //---
                if (!string.IsNullOrEmpty(villa.ImageUrl)) {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));
                    if (File.Exists(oldImagePath) && !oldImagePath.Contains("placeholder.png")) 
                        File.Delete(oldImagePath);
                }
                using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                villa.Image.CopyTo(fileStream);
                villa.ImageUrl = @"\Images\VillaImage\" + fileName;
            }
            else {
                villa.ImageUrl = @"\images\placeholder.png";
            }

            _unitOfWork.Villa.Update(villa);
            _unitOfWork.Villa.Save();
        }



        public bool DeleteVilla(int villaId)
        {
            try {
                Villa? objDb = _unitOfWork.Villa.Get(v => v.Id == villaId);

                if (objDb is not null) {
                    if (!string.IsNullOrEmpty(objDb.ImageUrl)) {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, objDb.ImageUrl.TrimStart('\\'));
                        if (File.Exists(oldImagePath) && !oldImagePath.Contains("placeholder.png")) 
                            File.Delete(oldImagePath);
                    }
                    _unitOfWork.Villa.Remove(objDb);
                    _unitOfWork.Villa.Save();
                    return true;
                }
                return false;
            }
            catch (Exception) {
                return false; 
            }            
        }


        public IEnumerable<Villa> GetVillasAvailabilityByDate(int nights, DateOnly checkInDate)
        {
            var villaList = this.GetAllVillas();
            var villaNumbers = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(b => b.Status == SD.StatusApproved ||
                                                         b.Status == SD.StatusCheckedIn).ToList();
            foreach (var villa in villaList) {
                int roomAvailable =
                    SD.VillaRoomsAvailable_Count(villa.Id, villaNumbers, checkInDate, nights, bookedVillas);
                villa.IsAvailable = roomAvailable > 0;
            }
            return villaList;                 
        }


        public bool IsVillaAvailableByDate(int villaId, int nights, DateOnly checkInDate)
        {
            var villaNumbers = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll( b=>b.Status==SD.StatusApproved || 
                                                              b.Status==SD.StatusCheckedIn)
                                                  .ToList();
            int roomAvailable = SD.VillaRoomsAvailable_Count(villaId, villaNumbers, checkInDate, nights, bookedVillas);
            return roomAvailable > 0;

        }
    }
}
