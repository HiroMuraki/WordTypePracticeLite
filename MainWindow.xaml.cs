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
    ///
    #region 字段与属性
    public partial class MainWindow : Window {
        static readonly private Random rnd = new Random();
        static readonly private SolidColorBrush colorTypeReady = new SolidColorBrush(Color.FromRgb(0xf9, 0xc5, 0x3a));
        static readonly private SolidColorBrush colorTypeStatic = new SolidColorBrush(Color.FromRgb(0x15, 0xb9, 0x79));
        static readonly private SolidColorBrush colorTyping = new SolidColorBrush(Color.FromRgb(0xbd, 0x2f, 0x54));
        private List<WordItem> originWordsList;
        private PracticeWords practiceWords;
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
        private DispatcherTimer timer = new DispatcherTimer();
    }
    #endregion

    #region 主窗口
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            GetWordsListFile();
            GetPracticeWords();
            OrderedMode();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(Timer_Tick);
        }
        private void txtInputString_TextChanged(object sender, TextChangedEventArgs e) {
            if (timer.IsEnabled == false) {
                this.lblColorIndicator.Background = colorTyping;
                timer.Start();
            }
            if (currentInputWord.Length >= currentPracticeWord.Length) {
                //如果输入单词等于联系单词，增加正确计数
                if (currentInputWord == currentPracticeWord) {
                    ++correctCount;
                }
                //读取下一个单词或者结算
                if (this.practiceWords.CurrentWordIndex + 1 != this.practiceWords.Size) {
                    this.practiceWords.ToNextWord();
                    this.seekWordIndex = practiceWords.CurrentWordIndex;
                    GetWord();
                } else {
                    GetStaticstic();
                    //MessageBox.Show("YZTXDY");
                }
            }
        }
        private void sliderSeekWordIndex_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            this.practiceWords.CurrentWordIndex = seekWordIndex;
            GetWord();
        }
        private void toggleShuffleMode_Click(object sender, RoutedEventArgs e) {
            if (isRandomMode == true) {
                RandomMode();
            } else {
                OrderedMode();
            }
        }
        private void Timer_Tick(object sender, EventArgs e) {
            ++usingTime;
            this.toggleShuffleMode.Content = usingTime;
        }
    }
    #endregion

    #region 窗口操作与快捷键
    public partial class MainWindow : Window {
        private void Window_Move(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.InitialDirectory = Directory.GetCurrentDirectory();
                if (ofd.ShowDialog() == true) {
                    currentWordListFile = ofd.FileName;
                    GetPracticeWords();
                    OrderedMode();
                }
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
                    GetWordsListFile();
                    GetPracticeWords();
                    OrderedMode();
                    break;
                case Key.Enter when (this.practiceWords.CurrentWordIndex + 1 != this.practiceWords.Size):
                    ToggleVisibilitiyOfExtraInformation();
                    break;
                case Key.Enter:
                    RandomMode();
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
                seekWordIndex -= 1;
            } else if (e.Delta < 0) {
                seekWordIndex += 1;
            }
        }
    }
    #endregion

    #region 包装方法
    public partial class MainWindow : Window {
        private void GetStaticstic() {
            timer.Stop();
            this.lblStarsLevel.Visibility = Visibility.Visible;
            this.txtInputString.IsReadOnly = true;
            this.lblColorIndicator.Background = colorTypeStatic;
            this.lblStarsLevel.Content = practiceWords.GetStars(usingTime, correctCount);
            this.correctCount = 0;
            this.usingTime = 0;
        }
        private void ResetStaticstic() {
            timer.Stop();
            this.usingTime = 0;
            this.correctCount = 0;
            this.seekWordIndex = 0;
            this.toggleShuffleMode.Content = 0;
            this.txtInputString.IsReadOnly = false;
            this.currentInputWord = "";
            this.currentPracticeWord = "";
            this.lblStarsLevel.Content = practiceWords.CurrentWord.Meaning;
            this.lblCurrentPosition.Content = 1;
            this.lblColorIndicator.Background = colorTypeReady;
        }
        private void GetPracticeWords() {
            originWordsList = new List<WordItem>();
            if (File.Exists(currentWordListFile)) {
                using (StreamReader reader = new StreamReader(currentWordListFile)) {
                    while (!reader.EndOfStream) {
                        string currentLine = reader.ReadLine();
                        if (currentLine != null && currentLine != "" && currentLine != "\n" && currentLine != "\r") {
                            originWordsList.Add(new WordItem(currentLine));
                        }
                    }
                }
            }
            if (originWordsList.Count == 0) {
                originWordsList.Add(new WordItem("YZTXDY", "确信"));
            }
            practiceWords = new PracticeWords(originWordsList);
            this.sliderSeekWordIndex.Maximum = practiceWords.Size - 1;
        }
        private void RandomMode() {
            ResetStaticstic();
            isRandomMode = true;
            this.practiceWords.ShuffleWords();
            this.lblPreWord.Content = practiceWords.PreWord.Word;
            this.currentPracticeWord = practiceWords.CurrentWord.Word;
            this.lblNextWord.Content = practiceWords.NextWord.Word;
            this.lblStarsLevel.Content = practiceWords.CurrentWord.Meaning;
        }
        private void OrderedMode() {
            ResetStaticstic();
            isRandomMode = false;
            this.practiceWords = new PracticeWords(originWordsList);
            this.lblPreWord.Content = practiceWords.PreWord.Word;
            this.currentPracticeWord = practiceWords.CurrentWord.Word;
            this.lblNextWord.Content = practiceWords.NextWord.Word;
            this.lblStarsLevel.Content = practiceWords.CurrentWord.Meaning;
        }
        private void GetWordsListFile() {
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
        private void ToggleVisibilitiyOfExtraInformation() {
            if (this.sliderSeekWordIndex.Visibility == Visibility.Visible) {
                this.sliderSeekWordIndex.Visibility = Visibility.Collapsed;
                this.lblStarsLevel.Visibility = Visibility.Collapsed;
                this.lblPreWord.Visibility = Visibility.Collapsed;
                this.lblNextWord.Visibility = Visibility.Collapsed;
            } else {
                this.sliderSeekWordIndex.Visibility = Visibility.Visible;
                this.lblStarsLevel.Visibility = Visibility.Visible;
                this.lblNextWord.Visibility = Visibility.Visible;
                this.lblPreWord.Visibility = Visibility.Visible;
            }
        }
        private void GetWord() {
            this.currentInputWord = "";
            this.currentPracticeWord = this.practiceWords.CurrentWord.Word;
            this.lblStarsLevel.Content = this.practiceWords.CurrentWord.Meaning;
            this.lblPreWord.Content = this.practiceWords.PreWord.Word;
            this.lblNextWord.Content = this.practiceWords.NextWord.Word;
            this.lblCurrentPosition.Content = this.practiceWords.CurrentWordIndex + 1;
        }
    }
    #endregion
}
