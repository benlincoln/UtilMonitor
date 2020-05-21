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
        //Ensures only integers can be entered for the values
       public static readonly Regex tempRegex = new Regex("[^0-9]+"); 
        
    }
}
