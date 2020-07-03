using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows;

namespace WordTypePracticeLite {
    class PracticeWords : IEnumerable<string> {
        public List<string> Words { get; set; }
        public string RandomWord { get { return Words[rnd.Next(WordsSize)]; } }
        public int WordsSize { get { return Words.Count; } }
        private int[] wordsIndex;
        readonly private Random rnd = new Random();
        public PracticeWords() {
            this.Words = new List<string>();
        }
        public PracticeWords(List<string> Words) {
            this.Words = Words;
            this.ShuffleWOrdsIndex();
        }
        public int[] ShuffleWOrdsIndex() {
            this.wordsIndex = new int[WordsSize];
            for (int i = 0; i < WordsSize; i++) {
                wordsIndex[i] = i;
            }
            for (int i = 0; i < WordsSize; i++) {
                int a = rnd.Next(WordsSize);
                int b = rnd.Next(WordsSize);
                Swap(ref wordsIndex[a], ref wordsIndex[b]);
            }
            return wordsIndex;
        }
        private void Swap(ref int x, ref int y) {
            int t = x;
            x = y;
            y = t;
        }
        public IEnumerable<string> GetShuffleWords() {
            if (wordsIndex == null) {
                ShuffleWOrdsIndex();
            }
            foreach (int index in this.wordsIndex) {
                yield return Words[index];
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
            double timeRatio = 4 / (timeUsing / WordsSize);
            double correctRatio = correctCount / (double)WordsSize;
            int scores = (int)(100 * timeRatio * correctRatio);
            scores = scores <= 100 ? scores : 100;
            return TypePricatice.GetStars(scores);
        }
    }
}
