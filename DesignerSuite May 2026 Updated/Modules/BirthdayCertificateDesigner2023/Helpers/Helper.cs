using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BirthdayCertificateDesigner2023.Helpers
{
    public static class Helper
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

        public static string SanitizeFileName(string fileName)
        {
            // Define a regular expression pattern to match invalid characters
            string invalidCharsPattern = string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars())));

            // Replace invalid characters with an underscore
            string sanitizedFileName = Regex.Replace(fileName, invalidCharsPattern, "_");

            return sanitizedFileName;
        }

        public static object GetVariableByName(object obj, string variableName)
        {
            var propertieInfos = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

            var property = propertieInfos.FirstOrDefault(o => o.Name.Equals(variableName, StringComparison.OrdinalIgnoreCase));

            if (property != null)
            {
                return property.GetValue(obj);
            }
            return null;
        }

        public static string FindFileByName(string folderPath, string fileNameWithoutExtension)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    string[] files = Directory.GetFiles(folderPath);

                    foreach (string filePath in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(filePath);

                        if (fileName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            return filePath;
                        }
                    }

                    return null; // File not found
                }
                else
                {
                    throw new DirectoryNotFoundException("Directory not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                return null;
            }
        }

        public static Dictionary<DateTime, Tuple<string, string>> ParseSunriseSunsetData(string data, int year)
        {
            Dictionary<DateTime, Tuple<string, string>> result = new Dictionary<DateTime, Tuple<string, string>>();
            string[] tokens = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int month = 0;
            int dayOfMonth = 1;
            if (tokens.Length > 1)
            {
                dayOfMonth = int.Parse(tokens[0]);
            }

            for (int i = 0; i < tokens.Length; i += 2)
            {
                try
                {
                    int sunriseIndex = i + 1;
                    int sunsetIndex = i + 2;

                    if (sunriseIndex >= tokens.Length || sunsetIndex >= tokens.Length)
                        continue;
                   
                    string sunrise = tokens[sunriseIndex];
                    string sunset = tokens[sunsetIndex];

                    DateTime date = DateTimeHelper.GetValidDate(year, ref month, dayOfMonth);

                    result[date] = Tuple.Create(sunrise, sunset);
                }
                catch (Exception) { }
            }

            return result;
        }



    }
}
