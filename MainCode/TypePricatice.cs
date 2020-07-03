using System;
using System.Linq;
using System.Linq.Expressions;
using static System.Console;

namespace WordTypePracticeLite {
    public static class TypePricatice {
        static readonly string[] UpperAlpha = new string[26] {
            "A","B","C","D","E","F","G",
            "H","I","J","K","L","M","N",
            "O","P","Q","R","S","T","U",
            "V","W","X","Y","Z"
        };
        static readonly string[] LowerAlpha = new string[26] {
            "a","b","c","d","e","f","g",
            "h","i","j","k","l","m","n",
            "o","p","q","r","s","t","u",
            "v","w","x","y","z"
        };
        static readonly string[] Digits = new string[10] {
            "0","1","2","3","4",
            "5","6","7","8","9"
        };
        static readonly string[] Symbols = new string[32] {
            "!", "\"", "#", "$", "%", "&", "'", "(", ")",
            "*", "+", ",", "-", ".", "/", ":", ";", "<",
            "=", ">", "?", "@", "[", "\\", "]", "^", "_",
            "`", "{", "|", "}", "~"
        };
        public static string GeneratePracticeString(bool?[] typeSettings, int stringLen) {
            if (IsInvalidSetting(typeSettings)) {
                return null;
            }
            Random rnd = new Random();
            string PracticeString = "";
            while (PracticeString.Length < stringLen) {
                switch (rnd.Next(4)) {
                    case 0 when typeSettings[0] == true:
                        PracticeString += UpperAlpha[rnd.Next(26)];
                        break;
                    case 1 when typeSettings[1] == true:
                        PracticeString += LowerAlpha[rnd.Next(26)];
                        break;
                    case 2 when typeSettings[2] == true:
                        PracticeString += Digits[rnd.Next(10)];
                        break;
                    case 3 when typeSettings[3] == true:
                        PracticeString += Symbols[rnd.Next(32)];
                        break;
                    default:
                        break;
                }
            }
            return PracticeString;
        }
#if false
        public static string GenerateTypeString(string Type, int Len) {
            Random rnd = new Random();
            string TypeString = "";
            int charType;
            if (Type == "0000") {
                return "0";
            }
            for (int i = 0; i < Len; ++i) {
                do {
                    charType = rnd.Next(4);
                } while (Type[charType] == '0');
                switch (charType) {
                    case 0://大写字母
                        TypeString += (char)rnd.Next(65, 91);
                        break;
                    case 1://小写字母
                        TypeString += (char)rnd.Next(97, 113);
                        break;
                    case 2://数字
                        TypeString += (char)rnd.Next(48, 58);
                        break;
                    case 3://符号
                        switch (rnd.Next(4)) {
                            case 0:
                                TypeString += (char)rnd.Next(33, 48);
                                break;
                            case 1:
                                TypeString += (char)rnd.Next(58, 65);
                                break;
                            case 2:
                                TypeString += (char)rnd.Next(91, 97);
                                break;
                            case 3:
                                TypeString += (char)rnd.Next(123, 127);
                                break;
                        }
                        break;
                }
            }
            return TypeString;
        }
#endif
        public static bool IsInvalidSetting(bool?[] typeSettings) {
            foreach (bool? setting in typeSettings) {
                if (setting == true) {
                    return false;
                }
            }
            return true;
        }
        public static double CorrectRatio(string src, string input) {
            int r = 0;
            for (int i = 0; i < src.Length; ++i) {
                if (src[i] == input[i]) {
                    ++r;
                }
            }
            return (double)r / src.Length;
        }
        public static int JudgeScores(bool?[] practiceType, int StrLen, int timeUsing, double correctRatio) {
            timeUsing = timeUsing > 0 ? timeUsing : 1;
            int level = 5;
            foreach (bool? set in practiceType) {
                if (set == true) {
                    --level;
                }
            }
            double PLN = StrLen * 2;
            double PTM = (StrLen / timeUsing) / (double)level;
            return (int)((PLN > 100 ? 100 : PLN) * (PTM > 1 ? 1 : PTM) * correctRatio);
        }
        public static string GetStars(int Scores) {
            string stars = "";
            for (int i = 0; i < Scores / 20; ++i) {
                stars += "★";
            }
            Scores /= 20;
            for (int i = 0; i < Scores / 10; ++i) {
                stars += "☆";
            }
            return stars;
        }
        public static string GetStars(bool?[] practiceType, string practiceString, string inputString, int timeUsing) {
            double correctRatio = CorrectRatio(practiceString, inputString);
            int scores = JudgeScores(practiceType, practiceString.Length, timeUsing, correctRatio);
            return GetStars(scores);
        }
    }
}
