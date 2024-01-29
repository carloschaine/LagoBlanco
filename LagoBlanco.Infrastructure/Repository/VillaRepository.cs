using LagoBlanco.Application.Common.Interfaces;
using LagoBlanco.Domain.Entities;
using LagoBlanco.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Infrastructure.Repository
{
    public class VillaRepository : Repository<Villa>, IVillaRepository
    {
        private readonly AppDbContext _db;
        public VillaRepository(AppDbContext db):base(db)
        {
            _db = db;
        }

        
        public void Update(Villa entity)
        {
            _db.Villas.Update(entity);
        }
        
        public void Save()
        {
            _db.SaveChanges();            
        }

    }
}

