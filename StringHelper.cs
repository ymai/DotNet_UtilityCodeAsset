using System;
using System.Text;
using System.Text.RegularExpressions;

namespace UtilityCodeAsset.Helper
{
    public class StringHelper
    {
        public static string RemoveSpecialCharacters(string input)
        {
            Regex r = new Regex(@"\s+");
            string str = r.Replace(input, @" ");
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c == ' ') || (c == ','))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Return the directory running the installed app
        /// </summary>
        /// <returns></returns>
        public static string GetInstallDir()
        {
            string t = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            t = System.Environment.CurrentDirectory;
            //string t = System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;
            if (t[t.Length - 1] != '\\')
                return t + "\\";
            return t;
        }

    }
}
