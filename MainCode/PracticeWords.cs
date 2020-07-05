using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;

namespace WordTypePracticeLite {
    class Backup {
        //private int[] wordsIndex;
        //public int[] ShuffleWOrdsIndex() {
        //    this.wordsIndex = new int[Size];
        //    for (int i = 0; i < Size; i++) {
        //        wordsIndex[i] = i;
        //    }
        //    for (int i = 0; i < Size; i++) {
        //        int a = rnd.Next(Size);
        //        int b = rnd.Next(Size);
        //        Swap(ref wordsIndex[a], ref wordsIndex[b]);
        //    }
        //    return wordsIndex;
        //}
        //private void Swap(ref int x, ref int y) {
        //    int t = x;
        //    x = y;
        //    y = t;
        //}
        //public IEnumerable<string> GetShuffleWords() {
        //    if (wordsIndex == null) {
        //        ShuffleWOrdsIndex();
        //    }
        //    foreach (int index in this.wordsIndex) {
        //        yield return Words[index];
        //    }
        //}
        //public string GetARandomWord { get { return Words[rnd.Next(Size)]; } }
    }
    struct WordItem {
        public string Word;
        public string Meaning;
        public WordItem(string wordString) {
            string[] wordItem = wordString.Split('#');
            if (wordItem.Length >= 2) {
                this.Word = wordItem[0].Trim();
                this.Meaning = wordItem[1].Trim();
            } else if (wordItem.Length == 1) {
                this.Word = wordItem[0].Trim();
                this.Meaning = "";
            } else {
                this.Word = "";
                this.Meaning = "";
            }
        }
        public WordItem(string word, string meaning) {
            this.Word = word;
            this.Meaning = meaning;
        }
    }
    class PracticeWords : IEnumerable<WordItem> {
        readonly private Random rnd = new Random();
        public int CurrentWordIndex { get; set; }
        public WordItem PreWord {
            get {
                if (CurrentWordIndex > 0) {
                    return Words[CurrentWordIndex - 1];
                } else {
                    return new WordItem("", "");
                }
            }
        }
        public WordItem CurrentWord {
            get {
                return Words[CurrentWordIndex];
            }
            set {
                Words[CurrentWordIndex] = value;
            }
        }
        public WordItem NextWord {
            get {
                if (CurrentWordIndex < this.Size - 1) {
                    return Words[CurrentWordIndex + 1];
                } else {
                    return new WordItem("", "");
                }
            }
        }
        public List<WordItem> Words { get; set; }
        public int Size { get { return Words.Count; } }
        public WordItem this[int index] {
            get {
                return this.Words[index];
            }
            set {
                this.Words[index] = value;
            }
        }
        public PracticeWords() {
            this.Words = new List<WordItem>();
            CurrentWordIndex = 0;
        }
        public PracticeWords(List<WordItem> Words) : this() {
            foreach (WordItem word in Words) {
                this.Words.Add(word);
            }
        }
        public void ShuffleWords() {
            for (int i = 0; i < this.Size; i++) {
                int a = rnd.Next(this.Size);
                int b = rnd.Next(this.Size);
                WordItem t = Words[a];
                Words[a] = Words[b];
                Words[b] = t;
            }
        }
        public void ToNextWord() {
            if (CurrentWordIndex < Size - 1) {
                ++CurrentWordIndex;
            } else {
                this.CurrentWordIndex = 0;
            }
        }
        public void ToPreWord() {
            if (CurrentWordIndex > 0) {
                --CurrentWordIndex;
            } else {
                this.CurrentWordIndex = 0;
            }
        }
        public IEnumerator<WordItem> GetEnumerator() {
            foreach (WordItem word in Words) {
                yield return word;
            }
        }
        public IEnumerable<string> GetWords() {
            foreach (WordItem wordItem in this.Words) {
                yield return wordItem.Word;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
        public int GetScore(double timeUsing, int correctCount) {
            double timeRatio = 3 / (timeUsing / Size);
            timeRatio = timeRatio <= 1.5 ? timeRatio : 1.5;
            double correctRatio = correctCount / (double)Size;
            correctRatio = correctRatio <= 1.5 ? correctRatio : 1.5;
            int scores = (int)(100 * timeRatio * correctRatio);
            scores = scores <= 100 ? scores : 100;
            return scores;
        }
        public string GetStars(double timeUsing, int correctCount) {
            int scores = GetScore(timeUsing, correctCount);
            return TypePricatice.GetStars(scores);
        }
    }
}
