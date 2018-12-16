using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LuckySpiner
{
    /// <summary>
    /// Interaction logic for SpinerWindow.xaml
    /// </summary>
    public partial class SpinerWindow : Window
    {

        public static readonly DependencyProperty ScrollOffsetProperty =
        DependencyProperty.Register("ScrollOffset", typeof(double), typeof(SpinerWindow),
            new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnScrollOffsetChanged)));

        public double ScrollOffset
        {
            get { return (double)GetValue(ScrollOffsetProperty); }
            set { SetValue(ScrollOffsetProperty, value); }
        }

        private static void OnScrollOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SpinerWindow myObj = obj as SpinerWindow;

            if (myObj != null)
                myObj.scroll.ScrollToHorizontalOffset(myObj.ScrollOffset);
        }


        private string mainPath;

        DoubleAnimation rotateAnimation;
        Storyboard storyboard;

        private int btnMode;
        private int rotateMode;

        DispatcherTimer countDownTimer;
        private int countDownValue;

        private bool isRotate;
        //SoundPlayer player;
        //private bool isplayingSound;

        private int totalImgSize;
        List<Grid> grids;
        List<ImgIfo> gridsbinding;

        DispatcherTimer rotateTimer;
        private int curentPos, nextPos;

        private string logoPath;

        public SpinerWindow(string lgpth, String bg, List<String> imgsPth, int size)
        {
            InitializeComponent();
            BitmapImage bi = new BitmapImage(new Uri(bg));
            background.ImageSource = bi;
            lsImgInfo = new List<ImgIfo>();
            foreach (var pth in imgsPth)
            {
                Image img = new Image();
                BitmapImage bmi = new BitmapImage();
                bmi.BeginInit();
                bmi.DecodePixelHeight = 300;
                bmi.UriSource = new Uri(pth);
                bmi.EndInit();
                img.Source = bmi;
                lsStack.Children.Add(img);

                ImgIfo info = new ImgIfo();
                info.path = pth;
                info.imgView = img;
                lsImgInfo.Add(info);
            }

            //gen grid opacity
            totalImgSize = size;
            grids = new List<Grid>();
            gridsbinding = new List<ImgIfo>();
            for (int i = 0; i < totalImgSize; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
            }
            int temp = 0;
            for (int i = 0; i < totalImgSize; i++)
            {
                for (int j = 0; j < totalImgSize; j++)
                {
                    Grid gridElement = new Grid();
                    gridElement.Background = Brushes.Black;
                    gridElement.Opacity = 0.5;
                    gridElement.SetValue(Grid.RowProperty, i);
                    gridElement.SetValue(Grid.ColumnProperty, j);
                    gridElement.SetValue(Grid.RowSpanProperty, 1);
                    gridElement.SetValue(Grid.ColumnSpanProperty, 1);
                    grids.Add(gridElement);
                    grid.Children.Add(gridElement);
                    if (i >= (totalImgSize / 4) && i < (totalImgSize * 3 / 4) && j >= (totalImgSize / 4) && j < (totalImgSize * 3 / 4)) //mainImage 
                    {
                        gridsbinding.Add(null);
                    } else
                    {
                        gridsbinding.Add(lsImgInfo[temp]);
                        temp = (temp + 1) % lsImgInfo.Count;
                    }
                }
            }
            mainPath = null;
            scroll.ScrollChanged += Scroll_ScrollChanged;

            rotateAnimation = new DoubleAnimation();
            storyboard = new Storyboard();
            storyboard.Children.Add(rotateAnimation);
            Storyboard.SetTarget(rotateAnimation, this);
            Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath(SpinerWindow.ScrollOffsetProperty)); // Attached dependency property
            //rotateAnimation.Completed += RotateAnimation_Completed;
            rotateAnimation.AutoReverse = true;
            rotateAnimation.RepeatBehavior = RepeatBehavior.Forever;
            btnMode = 0;
            rotateMode = -1;

            countDownTimer = new DispatcherTimer();
            countDownTimer.Tick += CountDownTimer_Tick;
            countDownTimer.Interval = TimeSpan.FromSeconds(1);
            countDownValue = 5;

            //player = new SoundPlayer(@"C:\Users\tuenh\Downloads\tick.wav");
            //player.Load();
            //isplayingSound = false;

            rotateTimer = new DispatcherTimer();
            rotateTimer.Tick += RotateTimer_Tick;
            rotateTimer.Interval = TimeSpan.FromMilliseconds(30);
            curentPos = 0;
            nextPos = 0;

            logoPath = lgpth;
            BitmapImage logobmp = new BitmapImage();
            logobmp.BeginInit();
            logobmp.DecodePixelHeight = 400;
            logobmp.UriSource = new Uri(logoPath);
            logobmp.EndInit();
            mainImg.Source = logobmp;
        }

        private void RotateTimer_Tick(object sender, EventArgs e)
        {
            if (!isRotate || !getNext()) return;
            grids[curentPos].Visibility = Visibility.Visible;
            grids[nextPos].Visibility = Visibility.Hidden;
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;
            mainPath = gridsbinding[nextPos].path;
            bw.RunWorkerAsync(gridsbinding[nextPos].path);
            curentPos = nextPos;
        }

        private bool getNext()
        {
            switch (rotateMode)
            {
                case 0: //random
                    Random rd = new Random();
                    do
                    {
                        nextPos = rd.Next(totalImgSize * totalImgSize);
                    } while (nextPos == curentPos || gridsbinding[nextPos] == null || gridsbinding[nextPos].isLucky);
                    return true;
                case 1: //lef to right
                    nextPos = (curentPos + 1) % (totalImgSize* totalImgSize);
                    while (gridsbinding[nextPos] == null || gridsbinding[nextPos].isLucky)
                    {
                        nextPos = (nextPos + 1) % (totalImgSize * totalImgSize);
                    }
                    return true;
                case 2: //Right to left
                    nextPos = curentPos;
                    do
                    {
                        nextPos--;
                        if (nextPos < 0)
                            nextPos = totalImgSize * totalImgSize - 1;
                    } while (gridsbinding[nextPos] == null || gridsbinding[nextPos].isLucky);
                  
                    return true;
                
                case 3: //up
                    nextPos = curentPos;
                    do
                    {
                        if (nextPos == 0)
                            nextPos = totalImgSize * totalImgSize - 1;
                        else if (nextPos < totalImgSize)
                            nextPos += totalImgSize * (totalImgSize - 1) - 1;
                        else
                            nextPos -= totalImgSize;
                    } while (gridsbinding[nextPos] == null || gridsbinding[nextPos].isLucky);
                    return true;
                case 4: //down
                    nextPos = curentPos;
                    do
                    {
                        if (nextPos == totalImgSize * totalImgSize - 1)
                            nextPos = 0;
                        else if (nextPos >= totalImgSize * (totalImgSize - 1))
                            nextPos -= totalImgSize * (totalImgSize - 1) - 1;
                        else
                            nextPos += totalImgSize;
                    } while (gridsbinding[nextPos] == null || gridsbinding[nextPos].isLucky);
                    return true;
                default:
                    return false;
            }
            
        }

        private void CountDownTimer_Tick(object sender, EventArgs e)
        {
            countDownValue--;
            Uri uri;
            switch (countDownValue)
            {
                case 4://show 5 image
                    uri = new Uri(@"pack://application:,,,/Resources/5.png");
                    break;
                case 3:
                    uri = new Uri(@"pack://application:,,,/Resources/4.png");
                    break;
                case 2:
                    uri = new Uri(@"pack://application:,,,/Resources/3.png");
                    break;
                case 1:
                    uri = new Uri(@"pack://application:,,,/Resources/2.png");
                    break;
                case 0:
                    uri = new Uri(@"pack://application:,,,/Resources/1.png");
                    break;
                case -1:
                    btnMode = 0;

                    stopRotate();
                    
                    BackgroundWorker bw = new BackgroundWorker();
                    bw.DoWork += Bw_DoWork1;
                    bw.RunWorkerAsync(mainPath);

                    countDownTimer.Stop();
                    cdImg.Visibility = Visibility.Hidden;
                    return;
                default:
                    countDownTimer.Stop();
                    return;
            }
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelHeight = 400;
            bi.UriSource = uri;
            bi.EndInit();
            cdImg.Source = bi;
            cdImg.Visibility = Visibility.Visible;
        }

        private void Scroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!isRotate)
            {
                return;
            }
            string newpth = getPath(e.HorizontalOffset);
            if (newpth == null || string.Equals(newpth, mainPath))
                return;
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += Bw_DoWork;
            mainPath = newpth;
            bw.RunWorkerAsync(newpth);

        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            String newpth = e.Argument as String;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = 600;
            bi.UriSource = new Uri(newpth);
            bi.EndInit();
            bi.Freeze();
            Dispatcher.Invoke(() => mainImg.Source = bi);
            //player.Stop();
            //player.Play();
        }

        private string getPath(double horizontalOffset)
        {
            foreach (var item in lsImgInfo)
            {
                if (item.startOffset <= horizontalOffset && item.endOffset >= horizontalOffset && !item.isLucky)
                {
                    return item.path;
                }
            }
            return null;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MainWindow mw = new MainWindow();
            mw.Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            calculatePosition();
            freshAnimation();
        }

        private bool isCalculate = false;
        public class ImgIfo
        {
            public string path { set; get; }
            public double startOffset { set; get; }
            public double endOffset { set; get; }
            public int position { set; get; }
            public Image imgView { set; get; }
            public bool isLucky { set; get; }
        }

        List<ImgIfo> lsImgInfo;
        private void calculatePosition()
        {
            if (isCalculate)
                return;
            isCalculate = true;
            double offset = 0;
            for (int i = 0; i < lsStack.Children.Count; i++)
            {
                ImgIfo info = lsImgInfo[i];
                info.startOffset = offset;
                offset += info.imgView.ActualWidth;
                info.endOffset = offset;
                info.position = i;
                info.isLucky = false;
            }
            double neededwidth = scroll.ViewportWidth;
            int posadd = 0;
            while (neededwidth > 0)
            {
                neededwidth -= lsImgInfo[posadd].imgView.ActualWidth;
                Image img = new Image();
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.DecodePixelHeight = 400;
                bi.UriSource = new Uri(lsImgInfo[posadd].path);
                bi.EndInit();
                img.Source = bi;
                lsStack.Children.Add(img);
                posadd++;
            }
        }

        private void freshAnimation()
        {
            int duration = (int)(scroll.ExtentWidth / scroll.ViewportWidth * 5000);

            rotateAnimation.From = 0;
            rotateAnimation.To = scroll.ExtentWidth;
            rotateAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(duration));

            storyboard.Begin();
        }

        private void scrollAnimation()//time per page
        {
            storyboard.Stop();
            int duration = (int)(scroll.ExtentWidth / scroll.ViewportWidth * 300);
            
            rotateAnimation.From = 0;
            rotateAnimation.To = scroll.ExtentWidth;
            rotateAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(duration));

            storyboard.Begin();
        }


        private void startRotate()
        {
            Random rad = new Random();
            int temp = rad.Next(6);
            while (temp == rotateMode)
                temp = rad.Next(6);
            rotateMode = temp;
            isRotate = true;
            switch (rotateMode)
            {
                case 0: //random
                case 1: //lef to right
                case 2: //Right to left
                case 3: //up
                case 4: //down
                    scroll.Visibility = Visibility.Hidden;
                    storyboard.Stop();
                    rotateTimer.Start();
                    break;
                case 5: //scroll
                    scrollAnimation();
                    break;
                default:
                    break;
            }
            
        }

        private void stopRotate()
        {
            isRotate = false;
            switch (rotateMode)
            {
                case 0: //random
                case 1: //lef to right
                case 2: //Right to left
                case 3: //up
                case 4: //down
                    grids[curentPos].Visibility = Visibility.Visible;
                    rotateTimer.Stop();
                    break;
                case 5: //scroll
                    storyboard.Stop();
                    scroll.Visibility = Visibility.Hidden;
                    break;
                default:
                    break;
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (btnMode == 0)
            {
                btnMode = 1;
                rotateBtn.Content = "Ngừng quay";
                startRotate();
            }
            else
            {
                rotateBtn.Visibility = Visibility.Hidden;
                rotateBtn.Content = "Quay thưởng";
                countDownValue = 5;
                countDownTimer.Start();
            }
           
        }

        private void Bw_DoWork1(object sender, DoWorkEventArgs e)
        {
            String newpth = e.Argument as String;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = 600;
            bi.UriSource = new Uri(newpth);
            bi.EndInit();
            bi.Freeze();
            Dispatcher.Invoke(() => awardImg.Source = bi);
            foreach(var item in lsImgInfo)
            {
                if (string.Equals(newpth, item.path))
                {
                    item.isLucky = true;
                    break;
                }
            }
            System.Threading.Thread.Sleep(1000);
            Dispatcher.Invoke(() => awardImg.Visibility = Visibility.Visible);
            Dispatcher.Invoke(() => winner.Visibility = Visibility.Visible);
            BitmapImage bi2 = new BitmapImage();
            bi2.BeginInit();
            bi2.DecodePixelHeight = 400;
            bi2.UriSource = new Uri(logoPath);
            bi2.EndInit();
            bi2.Freeze();
            Dispatcher.Invoke(() => mainImg.Source = bi2);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                hideLucky();
            }
        }

        private void hideLucky()
        {
            awardImg.Visibility = Visibility.Hidden;
            winner.Visibility = Visibility.Hidden;
            rotateBtn.Visibility = Visibility.Visible;
            scroll.Visibility = Visibility.Visible;
            freshAnimation();
        }
        private void awardImg_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount >= 2)
            {
                hideLucky();
            }
        }
      
    }

}
