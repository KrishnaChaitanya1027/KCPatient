using System;
using System.Text.RegularExpressions;

namespace KCClassLibrary
{
    public static class KCValidations
    {
        public static string KCCapitalize(string text)
        {
            text = text?.Trim().ToLower();
            if(text!=null)
            {
                return Regex.Replace(text, "^[a-z]", m => m.Value.ToUpper());
            }
            return text;
        }

        public static string KCExtractDigits(string text)
        {
            string number = Regex.Replace(text, @"\D", "");
            return number;
        }

        public static bool KCPostalCodeValidation(string text)
        {
            string pattern = @"^\d{5}-\d{4}|\d{5}|[A-Z]\d[A-Z] \d[A-Z]\d$";

            Regex reg = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if ((reg.IsMatch(text)) || text == "")
                return false;
            return true;
        }

        public static string KCPostalCodeFormat(string text)
        {
            var regex = new Regex(@"\s");
            var postalCode = text.ToUpper();
            if (!regex.IsMatch(text))
            {
                return postalCode.ToUpper().Insert(3, " ");
            }
            return postalCode;


        }

        public static bool KCZipCodeValidation(string text)
        {
            string number = Regex.Replace(text, @"\D", "");
           if(number.ToCharArray().Length == 5)
            {
                return true;
            }
            else if(number.ToCharArray().Length == 9)
            {
                Regex.Replace(text, @"^\d{ 5} (?:[-\s]\d{ 4})?$", "");
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
