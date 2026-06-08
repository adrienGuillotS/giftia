using PSQuickDesigner.Enums;
using PSQuickDesigner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSQuickDesigner.ViewModels
{
    public class CreateNewPresetViewModel : PropertyChangedBase
    {

        #region Binding properties
        private Preset _selectedPreset;
        public Preset SelectedPreset
        {
            get { return _selectedPreset; }
            set
            {
                _selectedPreset = value;
                OnPropertyChanged(nameof(SelectedPreset));

                SelectedUnit = Units?.FirstOrDefault(a => a.StringValue.Equals(SelectedPreset.Units));
                SelectedResolutionUnit = ResolutionUnits?.FirstOrDefault(a => a.StringValue.Equals(SelectedPreset.ResolutionUnits));
                SelectedColorProfile = ColorProfiles?.FirstOrDefault(a => a.Name.Equals(SelectedPreset.Profile));
                if (SelectedColorProfile == null)
                {
                    SelectedColorProfile = ColorProfiles?.FirstOrDefault();
                }

                SelectedPixelAspectRatio = PixelAspectRatios?.FirstOrDefault(a => a.Scale == SelectedPreset.Scale);

                SelectedColorMode = ColorModes?.FirstOrDefault(a => a.StringValue.Equals(SelectedPreset.Mode));
                SelectedFillColor = FillColors?.FirstOrDefault(a => a.StringValue.Equals(SelectedPreset.Fill));
            }
        }


        private Orientation _selectedOrientation = Orientation.Portrait;
        public Orientation SelectedOrientation
        {
            get { return _selectedOrientation; }
            set
            {

                if (_selectedOrientation != value)
                {
                    _selectedOrientation = value;
                    OnPropertyChanged(nameof(SelectedOrientation));

                    if (SelectedPreset != null)
                    {
                        var width = SelectedPreset.Width;
                        var height = SelectedPreset.Height;

                        SelectedPreset.Height = width;
                        SelectedPreset.Width = height;
                    }
                }
            }
        }
        #endregion

        #region Units

        private List<Unit> _units = GetUnits();
        public List<Unit> Units
        {
            get { return _units; }
            set
            {
                _units = value;
                OnPropertyChanged(nameof(Units));
            }
        }

        private Unit _selectedUnit;
        public Unit SelectedUnit
        {
            get { return _selectedUnit; }
            set
            {
                _selectedUnit = value;
                SelectedPreset.Units = value.StringValue;
                OnPropertyChanged(nameof(SelectedUnit));
            }
        }

        public static List<Unit> GetUnits()
        {
            var list = new List<Unit>()
            {
                new Unit{ Name="Pixel", StringValue="pixelsUnit",Value=Photoshop.PsUnits.psPixels},
                new Unit{ Name="Inches",StringValue="inchesUnit", Value=Photoshop.PsUnits.psInches},
                new Unit { Name = "Centimeters", StringValue="centimetersUnit", Value = Photoshop.PsUnits.psCM },
                new Unit{ Name="Millimeters", StringValue="millimetersUnit",Value=Photoshop.PsUnits.psMM},
                new Unit{ Name="Points", StringValue="pointsUnit",Value=Photoshop.PsUnits.psPoints},
                new Unit{ Name="Picas", StringValue="picasUnit",Value=Photoshop.PsUnits.psPicas},
            };
            return list;

        }

        #endregion

        #region Resolution Units

        private List<ResolutionUnit> _resolutionUnits = GetResolutionUnit();
        public List<ResolutionUnit> ResolutionUnits
        {
            get { return _resolutionUnits; }
            set
            {
                _resolutionUnits = value;
                OnPropertyChanged(nameof(ResolutionUnits));
            }
        }

        private ResolutionUnit _selectedResolutionUnit;
        public ResolutionUnit SelectedResolutionUnit
        {
            get { return _selectedResolutionUnit; }
            set
            {
                _selectedResolutionUnit = value;
                SelectedPreset.ResolutionUnits = value?.StringValue;
                OnPropertyChanged(nameof(SelectedResolutionUnit));
            }
        }

        public static List<ResolutionUnit> GetResolutionUnit()
        {
            var list = new List<ResolutionUnit>()
            {
                new ResolutionUnit{ Name="Pixels/Inch",StringValue="inchesUnit", Value=PsResolutions.psPixelsPerInch},
                new ResolutionUnit{ Name="Pixels/Centimeter", StringValue="centimetersUnit",Value=PsResolutions.psPixelsPerCentimeter},
            };
            return list;
        }

        #endregion

        #region Color Profiles

        private List<ColorProfile> _colorProfiles = GetColorProfiles();

        public List<ColorProfile> ColorProfiles
        {
            get { return _colorProfiles; }
            set
            {
                _colorProfiles = value;
                OnPropertyChanged(nameof(ColorProfiles));
            }
        }


        private ColorProfile _selectedColorProfile;
        public ColorProfile SelectedColorProfile
        {
            get { return _selectedColorProfile; }
            set
            {
                _selectedColorProfile = value;
                SelectedPreset.Profile = value?.StringValue;
                OnPropertyChanged(nameof(SelectedColorProfile));
            }
        }

        public static List<ColorProfile> GetColorProfiles()
        {
            var list = new List<ColorProfile>()
            {
                new ColorProfile { Name = "default" , StringValue = "sRGB IEC61966-2.1"},
                new ColorProfile { Name = "Working RGB: sRGB IEC61966-2.1" , StringValue = "sRGB IEC61966-2.1"},
                new ColorProfile{ Name = "Don't Color Manage" , StringValue = "none" },
                new ColorProfile{ Name = "Adobe RGB (1998)" , StringValue = "Adobe RGB (1998)" },
                new ColorProfile{ Name = "Apple RGB" , StringValue = "Apple RGB" },
                new ColorProfile{ Name = "ColorMatch RGB" , StringValue = "ColorMatch RGB" },
                new ColorProfile{ Name = "image P3" , StringValue = "image P3" },
                new ColorProfile{ Name = "ProPhoto RGB" , StringValue = "ProPhoto RGB" },
                new ColorProfile{ Name = "sRGB IEC61966-2.1" , StringValue = "sRGB IEC61966-2.1" },
                new ColorProfile{ Name = "CIE RGB" , StringValue = "CIE RGB" },
                new ColorProfile{ Name = "e-sRGB" , StringValue = "e-sRGB" },
                new ColorProfile{ Name = "HDTV (Rec. 709)" , StringValue = "HDTV (Rec. 709)" },
                new ColorProfile{ Name = "Laptop Internal LCD Monitor" , StringValue = "Laptop Internal LCD Monitor" },
                new ColorProfile{ Name = "Laptop Internal MaxBright Display" , StringValue = "Laptop Internal MaxBright Display" },
                new ColorProfile{ Name = "Lenovo Laptop Internal Display" , StringValue = "Lenovo Laptop Internal Display" },
                new ColorProfile{ Name = "Notebook PC Internal Display" , StringValue = "Notebook PC Internal Display" },
                new ColorProfile{ Name = "PAL/SECAM" , StringValue = "PAL/SECAM" },
                new ColorProfile{ Name = "ROMM-RGB" , StringValue = "ROMM-RGB" },
                new ColorProfile{ Name = "SMPTE-C" , StringValue = "SMPTE-C" },
                new ColorProfile{ Name = "Wide Gamut RGB" , StringValue = "Wide Gamut RGB" },
                new ColorProfile{ Name = "Wide viewing angle &amp; High density FlexView Display" , StringValue = "Wide viewing angle &amp; High density FlexView Display" },
                new ColorProfile{ Name = "* wscRGB" , StringValue = "* wscRGB" },
                new ColorProfile{ Name = "* wsRGB" , StringValue = "* wsRGB" }
            };
            return list;
        }
        #endregion

        #region Scale

        private List<PixelAspectRatio> _pixelAspectRatios = GetPixelAspectRatio();

        public List<PixelAspectRatio> PixelAspectRatios
        {
            get { return _pixelAspectRatios; }
            set
            {
                _pixelAspectRatios = value;
                OnPropertyChanged(nameof(PixelAspectRatios));
            }
        }


        private PixelAspectRatio _selectedPixelAspectRatio;
        public PixelAspectRatio SelectedPixelAspectRatio
        {
            get { return _selectedPixelAspectRatio; }
            set
            {
                _selectedPixelAspectRatio = value;
                if (value != null)
                    SelectedPreset.Scale = value.Scale;
                OnPropertyChanged(nameof(SelectedPixelAspectRatio));
            }
        }

        public static List<PixelAspectRatio> GetPixelAspectRatio()
        {

            var list = new List<PixelAspectRatio>()
            {
                new PixelAspectRatio { Name = "Square Pixel", Scale = 1.0 },
                new PixelAspectRatio { Name = "D1/DV NTSC (0.91)" , Scale = 0.9091 },
                new PixelAspectRatio { Name = "D1/DV PAL (1.09)" , Scale = 1.094 },
                new PixelAspectRatio { Name = "D1/DV NTSC Widescreen (1.21)" , Scale = 1.2121 },
                new PixelAspectRatio { Name = "HDV 1080/DVCPRO HD 720 (1.33)" , Scale = 1.333 },
                new PixelAspectRatio { Name = "D1/DV PAL Widescreen (1.46)" , Scale = 1.4587 },
                new PixelAspectRatio { Name = "Anamorphic 2:1 (2)" , Scale = 2.0 },
                new PixelAspectRatio { Name = "DVCPRO HD 1080 (1.5)" , Scale = 1.5}
            };
            return list;
        }
        #endregion

        #region Color Mode

        private List<ColorMode> _colorModes = GetColorModes();
        public List<ColorMode> ColorModes
        {
            get { return _colorModes; }
            set
            {
                _colorModes = value;
                OnPropertyChanged(nameof(ColorModes));
            }
        }


        private ColorMode _selectedColorMode;
        public ColorMode SelectedColorMode
        {
            get { return _selectedColorMode; }
            set
            {
                if (value == null || _selectedColorMode == value) { return; }

                _selectedColorMode = value;
                SelectedPreset.Mode = value.StringValue;
                OnPropertyChanged(nameof(SelectedColorMode));

                Depths = GetDepthSizes(SelectedColorMode.Value);
                SelectedDepth = Depths.FirstOrDefault(a => a.Size == SelectedPreset?.Depth);
            }
        }

        public static List<ColorMode> GetColorModes()
        {
            var list = new List<ColorMode>()
            {
                new ColorMode{ Name="Bitmap",StringValue="bitmap", Value=Photoshop.PsNewDocumentMode.psNewBitmap},
                new ColorMode{ Name="Grayscale",StringValue="grayscale", Value=Photoshop.PsNewDocumentMode.psNewGray},
                new ColorMode{ Name="RGB Color",StringValue="RGB", Value=Photoshop.PsNewDocumentMode.psNewRGB},
                new ColorMode{ Name="CMYK Color",StringValue="CMYK", Value=Photoshop.PsNewDocumentMode.psNewCMYK},
                new ColorMode{ Name="Lab Color",StringValue="Lab", Value=Photoshop.PsNewDocumentMode.psNewLab},
            };
            return list;
        }

        #endregion

        #region Depth

        private List<Depth> _depths;

        public List<Depth> Depths
        {
            get { return _depths; }
            set
            {
                _depths = value;
                OnPropertyChanged(nameof(Depths));
            }
        }


        private Depth _selectedDepth;
        public Depth SelectedDepth
        {
            get { return _selectedDepth; }
            set
            {
                if (value == null) { return; }

                _selectedDepth = value;
                SelectedPreset.Depth = value.Size;
                OnPropertyChanged(nameof(SelectedDepth));
            }
        }


        public List<Depth> GetDepthSizes(Photoshop.PsNewDocumentMode mode)
        {
            var list = new List<Depth>()
            {
                new Depth { Name = "1 bit", Size = 1, IsEnabled=true },
                new Depth { Name = "8 bit", Size = 8, IsEnabled=true },
                new Depth { Name = "16 bit", Size = 16, IsEnabled=true},
                new Depth { Name = "32 bit", Size = 32, IsEnabled=true },
            };

            if (mode == Photoshop.PsNewDocumentMode.psNewBitmap)
            {
                list.ForEach(depth => depth.IsEnabled = depth.Size == 1);
                SelectedPreset.Depth = 1;
            }
            else if (mode == Photoshop.PsNewDocumentMode.psNewRGB ||
                mode == Photoshop.PsNewDocumentMode.psNewGray)
            {
                foreach (var item in list)
                {
                    item.IsEnabled = item.Size != 1;
                }
                if (SelectedPreset.Depth == 1) { SelectedPreset.Depth = 8; }
            }
            else if (mode == Photoshop.PsNewDocumentMode.psNewCMYK ||
                mode == Photoshop.PsNewDocumentMode.psNewLab)
            {
                foreach (var item in list)
                {
                    item.IsEnabled = item.Size != 1 && item.Size != 32;
                }
                if (SelectedPreset.Depth == 1 || SelectedPreset.Depth == 32) { SelectedPreset.Depth = 8; }
            }

            return list;
        }
        #endregion

        #region Fill Color

        private List<FillColor> _fillColors = GetFillColors();

        public List<FillColor> FillColors
        {
            get { return _fillColors; }
            set
            {
                _fillColors = value;
                OnPropertyChanged(nameof(FillColors));
            }
        }


        private FillColor _selectedFillColor;
        public FillColor SelectedFillColor
        {
            get { return _selectedFillColor; }
            set
            {
                if (value == null) { return; }

                _selectedFillColor = value;
                SelectedPreset.Fill = value.StringValue;
                OnPropertyChanged(nameof(SelectedFillColor));
            }
        }

        public static List<FillColor> GetFillColors()
        {
            var list = new List<FillColor>()
            {
                new FillColor { Name = "White", StringValue="white", Fill =InitialFillEnum.psWhite},
                new FillColor { Name = "Background Color", StringValue="background", Fill =InitialFillEnum.psBackgroundColor},
                new FillColor { Name = "Transparent", StringValue="transparent", Fill =InitialFillEnum.psTransparent },
            };
            return list;
        }

        #endregion


        public CreateNewPresetViewModel()
        {
            SelectedPreset = new Preset()
            {
                Width = 300,
                Height = 400,
                Resolution = 300,
            };

            SelectedResolutionUnit = ResolutionUnits?.FirstOrDefault();
            SelectedColorMode = ColorModes?.FirstOrDefault();
            SelectedFillColor = FillColors?.FirstOrDefault();
            SelectedPixelAspectRatio = PixelAspectRatios?.FirstOrDefault();
            SelectedFillColor = FillColors?.FirstOrDefault();
            SelectedColorProfile = ColorProfiles?.FirstOrDefault();
        }
    }
}
