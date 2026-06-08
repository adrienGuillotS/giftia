using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayCertificateDesigner2023.Helpers
{
    public class NumerologyCalculator
    {
        public static int CalculateLifePathNumber(string dataOfBirth)
        {
            if (DateTime.TryParse(dataOfBirth, out DateTime birthDate))
            {
                // Calculate the life path number
                int lifePathNumber = CalculateLifePathNumber(birthDate);

                return lifePathNumber;
            }
            else
            {
                throw new Exception("Invalid date format. Please enter a valid date.");
            }
        }

        // Calculate the life path number
        public static int CalculateLifePathNumber(DateTime birthDate)
        {
            int day = birthDate.Day;
            int month = birthDate.Month;
            int year = birthDate.Year;

            int lifePathNumber = day + month + year;

            // Reduce to a single-digit number
            while (lifePathNumber > 9)
            {
                lifePathNumber = CalculateDigitSum(lifePathNumber);
            }

            return lifePathNumber;
        }

        // Helper method to calculate the sum of the digits in a number
        static int CalculateDigitSum(int number)
        {
            int sum = 0;

            while (number > 0)
            {
                sum += number % 10;
                number /= 10;
            }

            return sum;
        }
    }

}
