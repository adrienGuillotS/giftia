using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayCertificateDesigner2023.Exceptions
{
    [Serializable]
    public class InvalidBirthDateException: Exception
    {
        public InvalidBirthDateException() { }

        public InvalidBirthDateException(string date)
            : base($"Could not validate the '{date}' as valid date time object.")
        {

        }
    }
}
