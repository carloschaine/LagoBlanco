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
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _db;
        internal DbSet<T> dbSet; 
        public Repository(AppDbContext db)
        {
            _db = db;
            dbSet = _db.Set<T>();
        }


        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, 
                                     string? includeProperties = null,
                                     bool tracked = false)
        {
            IQueryable<T> query; 
            if (tracked) 
                query = dbSet; 
            else
                query = dbSet.AsNoTracking();
            //---
            if (filter is not null) query = query.Where(filter);
            //---
            if (!string.IsNullOrEmpty(includeProperties)) {
                //Villa,VillaNumber -- case sensitive
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                    query = query.Include(includeProp.Trim());
                }
            }
            //---
            return query.ToList();
        }


        public T Get(Expression<Func<T, bool>> filter, 
                     string? includeProperties = null, 
                     bool tracked = false)
        {            
            IQueryable<T> query;
            if (tracked)
                query = dbSet;
            else
                query = dbSet.AsNoTracking();
            //---
            if (filter is not null) query = query.Where(filter);
            //---
          

            if (!string.IsNullOrEmpty(includeProperties)) {
                // "VillaNumber, Villa"
                foreach (var property in includeProperties
                            .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
                    query = query.Include(property);
                }
            }
            //---
            return query.FirstOrDefault();
        }

        public bool Any(Expression<Func<T, bool>> filter)
        {
           return dbSet.Any(filter);
        }


        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);            
        }

    }
}
