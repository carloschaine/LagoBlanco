using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IAppUserRepository User { get; }   

        IVillaRepository Villa { get; }
        IVillaNumberRepository VillaNumber { get; }
        IAmenityRepository Amenity { get; }
        IBookingRepository Booking { get; }

        void Save();
    }
}
