using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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

            //String array of two strings (word, pronounciation)
            string[] wordPronounciation = GetCmuPronounciation(lineContainingWord);

            foreach (var pronounciation in wordPronounciation)
                Console.Write(pronounciation + " ");

            myDictionary.EngWord = wordPronounciation[0];
            myDictionary.Pronounciation = ReturnPronounciation(wordPronounciation.Last());

            //TODO: Get korean pronounciation
            myDictionary.KorWord = GetKorPronounciation(myDictionary.Pronounciation);

            //Return a dictionary
            return myDictionary;
        }



        /// <summary>
        /// For web application usage
        /// NOTE: This method is used to integrate with my personal website
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




        public static List<string> SplitSyllables(List<string> pronounciation)
        {
            return new List<string>();
            
        }






        private static List<string> ReturnPronounciation(string wordAndPronoun)
        {
            var pronounciation = wordAndPronoun.Split(' ').ToList();
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

                    if (!String.IsNullOrWhiteSpace(line) && line.Length < word.Length)
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
            if (String.IsNullOrWhiteSpace(lineFromDic))
                throw new Exception("Line containing the word was not given");

            return Regex.Split(lineFromDic, @"\s{2,}");
        }


        private static List<string> GetKorPronounciation(List<string> pronounciation)
        {
            List<string> koreanWord = new List<string>();

            for(int i = 0; i < pronounciation.Count; i++)
            {
                string korLetter;
                
                if (i == 0)
                    korLetter = GetKorLetter(pronounciation.ElementAt(i), true, false, pronounciation);
                else if (i == pronounciation.Count - 1)
                    korLetter = GetKorLetter(pronounciation.ElementAt(i), false, true, pronounciation);
                else
                    korLetter = GetKorLetter(pronounciation.ElementAt(i), false, false, pronounciation);

                koreanWord.Add(korLetter);
            }
            return koreanWord;
        }





        private static string GetKorLetter(string engLetter, bool isFirstLetter, bool isLastLetter, List<string> fullPronounciation)
        {
            //TODO: For more accurate romanization, examine the next letter (see if consonant or vowel) to determine the pronounciation of the current letter

            engLetter = engLetter.ToUpper();
            string korLetter = null;

            if (isFirstLetter)
            {
                korLetter = DealWithFirstLetter(engLetter);
                if (korLetter != "?")
                    return korLetter;
            }

            else if (isLastLetter)
            {
                korLetter = DealWithLastLetter(engLetter);
                if (korLetter != "?")
                    return korLetter;
            }

            switch (engLetter)
            {
                case "B":
                case "V":
                    return "ㅂ";
                case "CH":
                    return "ㅊ";
                case "D":
                case "DH":
                    return "ㄷ";
                case "AA":
                case "AA0":
                case "AA1":
                case "AA2":
                    return "ㅏ";
                case "AE":
                case "AE0":
                case "AE1":
                case "AE2":
                    return "ㅐ";
                case "AH":
                case "AH1":
                case "AH2":
                case "AH0":
                    return "ㅓ";
                case "AO":
                case "AO0":
                case "AO1":
                case "AO2":
                    return "ㅗ";
                case "AW":
                case "AW0":
                case "AW1":
                case "AW2":
                    return "ㅏ우";
                case "AY":
                case "AY0":
                case "AY2":
                    return "ㅣ";
                case "AY1":
                    return "ㅏ이";
                case "EH":
                case "EH0":
                case "EH1":
                case "EH2":
                    return "ㅔ";
                case "ER":
                case "ER0":
                case "ER1":
                case "ER2":
                    return "ㅓㄹ";
                case "EY":
                case "EY0":
                case "EY1":
                case "EY2":
                    return "ㅔ이";
                case "F":
                case "P":
                    return "ㅍ";
                case "G":
                    return "ㄱ";
                case "HH":
                    return "ㅎ";
                case "IH":
                case "IH0":
                case "IH1":
                case "IH2":
                case "IY":
                case "IY0":
                case "IY2":
                    return "ㅣ";
                case "IY1":
                    return "ㅓ";
                case "JH":
                case "Z":
                case "ZH":
                    return "ㅈ";
                case "K":
                case "Q":
                    return "ㅋ";
                case "L":
                case "R":
                    return "ㄹ";
                case "M":
                    return "ㅁ";
                case "N":
                    return "ㄴ";
                case "NG":
                case "Y":
                    return "ㅇ";
                case "OW":
                case "OW1":
                case "OW2":
                case "OW0":
                    return "ㅗ";
                case "OY":
                case "OY0":
                case "OY1":
                case "OY2":
                    return "ㅗ이";
                case "S":
                case "SH":
                    return "ㅅ";
                case "T":
                    return "ㅌ";
                case "TH":
                    return "ㅆ";
                case "UH":
                case "UH0":
                case "UH1":
                case "UH2":
                case "UW":
                case "UW0":
                case "UW1":
                case "UW2":
                    return "ㅜ";
                case "W":
                    return "우";
                default:
                    return "?";
            }
        }



        private static string DealWithLastLetter(string engLetter)
        {
            switch (engLetter)
            {
                case "SH":
                    return "쉬";
                case "DH":
                case "TH":
                    return "쓰";
                case "B":
                case "V":
                    return "브";
                case "CH":
                    return "츠";
                case "D":
                    return "드";
                case "F":
                case "P":
                    return "프";
                case "G":
                    return "그";
                case "K":
                case "Q":
                    return "크";
                case "Z":
                case "ZH":
                    return "즈";
                case "JH":
                    return "지";
                case "L":
                    return "르";
                case "N":
                    return "느";
                case "S":
                    return "스";
                case "T":
                    return "트";
                default:
                    return "?";
            }
        }

        private static string DealWithFirstLetter(string engLetter)
        {
            switch (engLetter)
            {
                case "W":
                    return "워";
                case "SH":
                    return "쉬";
                case "R":
                    return "루";
                case "EH2":
                case "EH":
                case "EH1":
                case "EH0":
                    return "에";
                case "IH0":
                case "IH":
                case "IH1":
                case "IH2":
                    return "이";
                case "AE0":
                case "AE":
                case "AE1":
                case "AE2":
                    return "애";
                case "AO1":
                case "AO":
                case "AO2":
                case "AO0":
                    return "오";
                default:
                    return "?";
            }
        }





    }
}
