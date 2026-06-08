using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    using System.Collections.Generic;

    public class OrderRoot
    {
        public string OrderId { get; set; }
        public string Asin { get; set; }
        public CustomizationData CustomizationData { get; set; }
    }

    public class CustomizationData
    {
        public List<SurfaceNode> Children { get; set; }
    }

    public class SurfaceNode
    {
        public string Name { get; set; }               // Mug Side 1, Mug Side 2, Coaster, etc.
        public List<CustomizationChild> Children { get; set; }
    }

    public class CustomizationChild
    {
        public string InputValue { get; set; }         // Text
        public string FontFamily { get; set; }
        public string ColorValue { get; set; }
        public string ImageName { get; set; }          // jpg/png/svg
        public List<CustomizationChild> Children { get; set; }
    }

}
