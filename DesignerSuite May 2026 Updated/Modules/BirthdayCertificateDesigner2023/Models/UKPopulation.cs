using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayCertificateDesigner2023.Models
{
    public class UKPopulation
    {
        public int Date { get; set; }
        public int Population { get; set; }
        public string PopulationFormatted { 
            get {
                string formattedNumber;
                if (Population >= 1000000)
                {
                    double millions = Population / 1000000.0;
                    formattedNumber = millions.ToString("0.0") + " million";
                }
                else
                {
                    formattedNumber = Population.ToString();
                }
                return $"UK Population {formattedNumber}";
            } }
    }
}

