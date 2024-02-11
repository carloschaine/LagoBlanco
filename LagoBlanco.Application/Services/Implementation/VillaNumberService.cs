using LagoBlanco.Application.Common.Interfaces;
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
    public class VillaNumberService : IVillaNumberService
    {

        private readonly IUnitOfWork _repo;
        public VillaNumberService(IUnitOfWork repo)
        { _repo = repo; 
        }


        public IEnumerable<VillaNumber> GetAllVillaNumbers()
        {
            return _repo.VillaNumber.GetAll(null, "Villa");
        }

        public IEnumerable<VillaNumber> GetAllVillaNumbersByVillaId(int villaId)
        {
            return _repo.VillaNumber.GetAll(vn=>vn.VillaId==villaId, "Villa");
        }

        public VillaNumber GetVillaNumberById(int id)
        {
            return  _repo.VillaNumber.Get(u => u.Villa_Number == id,"Villa");
        }




        public void CreateVillaNumber(VillaNumber villaNumber)
        {
            _repo.VillaNumber.Add(villaNumber);
            _repo.Save();
        }
        public void UpdateVillaNumber(VillaNumber villaNumber)
        {
            _repo.VillaNumber.Update(villaNumber);
            _repo.Save();

        }
        public bool DeleteVillaNumber(int id)
        {
            try {
                VillaNumber? objDb = _repo.VillaNumber.Get(v => v.Villa_Number == id);

                if (objDb is not null) {
                    _repo.VillaNumber.Remove(objDb);
                    _repo.Save();
                    return true;
                }
                return false;
            }
            catch (Exception) {
                return false;
            }
        }


        public bool CheckVillaNumberExists(int villa_Number)
        {
            return _repo.VillaNumber.Any(u => u.Villa_Number == villa_Number);
        }

    }
}
