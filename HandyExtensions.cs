using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public static class HandyExtensions
{
    /// <summary>
    /// Converts IEnumerable type into DataTable. Allows null values.
    /// </summary>
    /// <typeparam name="T">Generic type.</typeparam>
    /// <param name="items">IEnumerable type of items.</param>
    /// <returns>Returns IEnumerable.</returns>
    public static DataTable IEnumerableToDataTable<T>(this IEnumerable<T> items)
    {
        try
        {
            var tb = new DataTable(typeof(T).Name);
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }
            return tb;
        }
        catch (Exception ex)
        {
            throw new Exception("HandyExtensions class: " + ex.Message.ToString());
        }
    }

    /// <summary>
    /// Converts List type into DataTable. Allows null values.
    /// </summary>
    /// <typeparam name="T">Generic type.</typeparam>
    /// <param name="items">List type of items.</param>
    /// <returns>Returns List.</returns>
    public static DataTable ListToDataTable<T>(this List<T> items)
    {
        try
        {
            var tb = new DataTable(typeof(T).Name);
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }
            return tb;
        }
        catch (Exception ex)
        {
            throw new Exception("HandyExtensions class: " + ex.Message.ToString());
        }
    }
}
