using System.Globalization;

namespace TenhouDesktop
{
    // The app's interface language: Spanish or English. "auto" follows Windows (Spanish if Windows is
    // in Spanish, English otherwise).
    static class Lang
    {
        public static bool English { get; private set; }

        public static string Code
        {
            get { return English ? "en" : "es"; }
        }

        public static void Set(string setting)
        {
            English = setting == "en" ||
                (setting != "es" && CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != "es");
        }

        // Picks the text for the current language
        public static string T(string es, string en)
        {
            return English ? en : es;
        }
    }
}
