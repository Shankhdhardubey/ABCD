using DataModelMigration.Common;
using DataModelMigration.DAL;
using System;
using System.Collections.Generic;
using DataModelMigration.Model.DataModel;
using DataModelMigration.Script;
using System.IO;
using OfficeOpenXml;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace DataModelMigration.Service
{
    /// <summary>
    /// Data Model Migration
    /// </summary>
    public class Migration
    {
        public void DoProcess()
        {
            if (Convert.ToBoolean(AppConfiguration.CheckExcelMigration))
            {
                DateTime startTime = Helper.GetCurrenctDate();


              using (var stream = new FileStream(AppConfiguration.ExcelFilePath + AppConfiguration.ExcelFileName, FileMode.Open))
                {
                    ISheet sheet;
                    HSSFWorkbook workBook = new HSSFWorkbook(stream);
                    if (workBook.Count > 0)
                    {
                        IGenericRepository<Generic_Lookup_Data> repoContext = new GenericRepository<Generic_Lookup_Data>();
                        new ExcelMigrationScript().RestoreExcelMigrationTable();
                        Dictionary<string, string> classPropertyDict = new Dictionary<string, string>();
                        classPropertyDict = Helper.GetClassPropertyWithValue<ExcelMigrationScript>();
                        ExecuteMigration(classPropertyDict);
                        for (int index=0; index< workBook.Count; index++)
                        {
                            string sheetName = workBook.GetSheetAt(index).SheetName;
                            string lookupValue = AppConfiguration.GetLookupValue(sheetName.Replace(" ", ""));
                            if (!string.IsNullOrEmpty(lookupValue))
                            {
                                sheet = (HSSFSheet)workBook.GetSheet(sheetName);
                                for (int row =Convert.ToInt32(AppConfiguration.ExcelRowStartingIndex); row < sheet.LastRowNum; row++)
                                {
                                    var headerRow = sheet.GetRow(row);
                                    Generic_Lookup_Data lookup = new Generic_Lookup_Data
                                    {
                                        Source_Code = headerRow.Cells[1].ToString().Replace(" ", ""),
                                        Source_Description = headerRow.Cells[2].ToString().Replace(" ", ""),
                                        Target_Code = headerRow.Cells[4].StringCellValue.ToString().Replace(" ", ""),
                                        Target_Description = headerRow.Cells[5].ToString().Replace(" ", ""),
                                        LookUp_Name = lookupValue
                                    };
                                    if (!string.IsNullOrEmpty(lookup.Source_Code))
                                        {
                                            repoContext.Insert(lookup);
                                            repoContext.Save();
                                        }
                                }
                            }
                        }
                    }
                    DateTime endTime = Helper.GetCurrenctDate();
                    Helper.WriteToFile($"{Helper.GetCurrenctDate()} -- Execution Time {Helper.GetTimeDifference(startTime, endTime)}", AppConfiguration.File_FileName);
                }
            }
            if (Convert.ToBoolean(AppConfiguration.CheckDataModelMigration))
            {

                IList<MigrationTypes> enumVales = EnumHelper<MigrationTypes>.GetValues(new MigrationTypes());

                Dictionary<string, string> classPropertyDict = new Dictionary<string, string>();

                classPropertyDict = Helper.GetClassPropertyWithValue<DataModelMigrationcs>();

                ExecuteMigration(classPropertyDict);

                new DataModelMigrationcs().ExecuteOutputTables();
            }
            Helper.WriteToFile($"", AppConfiguration.File_SqlFileName);
            Helper.WriteToFile($"----------------------------------------", AppConfiguration.File_SqlFileName);
            Helper.WriteToFile($"----------------------------------------", AppConfiguration.File_SqlFileName);
            Helper.WriteToFile($"", AppConfiguration.File_SqlFileName);


        }

        private void ExecuteMigration(Dictionary<string, string> classPropertyDict)
        {
            string scriptName, script;

            IGenericRepository<ScriptResult> repoContext = new GenericRepository<ScriptResult>();
            foreach (KeyValuePair<string, string> field in classPropertyDict)
            {
                scriptName = field.Key;
                script = field.Value.Replace("@@VARIABLENAME@@", field.Key);

                DateTime startTime = Helper.GetCurrenctDate();
                if (string.IsNullOrWhiteSpace(script))
                {
                    continue;
                }
                else
                {
                    repoContext.ExecuteSqlCommand(script);
                }

                DateTime endTime = Helper.GetCurrenctDate();
                Helper.WriteToFile($"--------------------{scriptName}--------------------", AppConfiguration.File_SqlFileName);
                Helper.WriteToFile(script, AppConfiguration.File_SqlFileName);
                Helper.WriteToFile($"", AppConfiguration.File_SqlFileName);
                Helper.WriteToFile($"GO", AppConfiguration.File_SqlFileName);
                Helper.WriteToFile($"", AppConfiguration.File_SqlFileName);

                Helper.WriteToFile($"{Helper.GetCurrenctDate()} -- {scriptName} - Execution Time {Helper.GetTimeDifference(startTime, endTime)}", AppConfiguration.File_FileName);
            }
        }
    }
}
