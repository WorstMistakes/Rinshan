using System.Globalization;

namespace Rinshan
{
    // The app's interface language: Spanish, English or Portuguese. "auto" follows Windows (Spanish or
    // Portuguese if Windows is in one of them, English otherwise).
    static class Lang
    {
        public static string Code { get; private set; }

        static Lang()
        {
            Code = "en";
        }

        public static void Set(string setting)
        {
            var system = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            Code = IsSupported(setting) ? setting : system == "es" || system == "pt" ? system : "en";
        }

        public static bool IsSupported(string code)
        {
            return code == "es" || code == "en" || code == "pt";
        }

        // Picks the text for the current language
        public static string T(string es, string en, string pt)
        {
            return Code == "es" ? es : Code == "pt" ? pt : en;
        }
    }
}
