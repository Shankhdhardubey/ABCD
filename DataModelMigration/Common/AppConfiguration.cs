using DataModelMigration.Common;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataModelMigration.Common
{
    public static class AppConfiguration
    {
        private static readonly IConfigurationRoot _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

        #region Environment Settings
        private static bool? _migrateDatabase;
        public static bool MigrateDatabase
        {
            get => _migrateDatabase == null ? GetAppSettingData("MigrateDatabase").ConvertTo<bool>() : _migrateDatabase.ConvertTo<bool>();
            set => _migrateDatabase = value;
        }

        #endregion

        #region User Settings

        private static string _dataBase_Source_Server;
        public static string DataBase_Source_Server
        {
            get => _dataBase_Source_Server ?? GetAppSettingData("DataBase:Source:Server");
            set => _dataBase_Source_Server = value;
        }

        private static string _dataBase_Source_Name;
        public static string DataBase_Source_Name
        {
            get => _dataBase_Source_Name ?? GetAppSettingData("DataBase:Source:Name");
            set => _dataBase_Source_Name = value;
        }

        private static string _dataBase_Source_Credentials_UserName;
        public static string DataBase_Source_Credentials_UserName
        {
            get => _dataBase_Source_Credentials_UserName ?? GetAppSettingData("DataBase:Source:Credentials:UserName");
            set => _dataBase_Source_Credentials_UserName = value;
        }

        private static string _dataBase_Source_Credentials_Password;
        public static string DataBase_Source_Credentials_Password
        {
            get => _dataBase_Source_Credentials_Password ?? GetAppSettingData("DataBase:Source:Credentials:Password");
            set => _dataBase_Source_Credentials_Password = value;
        }

        private static string _file_Path;
        public static string File_Path
        {
            get => _file_Path ?? GetAppSettingData("File:Path");
            set => _file_Path = value;
        }

        private static string _file_FileName;
        public static string File_FileName
        {
            get => _file_FileName ?? GetAppSettingData("File:FileName");
            set => _file_FileName = value;
        }

        private static string _file_SqlFileName;
        public static string File_SqlFileName
        {
            get => _file_SqlFileName ?? GetAppSettingData("File:SqlFileName");
            set => _file_SqlFileName = value;
        }


        private static string _defaultUser;
        public static string DefaultUser
        {
            get => _defaultUser ?? GetAppSettingData("DefaultUser");
            set => _defaultUser = value;
        }

        #endregion

        #region Default Settings
       
        public static string SourceDataModelCode => GetAppSettingData("SourceDataModelCode");
        public static string TargetDataModelCode => GetAppSettingData("TargetDataModelCode");

        public static string LookupTable => GetAppSettingData("LookupTable");
   
        public static string CheckExcelMigration => GetAppSettingData("MigrationCheck:ExcelMigration");
        public static string CheckDataModelMigration=> GetAppSettingData("MigrationCheck:DataModelMigration");

        public static string ExcelFilePath=> GetAppSettingData("ExcelFile:Path");
        public static string ExcelFileName => GetAppSettingData("ExcelFile:Name");
        #endregion
        public static string ExcelRowStartingIndex=> GetAppSettingData("ExcelRowStartingIndex");
        private static string GetAppSettingData(string key)
        {
            return _configuration[key];
        }

        public static string GetLookupValue(string key)
        {
            string lookup=string.Empty;
            var   lookupValue = _configuration.GetSection("LookupValues")
                .GetChildren().Where(x => x.Key == key).Select(y => y.Value);
            foreach (string s in lookupValue)
            {
                lookup = s;
            }

            return lookup.ToString();
            // Dictionary<string, string> myDict =
            //new Dictionary<string, string>();

            // var valuesSection = _configuration.GetSection("MySettings:MyValues");
            // foreach (IConfigurationSection section in valuesSection.GetChildren())
            // {
            //     myDict.Add(section.GetValue<string>("Key"), section.GetValue<string>("Value"));

            // }

        }
    }
}