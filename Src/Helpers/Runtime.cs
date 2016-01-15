using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace SqlEtl.Helpers
{
    internal static class Runtime
    {
        #region public
        internal static Regex RegEx { get; } = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        internal static string Path => @"c:\scripts\";

        #endregion

        #region internal

        /// <summary>
        /// returns SqlDbType
        /// </summary>
        /// <param name="v">value of string type of SqlDbType</param>
        /// <returns>returns SqlDbType</returns>
        internal static SqlDbType GetType(string v)
        {
            switch (v)
            {
                case "sql_variant":
                    return SqlDbType.Variant;
                default:
                    return (SqlDbType)Enum.Parse(typeof(SqlDbType), v, true);
            }
        }

        /// <summary>
        /// returns the native sql type of string value
        /// </summary>
        /// <param name="t">SqlDbType</param>
        /// <returns>returns the native sql type of string value</returns>
        internal static string GetSqlType(SqlDbType t)
        {
            switch (t)
            {
                case SqlDbType.Variant:
                    return "sql_variant";
                default:
                    return t.ToString();
            }
        }

        /// <summary>
        /// calculates the length for nvarchar(max), varchar(max) and decimal
        /// </summary>
        /// <param name="type">sql native type</param>
        /// <param name="length">given length</param>
        /// <returns>returns length for nvarchar(max), varchar(max) and decimal</returns>
        internal static string GetLength(string type, int length)
        {
            if (type.IndexOf("char", StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                return length > 0 ? " (" + length + ")" : " (max)";
            }
            return type.IndexOf("decimal", StringComparison.InvariantCultureIgnoreCase) != -1 ? " (18,6)" : string.Empty;
        }

        /// <summary>
        /// Use this method to skip or hardcode some primary keys
        /// </summary>
        /// <param name="internalPkDictionary"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool CheckInternalKey(Dictionary<string, string[]> internalPkDictionary, string tableName, string columnName, bool value)
        {
            if (!internalPkDictionary.ContainsKey(tableName)) return value;
            string[] internalKeys;
            return !internalPkDictionary.TryGetValue(tableName, out internalKeys) ? value : internalKeys.Contains(columnName.ToLower());
        }

        #endregion
    }
}