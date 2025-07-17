using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Functions
{
    public static class CL_ObjectComparer
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

        public static bool IsUnmodified<T>(T obj, T defaultObj)
        {
            if (obj == null || defaultObj == null)
                return false; // Avoid null reference exceptions

            var properties = _propertyCache.GetOrAdd(typeof(T), type => type.GetProperties());

            foreach (var prop in properties)
            {
                var objValue = prop.GetValue(obj);
                var defaultValue = prop.GetValue(defaultObj);

                if (!Equals(objValue, defaultValue))
                    return false; // If any property differs, return false
            }

            return true; // All properties match the default instance
        }
        /*
         *             if (CL_ObjectComparer.IsUnmodified(TheRow, DefaultInvoice)) //this is now gonna use because the anbar would be diffrent object cuz the default value will change

         */
    }
}
