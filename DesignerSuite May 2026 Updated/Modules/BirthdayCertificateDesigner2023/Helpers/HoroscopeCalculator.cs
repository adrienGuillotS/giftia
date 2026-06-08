using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayCertificateDesigner2023.Helpers
{
    public static class HoroscopeCalculator
    {
        public static string CalculateHoroscope(DateTime birthdate)
        {
            int month = birthdate.Month;
            int day = birthdate.Day;
            switch (month)
            {
                case 1:
                    if (day <= 19) // Capricorn 22 December - 19 January
                        return "Capricorn";
                    else
                        return "Aquarius";
                case 2:  // Aquarius 20 January - 18 February                                
                    if (day <= 18)
                        return "Aquarius";
                    else
                        return "Pisces";
                case 3:  // Pisces 19 February - 20 March                                
                    if (day <= 20)
                        return "Pisces";
                    else
                        return "Aries";
                case 4: // Aries 21 March - 19 April
                    if (day <= 19)
                        return "Aries";
                    else
                        return "Taurus";
                case 5: // Taurus 20 April - 20 May
                    if (day <= 20)
                        return "Taurus";
                    else
                        return "Gemini";
                case 6: // Gemini 21 May - 21 June
                    if (day <= 21)
                        return "Gemini";
                    else
                        return "Cancer";
                case 7: // Cancer 22 June - 22 July
                    if (day <= 22)
                        return "Cancer";
                    else
                        return "Leo";
                case 8:  // Leo 23 July - 22 August
                    if (day <= 22)
                        return "Leo";
                    else
                        return "Virgo";
                case 9:  // Virgo 23 August - 22 September
                    if (day <= 22)
                        return "Virgo";
                    else
                        return "Libra";
                case 10: // Libra 23 September - 23 October
                    if (day <= 23)
                        return "Libra";
                    else
                        return "Scorpio";
                case 11:   // Scorpio 24 October - 21 November
                    if (day <= 21)
                        return "Scorpio";
                    else
                        return "Sagittarius";
                case 12:  // Sagittarius 22 November - 21 December
                    if (day <= 21)
                        return "Sagittarius";
                    else
                        return "Capricorn";
            }
            return "";
        }
    }
}
