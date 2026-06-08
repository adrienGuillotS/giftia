using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayCertificateDesigner2023.Models
{
    public class PrimeMinsiter
    {
        public DateTime DateJoin { get; set; }
        public DateTime DateLeave { get; set; }
        public string PrimeMinister { get; set; }
        public string Party { get; set; }

        public override string ToString()
        {
            if (PrimeMinister != null && Party != null)
            {
                return $"{PrimeMinister} ({Party}) was Prime Minister";
            }
            return string.Empty;
        }
    }
}
