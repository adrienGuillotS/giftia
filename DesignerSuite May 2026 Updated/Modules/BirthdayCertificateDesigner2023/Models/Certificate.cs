using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayCertificateDesigner2023.Models
{
    public class Certificate
    {
        public string PersonName { get; set; }
        public string DateOfBirth { get; set; }
        public string City { get; set; }
        public int LuckyNumber { get; set; }
        public string Sunrise { get; set; }
        public string Sunset { get; set; }
        public string BirthStar { get; set; }
        public string BirthStarImagePath { get; set; }
        public string BirthStone { get; set; }
        public string BirthStoneImagePath { get; set; }

        public string ShareBirthdayWithCelebrity { get; set; }

        public string ChineseYear { get; set; }
        public string ChineseYearZodiacAnimalImagePath { get; set; }
        
        public string Personality { get; set; }
        
        public string Headline1 { get; set; }
        public string Headline2 { get; set; }
        public string Headline3 { get; set; }
        public string Headline4 { get; set; }

        public string LoafBreadPrice { get; set; }
        public string MilkPintPrice { get; set; }
        public string PetrolPerLitrePrice { get; set; }
        public string EggsPerDozenPrice { get; set; }

        public string PrimeMinister { get; set; }
        
        public string AverageCarCost { get; set; }
        public string AverageAnnualSalary { get; set; }
        public string AverageHouseCost { get; set; }

        public string UkPopulation { get; set; }

    }
}
