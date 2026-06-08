using BirthdayCertificateDesigner2023.Exceptions;
using BirthdayCertificateDesigner2023.Helpers;
using BirthdayCertificateDesigner2023.Models;
using BirthdayCertificateDesigner2023.Module.Helpers;
using DesignerSuite.Core.Enums;
using DesignerSuite.Core.Static;
using DesignerSuite.Core.Utilities;
using DesignerSuite.Core.Utilities.Corel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using Corel.Interop.VGCore;
using WindowsAPICodePack.Dialogs;
using System.Windows.Documents;

namespace BirthdayCertificateDesigner2023.Views
{
    public partial class MainPage : System.Windows.Controls.Page
    {
        #region Private Fields

        private Corel.Interop.VGCore.Application? corelApp;


        private string extractPath = "";
        private string processedZipFilesPath = "";

        private string cdrFolderPath = "";

        private readonly CertificateManager _certificateManager;

        private const string CorelDraw_2022_RootFolderName = "CorelDRAW Graphics Suite 2022";

        #endregion

        #region Constructor

        public MainPage()
        {
            InitializeComponent();
            _certificateManager = new CertificateManager();
        }
        #endregion

        #region Event Handlers

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            ZipsFolderPathTxt.Text = @"D:\Testing Workspace\Umar\DesignerSuite\Birthday Certificate Designer files\‏Milestone 3\wetransfer_processed_2024-03-18_1315\processed";
            CDRFilesLocationTxt.Text = @"D:\Testing Workspace\Umar\DesignerSuite\Birthday Certificate Designer files\‏Milestone 3\wetransfer_processed_2024-03-18_1315\processed";
#endif
            if (!string.IsNullOrEmpty(AppDefaultDirectories.BirthdayCertificateDesigner))
            {
                ZipsFolderPathTxt.Text = AppDefaultDirectories.BirthdayCertificateDesigner;
                ManuallyDesignedCDRFileLocationTxt.Text = CDRFilesLocationTxt.Text = AppDefaultDirectories.BirthdayCertificateDesigner;
            }
        }

        private void ImagesBrowse_Click(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog dialog = new()
            {
                IsFolderPicker = true,
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Select Source Zip Files Folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok &&
                !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                string selectedFolderPath = dialog.FileName;

                // Assuming ZipFolderPathCB is a TextBox or ComboBox in your XAML
                ZipsFolderPathTxt.Text = selectedFolderPath;
            }
        }

        private void CDRBrowse_Click(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog dialog = new()
            {
                IsFolderPicker = true,
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Select CDR Destination Files Folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok &&
                !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                string selectedFolderPath = dialog.FileName;

                // Assuming ZipFolderPathCB is a TextBox or ComboBox in your XAML
                CDRFilesLocationTxt.Text = selectedFolderPath;
            }
        }


        private async void GenerateAmazonBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CDRFilesLocationTxt.Text == "")
            {
                MessageBox.Show("Please select CDR files location", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (ZipsFolderPathTxt.Text == "")
            {
                MessageBox.Show("Please select zip files location", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {

                DisableEveryThing();
                var progress = new Progress<object>(v =>
                {
                    if (v is int value)
                    {
                        Progress_Bar.Value = value;
                    }
                    else
                    {
                        ErrorConsoleRichTxt.AppendText($"{v}{Environment.NewLine}");
                    }
                });
                processedZipFilesPath = ZipsFolderPathTxt.Text + @"\processed\";
                extractPath = CDRFilesLocationTxt.Text + @"\temp\";
                string zipFolderPath = ZipsFolderPathTxt.Text;
                string cdrFilesText = CDRFilesLocationTxt.Text;
                cdrFolderPath = cdrFilesText;

                if (GetZipFiles(zipFolderPath)?.Length > 0)
                {
                    await Task.Run(() =>
                    {
                        corelApp = GetCorelApplicationOrThrow();
                        Generate(progress, zipFolderPath, cdrFilesText);
                    });
                    MessageBox.Show("Files generated successfully!", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"No zip File found inside '{zipFolderPath}'", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                EnableEveryThing();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                EnableEveryThing();
            }
        }

        private static Corel.Interop.VGCore.Application GetCorelApplicationOrThrow()
        {
            var connector = new CorelConnector();

            if (!connector.ConnectToCorel() || connector.CorelApp is null)
            {
                throw new InvalidOperationException(
                    "CorelDRAW is not started or not compatible with this software.");
            }

            return connector.CorelApp;
        }

        public void StartTask(BgTask bgTask)
        {
            if (bgTask == null)
                return;
            ICUITaskManager taskManager = corelApp.FrameWork.TaskManager;
            try
            {
                taskManager.RunInBackground(cuiTaskPriority.kSoon, bgTask);
            }
            catch (Exception erro)
            {
                corelApp.FrameWork.ShowMessageBox(erro.Message);
            }
        }

        #endregion

        #region Methods

        private void Generate(IProgress<object> progress, string zipFolderPath, string destinationFolderPath, WorkingModeEnum workingMode = WorkingModeEnum.Amazon)
        {
            ArgumentNullException.ThrowIfNull(corelApp);

            string currentDirectoryPath = CoreHelper.GetAppAssemblyPath();
            string corelVersion = currentDirectoryPath.Contains(CorelDraw_2022_RootFolderName) ?
               "2022" : "2019";
#if DEBUG
            corelVersion = "2019";
#endif
            Directory.CreateDirectory(cdrFolderPath);
            Directory.CreateDirectory(extractPath);
            CoreHelper.EmptyTempDirectory(extractPath);

            string templateFilePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentDirectoryPath,
                "Resources\\Birthday Certificate Designer 2023\\Certificate Files\\", corelVersion, "Birth Certificate.cdr"));

            if (!_certificateManager.IsExcelDataLoaded())
            {
                _certificateManager.LoadCertificateData();
                _certificateManager.LoadSunriseSunsetTimingData();
            }
            Dictionary<string, Order> ordersDictionary = new Dictionary<string, Order>();

            string[] zipFiles = GetZipFiles(zipFolderPath);
            foreach (string zipFilePath in zipFiles)
            {
                Order order = workingMode == WorkingModeEnum.Amazon ? GetCustomerDataFromXmlInsideZip(zipFilePath) :
                    TemuOrderZipFileHelper.GetCustomerDataFromJsonInsideZip(zipFilePath, extractPath);
                if (order != null)
                {
                    ordersDictionary.Add(zipFilePath, order);
                }
            }
            ordersDictionary = ModifyDuplicateOrderIds(ordersDictionary);

            Document document = null;
            List<string> processedZipFiles = new List<string>();

            List<MetaData> documentMetaData = _certificateManager.GetMetaDataList();
            string fileName = string.Empty;
            int zipCount = 1;

            string tempTemplateFilePath = System.IO.Path.Combine(cdrFolderPath, $"Birth Certificate Temp Template.cdr");

            if (ordersDictionary.Count > 0)
            {
                document = NewDocument(corelApp, templateFilePath, tempTemplateFilePath);
                document.Activate();
            }

            foreach (var item in ordersDictionary)
            {
                string zipFilePath = item.Key;
                var order = item.Value;
                try
                {
                    foreach (var info in order.CertificateInfos)
                    {
                        Certificate certificate = GetCertificateDetails(info, zipFilePath, workingMode);

                        if (certificate != null)
                        {
                            FillDocument(document, certificate, documentMetaData);

                            fileName = destinationFolderPath + "\\" + order.OrderId + ".cdr";

                            document?.SaveAs(fileName);
                            corelApp?.Refresh();
                            processedZipFiles.Add(zipFilePath);
                        }
                    }
                    zipCount++;
                    ReportProgress(progress, ((100 - 10) / zipFiles.Length) * zipCount);
                }

                catch (Exception ex)
                {
                    AppendErrorConsoleRichTxt($"Zip file '{zipFilePath}' processing failed. {ex.Message}{Environment.NewLine}");
                }
            }


            if (!string.IsNullOrWhiteSpace(fileName) && document != null)
            {
                document?.Close();
                try
                {
                    if (File.Exists(tempTemplateFilePath)) { File.Delete(tempTemplateFilePath); }
                }
                catch { }
            }
            MoveZipFiles(processedZipFiles);
            ReportProgress(progress, 100);
        }
        private Certificate GetCertificateDetails(CertificateInfo info, string zipFilePath, WorkingModeEnum workingMode = WorkingModeEnum.Amazon)
        {
            var date = ValidateDateOfBirthDateFormat(info, zipFilePath, workingMode);
            if (date == null)
            {
                return null;
            }
            info.DateOfBirth = date?.ToShortDateString();
            return _certificateManager.GetCertificateDetails(info) ?? throw new Exception("Certificate data cannot be null");
        }
        private DateTime? ValidateDateOfBirthDateFormat(CertificateInfo info, string zipFile, WorkingModeEnum workingMode)
        {
            DateTime? date = null;
            try
            {
                date = DateTimeHelper.ParseDate(info.DateOfBirth);
            }
            catch (InvalidBirthDateException)
            {
                RunOnUIThread_Invoke(() =>
                {
                    BirthDateErrorDialog dialog = new BirthDateErrorDialog($"'{info.DateOfBirth}' is invalid.");
                    if (dialog.ShowDialog() == true)
                    {
                        switch (dialog.Result)
                        {
                            case DateDialogResult.Ignore:
                                date = null;
                                break;
                            case DateDialogResult.Continue:
                                date = dialog.DateOfBirth;
                                break;
                            case DateDialogResult.SaveAndContinue:

                                if (!File.Exists(info.FileName))
                                {
                                    throw new FileNotFoundException($"{info.FileName} could not be found.");
                                }

                                date = dialog.DateOfBirth;

                                string fileName = System.IO.Path.GetFileName(info.FileName);
                                string tempZipFile = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(info.FileName), Path.GetFileName(zipFile)));
                                // Save the xml file in original zip file
                                if (workingMode == WorkingModeEnum.Amazon)
                                {
                                    // Load the xml file and update the date
                                    XmlDocument xmlDoc = new XmlDocument();
                                    xmlDoc.Load(info.FileName);

                                    XmlFileHepler.UpdateInputValue(xmlDoc, date?.ToShortDateString());
                                    ZipFileHelper.ReplaceSourceFileInsideZip(zipFile, fileName, xmlDoc.OuterXml, tempZipFile);
                                }
                                else if (workingMode == WorkingModeEnum.Temu)
                                {
                                    string jsonString = File.ReadAllText(info.FileName, System.Text.Encoding.UTF8);

                                    string updateJsonString = XmlFileHepler.UpdateBirthdayInJsonFile(jsonString, date?.ToShortDateString());
                                    ZipFileHelper.ReplaceSourceFileInsideZip(zipFile, fileName, updateJsonString, tempZipFile);
                                }

                                break;
                            default:
                                throw new ArgumentException($"'{dialog.Result}' case is not implemented");
                        }
                    }
                });
            }
            return date;
        }
        private static Dictionary<string, Order> ModifyDuplicateOrderIds(Dictionary<string, Order> ordersDictionary)
        {
            Dictionary<string, int> orderCounters = new Dictionary<string, int>();

            foreach (var kvp in ordersDictionary)
            {
                string orderId = kvp.Value.OrderId ?? throw new Exception($"Order Id cannot be empty or null. Check if the zip file '{kvp.Key}' is valid or not.");
                // Check if orderId is already in orderCounters
                if (orderCounters.ContainsKey(orderId))
                {
                    int counter = orderCounters[orderId];
                    counter++;
                    orderCounters[orderId] = counter;
                    kvp.Value.OrderId = $"{orderId} {counter:D2}";
                }
                else
                {
                    orderCounters.Add(orderId, 1);
                }
            }

            return ordersDictionary;
        }
        private void FillDocument(Document document, Certificate certificate, List<MetaData> documentMetaData)
        {
            if (document != null)
            {
                // Get the active page
                Page activePage = document.ActivePage;

                if (activePage != null)
                {
                    // Get all objects on the active page
                    Shapes allObjects = activePage.Shapes;

                    // Iterate through each object

                    foreach (Shape shape in allObjects)
                    {
                        try
                        {
                            var metaData = documentMetaData.FirstOrDefault(o => o.Object_Name.Equals(shape.Name));
                            if (metaData != null)
                            {
                                string value = "";
                                bool applyStyles = false, applyOutlineWidth = false;
                                switch (metaData.Object_Name)
                                {
                                    case "text_customer_name":
                                        value = certificate.PersonName;
                                        applyStyles = true;
                                        break;
                                    case "text_born_on":
                                        value = certificate.DateOfBirth;
                                        applyStyles = true;
                                        break;
                                    case "text_location":
                                        value = certificate.City;
                                        applyStyles = true;
                                        break;
                                    case "text_population":
                                        value = certificate.UkPopulation;
                                        break;
                                    case "text_chinese_year":
                                        applyStyles = true;
                                        value = certificate.ChineseYear;
                                        break;
                                    case "text_prime_minster_name":
                                        value = certificate.PrimeMinister;
                                        break;
                                    case "text_horoscope":
                                        //value = certificate.BirthStar;
                                        SetShapeVisibility(certificate, metaData, certificate.BirthStar);
                                        break;
                                    case "text_stone":
                                        SetShapeVisibility(certificate, metaData, certificate.BirthStone);
                                        break;
                                    case "image_horoscope":
                                        value = certificate.BirthStarImagePath;
                                        break;
                                    case "image_stone":
                                        value = certificate.BirthStoneImagePath;
                                        break;
                                    case "text_loaf_of_bread":
                                        applyOutlineWidth = applyStyles = true;
                                        value = certificate.LoafBreadPrice;
                                        break;
                                    case "text_pint_of_milk":
                                        applyOutlineWidth = applyStyles = true;
                                        value = certificate.MilkPintPrice;
                                        break;
                                    case "text_petrol_per_liter":
                                        applyOutlineWidth = applyStyles = true;
                                        value = certificate.PetrolPerLitrePrice;
                                        break;
                                    case "text_dozen_eggs":
                                        applyOutlineWidth = applyStyles = true;
                                        value = certificate.EggsPerDozenPrice;
                                        break;
                                    case "text_house_price":
                                        value = certificate.AverageHouseCost;
                                        break;
                                    case "text_salary":
                                        value = certificate.AverageAnnualSalary;
                                        break;
                                    case "text_car_cost":
                                        value = certificate.AverageCarCost;
                                        break;
                                    case "text_headline_1":
                                        value = certificate.Headline1;
                                        break;
                                    case "text_headline_2":
                                        value = certificate.Headline2;
                                        break;
                                    case "text_headline_3":
                                        value = certificate.Headline3;
                                        break;
                                    case "text_headline_4":
                                        value = certificate.Headline4;
                                        break;
                                    case "text_birthday":
                                        applyStyles = true;
                                        value = certificate.ShareBirthdayWithCelebrity;
                                        break;
                                    case "text_lucky_number":
                                        value = certificate.LuckyNumber.ToString();
                                        break;
                                    case "text_personality":
                                        value = certificate.Personality;
                                        applyStyles = true;
                                        break;
                                    case "image_chinese_year":
                                        value = certificate.ChineseYearZodiacAnimalImagePath;
                                        break;
                                    case "text_sunrise":
                                        value = certificate.Sunrise;
                                        break;
                                    case "text_sunset":
                                        value = certificate.Sunset;
                                        break;
                                    case "chinese_year_group":
                                        //metaData.Object_Name is "chinese_year_group"
                                        SetShapeVisibility(certificate, metaData, certificate.ChineseYear);
                                        break;
                                    default:
                                        throw new Exception($"'{metaData.Object_Name}' case not yet implemented");
                                }

                                if (shape.Type == cdrShapeType.cdrTextShape && !string.IsNullOrWhiteSpace(value))
                                {
                                    shape.Text.Story.Text = value;

                                    if (metaData.Max_Height > 0)
                                    {
                                        shape.SizeHeight = Math.Min(shape.SizeHeight, metaData.Max_Height);
                                    }
                                    if (metaData.Max_Width > 0)
                                    {
                                        shape.SizeWidth = Math.Min(shape.SizeWidth, metaData.Max_Width);
                                    }
                                    if (metaData.Font_Size != 0)
                                    {
                                        shape.Text.FontProperties.Size = metaData.Font_Size;
                                        shape.Text.FontProperties.Name = metaData.Font;
                                        // shape.Text.Story.Size = metaData.Font_Size;
                                    }

                                    if (applyStyles)
                                    {
                                        shape.SetPositionEx(cdrReferencePoint.cdrCenter, metaData.X, metaData.Y);
                                    }
                                    if (applyOutlineWidth)
                                    {
                                        shape.Outline.Width = 0;
                                    }
                                }
                                else if (shape.Type == cdrShapeType.cdrBitmapShape)
                                {
                                    InsertBitmapShape(activePage, shape, metaData, value);
                                }
                            }
                        }
                        catch (Exception)
                        {

                            throw;
                        }


                    }
                    //   document.Save();
                }
                else
                {
                    Console.WriteLine("No active page.");
                }
            }
            else
            {
                Console.WriteLine("No active document.");
            }
        }

        private void SetShapeVisibility(Certificate certificate, MetaData metaData, string shapeName)
        {
            var groupShape = corelApp?.ActiveDocument.ActiveLayer.Shapes.FindShape(metaData.Object_Name);
            if (groupShape != null)
            {
                foreach (Shape shapeGroup in groupShape.Shapes)
                {
                    shapeGroup.Visible = shapeGroup.Name.Equals(shapeName, StringComparison.OrdinalIgnoreCase);
                }
            }
            //return groupShape;
        }

        private static void InsertBitmapShape(Page activePage, Shape shape, MetaData metaData, string value)
        {
            if (!File.Exists(value)) { throw new Exception($"{value} not found."); }

            ImportFilter importFilter = activePage.ActiveLayer.ImportEx(value);
            importFilter.Finish();

            var shape1 = activePage.Shapes.FindShape(System.IO.Path.GetFileName(value));

            if (metaData.Max_Height != 0)
            {
                shape1.SizeHeight = Math.Min(shape1.Bitmap.SizeHeight, metaData.Max_Height);
            }
            if (metaData.Max_Width > 0)
            {
                shape1.SizeWidth = Math.Min(shape1.Bitmap.SizeWidth, metaData.Max_Width);
            }

            shape1.SetPositionEx(cdrReferencePoint.cdrCenter, metaData.X, metaData.Y);
            shape?.Delete();
            shape1.Name = metaData.Object_Name;
        }
        /// <summary>
        /// Expects single order xml file per zip
        /// </summary>
        /// <param name="zipFilePath"></param>
        /// <returns></returns>
        private Order GetCustomerDataFromXmlInsideZip(string zipFilePath)
        {
            extractPath = Path.GetFullPath(extractPath);

            Order order = new Order();

            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        string destinationPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(extractPath, entry.FullName));
                        if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                        {
                            entry.ExtractToFile(destinationPath, true);
                            string xmlFileData = File.ReadAllText(destinationPath).Replace("\n", "~`~").Replace("\r", "~`~");
                            Regex orderIdRegex = new Regex(@"<orderId>(((?!<\/orderId>).)*)<\/orderId>|<orderId\/>");
                            foreach (Match match in orderIdRegex.Matches(xmlFileData))
                            {
                                // currenlty it only handles 1 order
                                order.OrderId = match.Groups[1].Value.Replace("~`~", Environment.NewLine);
                                break;
                            }

                            order.CertificateInfos = new List<CertificateInfo>();

                            Regex inputRegex = new Regex(@"<inputValue>(((?!<\/inputValue>).)*)<\/inputValue>|<inputValue\/>");


                            CertificateInfo certificateInfo = new CertificateInfo
                            {
                                FileName = destinationPath
                            };

                            var matches = inputRegex.Matches(xmlFileData);
                            for (int i = 0; i < matches.Count; i++)
                            {
                                if (i == 0)
                                {
                                    certificateInfo.PersonName = matches[i].Groups[1].Value.Replace("~`~", Environment.NewLine);
                                }
                                if (i == 1)
                                {
                                    certificateInfo.DateOfBirth = matches[i].Groups[1].Value.Replace("~`~", Environment.NewLine).ToString();
                                }
                                if (i == 2)
                                {
                                    certificateInfo.City = matches[i].Groups[1].Value.Replace("~`~", Environment.NewLine);
                                    break;
                                }
                            }
                            order.CertificateInfos.Add(certificateInfo);
                        }
                    }
                }
            }
            return order;
        }
        private string[] GetZipFiles(string directoryPath)
        {
            Directory.CreateDirectory(processedZipFilesPath);
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            return directory.GetFiles("*.zip").OrderBy(p => p.CreationTime).Select(f => f.FullName).ToArray();
        }

        private Document NewDocument(Corel.Interop.VGCore.Application application, string templateFilePath, string newFilePath)
        {
            if (!File.Exists(templateFilePath))
            {
                throw new Exception($"Could not find the Template CDR file inside {Path.GetDirectoryName(templateFilePath)}.");
            }
            File.Copy(templateFilePath, newFilePath, true);
            Document document = application.OpenDocument(newFilePath);
            document.Activate();
            document.Unit = cdrUnit.cdrMillimeter;
            document.Rulers.VUnits = document.Rulers.HUnits = cdrUnit.cdrMillimeter;
            return document;
        }
        private void MoveZipFiles(List<string> zipFiles)
        {
            foreach (string zipFile in zipFiles)
            {
                try
                {
                    File.Move(zipFile, processedZipFilesPath + Path.GetFileName(zipFile));

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error moving {zipFile}: {ex.Message}");
                }
            }
            zipFiles.RemoveAll(s => true);
        }
        private static bool IsHexColorCode(string input)
        {
            // Define a regular expression pattern for a valid hex color code
            string pattern = @"^#?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";

            // Use Regex.IsMatch to check if the input matches the pattern
            return Regex.IsMatch(input, pattern);
        }
        private void RunOnUIThread_BeginInvoke(Action body, DispatcherPriority priority = DispatcherPriority.Render)
        {
            this.Dispatcher.BeginInvoke(body, priority);
        }
        private void RunOnUIThread_Invoke(Action body, DispatcherPriority priority = DispatcherPriority.Render)
        {
            this.Dispatcher.Invoke(body, priority);
        }
        private void AppendErrorConsoleRichTxt(string msg)
        {
            RunOnUIThread_BeginInvoke(() =>
            {
                ErrorConsoleRichTxt.AppendText(msg + Environment.NewLine);
            });
        }
        private void ReportProgress(IProgress<object> progress, object value)
        {
            RunOnUIThread_BeginInvoke(() =>
            {
                progress.Report(value);
            });
        }
        private void DisableEveryThing()
        {
            ZipsFolderPathTxt.IsEnabled =
                ImagesBrowse.IsEnabled =
                CDRFilesLocationTxt.IsEnabled =
                FileNameTxt.IsEnabled =
                PersonNameTxt.IsEnabled=
                CityTxt.IsEnabled=
                DateOfBirthTxt.IsEnabled=
                ManuallyDesignedCDRFileLocationTxt.IsEnabled =
                CDRBrowseBtn.IsEnabled =
                BrowseBtn.IsEnabled =
              GenerateTemuBtn.IsEnabled =
              GenerateAmazonBtn.IsEnabled =
                GenerateManualCertificateBtn.IsEnabled =

                false;
            ErrorConsoleRichTxt.Document.Blocks.Clear();
            ErrorConsoleManualTabRichTxt.Document.Blocks.Clear();
        }
        private void EnableEveryThing()
        {
            ZipsFolderPathTxt.IsEnabled =
                ImagesBrowse.IsEnabled =
                GenerateTemuBtn.IsEnabled =
                CDRFilesLocationTxt.IsEnabled =
                ManuallyDesignedCDRFileLocationTxt.IsEnabled =
                BrowseBtn.IsEnabled =
                CDRBrowseBtn.IsEnabled =
                GenerateManualCertificateBtn.IsEnabled =
                GenerateAmazonBtn.IsEnabled =
                true;
            Progress_Bar.Value = 0;
        }
        #endregion

        private async void GenerateTemuBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CDRFilesLocationTxt.Text == "")
            {
                MessageBox.Show("Please select CDR files location", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (ZipsFolderPathTxt.Text == "")
            {
                MessageBox.Show("Please select zip files location", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {

                DisableEveryThing();
                var progress = new Progress<object>(v =>
                {
                    if (v is int value)
                    {
                        Progress_Bar.Value = value;
                    }
                    else
                    {
                        ErrorConsoleRichTxt.AppendText($"{v}{Environment.NewLine}");
                    }
                });
                processedZipFilesPath = ZipsFolderPathTxt.Text + @"\processed\";
                extractPath = CDRFilesLocationTxt.Text + @"\temp\";
                string zipFolderPath = ZipsFolderPathTxt.Text;
                string cdrFilesText = CDRFilesLocationTxt.Text;
                cdrFolderPath = cdrFilesText;

                if (GetZipFiles(zipFolderPath)?.Length > 0)
                {
                    await Task.Run(() =>
                    {
                        corelApp = GetCorelApplicationOrThrow();
                        Generate(progress, zipFolderPath, cdrFilesText, WorkingModeEnum.Temu);
                    });
                    MessageBox.Show("Files generated successfully!", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"No zip File found inside '{zipFolderPath}'", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                EnableEveryThing();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                EnableEveryThing();
            }
        }

        #region Manual Certificate Design Code behind 


        private void CDRBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            using CommonOpenFileDialog dialog = new()
            {
                IsFolderPicker = true,
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Select CDR Destination Files Folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok &&
                !string.IsNullOrWhiteSpace(dialog.FileName))
            {
                string selectedFolderPath = dialog.FileName;

                // Assuming ZipFolderPathCB is a TextBox or ComboBox in your XAML
                ManuallyDesignedCDRFileLocationTxt.Text = selectedFolderPath;
            }
        }

        private async void GenerateManualCertificateBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorConsoleManualTabRichTxt.Document.Blocks.Clear();

            // Validate CDR Folder
            if (string.IsNullOrWhiteSpace(ManuallyDesignedCDRFileLocationTxt.Text))
            {
                ShowError("Please select CDR file destination folder.");
                ManuallyDesignedCDRFileLocationTxt.Focus();
                return;
            }

            if (!Directory.Exists(ManuallyDesignedCDRFileLocationTxt.Text))
            {
                ShowError("Selected CDR destination folder does not exist.");
                ManuallyDesignedCDRFileLocationTxt.Focus();
                return;
            }

            // Validate File Name
            if (string.IsNullOrWhiteSpace(FileNameTxt.Text))
            {
                ShowError("Please enter file name.");
                FileNameTxt.Focus();
                return;
            }

            // Invalid filename characters check
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (FileNameTxt.Text.Contains(c))
                {
                    ShowError("File name contains invalid characters.");
                    FileNameTxt.Focus();
                    return;
                }
            }

            // Validate Person Name
            if (string.IsNullOrWhiteSpace(PersonNameTxt.Text))
            {
                ShowError("Please enter person name.");
                PersonNameTxt.Focus();
                return;
            }

            // Validate Date Of Birth
            if (DateOfBirthTxt.SelectedDateTime == null ||
                DateOfBirthTxt.SelectedDateTime.Value.Date == default)
            {
                ShowError("Please select date of birth.");
                DateOfBirthTxt.Focus();
                return;
            }

            // Optional future DOB check
            if (DateOfBirthTxt.SelectedDateTime.Value.Date > DateTime.Now.Date)
            {
                ShowError("Date of birth cannot be in future.");
                DateOfBirthTxt.Focus();
                return;
            }

            // Validate City
            if (string.IsNullOrWhiteSpace(CityTxt.Text))
            {
                ShowError("Please enter city.");
                CityTxt.Focus();
                return;
            }

            // All validations passed
            ShowSuccess("Validation passed. Ready to generate certificate.");

            CertificateInfo certificateInfo = new CertificateInfo
            {
                FileName = FileNameTxt.Text,
                City = CityTxt.Text,
                DateOfBirth = DateOfBirthTxt.SelectedDateTime.Value.Date.ToString(),
                PersonName = PersonNameTxt.Text
            };
            try
            {
                DisableEveryThing();

                string cdrLocation = ManuallyDesignedCDRFileLocationTxt.Text;
                // Your generation code here
                await Task.Run(() =>
                {
                    corelApp = GetCorelApplicationOrThrow();
                    GenerateManualCertificate(certificateInfo, cdrLocation);
                });
                MessageBox.Show("File generated successfully!", this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                EnableEveryThing();
            }
        }

        private void GenerateManualCertificate(CertificateInfo certificateInfo, string cdrSaveLocation)
        {
            ArgumentNullException.ThrowIfNull(corelApp);
            ArgumentNullException.ThrowIfNull(certificateInfo);

            string currentDirectoryPath = CoreHelper.GetAppAssemblyPath();
            string corelVersion = currentDirectoryPath.Contains(CorelDraw_2022_RootFolderName) ?
               "2022" : "2019";
#if DEBUG
            corelVersion = "2019";
#endif
            Directory.CreateDirectory(cdrSaveLocation);
            CoreHelper.EmptyTempDirectory(extractPath);

            string templateFilePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(currentDirectoryPath,
                "Resources\\Birthday Certificate Designer 2023\\Certificate Files\\", corelVersion, "Birth Certificate.cdr"));

            if (!_certificateManager.IsExcelDataLoaded())
            {
                _certificateManager.LoadCertificateData();
                _certificateManager.LoadSunriseSunsetTimingData();
            }

            Document document = null;

            List<MetaData> documentMetaData = _certificateManager.GetMetaDataList();
            string fileName = string.Empty;

            string tempTemplateFilePath = System.IO.Path.Combine(cdrSaveLocation, $"Birth Certificate Temp Template.cdr");

            document = NewDocument(corelApp, templateFilePath, tempTemplateFilePath);
            document.Activate();

            try
            {

                Certificate certificate = _certificateManager.GetCertificateDetails(certificateInfo) ?? throw new Exception("Certificate data cannot be null");

                if (certificate != null)
                {
                    FillDocument(document, certificate, documentMetaData);

                    fileName = cdrSaveLocation + "\\" + certificateInfo.FileName + ".cdr";

                    document?.SaveAs(fileName);
                    corelApp?.Refresh();
                }
            }

            catch (Exception ex)
            {
                ShowError(ex.Message);
            }


            if (!string.IsNullOrWhiteSpace(fileName) && document != null)
            {
                document?.Close();
                try
                {
                    if (File.Exists(tempTemplateFilePath)) { File.Delete(tempTemplateFilePath); }
                }
                catch { }
            }
        }

        private void ShowError(string message)
        {
            ErrorConsoleManualTabRichTxt.Document.Blocks.Add(
                new Paragraph(new Run("ERROR: " + message))
            );
        }

        private void ShowSuccess(string message)
        {
            ErrorConsoleManualTabRichTxt.Document.Blocks.Add(
                new Paragraph(new Run("SUCCESS: " + message))
            );
        }
        #endregion
    }
}
