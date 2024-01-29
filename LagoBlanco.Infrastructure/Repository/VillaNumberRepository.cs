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
   
    public class VillaNumberRepository : Repository<VillaNumber>, IVillaNumberRepository
    {
        private readonly AppDbContext _db;
        public VillaNumberRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(VillaNumber entity)
        {
            _db.VillaNumbers.Update(entity);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }
}
