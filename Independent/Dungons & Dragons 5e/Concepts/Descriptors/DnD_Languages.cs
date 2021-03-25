using System.Collections.Generic;

namespace Dungeons_and_Dragons
{
    public enum Language 
    {
        Common,
        Dwrwish,
        Elvish,
        Giant,
        Gnomish,
        Goblin, Halfling,
        Orc,
        Abyssal = 100,
        Celestial = 101,
        DeepSpeech = 102,
        Draconic = 103,
        Infernal = 104,
        Primordial = 105,
        Sylvan = 106,
        Undercommon = 107,

        NOT_FOUND = 999
    }

    public static class LanguagesExtensions 
    {
        public static bool IsExotic(this Language lang) => ((int)lang) > 99;

        public static Language GetCommonWith (this List<Language> langs, List<Language> other) 
        {
            foreach (var lang in langs) 
                if (other.Contains(lang)) 
                    return lang;

            return Language.NOT_FOUND;
        }
    }

}
