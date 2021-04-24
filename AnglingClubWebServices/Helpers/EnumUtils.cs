using System;
using System.Collections.Generic;
using System.Linq;

namespace AnglingClubWebServices.Helpers
{
    public static class EnumUtils
    {
        /// <summary>
        /// Enumerates the values of the enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
