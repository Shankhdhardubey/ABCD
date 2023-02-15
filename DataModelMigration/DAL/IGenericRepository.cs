using System.Collections.Generic;

namespace DataModelMigration.DAL
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> Query(string query);
        int ExecuteSqlCommand(string query);
        void Insert(T entity);
        int Save();
        object CheckTableExist(string query);

    }
}
