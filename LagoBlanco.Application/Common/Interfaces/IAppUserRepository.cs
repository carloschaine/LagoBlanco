using LagoBlanco.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Common.Interfaces
{
    public interface IAppUserRepository : IRepository<ApplicationUser>
    {
       // void Update(ApplicationUser entity);
       // void Save();
    }
}
