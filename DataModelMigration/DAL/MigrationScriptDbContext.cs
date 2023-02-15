using DataModelMigration.Common;
using DataModelMigration.Model.ClassModel;
using DataModelMigration.Model.DataModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DataModelMigration.DAL
{
    public class MigrationScriptDbContext : DbContext
    {
        /// <summary>
        /// Saves connection string
        /// </summary>
        private static readonly string _connectionString = Helper.CreateConnectionString();

        public MigrationScriptDbContext() : base()
        {
            Database.SetCommandTimeout(3000);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public DbSet<ScriptResult> ScriptResult { get; set; }

        public DbSet<OutputTable> GetTables { get; set; }

        public DbSet <Generic_Lookup_Data>LookupData { get; set; }


    }
}
