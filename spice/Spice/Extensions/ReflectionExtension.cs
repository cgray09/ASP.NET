using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Extensions
{
    public static class ReflectionExtension
    {
        // gets the value of what ever property we pass into it
        public static string GetPropertyValue<T>(this T item, string propertyName)
        {
            return item.GetType().GetProperty(propertyName).GetValue(item, null).ToString();
        }
    }
}
