using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DictionaryReader
{
	public class Reader
	{
		public static void Main()
		{
		    bool performCode = true;

		    while (performCode)
		    {
		        RunMainLoop();
		        performCode = AskTryAgain();
		    }
		}

        private static Dictionary RunMainLoop(string givenWord = null)
        {
            bool valid = false;
            string lineContainingWord = null;

            var myDictionary = new Dictionary();

            //If running from console application
            if (givenWord == null)
            {
                while (!valid)
                {
                    lineContainingWord = PerformWordSearch(AskForUserInput());
                    if (lineContainingWord != null)
                        valid = true;
                }
            }
            
            //if running from web application
            else
            {
                lineContainingWord = PerformWordSearch(givenWord);
                if (lineContainingWord == null)
                    return null;
            }
            
            string[] wordPronounciation = GetCmuPronounciation(lineContainingWord);

            foreach (var pronounciation in wordPronounciation)
                Console.Write(pronounciation + " ");

            myDictionary.EngWord = wordPronounciation[0];
            myDictionary.Pronounciation = ReturnPronounciation(wordPronounciation);
            //TODO: Get korean pronounciation

            //Return a dictionary
            return myDictionary;
        }



        /// <summary>
        /// For web application usage
        /// </summary>
        /// <param name="userInputWord"></param>
        /// <returns></returns>
	    public static Dictionary<List<string>, List<string>> PerformOnWebApp(string userInputWord)
	    {
	        var myDictionary = RunMainLoop(userInputWord);
            if (myDictionary == null)
                return null;

            var newDictionary = new Dictionary<List<string>, List<string>>();
            newDictionary.Add(myDictionary.Pronounciation, myDictionary.KorWord);
	        
            return newDictionary;
	    }





	    private static List<string> ReturnPronounciation(string[] wordAndPronoun)
	    {
            var pronounciation = new List<string>();

            //Skip one index because it is the actual word not the pronounciation
            for (var i = 1; i < wordAndPronoun.Length; i++)
                pronounciation.Add(wordAndPronoun[i]);

	        return pronounciation;
	    }





	 


        private static string AskForUserInput()
        {
            Console.WriteLine("Enter a word:");
            var userWord = Console.ReadLine();

            return userWord;
        }

	    private static bool AskTryAgain()
	    {
	        bool answer = false;
	        bool validAnswer = false;

            Console.WriteLine();

	        while (!validAnswer)
	        {
                Console.WriteLine("Try Again? Y or N");
                var userAnswer = Console.ReadLine();

	            if (String.Equals(userAnswer, "Y", StringComparison.InvariantCultureIgnoreCase))
	            {
                    answer = true;
	                validAnswer = true;
	            }
                else if (String.Equals(userAnswer, "N", StringComparison.InvariantCultureIgnoreCase))
                {
                    answer = false;
                    validAnswer = true;
                }

                else
                {
                    Console.WriteLine("Y or N");
                    validAnswer = false;
                }
	        }

            return answer;
	    }



        /// <summary>
        /// Search for the user inputted word in the dictionary
        /// If the word was not found, returns null
        /// </summary>
        /// <param name="engWord"></param>
        /// <returns>line containing the word and its pronounciation</returns>
		private static string PerformWordSearch(string engWord)
		{
			var word = engWord.ToLowerInvariant();

            using (StreamReader reader = new StreamReader(Properties.Resources.FilePathCmuDictionary))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					
					if(!String.IsNullOrWhiteSpace(line) && line.Length < word.Length)
						continue;

                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        var foundWord = line.ToLowerInvariant().Substring(0, word.Length);
                        if (foundWord.Equals(word))
                            return line;
                    }
				}
			}
            Console.WriteLine("Could not find the word in the dictionary. Please enter a different word");
            return null;
		}


        /// <summary>
        /// Split the line of string into the word and its pronounciation
        /// </summary>
        /// <param name="lineFromDic"></param>
        /// <returns></returns>
		private static string[] GetCmuPronounciation(string lineFromDic)
		{
			if(String.IsNullOrWhiteSpace(lineFromDic))
				throw new Exception("Line containing the word was not given");

			return Regex.Split(lineFromDic, @"\s{2,}");
		}


		private static string GetKorPronounciation(string[] pronounciation)
		{
            return null;
		}





		private static string GetKorVowel(string engVowel)
		{
			engVowel = engVowel.ToUpper();
			string korVowel = null;

			switch (engVowel)
			{
				case "AA":
				case "AA0":
				case "AA1":
				case "AA2":
					korVowel = "ㅏ";
					break;
				case "AE":
				case "AE0":
				case "AE1":
				case "AE2":
					korVowel = "ㅐ";
					break;
				case "AH":
				case "AH0":
				case "AH1":
				case "AH2":
					korVowel = "ㅓ";
					break;
				case "AO":
				case "AO0":
				case "AO1":
				case "AO2":
					korVowel = "ㅗ";
					break;
				case "AW":
				case "AW0":
				case "AW1":
				case "AW2":
					korVowel = "ㅏ우";
					break;
				case "AY":
				case "AY0":
				case "AY1":
				case "AY2":
					korVowel = "ㅣ";
					break;
				case "EH":
				case "EH0":
				case "EH1":
				case "EH2":
					korVowel = "ㅔ";
					break;
				case "ER":
				case "ER0":
				case "ER1":
				case "ER2":
					korVowel = "ㅓㄹ";
					break;
				case "EY":
				case "EY0":
				case "EY1":
				case "EY2":
					korVowel = "ㅔ이";
					break;
			}

			return korVowel;
		}



		private static string GetKorConsonant(string engConsonant)
		{
			engConsonant = engConsonant.ToUpper();

            string koreanConsotant = null;

			switch (engConsonant)
			{
				case "B":
					koreanConsotant = "ㅂ";
                    break;
				case "CH":
					koreanConsotant =  "ㅊ";
                    break;
				case "D":
					koreanConsotant = "ㄷ";
                    break;
				case "DH":
					koreanConsotant = "ㅆ";
                    break;
				case "F":
					koreanConsotant = "ㅍ";
                    break;
				case "G":
					koreanConsotant  = "ㄱ";
                    break;
			}

            return koreanConsotant;
		}





	}
}
