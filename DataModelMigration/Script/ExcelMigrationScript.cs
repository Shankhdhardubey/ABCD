using DataModelMigration.Common;
using DataModelMigration.DAL;
using DataModelMigration.Model.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModelMigration.Script
{
    class ExcelMigrationScript
    {
        public string Create_HIUAGH_Generic_Lookup_Data { get; set; } = $@"----------/**/----------
            CREATE TABLE[dbo].[Generic_Lookup_Data]
        (

   [Generic_Lookup_Data_id][int] IDENTITY(1,1) NOT NULL,
  [Source_Code] [char](10) NOT NULL,
  [Source_Description] [varchar] (255) NULL,
    [Target_Code] [char](10) NOT NULL,
    [Target_Description] [varchar] (255) NULL,
    [LookUp_Name] [varchar] (255) NOT NULL,
  CONSTRAINT[PK__Generic_Lookup] PRIMARY KEY CLUSTERED
(
    [Generic_Lookup_Data_id] ASC
)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON[PRIMARY]
) ON[PRIMARY]";

        public void RestoreExcelMigrationTable()
        {
            IGenericRepository<object> repoContext = new GenericRepository<object>();
            string excelMigrationTable = "if exists(select table_name FROM information_schema.tables WHERE table_name = '"+AppConfiguration.LookupTable+"')select 1 else select 0";
            bool checkOutputTableExist = Convert.ToBoolean(repoContext.CheckTableExist(excelMigrationTable));
            if (checkOutputTableExist)
            {
                repoContext.ExecuteSqlCommand("Drop table "+ AppConfiguration.LookupTable);
            }

        }
    }

}
