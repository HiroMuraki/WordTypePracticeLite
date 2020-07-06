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
using System.Windows.Resources;
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
                return this.practiceWords.CurrentWord.Word;
            }
        }
        private string currentPracticeWordDisplay {
            get {
                return this.txtPracticeString.Text;
            }
            set {
                this.txtPracticeString.Text = value;
            }
        }
        private int usingTime = 0;
        private int correctCount = 0;
        private bool? isRandomMode {
            get {
                return this.toggleIndicatorBottom.IsChecked;
            }
            set {
                this.toggleIndicatorBottom.IsChecked = value;
            }
        }
        private bool? isCurrentWordLocked {
            get {
                return this.toggleIndicatorTop.IsChecked;
            }
            set {
                this.toggleIndicatorTop.IsChecked = value;
            }
        }
        private bool isPracticeCompleted {
            get {
                return this.practiceWords.CurrentWordIndex + 1 == this.practiceWords.Size;
            }
        }
        private bool isImmerseMode {
            get {
                if (this.lblCurrentWordMeaning.Visibility == Visibility.Visible) {
                    return false;
                } else {
                    return true;
                }
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
            if (timer.IsEnabled == false && isCurrentWordLocked == false) {
                timer.Start();
            }
            if (currentInputWord.Length >= currentPracticeWord.Length) {
                //如果打开了锁定模式，则跳过结算判定
                if (isCurrentWordLocked == false) {
                    //如果输入单词等于联系单词，增加正确计数
                    if (currentInputWord == this.currentPracticeWord) {
                        ++correctCount;
                    }
                    //读取下一个单词或者结算
                    if (isPracticeCompleted) {
                        GetStaticstic();
                        return;
                        //MessageBox.Show("YZTXDY");
                    } else {
                        this.practiceWords.ToNextWord();
                    }
                }
                GetWord();
            }
            CmpWord();
        }
        private void sliderSeekWordIndex_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            this.sliderSeekWordIndex.ToolTip = $"{this.practiceWords.CurrentWordIndex}/{this.practiceWords.Size}";
            this.practiceWords.CurrentWordIndex = seekWordIndex;
            GetWord();
        }
        private void toggleIndicatorTop_Click(object sender, RoutedEventArgs e) {
            if (this.toggleIndicatorTop.IsChecked == true) {
                LockWordMode();
            } else {
                UnlockWordMode();
            }
        }
        private void toggleIndicatorBottom_Click(object sender, RoutedEventArgs e) {
            if (this.toggleIndicatorBottom.IsChecked == true) {
                RandomMode();
            } else {
                OrderedMode();
            }
        }
    }
    #endregion

    #region 窗口操作与快捷键
    public partial class MainWindow : Window {
        private void Window_Move(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) {
                LoadFile();
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
                case Key.Enter when !isPracticeCompleted:
                    SwitchImmerseMode();
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
        private void Window_Close(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }
    }
    #endregion

    #region 包装方法
    public partial class MainWindow : Window {
        //获取评分信息
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
        //重置状态
        private void ResetStaticstic() {
            timer.Stop();
            this.usingTime = 0;
            this.correctCount = 0;
            this.seekWordIndex = 0;
            this.toggleIndicatorTop.Content = 0;
            this.txtInputString.IsReadOnly = false;
            this.isCurrentWordLocked = false;
            this.currentInputWord = "";
            this.currentPracticeWordDisplay = "";
            this.panelStarsCount.Children.Clear();
            this.lblCurrentWordMeaning.Content = practiceWords.CurrentWord.Meaning;
            this.lblCurrentPosition.Content = 1;
        }
        //获取文件列表
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
        //获取练习单词列表
        private void GetPracticeWords() {
            originWordsList = new List<WordItem>();
            if (File.Exists(currentWordListFile)) {
                using (StreamReader reader = new StreamReader(currentWordListFile)) {
                    while (!reader.EndOfStream) {
                        string currentLine = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(currentLine)) {
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
        //随机模式与顺序模式及开关
        private void RandomMode() {
            ResetStaticstic();
            this.toggleIndicatorBottom.ToolTip = "随机模式";
            this.practiceWords.ShuffleWords();
            this.isCurrentWordLocked = false;
            this.isRandomMode = true;
            GetWord();
        }
        private void OrderedMode() {
            ResetStaticstic();
            this.toggleIndicatorBottom.ToolTip = "顺序模式";
            this.practiceWords = new PracticeWords(originWordsList);
            this.isCurrentWordLocked = false;
            this.isRandomMode = false;
            GetWord();
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
        //锁定模式与正常模式及开关
        private void UnlockWordMode() {
            this.timer.Start();
            this.toggleIndicatorTop.ToolTip = "正常模式";
            this.lblCurrentPosition.Content = practiceWords.CurrentWordIndex + 1;
            this.isCurrentWordLocked = false;
        }
        private void LockWordMode() {
            this.timer.Stop();
            this.toggleIndicatorTop.ToolTip = "锁定模式";
            this.lblCurrentPosition.Content = "\u267e";
            this.isCurrentWordLocked = true;
        }
        private void SwitchLockCurrentWord() {
            if (this.isCurrentWordLocked == true) {
                UnlockWordMode();
                this.isCurrentWordLocked = false;
            } else {
                LockWordMode();
                this.isCurrentWordLocked = true;
            }
        }
        //切换沉浸模式
        private void ImmerseMode() {
            this.sliderSeekWordIndex.Visibility = Visibility.Collapsed;
            this.lblCurrentWordMeaning.Visibility = Visibility.Collapsed;
            this.lblPreWord.Visibility = Visibility.Collapsed;
            this.txtPracticeString.Visibility = Visibility.Hidden;
            this.lblNextWord.Visibility = Visibility.Collapsed;
        }
        private void NotableMode() {
            this.sliderSeekWordIndex.Visibility = Visibility.Visible;
            this.lblCurrentWordMeaning.Visibility = Visibility.Visible;
            this.lblNextWord.Visibility = Visibility.Visible;
            this.txtPracticeString.Visibility = Visibility.Visible;
            this.lblPreWord.Visibility = Visibility.Visible;
        }
        private void SwitchImmerseMode() {
            if (isImmerseMode) {
                NotableMode();
            } else {
                ImmerseMode();
            }
            CmpWord();
        }
        //其他
        private void LoadFile() {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "文本文件|*.txt|所有文件|*.*";
            ofd.InitialDirectory = Directory.GetCurrentDirectory();
            if (ofd.ShowDialog() == true) {
                this.currentWordListFile = ofd.FileName;
                GetPracticeWords();
                OrderedMode();
            }
        }
        private void GetWord() {
            this.currentInputWord = "";
            this.seekWordIndex = practiceWords.CurrentWordIndex;
            this.currentPracticeWordDisplay = this.practiceWords.CurrentWord.Word;
            this.lblCurrentWordMeaning.Content = this.practiceWords.CurrentWord.Meaning;
            this.lblPreWord.Content = this.practiceWords.PreWord.Word;
            this.lblNextWord.Content = this.practiceWords.NextWord.Word;
            if (isCurrentWordLocked == false) {
                this.lblCurrentPosition.Content = this.practiceWords.CurrentWordIndex + 1;
            }
        }
        private void CmpWord() {
            this.txtInputString.Foreground = ResDict.PreSetting["ColorTypeStatic"] as SolidColorBrush;
            if (isImmerseMode) {
                if (this.currentInputWord == "") {
                    this.txtPracticeString.Visibility = Visibility.Visible;
                } else {
                    this.txtPracticeString.Visibility = Visibility.Hidden;
                }
            } else {
                StringBuilder tempWord = new StringBuilder(this.currentPracticeWord.Length);
                for (int i = 0; i < this.currentInputWord.Length; i++) {
                    tempWord.Append(" ");
                    if (this.currentInputWord[i] != this.currentPracticeWord[i]) {
                        this.txtInputString.Foreground = ResDict.PreSetting["ColorTyping"] as SolidColorBrush;
                    }
                }
                tempWord.Append(this.currentPracticeWord.Substring(this.currentInputWord.Length));
                this.currentPracticeWordDisplay = tempWord.ToString();
            }
        }
        private void Timer_Tick(object sender, EventArgs e) {
            ++usingTime;
            this.toggleIndicatorTop.Content = usingTime;
        }
    }
    #endregion
}
