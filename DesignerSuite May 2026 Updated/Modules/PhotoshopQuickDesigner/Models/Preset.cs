using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSQuickDesigner
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class Preset : PropertyChangedBase, IEquatable<Preset>
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string identifier;
        public string Identifier
        {
            get { return identifier; }
            set
            {
                if (value != identifier)
                {
                    identifier = value;
                    OnPropertyChanged(nameof(Identifier));
                }
            }
        }

        private string group;
        public string Group
        {
            get { return group; }
            set
            {
                if (value != group)
                {
                    group = value;
                    OnPropertyChanged(nameof(Group));
                }
            }
        }

        private double width;
        public double Width
        {
            get { return width; }
            set
            {
                if (value != width)
                {
                    width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        private double height;
        public double Height
        {
            get { return height; }
            set
            {
                if (value != height)
                {
                    height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        private string units = "centimetersUnit";
        public string Units
        {
            get { return units; }
            set
            {
                if (value != units)
                {
                    units = value;
                    OnPropertyChanged(nameof(Units));
                }
            }
        }

        private string profile;
        public string Profile
        {
            get { return profile; }
            set
            {
                if (value != profile)
                {
                    profile = value;
                    OnPropertyChanged(nameof(Profile));
                }
            }
        }

        private float resolution;
        public float Resolution
        {
            get { return resolution; }
            set
            {
                if (value != resolution)
                {
                    resolution = value;
                    OnPropertyChanged(nameof(Resolution));
                }
            }
        }

        private string resolutionUnits;
        public string ResolutionUnits
        {
            get { return resolutionUnits; }
            set
            {
                if (value != resolutionUnits)
                {
                    resolutionUnits = value;
                    OnPropertyChanged(nameof(ResolutionUnits));
                }
            }
        }

        private int depth;
        public int Depth
        {
            get { return depth; }
            set
            {
                if (value != depth)
                {
                    depth = value;
                    OnPropertyChanged(nameof(Depth));
                }
            }
        }

        private double scale;
        public double Scale
        {
            get { return scale; }
            set
            {
                if (value != scale)
                {
                    scale = value;
                    OnPropertyChanged(nameof(Scale));
                }
            }
        }

        private string mode;
        public string Mode
        {
            get { return mode; }
            set
            {
                if (value != mode)
                {
                    mode = value;
                    OnPropertyChanged(nameof(Mode));
                }
            }
        }

        private object fill;
        //[JsonIgnore]
        public object Fill
        {
            get { return fill; }
            set
            {
                if (value != fill)
                {
                    fill = value;
                    OnPropertyChanged(nameof(Fill));
                }
            }
        }

        private List<object> guides;
        public List<object> Guides
        {
            get { return guides; }
            set
            {
                if (value != guides)
                {
                    guides = value;
                    OnPropertyChanged(nameof(Guides));
                }
            }
        }

        private List<Artboard> artboards;
        public List<Artboard> Artboards
        {
            get { return artboards; }
            set
            {
                if (value != artboards)
                {
                    artboards = value;
                    OnPropertyChanged(nameof(Artboards));
                }
            }
        }

        private long lastUsedTime;
        public long LastUsedTime
        {
            get { return lastUsedTime; }
            set
            {
                if (value != lastUsedTime)
                {
                    lastUsedTime = value;
                    OnPropertyChanged(nameof(LastUsedTime));
                }
            }
        }

        public bool Equals(Preset other)
        {
            if (other == null) return false;

            return string.Equals(Name, other.Name) &&
                   string.Equals(Identifier, other.Identifier) &&
                   string.Equals(Group, other.Group) &&
                   Width.Equals(other.Width) &&
                   Height.Equals(other.Height) &&
                   string.Equals(Units, other.Units) &&
                   string.Equals(Profile, other.Profile) &&
                   Resolution.Equals(other.Resolution) &&
                   string.Equals(ResolutionUnits, other.ResolutionUnits) &&
                   Depth.Equals(other.Depth) &&
                   Scale.Equals(other.Scale) &&
                   string.Equals(Mode, other.Mode) &&
                   Equals(Fill, other.Fill) &&
                   ((Guides == null && other.Guides == null) || (Guides != null && other.Guides != null && Guides.SequenceEqual(other.Guides))) &&
                   ((Artboards == null && other.Artboards == null) || (Artboards != null && other.Artboards != null && Artboards.SequenceEqual(other.Artboards))) &&
                   LastUsedTime.Equals(other.LastUsedTime);
        }
    }

    public class Artboard : IEquatable<Artboard>
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public double Bottom { get; set; }
        public double Right { get; set; }

        public bool Equals(Artboard other)
        {
            if (other == null)
                return false;

            // Implement equality comparison based on your criteria
            return Top.Equals(other.Top) &&
                   Left.Equals(other.Left) &&
                   Bottom.Equals(other.Bottom) &&
                   Right.Equals(other.Right);
        }

        // Optionally override GetHashCode as well
        //public override int GetHashCode()
        //{
        //    // Implement a suitable hash code calculation based on your criteria
        //    return HashCode.Combine(Top, Left, Bottom, Right);
        //}

    }



    public class Root
    {
        public List<Preset> Presets { get; set; }
    }

}
