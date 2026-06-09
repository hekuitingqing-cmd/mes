using System.Collections.Generic;

namespace MES.DAL.Interfaces
{
    public interface IRepository<T> where T : class
    {
        T GetById(string id);
        List<T> GetAll();
        List<T> GetByCondition(string whereClause, object parameters);
        bool Insert(T entity);
        bool Update(T entity);
        bool Delete(string id);
    }
}
