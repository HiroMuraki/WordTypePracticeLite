using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;

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
    class PracticeWords : IEnumerable<string> {
        readonly private Random rnd = new Random();
        public int CurrentWordIndex { get; set; }
        public string CurrentWord {
            get {
                return Words[CurrentWordIndex];
            }
            set {
                Words[CurrentWordIndex] = value;
            }
        }
        public List<string> Words { get; set; }
        public int Size { get { return Words.Count; } }
        public string this[int index] {
            get {
                return this.Words[index];
            }
            set {
                this.Words[index] = value;
            }
        }
        public PracticeWords() {
            this.Words = new List<string>();
            CurrentWordIndex = 0;
        }
        public PracticeWords(List<string> Words) : this() {
            foreach (string word in Words) {
                this.Words.Add(word);
            }
        }
        public void ShuffleWords() {
            for (int i = 0; i < this.Size; i++) {
                int a = rnd.Next(this.Size);
                int b = rnd.Next(this.Size);
                string t = Words[a];
                Words[a] = Words[b];
                Words[b] = t;
            }
        }
        public void NextWord() {
            if (CurrentWordIndex < Size - 1) {
                ++CurrentWordIndex;
            } else {
                this.CurrentWordIndex = 0;
            }
        }
        public void PreWord() {
            if (CurrentWordIndex > 0) {
                --CurrentWordIndex;
            } else {
                this.CurrentWordIndex = 0;
            }
        }
        public IEnumerator<string> GetEnumerator() {
            foreach (string word in Words) {
                yield return word;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
        public string GetStars(double timeUsing, int correctCount) {
            double timeRatio = 4 / (timeUsing / Size);
            double correctRatio = correctCount / (double)Size;
            int scores = (int)(100 * timeRatio * correctRatio);
            scores = scores <= 100 ? scores : 100;
            return TypePricatice.GetStars(scores);
        }
    }
}
