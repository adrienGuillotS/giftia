using BirthdayCertificateDesigner2023.Constants;
using BirthdayCertificateDesigner2023.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayCertificateDesigner2023.Helpers
{
    public static class ExcelDataExtractor
    {
        public static string[] GetWorksheetNames(string filePath, bool hdr)
        {
            string connectionString = Helper.GetOledbConnectionString(filePath, hdr);

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
            string connectionString = Helper.GetOledbConnectionString(filePath);


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

        public static Dictionary<DateTime, Tuple<string, string>> ReadSunriseSunsetExcelFile(string excelFile)
        {
            // Replace "YourExcelFile.xlsx" with the path to your Excel file
            string connectionString = Helper.GetOledbConnectionString(excelFile, false);

            Dictionary<DateTime, Tuple<string, string>> yearlyData = new Dictionary<DateTime, Tuple<string, string>>();

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();

                foreach (var sheetName in GetWorksheetNames(excelFile, false))
                {
                    string worksheetName = sheetName.Replace("$", string.Empty).Replace("'", string.Empty);
                    // Assuming values are in column I, rows 7 to 40
                    string query = $"SELECT * FROM [{worksheetName}$I10:I40]";

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Access each cell value using reader
                                string cellValue = reader[0].ToString();
                                if (!string.IsNullOrWhiteSpace(cellValue))
                                {
                                    int year = 0;
                                    try
                                    {
                                        year = int.Parse(worksheetName);
                                    }
                                    catch (Exception)
                                    {
                                        throw new Exception($"Sheetname '{worksheetName}' is invalid, it must be an year representing the sunrise, sunset data.");
                                    }
                                    var data = Helper.ParseSunriseSunsetData(cellValue, year);
                                    foreach (var sunriseSunshine in data)
                                    {
                                        yearlyData.Add(sunriseSunshine.Key, sunriseSunshine.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return yearlyData;
        }

      }
}