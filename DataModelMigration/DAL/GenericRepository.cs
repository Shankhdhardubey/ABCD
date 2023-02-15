using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
namespace DataModelMigration.DAL
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly MigrationScriptDbContext _context;
        public GenericRepository()
        {
            _context = new MigrationScriptDbContext();
        }

        #region Public Members
        public IEnumerable<T> Query(string query)
        {
            return _context.Set<T>().FromSqlRaw(query);
        }

        public int ExecuteSqlCommand(string query)
        {
            return _context.Database.ExecuteSqlCommand(query);
        }

        public virtual void Insert(T entity)
        {
            _context.Add(entity);
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        public object CheckTableExist(string query)
        {
            object result;
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                _context.Database.OpenConnection();
                 result = command.ExecuteScalar();
            }
            return result;
        }
        #endregion
    }
}