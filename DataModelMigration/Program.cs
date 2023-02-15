using DataModelMigration.Common;
using DataModelMigration.Service;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DataModelMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                

                RunMigration(args);
            }
            catch (Exception ex)
            {              
                Helper.WriteToFile($"Error Message - {ex.Message}", AppConfiguration.File_FileName);
            }
        }


        private static void RunMigration(string[] args)
        {
            AppConstants.ConfigSettings = Helper.ConvertJSonToXML(Helper.ReadFromFile(AppDomain.CurrentDomain.BaseDirectory + "\\appsettings.json")).ToString().Replace('\'', '"');
            //if (AppConfiguration.IsRunningOnServer && !Helper.CheckIfAppRunAsAdmin())
            //{
            //    Helper.WriteToFile($"Application must be run as administrator", AppConfiguration.File_FileName);
            //    return;
            //}

            DateTime start = Helper.GetCurrenctDate();
            Helper.WriteToFile($"Migration start time {start}", AppConfiguration.File_FileName);

            //new DatabaseBackupandRestore().BackupDataBase(AppConfiguration.DataBase_Source_Name, true, false);
            //new DatabaseBackupandRestore().BackupDataBase(AppConfiguration.DataBase_Target_Name, true);
            //_isBackupCompleted = true;

                new Migration().DoProcess();

           // new DatabaseBackupandRestore().BackupDataBase(AppConfiguration.DataBase_Target_Name, false);
           // new DatabaseBackupandRestore().BackupDataBase(AppConfiguration.DataBase_Temp_Name, false);

            DateTime end = Helper.GetCurrenctDate();
            Helper.WriteToFile($"Migration end time {end}", AppConfiguration.File_FileName);
            Helper.WriteToFile($"Total migration time {Helper.GetTimeDifference(start, end)}", AppConfiguration.File_FileName);
        }
    }
}
