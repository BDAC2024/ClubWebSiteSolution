using System.ComponentModel;
using System;

namespace AnglingClubWebServices.Services
{
    public static class EnumEx
    {
        public static T GetValueFromDescription<T>(this string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field,
                typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description.ToLower() == description.ToLower())
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name.ToLower() == description.ToLower())
                        return (T)field.GetValue(null);
                }
            }

            throw new ArgumentException("Not found.", nameof(description));
            // Or return default(T);
        }
    }
}
