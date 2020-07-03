using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        static readonly private Random rnd = new Random();
        static readonly private SolidColorBrush colorTypeReady = new SolidColorBrush(Color.FromRgb(0xf9, 0xc5, 0x3a));
        static readonly private SolidColorBrush colorTypeStatic = new SolidColorBrush(Color.FromRgb(0x15, 0xb9, 0x79));
        static readonly private SolidColorBrush colorTyping = new SolidColorBrush(Color.FromRgb(0xbd, 0x2f, 0x54));
        private string currentWordListFile;
        private int seekWordIndex {
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
        private int usingTime = 0;
        private int correctCount = 0;
        private PracticeWords practiceWords;
        List<int> RestWordsIndex;
        private DispatcherTimer timer = new DispatcherTimer();
        #endregion
        public MainWindow() {
            InitializeComponent();
            GetWordsListFiles();
            GetPracticeWords();
            OrderedMode();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(Timer_Tick);
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
            this.toggleShuffleMode.Content = usingTime;
            ++usingTime;
        }
        private void Window_Move(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = Directory.GetCurrentDirectory();
                if (ofd.ShowDialog() == true) {
                    currentWordListFile = ofd.FileName;
                }
                GetPracticeWords();
                OrderedMode();
            } else {
                this.DragMove();
            }
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
                    GetWordsListFiles();
                    GetPracticeWords();
                    OrderedMode();
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
            string stars = practiceWords.GetStars(usingTime, correctCount);
            this.lblStarsLevel.Content = stars;
            correctCount = 0;
            usingTime = 0;
        }
        private void ResetStaticstic() {
            timer.Stop();
            usingTime = 0;
            correctCount = 0;
            this.toggleShuffleMode.Content = 0;
            this.currentInputWord = "";
            this.currentPracticeWord = "";
            this.lblStarsLevel.Content = $"0/{practiceWords.WordsSize}";
            this.lblColorIndicator.Background = colorTypeReady;
        }
        private void GetPracticeWords() {
            List<string> words = new List<string>();
            if (File.Exists(currentWordListFile)) {
                using (StreamReader reader = new StreamReader(currentWordListFile)) {
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
            this.sliderSeekWordIndex.Maximum = practiceWords.WordsSize - 1;
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
        private void GetWordsListFiles() {
            List<string> possibleFiles = new List<string>();
            string currentPath = Directory.GetCurrentDirectory();
            foreach (string filePath in Directory.GetFiles(currentPath)) {
                string fileName = System.IO.Path.GetFileName(filePath);
                if (fileName.Contains("WordsListWP.txt")) {
                    possibleFiles.Add(fileName);
                }
            }
            if (possibleFiles.Count > 0) {
                currentWordListFile = possibleFiles[rnd.Next(possibleFiles.Count)];
            } else {
                currentWordListFile = null;
            }
        }
    }
}
