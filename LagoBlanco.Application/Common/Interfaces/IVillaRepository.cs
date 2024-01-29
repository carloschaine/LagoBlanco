using LagoBlanco.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LagoBlanco.Application.Common.Interfaces
{
    public interface IVillaRepository:IRepository<Villa>
    {
        //IEnumerable<Villa> GetAll(Expression<Func<Villa, bool>>? filter = null, //filtro como Expression
        //                          string? includeProperties = null);  //si Villa es parte de un Include(). ej.VillaNumber.
        //Villa Get(Expression<Func<Villa, bool>>? filter, string? includeProperties = null);
        //void Add(Villa entity); 
        //void Delete(Villa entity);

        void Update(Villa entity);
        void Save(); 
    }
}
