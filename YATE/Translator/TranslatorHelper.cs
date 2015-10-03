using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Semagsoft.Translator
{
    public class TranslatorHelper
    {
        public string Translate(string text, Microsoft.DetectedLanguage from, Microsoft.Language to)
        {
            Microsoft.TranslatorContainer tc = InitializeTranslatorContainer();
            var translationResult = TranslateString(tc, text, from, to);
            string s = translationResult.Text;
            return s;
        }

        private static Microsoft.TranslatorContainer InitializeTranslatorContainer()
        {
            // this is the service root uri for the Microsoft Translator service 
            var serviceRootUri = new Uri("https://api.datamarket.azure.com/Data.ashx/Bing/MicrosoftTranslator/v1");

            // this is the Account Key I generated for this app
            var accountKey = "/0b+tmBWHiSXno8WmLzEtxxl7pDfrwlQO2Cqa/vcyow=";

            // Replace the account key above with your own and then delete 
            // the following line of code. You can get your own account key
            // for from here: https://datamarket.azure.com/account/keys
            //throw new Exception("Invalid Account Key");

            // the TranslatorContainer gives us access to the Microsoft Translator services
            var tc = new Microsoft.TranslatorContainer(serviceRootUri);

            // Give the TranslatorContainer access to your subscription
            tc.Credentials = new System.Net.NetworkCredential(accountKey, accountKey);
            return tc;
        }

        private static Microsoft.Translation TranslateString(Microsoft.TranslatorContainer tc, string inputString, Microsoft.DetectedLanguage sourceLanguage, Microsoft.Language targetLanguage)
        {
            // Generate the query
            var translationQuery = tc.Translate(inputString, targetLanguage.Code, sourceLanguage.Code);

            // Call the query and get the results as a List
            var translationResults = translationQuery.Execute().ToList();

            // Verify there was a result
            if (translationResults.Count() <= 0)
            {
                return null;
            }

            // In case there were multiple results, pick the first one
            var translationResult = translationResults.First();

            return translationResult;
        }
    }
}