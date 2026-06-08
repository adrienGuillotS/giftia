using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignerSuite.Core.Utilities
{
    public static class ExcelDataHelper
    {
        public static string GetOledbConnectionString(string filepath, bool hdr = true)
        {
            string connectionString = string.Empty;
            string hdrValue = hdr ? "YES" : "NO";
            // Set HDR=No if you want to get the header row as well, else set it to true to ignore it
            if (Path.GetExtension(filepath) == ".xls")
            {
                connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filepath + "; Extended Properties='Excel 8.0;HDR=" + hdrValue + ";IMEX=1;';";
            }
            else if (Path.GetExtension(filepath) == ".xlsx")
            {
                connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filepath + "; Extended Properties = 'Excel 12.0;HDR=" + hdrValue + ";IMEX=1;';";
            }

            return connectionString;
        }
        public static string[] GetWorksheetNames(string filePath, bool hdr)
        {
            string connectionString = GetOledbConnectionString(filePath, hdr);

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();

                DataTable schema = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (schema == null)
                {
                    throw new Exception("No worksheets found in the Excel file.");
                }

                string[] worksheetNames = new string[schema.Rows.Count];

                for (int i = 0; i < schema.Rows.Count; i++)
                {
                    worksheetNames[i] = schema.Rows[i]["TABLE_NAME"].ToString();
                }

                return worksheetNames;
            }
        }

        public static List<T> ReadExcelData<T>(string filePath, string sheetName) where T : new()
        {
            List<T> result = new List<T>();
            string connectionString = GetOledbConnectionString(filePath);


            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                using (OleDbCommand cmd = new OleDbCommand($"SELECT * FROM [{sheetName}$] AS [{sheetName.Replace('$', '_')}$]", connection))
                {
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        var properties = typeof(T).GetProperties();

                        foreach (DataRow row in dataTable.Rows)
                        {
                            T item = new T();
                            bool hasValue = false;
                            foreach (DataColumn column in dataTable.Columns)
                            {
                                var columnName = column.ColumnName.Trim();

                                // Find the property with a matching name (case-insensitive)
                                var property = properties.FirstOrDefault(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                                if (property != null)
                                {
                                    var value = row[column].ToString().Trim();

                                    if (value.EndsWith(" mm", StringComparison.OrdinalIgnoreCase) ||
                                        value.EndsWith("pt", StringComparison.OrdinalIgnoreCase) && property.PropertyType == typeof(double))
                                    {
                                        // Handle " mm" values for double properties
                                        value = value.ConvertStringToDouble().ToString();
                                        double numericValue;
                                        if (double.TryParse(value, out numericValue))
                                        {
                                            property.SetValue(item, numericValue, null);
                                            hasValue = true;
                                        }
                                        else
                                        {
                                            // Handle conversion failure
                                        }
                                    }
                                    else
                                    {
                                        // Convert and set the property's value based on its type
                                        Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                                        if (!string.IsNullOrWhiteSpace(value))
                                        {
                                            if (value.Equals("Now", StringComparison.OrdinalIgnoreCase))
                                            {
                                                value = DateTime.Now.ToShortDateString().ToString();
                                            }
                                            object convertedValue = Convert.ChangeType(value, propertyType);
                                            property.SetValue(item, convertedValue, null);
                                            hasValue = true;
                                        }
                                    }
                                }
                            }
                            if (hasValue)
                                result.Add(item);
                        }
                    }
                }
                return result;
            }


        }
    }
}
