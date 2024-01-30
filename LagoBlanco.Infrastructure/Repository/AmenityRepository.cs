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

    public class AmenityRepository : Repository<Amenity>, IAmenityRepository
    {
        private readonly AppDbContext _db;
        public AmenityRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }


        public void Update(Amenity entity)
        {
            _db.Amenities.Update(entity);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

    }

}
