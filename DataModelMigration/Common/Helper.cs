
using Microsoft.Win32;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace DataModelMigration.Common
{
    public static class Helper
    {
        public static Dictionary<string, string> GetClassPropertyWithValue<T>() where T : class, new()
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            BindingFlags bindingFlags = BindingFlags.Public |
                         BindingFlags.NonPublic |
                         BindingFlags.Instance |
                         BindingFlags.Static;

            FieldInfo[] dd = typeof(T).GetFields(bindingFlags);

            T instance = new T();
            foreach (FieldInfo item in dd)
            {
                res.Add(item.Name.Substring(item.Name.IndexOf('<') + 1, item.Name.IndexOf('>') - 1), (string)item.GetValue(instance));
            }
            return res;
        }

        public static string GetTimeDifference(DateTime startDate, DateTime endDate)
        {
            return endDate.Subtract(startDate).ToString();
        }

        public static DateTime GetCurrenctDate()
        {
            return DateTime.Now;
        }

        public static void WriteToFile(string data, string fileName)
        {
            string filepath = !string.IsNullOrWhiteSpace(AppConfiguration.File_Path) ? AppConfiguration.File_Path : AppConfiguration.File_Path;
            CreateFile(filepath, fileName);
            WriteFile(filepath + fileName, data);

            if (string.Equals(fileName, AppConfiguration.File_FileName))
            {
                Console.WriteLine(data);
            }
        }

        private static readonly ReaderWriterLock _rwl = new ReaderWriterLock();
        public static void WriteFile(string filePath, string data)
        {
            _rwl.AcquireWriterLock(Timeout.Infinite);
            StreamWriter sw = new StreamWriter(filePath, true);
            sw.WriteLine(data);
            sw.Close();
            _rwl.ReleaseWriterLock();
        }

        public static bool IsFileExist(string filePath)
        {
            return File.Exists(filePath);
        }

        private static void CreateFile(string filePath, string fileName)
        {
            CreateFolder(filePath);
            CreateFile(filePath + fileName);
        }

        public static void CreateFile(string filePath)
        {
            if (!IsFileExist(filePath))
            {
                FileStream stream = File.Create(filePath);
                stream.Close();
            }
        }

        public static void DeleteFile(string filePath)
        {
            if (IsFileExist(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public static bool CopyFile(string sourceFilePath, string targetFilePath, bool overwrite = true)
        {
            if (IsFileExist(sourceFilePath))
            {
                CreateFolder((new FileInfo(targetFilePath)).DirectoryName);
                File.Copy(sourceFilePath, targetFilePath, overwrite);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Constructing connection string from the inputs
        /// </summary>
        /// <returns></returns>
        public static string CreateConnectionString()
        {
            StringBuilder con = new StringBuilder("Server=");
            con.Append(AppConfiguration.DataBase_Source_Server);
            con.Append(";Database=");
            con.Append(AppConfiguration.DataBase_Source_Name);


                con.Append(";User Id=");
                con.Append(AppConfiguration.DataBase_Source_Credentials_UserName);
                con.Append(";Password=");
                con.Append(AppConfiguration.DataBase_Source_Credentials_Password);
            
            return con.ToString();
        }

        /// <summary>
        /// Reads data from the file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadFromFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// Convert json to xml
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static XDocument ConvertJSonToXML(string jsonString)
        {
            return JsonConvert.DeserializeXNode(jsonString, "Root");
        }

        /// <summary>
        /// Execute .exe file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        public static void ExecuteExe(string filePath, string fileName)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                FileName = $"{filePath}\\{fileName}",
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas"
            };

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }

        /// <summary>
        /// Create registy key for the local machine
        /// </summary>
        /// <param name="registryPath"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void CreateLocalMachineRegistry(string registryPath, string key, string value)
        {
            RegistryKey registry;
            registry = Registry.LocalMachine.CreateSubKey(registryPath);
            registry.SetValue(key, value);
            registry.Close();
        }

        /// <summary>
        /// Check if application running as admin
        /// </summary>
        /// <returns></returns>
        public static bool CheckIfAppRunAsAdmin()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    return true;
                }
            }
            return false;
        }

        #region Excel
        public static XSSFWorkbook GetExcelMetaData(string filePath)
        {
            if (!Helper.IsFileExist(filePath))
            {
                return null;
            }

            XSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new XSSFWorkbook(file);
            }
            return hssfwb;
        }

        public static ISheet ReadExcel(string filePath, string sheetName)
        {
            XSSFWorkbook hssfwb = GetExcelMetaData(filePath);
            if (hssfwb != null)
            {
                ISheet sheet = hssfwb.GetSheet(sheetName);
                return sheet;
            }
            return null;
        }

        public static string GetCellValue(ISheet sheet, int rowNum, int cellNum)
        {
            string cellVal = string.Empty;
            if (sheet.GetRow(rowNum).GetCell(cellNum) != null)
            {
                if (sheet.GetRow(rowNum).GetCell(cellNum).CellType == CellType.Boolean)
                {
                    cellVal = sheet.GetRow(rowNum).GetCell(cellNum).BooleanCellValue.ToString();
                }
                else if (sheet.GetRow(rowNum).GetCell(cellNum).CellType == CellType.Numeric)
                {
                    cellVal = sheet.GetRow(rowNum).GetCell(cellNum).NumericCellValue.ToString();
                }
                else if (sheet.GetRow(rowNum).GetCell(cellNum).CellType == CellType.String)
                {
                    cellVal = sheet.GetRow(rowNum).GetCell(cellNum).StringCellValue;
                }
            }
            return cellVal;
        }
        #endregion

        //public static string GetProductCodeChangeScript()
        //{
        //    string productCodeChangeScript = string.Empty;
        //    string[] productCodeList = AppConfiguration.Product_ChangeProductCode.Split(',');
        //    foreach (string productCode in productCodeList)
        //    {
        //        string[] newOldProductCodes = productCode.Split(':');
        //        productCodeChangeScript = $@"{productCodeChangeScript}update {AppConstants.TargetDatabaseWithServer}[Product] set code = '{newOldProductCodes[1]}' where code = '{newOldProductCodes[0]}';";
        //    }

        //    return productCodeChangeScript;
        //}

        public static string GetMimeType(string extension)
        {
            if (extension == null)
                throw new ArgumentNullException("extension");

            if (extension.StartsWith("."))
                extension = extension.Substring(1);

            switch (extension.ToLower())
            {
                case "msg": return "application/vnd.ms-outlook";
                case "pdf": return "application/pdf";
                case "doc": return "application/msword";
                case "docx": return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "xls": return "application/vnd.ms-excel";
                case "xlsx": return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                default: return "application/octet-stream";
            }
        }

        public static T DeserializeObject<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
