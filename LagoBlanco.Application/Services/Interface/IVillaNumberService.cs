using LagoBlanco.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Services.Interface
{
    public interface IVillaNumberService
    {

        IEnumerable<VillaNumber> GetAllVillaNumbers();
        IEnumerable<VillaNumber> GetAllVillaNumbersByVillaId(int villaId);
        VillaNumber GetVillaNumberById(int id);

        void CreateVillaNumber(VillaNumber villaNumber);
        void UpdateVillaNumber(VillaNumber villaNumber);
        bool DeleteVillaNumber(int id);

        bool CheckVillaNumberExists(int villa_Number);



    }
}
