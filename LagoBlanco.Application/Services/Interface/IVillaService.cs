using LagoBlanco.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Services.Interface
{
    public interface IVillaService
    {
        IEnumerable<Villa> GetAllVillas();
        Villa GetVillaById(int villaId);

        void CreateVilla(Villa villa);
        void UpdateVilla(Villa villa);
        bool DeleteVilla(int villaId);

        bool IsVillaAvailableByDate(int villaId, int nights, DateOnly checkInDate);
        IEnumerable<Villa> GetVillasAvailabilityByDate(int nights, DateOnly checkInDate);
    }
}
