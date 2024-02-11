using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Domain.Entities;
using LagoBlanco.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Infrastructure.Repository
{

    public class AppUserdddRepository : Repository<ApplicationUser>, IAppUserRepository
    {
        private readonly AppDbContext _db;
        public AppUserdddRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }


       

    }

}
