using BirthdayCertificateDesigner2023.Exceptions;
using System;
using System.Globalization;

namespace BirthdayCertificateDesigner2023.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime ParseDate(string inputDate)
        {
            inputDate = inputDate.Replace("st", "")
                .Replace("nd", "")
                .Replace("rd", "")
                .Replace("th", "")
                .Replace("Augu","August")
                .Replace("augu","August")
                .Trim();


          //  inputDate = inputDate.Replace(",", string.Empty);
            if (DateTime.TryParse(inputDate, out DateTime parsedDate))
            {
                return parsedDate;
            }

            throw new InvalidBirthDateException(inputDate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns>Date string with the day name also included.</returns>
        public static string FormatDate(DateTime date)
        {
            string dayWithSuffix =/* date.ToString("dd") +*/ GetDaySuffix(date.Day);
            return date.ToString($"dddd d- MMMM yyyy", CultureInfo.InvariantCulture)
                .Replace("-", dayWithSuffix);
        }

        public static string GetDaySuffix(int day)
        {
            if (day >= 11 && day <= 13)
                return "th";

            switch (day % 10)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }

        public static string FormatTime(string time, string sign = ":")
        {
            if (time.Length == 4)
            {
                // Insert a colon after the first two digits
                time = time.Insert(2, sign);
            }

            return time;
        }

        public static DateTime GetValidDate(int year, ref int month, int dayOfMonth)
        {
            DateTime dateTime;
            // Assuming the year is the current year; you might need to adjust this based on your data
            try
            {
                dateTime = new DateTime(year, ++month, dayOfMonth);
            }
            catch (Exception)
            {
                dateTime = new DateTime(year, ++month, dayOfMonth);
            }
            return dateTime;
        }
    }
}
