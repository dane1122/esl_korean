using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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

            //TODO: split the pronounciation in syllables
            var wordSyllables = SplitSyllables(myDictionary.Pronounciation);

            //TODO: Get korean pronounciation
            var korUnicodes = GetKorUnicode(wordSyllables);

            myDictionary.KorWord = new List<char>();
            foreach (var unicode in korUnicodes)
            {
                myDictionary.KorWord.Add(Convert.ToChar(unicode));
            }

            //Return a dictionary
            return myDictionary;
        }



        /// <summary>
        /// For web application usage
        /// NOTE: This method is used to integrate with my personal website
        /// </summary>
        /// <param name="userInputWord"></param>
        /// <returns></returns>
        public static Dictionary<List<string>, List<char>> PerformOnWebApp(string userInputWord)
        {
            var myDictionary = RunMainLoop(userInputWord);
            if (myDictionary == null)
                return null;

            var newDictionary = new Dictionary<List<string>, List<char>>();
            newDictionary.Add(myDictionary.Pronounciation, myDictionary.KorWord);
            return newDictionary;
        }




        /// <summary>
        /// Split pronounciation into Korean readable syllables
        /// </summary>
        public static List<string> SplitSyllables(List<string> pronounciations)
        {
            List<string> consonants = GetConsonantDictionary(Properties.Resources.FilePathConsonants);

            //dictionary of pronounciation and integer indicating whether if it's a vowel or consonant
            //if vowel int = 0, if consonant int = 1
            
            List<string> syllables = new List<string>();


            for (int i = 0; i < pronounciations.Count; i++)
            {
                if (i == pronounciations.Count - 1)
                {
                    syllables.Add(pronounciations.Last());
                    break;
                }

                var current = pronounciations.ElementAt(i);
                var next1 = pronounciations.ElementAt(i + 1);

                string next2 = null;
                if (i + 2 < pronounciations.Count)
                {
                    next2 = pronounciations.ElementAt(i + 2);
                }
                
                //if the letter is a vowel
                if(!consonants.Contains(current))
                {
                    //next two following characters are consonants, the current letter forms a block with the next letter
                    if (consonants.Contains(next1) && consonants.Contains(next2))
                    {
                        syllables.Add(current + ' ' + next1);
                        i = i + 1;
                    }
                    
                    //only the next character is a consonant, the current letter forms a block on its own
                    else if(consonants.Contains(next1) && !consonants.Contains(next2))
                        syllables.Add(current);

                    //TODO: Remove this. For debug purpose
                    else
                        throw new Exception();
                }

                //if the letter is a consonant
                else if(consonants.Contains(current))
                {
                    //if only the next character is availabe
                    if (next2 == null)
                    {
                        syllables.Add(current + ' ' + next1);
                        i = i+1;
                    }

                    //if only the next two characters are availabe
                    else
                    {
                        if (!consonants.Contains(next1) && consonants.Contains(next2))
                        {
                            syllables.Add(current + ' ' + next1 + ' ' + next2);
                            i = i + 2;
                        }

                        else if (!consonants.Contains(next1) && !consonants.Contains(next2))
                        {
                            syllables.Add(current + ' ' + next1);
                            i = i+1;
                        }

                        else if(consonants.Contains(next1))
                            syllables.Add(current);
                        
                        else
                            throw new Exception();
                    }
                }
            }

            return syllables;
            
        }

        /// <summary>
        /// Read text file to get consonants or vowels
        /// </summary>
        public static List<string> GetConsonantDictionary(string filePath)
        {
            List<string> list = new List<string>();
            using (StreamReader reader = new StreamReader(filePath))
            {
                int i = 0;
                while (!reader.EndOfStream)
                    list.Add(reader.ReadLine());
            }
            return list;
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

        /// <summary>
        /// Ask for user input on console
        /// </summary>
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


        private static List<int> GetKorUnicode(List<string> engPronounc)
        {
            List<int> korUnicode = new List<int>();
            List<string> consonants = GetConsonantDictionary(Properties.Resources.FilePathConsonants);

            foreach (var pronoun in engPronounc)
            {
                string[] lettersInPronoun = pronoun.Split(' ');

                int initial = 11;
                int medial = 0;
                int final = 0;

                //Formular: ((initial*588)+(medial*28)+final) + 44032

                //If only one letter, only the mediator changes
                if (lettersInPronoun.Length == 1)
                {
                    if (consonants.Contains(lettersInPronoun.First()))
                    {
                        initial = GetInitialCode(lettersInPronoun.First());
                        medial = 18;
                    }
                    else
                        medial = GetMedialCode(lettersInPronoun.First());
                }

                //If two letters, initial adn the mediator change
                else if (lettersInPronoun.Length == 2)
                {
                    //If the first letter is a vowel
                    //this letter is actually a three component letter
                    if (!consonants.Contains(lettersInPronoun.First()))
                    {
                        initial = 11;
                        medial = GetMedialCode(lettersInPronoun.First());
                        final = GetFinalCode(lettersInPronoun.Last());
                    }
                    else
                    {
                        initial = GetInitialCode(lettersInPronoun.First());
                        medial = GetMedialCode(lettersInPronoun.Last());
                    }
                }

                else
                {
                    initial = GetInitialCode(lettersInPronoun.First());
                    medial = GetMedialCode(lettersInPronoun[1]);
                    final = GetFinalCode(lettersInPronoun.Last());
                }

                //If any of the korean character components are equal to -1, throw an exception
                //as -1 indicates that the mapping was not done correctly
                if (medial == -1 || initial == -1 || final == -1)
                    throw new Exception();

                int oneKorCharUnicode = ((initial * 588) + (medial * 28) + final) + 44032;
                korUnicode.Add(oneKorCharUnicode);
            }

            return korUnicode;
        }



        private static int GetMedialCode(string engPronoun)
        {
            engPronoun = engPronoun.ToUpper();

            switch (engPronoun)
            {
                case "AA":
                case "AA0":
                case "AA1":
                case "AA2":
                    return 0;
                case "AE":
                case "AE0":
                case "AE1":
                case "AE2":
                    return 1;
                case "AH":
                case "AH1":
                case "AH2":
                case "AH0":
                    return 4;
                case "AO":
                case "AO0":
                case "AO1":
                case "AO2":
                    return 8;
                case "AW":
                case "AW0":
                case "AW1":
                case "AW2":
                    return 9;
                case "AY":
                case "AY0":
                case "AY2":
                    return 20;
                case "AY1":
                    return 1;
                case "EH":
                case "EH0":
                case "EH1":
                case "EH2":
                    return 5;
                case "ER":
                case "ER0":
                case "ER1":
                case "ER2":
                    return 4;
                case "EY":
                case "EY0":
                case "EY1":
                case "EY2":
                    return 5;
                case "IH":
                case "IH0":
                case "IH1":
                case "IH2":
                case "IY":
                case "IY0":
                case "IY2":
                    return 20;
                case "IY1":
                    return 4;
                case "OW":
                case "OW1":
                case "OW2":
                case "OW0":
                    return 8;
                case "OY":
                case "OY0":
                case "OY1":
                case "OY2":
                    return 11;
                case "UH":
                case "UH0":
                case "UH1":
                case "UH2":
                case "UW":
                case "UW0":
                case "UW1":
                case "UW2":
                    return 13;
                case "Y":
                    return 17;
                default:
                    return -1;
            }
        }



        private static int GetInitialCode(string engPronoun)
        {
            //TODO: For more accurate romanization, examine the next letter (see if consonant or vowel) to determine the pronounciation of the current letter

            engPronoun = engPronoun.ToUpper();

            switch (engPronoun)
            {
                case "B":
                case "V":
                    return 7;
                case "CH":
                    return 14;
                case "D":
                case "DH":
                    return 3;
                case "F":
                case "P":
                    return 17;
                case "G":
                    return 0;
                case "HH":
                    return 18;
                case "JH":
                case "Z":
                case "ZH":
                    return 12;
                case "K":
                case "Q":
                    return 15;
                case "L":
                case "R":
                    return 5;
                case "M":
                    return 6;
                case "N":
                    return 2;
                case "NG":
                case "Y":
                    return 11;
                case "S":
                case "SH":
                    return 9;
                case "T":
                    return 16;
                case "TH":
                    return 10;
                case "W":
                    return 11;
                default:
                    return -1;
            }
        }



        private static int GetFinalCode(string engLetter)
        {
            var engPronoun = engLetter.ToUpper();

            switch (engPronoun)
            {
                case "B":
                case "V":
                    return 17;
                case "CH":
                    return 23;
                case "D":
                case "DH":
                    return 7;
                case "F":
                case "P":
                    return 26;
                case "G":
                    return 1;
                case "HH":
                    return 27;
                case "JH":
                case "Z":
                case "ZH":
                    return 22;
                case "K":
                case "Q":
                    return 24;
                case "L":
                case "R":
                    return 8;
                case "M":
                    return 16;
                case "N":
                    return 4;
                case "NG":
                case "Y":
                    return 21;
                case "S":
                case "SH":
                    return 19;
                case "T":
                    return 25;
                case "TH":
                    return 20;
                case "W":
                    return 21;
                default:
                    return -1;
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
