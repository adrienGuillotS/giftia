using BirthdayCertificateDesigner2023.Constants;
using BirthdayCertificateDesigner2023.Helpers;
using BirthdayCertificateDesigner2023.Models;
using DesignerSuite.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BirthdayCertificateDesigner2023
{
    public class CertificateManager
    {
        #region Fields

        private const string Certificate_Files_Directory = "Resources\\Birthday Certificate Designer 2023\\Certificate Files\\";
        private const string Certificate_Excel_File = "Data Sheets for Keepsake.xlsx";
        private const string SunriseSunsetTiming_Excel_File = "Sunrise and sunset timming.xlsx";

        private readonly string Birth_Stone_Images_Folder_Path = @"{0}\Resources\Birthday Certificate Designer 2023\Birth Stone\";
        private readonly string Horoscopes_Images_Folder_Path = @"{0}\Resources\Birthday Certificate Designer 2023\Horoscopes\";
        private readonly string Chinese_Year_Images_Folder_Path = @"{0}\Resources\Birthday Certificate Designer 2023\Chinese Year images\";
        private readonly string _certificateTemplateFilePath;
        private readonly string _sunriseSunsetTimingFilePath;


        private bool _isExcelDataLoaded = false;

        private List<MetaData> _metaDataList = null;
        //private List<StringConcatination> _stringConcatinations = null;
        private List<CelebrityBirthday> _celebrityBirthdayList = null;
        private List<ChineseYear> _chineseYearList = null;
        //private List<Horoscope> _horoscopesList = null;
        private List<NewsOfYear> _newsOfYearList = null;
        private List<PrimeMinsiter> _primeMinsiterList = null;

        private List<Stone> _stonesList = null;
        private List<Egg> _eggsPriceList = null;
        private List<House> _averageHouseCostList = null;
        private List<LoafBread> _loafBreadPriceList = null;
        private List<MilkPint> _milkPintPriceList = null;
        private List<Petrol> _petrolPriceList = null;
        private List<Car> _averageCarPricesList = null;
        private List<SalarySheet> _averageAnnualSalaryList = null;
        private List<UKPopulation> _uKPopulationList = null;


        // Sunrise Sunset data
        Dictionary<DateTime, Tuple<string, string>> _yearlySunriseSunsetData = null;
        #endregion

        #region Properties


        #endregion
        public CertificateManager()
        {
            string currentDirectoryPath = CoreHelper.GetAppAssemblyPath();

            _certificateTemplateFilePath = Path.Combine(currentDirectoryPath, Certificate_Files_Directory, Certificate_Excel_File);
            _sunriseSunsetTimingFilePath = Path.Combine(currentDirectoryPath, Certificate_Files_Directory, SunriseSunsetTiming_Excel_File);

            Birth_Stone_Images_Folder_Path = string.Format(Birth_Stone_Images_Folder_Path, currentDirectoryPath);
            Horoscopes_Images_Folder_Path = string.Format(Horoscopes_Images_Folder_Path, currentDirectoryPath);
            Chinese_Year_Images_Folder_Path = string.Format(Chinese_Year_Images_Folder_Path, currentDirectoryPath);

            _isExcelDataLoaded = false;
        }

        #region Public Methods

        public Certificate GetCertificateDetails(CertificateInfo info)
        {
            DateTime birthDate = DateTimeHelper.ParseDate(info.DateOfBirth);
            string birthDateYear = birthDate.ToString("yyyy");

            var timing = ProcessSunsetSunriseData(birthDate);

            Certificate certificate = new Certificate
            {
                // Personal details
                PersonName = info.PersonName,
                DateOfBirth = $"Born on {ProcessDateOfBirth(info.DateOfBirth)}".ToUpperInvariant(),
                City = info.City,

                // Lucky Number
                LuckyNumber = NumerologyCalculator.CalculateLifePathNumber(birthDate),
                Sunrise = DateTimeHelper.FormatTime(timing.Item1),
                Sunset = DateTimeHelper.FormatTime(timing.Item2),

                // Horoscope
                BirthStar = HoroscopeCalculator.CalculateHoroscope(birthDate)
            };
            certificate.BirthStarImagePath = Helper.FindFileByName(Horoscopes_Images_Folder_Path, certificate.BirthStar);

            // Birth stone
            certificate.BirthStone = _stonesList.FirstOrDefault(a => a.Month == birthDate.Month)?.Stones;
            certificate.BirthStoneImagePath = Helper.FindFileByName(Birth_Stone_Images_Folder_Path, certificate.BirthStone);

            // Celebrity birthday
            certificate.ShareBirthdayWithCelebrity = $"You share your birthday with {FindCelebritybyBirthdate(birthDate)}";

            // chinese year
            var chineseYear = _chineseYearList.FirstOrDefault(o => o.Years.Contains(birthDateYear));
            if (chineseYear != null)
            {
                //certificate.ChineseYear = $"Chinese year of the {chineseYear?.ZodiacAnimal}".ToUpperInvariant();
                certificate.ChineseYear = chineseYear?.ZodiacAnimal.ToUpperInvariant();
                certificate.ChineseYearZodiacAnimalImagePath = Helper.FindFileByName(Chinese_Year_Images_Folder_Path, chineseYear?.ZodiacAnimal);
                certificate.Personality = chineseYear.ToString();
            }
            else
            {
                throw new Exception($"Cannot read the chinese year data for the year {birthDateYear}.");
            }
            // News of year
            var topHeadlines = GetTopHeadlinesOrdered(birthDateYear);

            if (topHeadlines.Count == 4)
            {
                certificate.Headline1 = topHeadlines[0];
                certificate.Headline2 = topHeadlines[1];
                certificate.Headline3 = topHeadlines[2];
                certificate.Headline4 = topHeadlines[3];
            }
            else
            {
                throw new Exception($"Cannot read the top four headlines for the year {birthDateYear}.");
            }

            // Prices
            certificate.LoafBreadPrice = $"{_loafBreadPriceList.FirstOrDefault(o => o.Year.Equals(birthDate.Year))?.Price} pence";
            certificate.MilkPintPrice = $"{_milkPintPriceList.FirstOrDefault(o => o.Year.Equals(birthDate.Year))?.Price} pence";
            certificate.PetrolPerLitrePrice = $"{_petrolPriceList.FirstOrDefault(o => o.Year.Equals(birthDate.Year))?.PetrolPrice} pence";
            certificate.EggsPerDozenPrice = $"{_eggsPriceList.FirstOrDefault(o => o.Year.Equals(birthDate.Year))?.Price} pence";

            // Prime Minister
            var primeMinister = _primeMinsiterList
                .FirstOrDefault(dateRange => birthDate >= dateRange.DateJoin
                && birthDate <= dateRange.DateLeave);

            if (primeMinister != null)
            {
                certificate.PrimeMinister = $"{primeMinister.PrimeMinister} ({primeMinister.Party}) was Prime Minister.";
            }
            else
            {
                throw new Exception($"Cannot read the Prime Minister data for the year {birthDateYear}.");
            }
            string thousandSeparator = "{0:0,0}";
            // costs that year
            certificate.AverageCarCost = $"£{string.Format(thousandSeparator, _averageCarPricesList.FirstOrDefault(o => o.Year.Equals(birthDate.Year)).Price)}";
            certificate.AverageAnnualSalary = $"£{string.Format(thousandSeparator, _averageAnnualSalaryList.FirstOrDefault(o => o.Year.Equals(birthDate.Year)).Salary)}";
            certificate.AverageHouseCost = $"£{string.Format(thousandSeparator, _averageHouseCostList.FirstOrDefault(o => o.Year.Equals(birthDate.Year)).Price)}";

            // UK population
            certificate.UkPopulation = $"{_uKPopulationList.FirstOrDefault(o => o.Date.Equals(birthDate.Year)).PopulationFormatted}";


            return certificate;
        }

        private Tuple<string, string> ProcessSunsetSunriseData(DateTime dateToLookup)
        {
            if (_yearlySunriseSunsetData.TryGetValue(dateToLookup, out var times))
            {
                return times;
            }
            else
            {
                throw new Exception($"No sunrise and sunset data available for {dateToLookup.ToShortDateString()}");
            }
        }
        public void LoadCertificateData()
        {
            _metaDataList = ExcelDataExtractor.ReadExcelData<MetaData>(_certificateTemplateFilePath, WorksheetNames.MetadataWorksheetName);
            // _stringConcatinations = ExcelDataExtractor.ReadExcelData<StringConcatination>(_certificateTemplateFilePath, WorksheetNames.StringConcatinationWorksheetName);

            _celebrityBirthdayList = ExcelDataExtractor.ReadExcelData<CelebrityBirthday>(_certificateTemplateFilePath, WorksheetNames.CelebrityBirthdayWorksheetName);
            _chineseYearList = ExcelDataExtractor.ReadExcelData<ChineseYear>(_certificateTemplateFilePath, WorksheetNames.ChineseYearWorksheetName);
            //  _horoscopesList = ExcelDataExtractor.ReadExcelData<Horoscope>(_certificateTemplateFilePath, WorksheetNames.HoroscopeWorksheetName);
            _newsOfYearList = ExcelDataExtractor.ReadExcelData<NewsOfYear>(_certificateTemplateFilePath, WorksheetNames.NewsOfYearWorksheetName);
            _primeMinsiterList = ExcelDataExtractor.ReadExcelData<PrimeMinsiter>(_certificateTemplateFilePath, WorksheetNames.PrimeMinsiterWorksheetName);
            _stonesList = ExcelDataExtractor.ReadExcelData<Stone>(_certificateTemplateFilePath, WorksheetNames.StonesWorksheetName);

            _eggsPriceList = ExcelDataExtractor.ReadExcelData<Egg>(_certificateTemplateFilePath, WorksheetNames.EggsWorksheetName);
            _averageHouseCostList = ExcelDataExtractor.ReadExcelData<House>(_certificateTemplateFilePath, WorksheetNames.HouseWorkseetName);
            _loafBreadPriceList = ExcelDataExtractor.ReadExcelData<LoafBread>(_certificateTemplateFilePath, WorksheetNames.LoafBreadWorksheetName);
            _milkPintPriceList = ExcelDataExtractor.ReadExcelData<MilkPint>(_certificateTemplateFilePath, WorksheetNames.MilkPintWorksheetName);
            _petrolPriceList = ExcelDataExtractor.ReadExcelData<Petrol>(_certificateTemplateFilePath, WorksheetNames.PetrolWorksheetName);
            _averageCarPricesList = ExcelDataExtractor.ReadExcelData<Car>(_certificateTemplateFilePath, WorksheetNames.CarWorksheetName);

            _averageAnnualSalaryList = ExcelDataExtractor.ReadExcelData<SalarySheet>(_certificateTemplateFilePath, WorksheetNames.SalaryWorksheetName);
            _uKPopulationList = ExcelDataExtractor.ReadExcelData<UKPopulation>(_certificateTemplateFilePath, WorksheetNames.UK_PopulationWorksheetName);

            _isExcelDataLoaded = true;
        }

        public void LoadSunriseSunsetTimingData()
        {
            if (!File.Exists(_sunriseSunsetTimingFilePath))
            {
                throw new Exception($"{_sunriseSunsetTimingFilePath} not found");
            }
            _yearlySunriseSunsetData = ExcelDataExtractor.ReadSunriseSunsetExcelFile(_sunriseSunsetTimingFilePath);
        }

        public List<MetaData> GetMetaDataList()
        {
            return _metaDataList;
        }

        public bool IsExcelDataLoaded()
        {
            return _isExcelDataLoaded;
        }

        #endregion

        #region Private Methods

        private string FindCelebritybyBirthdate(DateTime birthDate)
        {
            CelebrityBirthday celebritiesBirthdayOnThisDay = _celebrityBirthdayList
                .FirstOrDefault(o => o.Day == birthDate.Day) ??
                throw new Exception("No matching birthday found.");

            string month = birthDate.ToString("MMMM");

            var celebrityName = Helper.GetVariableByName(celebritiesBirthdayOnThisDay, month);

            return celebrityName is null ? string.Empty : celebrityName.ToString();
        }

        private List<string> GetTopHeadlinesOrdered(string year)
        {
            var newsOfYear = _newsOfYearList.FirstOrDefault(o => o.Year.ToString().Equals(year, StringComparison.OrdinalIgnoreCase));

            var headlines = new string[] { newsOfYear.Headline1, newsOfYear.Headline2, newsOfYear.Headline3, newsOfYear.Headline4 };

            return headlines
                 .OrderByDescending(headline => headline.Length)
                 .Take(4)
                 .ToList();
        }


        private string ProcessDateOfBirth(string dateOfBirth)
        {
            return DateTimeHelper.FormatDate(DateTimeHelper.ParseDate(dateOfBirth));
        }
        #endregion
    }
}
