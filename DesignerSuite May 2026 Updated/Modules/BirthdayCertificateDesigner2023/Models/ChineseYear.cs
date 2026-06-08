using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayCertificateDesigner2023.Models
{
    public class ChineseYear
    {
        public string ZodiacAnimal { get; set; }
        public string Years { get; set; }
        public string Personality { get; set; }

        public override string ToString()
        {
            return $"You are {Personality}";
        }
    }
}
