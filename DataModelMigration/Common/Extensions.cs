using System;

namespace DataModelMigration.Common
{
    public static class Extensions
    {
        public static T ConvertTo<T>(this object obj)
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public static string FirstLetterToUpper(this string str)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length > 1)
            {
                return char.ToUpper(str[0]) + str.Substring(1).ToLower();
            }

            return str.ToUpper();
        }

        public static double GetFileSizeInMB(this long size)
        {
            return size / 1048576.0;
        }
    }
}
