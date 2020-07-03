﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WordTypePracticeLite;

namespace WordTypePracticeLite {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        #region 字段
        static readonly string defaultWordListFile = "TPWordList.txt";
        public int seekWordIndex {
            get {
                return (int)this.sliderSeekWordIndex.Value;
            }
            set {
                this.sliderSeekWordIndex.Value = value;
            }
        }
        private string currentInputWord {
            get {
                return this.txtInputString.Text;
            }
            set {
                this.txtInputString.Text = value;
            }
        }
        private string currentPracticeWord {
            get {
                return this.txtPracticeString.Text;
            }
            set {
                this.txtPracticeString.Text = value;
            }
        }
        private bool? isRandomMode {
            get {
                return this.toggleShuffleMode.IsChecked;
            }
            set {
                this.toggleShuffleMode.IsChecked = value;
            }
        }
        private int usingTimeCount = 0;
        private int correctCount = 0;
        private PracticeWords practiceWords;
        List<int> RestWordsIndex;
        private DispatcherTimer timer = new DispatcherTimer();
        private SolidColorBrush colorTypeReady = new SolidColorBrush(Color.FromRgb(0xf9, 0xc5, 0x3a));
        private SolidColorBrush colorTypeStatic = new SolidColorBrush(Color.FromRgb(0x15, 0xb9, 0x79));
        private SolidColorBrush colorTyping = new SolidColorBrush(Color.FromRgb(0xbd, 0x2f, 0x54));
        private SolidColorBrush colorNotClick = new SolidColorBrush(Color.FromRgb(0xa0, 0xa0, 0xa0));
        private SolidColorBrush colorClicked = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30));
        #endregion
        public MainWindow() {
            InitializeComponent();
            if (File.Exists("TPWordList.txt") == false) {
                MessageBox.Show($"未找到{defaultWordListFile}文件，请确保{defaultWordListFile}与该程序位于同一目录");
                Application.Current.Shutdown();
                return;
            }
            GetPracticeWords();
            this.sliderSeekWordIndex.Maximum = practiceWords.WordsSize - 1;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(Timer_Tick);
            OrderedMode();
        }
        private void txtInputString_TextChanged(object sender, TextChangedEventArgs e) {
            if (this.RestWordsIndex == null) {
                RandomMode();
            }
            if (timer.IsEnabled == false) {
                this.lblColorIndicator.Background = colorTyping;
                timer.Start();
            }
            if (currentInputWord.Length >= currentPracticeWord.Length) {
                if (this.RestWordsIndex.Count > 0) {
                    //如果输入单词等于联系单词，增加正确计数
                    if (currentInputWord == currentPracticeWord) {
                        ++correctCount;
                    }
                    //读取下一个单词
                    seekWordIndex = RestWordsIndex[0];
                    this.currentInputWord = "";
                    this.currentPracticeWord = practiceWords.Words[RestWordsIndex[0]];
                    RestWordsIndex.RemoveAt(0);
                    //同步练习进度
                    this.lblStarsLevel.Content = $"{practiceWords.WordsSize - RestWordsIndex.Count}/{practiceWords.WordsSize}";
                } else if (this.RestWordsIndex.Count == 0) {
                    if (currentInputWord == currentPracticeWord) {
                        ++correctCount;
                    }
                    GetStaticstic();
                    //MessageBox.Show("YZTXDY");
                }
            }
        }
        private void sliderSeekWordIndex_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            currentInputWord = "";
            currentPracticeWord = practiceWords.Words[seekWordIndex];
        }
        private void Timer_Tick(object sender, EventArgs e) {
            this.toggleShuffleMode.Content = usingTimeCount;
            ++usingTimeCount;
        }
        private void Window_Move(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }
        private void Window_KeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.F1:
                    RandomMode();
                    break;
                case Key.F2:
                    OrderedMode();
                    break;
                case Key.F5:
                    RandomMode();
                    break;
                case Key.Enter:
                    if (this.sliderSeekWordIndex.Visibility == Visibility.Visible) {
                        this.sliderSeekWordIndex.Visibility = Visibility.Collapsed;
                    } else {
                        this.sliderSeekWordIndex.Visibility = Visibility.Visible;
                    }
                    break;
                case Key.Escape:
                    Application.Current.Shutdown();
                    break;
                default:
                    break;
            }
        }
        private void Window_MouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                seekWordIndex += 1;
            } else if (e.Delta < 0) {
                seekWordIndex -= 1;
            }
        }
        private void toggleShuffleMode_Click(object sender, RoutedEventArgs e) {
            if (isRandomMode == true) {
                RandomMode();
            } else {
                OrderedMode();
            }
        }
    }

    public partial class MainWindow : Window {
        private void GetStaticstic() {
            timer.Stop();
            this.RestWordsIndex = null;
            this.lblColorIndicator.Background = colorTypeStatic;
            string stars = practiceWords.GetStars(usingTimeCount, correctCount);
            this.lblStarsLevel.Content = stars;
            correctCount = 0;
            usingTimeCount = 0;
        }
        private void ResetStaticstic() {
            timer.Stop();
            usingTimeCount = 0;
            correctCount = 0;
            this.toggleShuffleMode.Content = 0;
            this.currentInputWord = "";
            this.currentPracticeWord = "";
            this.lblStarsLevel.Content = $"0/{practiceWords.WordsSize}";
            this.lblColorIndicator.Background = colorTypeReady;
        }
        private void GetPracticeWords() {
            List<string> words = new List<string>();
            if (File.Exists(defaultWordListFile)) {
                using (StreamReader reader = new StreamReader(defaultWordListFile)) {
                    while (!reader.EndOfStream) {
                        string currentLine = reader.ReadLine();
                        if (currentLine != null && currentLine != "" && currentLine != "\n" && currentLine != "\r") {
                            words.Add(currentLine);
                        }
                    }
                }
            }
            practiceWords = new PracticeWords(words);
            if (practiceWords.Words.Count == 0) {
                practiceWords.Words.Add("NULL_WORDS");
            }
        }
        private void RandomMode() {
            ResetStaticstic();
            isRandomMode = true;
            this.RestWordsIndex = new List<int>(practiceWords.ShuffleWOrdsIndex());
            currentPracticeWord = practiceWords.Words[RestWordsIndex[0]];
            RestWordsIndex.RemoveAt(0);
        }
        private void OrderedMode() {
            ResetStaticstic();
            isRandomMode = false;
            this.RestWordsIndex = new List<int>();
            for (int i = 0; i < practiceWords.WordsSize; i++) {
                RestWordsIndex.Add(i);
            }
            currentPracticeWord = practiceWords.Words[RestWordsIndex[0]];
            RestWordsIndex.RemoveAt(0);
        }
    }
}
