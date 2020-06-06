using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UtilMonitor.Non_WPF_Code
{
    public static class RegexChecker
    {
        //Ensures only integers can be entered for the temp values
        public static readonly Regex tempRegex = new Regex("[^0-9]+");
        // Checks for values between 0 and 100. Below acts same as above, will look further into
        public static readonly Regex percentRegex = new Regex("[^1]?[0-9]{2}$");

    }
}
