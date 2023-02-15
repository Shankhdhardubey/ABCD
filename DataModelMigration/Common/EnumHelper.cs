using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DataModelMigration.Common
{
    public static class EnumHelper<T>
    {
        public static IList<T> GetValues(Enum value)
        {
            List<T> enumValues = new List<T>();

            foreach (FieldInfo fi in value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                enumValues.Add((T)Enum.Parse(value.GetType(), fi.Name, false));
            }
            return enumValues;
        }

        public static IList<string> GetDisplayValues(Enum value)
        {
            return GetNames(value).Select(obj => GetDisplayValue(Parse(obj))).ToList();
        }

        private static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static IList<string> GetNames(Enum value)
        {
            return value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public).Select(fi => fi.Name).ToList();
        }

        public static string GetDisplayValue(T value)
        {
            MemberInfo memberInfo = typeof(T).GetMember(value.ToString()).First();

            // we can then attempt to retrieve the    
            // description attribute from the member info    
            DescriptionAttribute descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();

            // if we find the attribute we can access its values    
            if (descriptionAttribute != null)
            {
                return descriptionAttribute.Description;
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
