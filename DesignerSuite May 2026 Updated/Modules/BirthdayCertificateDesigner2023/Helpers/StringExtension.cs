using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayCertificateDesigner2023.Helpers
{
    public static class StringExtension
    {
        public static double ConvertObjectToDouble(this object dataRow)
        {
            double convertedValue = default;
            if (dataRow == null)
            {
                convertedValue = dataRow.ToString().ConvertStringToDouble();
            }
            return convertedValue;
        }
        public static double ConvertStringToDouble(this string value)
        {
            double convertedValue = default;
            if (!string.IsNullOrWhiteSpace(value))
            {
                value = value.Replace("mm", string.Empty)
                    .Replace("pt", string.Empty)
                    .Replace(" ", string.Empty);

                double.TryParse(value, out convertedValue);
            }
            return convertedValue;
        }
    }
}
