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
using static WordTypePracticeLite.ResDict;

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
        static readonly private SolidColorBrush colorShuffleMode = new SolidColorBrush(Color.FromRgb(0x24, 0xa0, 0xff));
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
                return this.toggleIndicatorBottom.IsChecked;
            }
            set {
                this.toggleIndicatorBottom.IsChecked = value;
            }
        }
        private int usingTime = 0;
        private int correctCount = 0;
        private bool? isCurrentWordLocked {
            get {
                return this.toggleIndicatorTop.IsChecked;
            }
            set {
                this.toggleIndicatorTop.IsChecked = value;
            }
        }
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
            if (isCurrentWordLocked == false) {
                if (timer.IsEnabled == false) {
                    timer.Start();
                }
                if (currentInputWord.Length >= currentPracticeWord.Length) {
                    //如果输入单词等于联系单词，增加正确计数
                    if (currentInputWord == this.practiceWords.CurrentWord.Word) {
                        ++correctCount;
                    }
                    //读取下一个单词或者结算
                    if (this.practiceWords.CurrentWordIndex + 1 != this.practiceWords.Size) {
                        this.practiceWords.ToNextWord();
                        GetWord();
                    } else {
                        GetStaticstic();
                        return;
                        //MessageBox.Show("YZTXDY");
                    }
                }
            }
            CmpWord();
        }
        private void sliderSeekWordIndex_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            this.practiceWords.CurrentWordIndex = seekWordIndex;
            GetWord();
        }
        private void toggleIndicatorTop_Click(object sender, RoutedEventArgs e) {
            if (this.toggleIndicatorTop.IsChecked == true) {
                this.lblCurrentPosition.Content = "\u267e";
                timer.Stop();
            } else {
                this.lblCurrentPosition.Content = practiceWords.CurrentWordIndex + 1;
                timer.Start();
            }
        }
        private void toggleIndicatorBottom_Click(object sender, RoutedEventArgs e) {
            ResetStaticstic();
            if (this.toggleIndicatorBottom.IsChecked == true) {
                this.practiceWords.ShuffleWords();
            } else {
                this.practiceWords = new PracticeWords(originWordsList);
            }
            GetWord();
        }
    }
    #endregion

    #region 窗口操作与快捷键
    public partial class MainWindow : Window {
        private void Window_Move(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "文本文件|*.txt|所有文件|*.*";
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
                    OrderedMode();
                    break;
                case Key.F2:
                    RandomMode();
                    break;
                case Key.F5:
                    GetWordsListFile();
                    GetPracticeWords();
                    OrderedMode();
                    break;
                case Key.LeftCtrl:
                    SwitchShuffleMode();
                    break;
                case Key.RightCtrl:
                    SwitchLockCurrentWord();
                    break;
                case Key.Enter when (this.practiceWords.CurrentWordIndex + 1 != this.practiceWords.Size):
                    SwitchVisibilitiyOfExtraInformation();
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
            this.txtInputString.IsReadOnly = true;
            this.toggleIndicatorBottom.IsChecked = null;
            this.toggleIndicatorTop.IsChecked = false;
            this.lblCurrentWordMeaning.Content = null;
            foreach (char ch in practiceWords.GetStars(usingTime, correctCount)) {
                Image star = new Image();
                if (ch == '★') {
                    star.Style = ResDict.PreSetting["ImageStarA"] as Style;
                } else if (ch == '☆') {
                    star.Style = ResDict.PreSetting["ImageStarB"] as Style;
                }
                this.panelStarsCount.Children.Add(star);
            }
            this.correctCount = 0;
            this.usingTime = 0;
        }
        private void ResetStaticstic() {
            timer.Stop();
            this.usingTime = 0;
            this.correctCount = 0;
            this.seekWordIndex = 0;
            this.toggleIndicatorTop.Content = 0;
            this.txtInputString.IsReadOnly = false;
            this.isCurrentWordLocked = false;
            this.currentInputWord = "";
            this.currentPracticeWord = "";
            this.panelStarsCount.Children.Clear();
            this.lblCurrentWordMeaning.Content = practiceWords.CurrentWord.Meaning;
            this.lblCurrentPosition.Content = 1;
        }
        private void GetWordsListFile() {
            List<string> possibleFiles = new List<string>();
            string currentPath = Directory.GetCurrentDirectory();
            foreach (string filePath in Directory.GetFiles(currentPath)) {
                string fileName = System.IO.Path.GetFileName(filePath);
                if (fileName.Contains("WordsListWP")) {
                    possibleFiles.Add(fileName);
                }
            }
            if (possibleFiles.Count > 0) {
                currentWordListFile = possibleFiles[rnd.Next(possibleFiles.Count)];
            } else {
                currentWordListFile = null;
            }
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
            this.practiceWords.ShuffleWords();
            this.isCurrentWordLocked = false;
            this.isRandomMode = true;
            GetWord();
        }
        private void OrderedMode() {
            ResetStaticstic();
            this.practiceWords = new PracticeWords(originWordsList);
            this.isCurrentWordLocked = false;
            this.isRandomMode = false;
            GetWord();
        }
        private void SwitchVisibilitiyOfExtraInformation() {
            if (this.sliderSeekWordIndex.Visibility == Visibility.Visible) {
                this.sliderSeekWordIndex.Visibility = Visibility.Collapsed;
                this.lblCurrentWordMeaning.Visibility = Visibility.Collapsed;
                this.lblPreWord.Visibility = Visibility.Collapsed;
                this.lblNextWord.Visibility = Visibility.Collapsed;
            } else {
                this.sliderSeekWordIndex.Visibility = Visibility.Visible;
                this.lblCurrentWordMeaning.Visibility = Visibility.Visible;
                this.lblNextWord.Visibility = Visibility.Visible;
                this.lblPreWord.Visibility = Visibility.Visible;
            }
        }
        private void SwitchShuffleMode() {
            if (this.isRandomMode == true) {
                OrderedMode();
                this.isRandomMode = false;
            } else {
                RandomMode();
                this.isRandomMode = true;
            }
        }
        private void SwitchLockCurrentWord() {
            if (this.isCurrentWordLocked == true) {
                this.timer.Start();
                this.lblCurrentPosition.Content = practiceWords.CurrentWordIndex + 1;
                this.isCurrentWordLocked = false;
            } else {
                this.timer.Stop();
                this.lblCurrentPosition.Content = "\u267e";
                this.isCurrentWordLocked = true;
            }
        }
        private void GetWord() {
            this.currentInputWord = "";
            this.seekWordIndex = practiceWords.CurrentWordIndex;
            this.currentPracticeWord = this.practiceWords.CurrentWord.Word;
            this.lblCurrentWordMeaning.Content = this.practiceWords.CurrentWord.Meaning;
            this.lblPreWord.Content = this.practiceWords.PreWord.Word;
            this.lblNextWord.Content = this.practiceWords.NextWord.Word;
            if (isCurrentWordLocked == false) {
                this.lblCurrentPosition.Content = this.practiceWords.CurrentWordIndex + 1;
            }
        }
        private void CmpWord() {
            string temp = this.practiceWords.CurrentWord.Word;
            this.currentPracticeWord = "";
            for (int i = 0; i < this.practiceWords.CurrentWord.Word.Length; i++) {
                if (i < this.currentInputWord.Length) {
                    this.currentPracticeWord += " ";
                } else {
                    this.currentPracticeWord += temp[i];
                }
            }
        }
        private void Timer_Tick(object sender, EventArgs e) {
            ++usingTime;
            this.toggleIndicatorTop.Content = usingTime;
        }
    }
    #endregion
}
