using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KeychainQuickDesigner.Module.Constants
{
    public static class AppConstants
    {
        /// <summary>
        /// Represents a compiled regular expression used to validate CDR file names that follow a specific pattern.
        /// </summary>
        /// <remarks>The file name pattern is defined as a numeric prefix followed by an underscore and a
        /// GUID in the format:
        /// <c>^\d+_[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$</c>. This ensures that
        /// the file name starts with one or more digits, followed by an underscore, and ends with a valid
        /// GUID.</remarks>
        public static Regex FileNameRegex = new(@"^\d+_[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", RegexOptions.Compiled);

    }
}
