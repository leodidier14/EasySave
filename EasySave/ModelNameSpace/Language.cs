using System.Collections.Generic;
using System.Xml;

namespace EasySave.ModelNameSpace
{
    class Language
    {
        public static List<string> switchLanguage(string language)
        {
            List<string> translationList = new List<string>();
            
            XmlDocument xml = new XmlDocument();

            //if the chosen language is French load the French xml
            if (language == "fr")
            {
                xml.LoadXml(ResourceFiles.Resource.french);
            }
            //if the chosen language is English load the English xml
            else
            {
                xml.LoadXml(ResourceFiles.Resource.english);
            }

            //for each node in loaded xml add the value (word) in the translation list
            foreach (XmlNode word in xml.DocumentElement.ChildNodes)
            {
                translationList.Add(word.Attributes["val"]?.InnerText);
            }

            return translationList;
        }

    }
}

