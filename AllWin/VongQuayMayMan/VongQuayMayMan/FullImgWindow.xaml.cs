using System;
using System.Collections.Generic;
using System.Linq;
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

namespace VongQuayMayMan
{
    /// <summary>
    /// Interaction logic for FullImgWindow.xaml
    /// </summary>
    public partial class FullImgWindow : Window
    {
        List<String> imgSources;
        List<UserImg> lsImgs;
        UserImg mainImg;
        int gridSize;
        public int RotateMode;

        DispatcherTimer timer;
        DispatcherTimer rotateTimer;
        DispatcherTimer Awardtimer;

        int pre, next;
        public FullImgWindow(List<String> ls)
        {
            InitializeComponent();
            imgSources = new List<string>();
            lsImgs = new List<UserImg>();
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(700);

            rotateTimer = new DispatcherTimer();
            rotateTimer.Tick += RotateTimer_Tick;
            rotateTimer.Interval = TimeSpan.FromMilliseconds(30);
            foreach (var src in ls)
            {
                imgSources.Add(src);
            }
            InitGrid();
            RotateMode = 3;
            pre = 0;
            next = 0;
        }

        private void GenNext()
        {
            switch (RotateMode)
            {
                case 1: //xoan oc
                    break;
                case 2: //ngau nhien
                    Random rand = new Random();
                    next = rand.Next(lsImgs.Count);
                    while (next == pre || lsImgs[next] == null) next = rand.Next(lsImgs.Count);
                    break;
                case 3: //trai phai
                    next = pre + 1;
                    if (next >= lsImgs.Count)
                        next = 0;
                    while (lsImgs[next] == null) next++;
                    break;
                case 4: //tren duoi
                    //next = pre + gridSize;
                    //if (ne)
                    //hile(lsImgs[next] == null) next+= gridSize;
                    break;
                default:
                    break;
            }
        }
        private void RotateTimer_Tick(object sender, EventArgs e)
        {
            GenNext();
            NextImage();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Random rand = new Random();
            int trytime = 0;
            int i = rand.Next(lsImgs.Count);

            while (lsImgs[i] == null || lsImgs[i].duringAnimation) {
                trytime++;
                if (trytime > 100) return;
                i = rand.Next(lsImgs.Count);
            }
            trytime = 0;
            int j = rand.Next(lsImgs.Count);
            while (i == j || lsImgs[j] == null || lsImgs[j].duringAnimation) {
                trytime++;
                if (trytime > 100) return;
                j = rand.Next(lsImgs.Count);
            }
            String temp = lsImgs[i].imgPath;

            lsImgs[i].changeImgSource(lsImgs[j].imgPath, 0, 3000);
            lsImgs[j].changeImgSource(temp, 0, 3000);
        }

        private void InitGrid()
        {
            //clear data
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            grid.Children.Clear();

            //calculate size
            gridSize = 4;
            while ((gridSize * gridSize * 3 / 4) < imgSources.Count) gridSize += 4;

            //add grid column and row
            for (int i = 0; i < gridSize; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
            }

            //create main image
            mainImg = new UserImg(null, 600);
            mainImg.SetValue(Grid.RowProperty, gridSize / 4);
            mainImg.SetValue(Grid.ColumnProperty, gridSize / 4);
            mainImg.SetValue(Grid.RowSpanProperty, gridSize / 2);
            mainImg.SetValue(Grid.ColumnSpanProperty, gridSize / 2);
            grid.Children.Add(mainImg);

            //fill image to grid
            int count = 0;
            for (int i= 0; i < gridSize; i ++)
            {
                for (int j = 0; j < gridSize; j ++)
                {
                    if (( (i >= gridSize / 4) && (i < gridSize * 3 / 4) ) && ((j >= gridSize / 4) && (j < gridSize * 3 / 4)) )
                    {
                        lsImgs.Add(null);
                    } else
                    {
                        UserImg item = new UserImg(imgSources[count]);
                        item.SetValue(Grid.RowProperty, i);
                        item.SetValue(Grid.ColumnProperty, j);
                        item.SetValue(Grid.RowSpanProperty, 1);
                        item.SetValue(Grid.ColumnSpanProperty, 1);
                        grid.Children.Add(item);
                        count++;
                        if (count >= imgSources.Count)
                            count = 0;
                        lsImgs.Add(item);
                    }
                    
                }
            }


        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        public void startRotate()
        {
            foreach(var item in lsImgs)
            {
                if (item != null)
                {
                    item.stopAnimation();
                }
            }
            if (timer.IsEnabled)
            {
                timer.Stop();
            }
            if (rotateTimer.IsEnabled)
            {
                rotateTimer.Stop();
            }
            rotateTimer.Start();
        }

        private void NextImage()
        {
            mainImg.changeSource(lsImgs[next].imgPath);
            if (pre >= 0 && pre < lsImgs.Count && lsImgs[pre] != null)
            {
                lsImgs[pre].Visibility = System.Windows.Visibility.Visible;
            }
            if (next >= 0 && next < lsImgs.Count && lsImgs[next] != null)
            {
                lsImgs[next].Visibility = System.Windows.Visibility.Collapsed;
            }
            pre = next;
        }
        int animationDelay;
        int animationTime;

        public void StopRotate()
        {
            rotateTimer.Stop();
            lsImgs[next].Visibility = System.Windows.Visibility.Visible;
            animationDelay = 100;
            animationTime = 50;
            animation_Completed(null, null);
        }

        void animation_Completed(object sender, EventArgs e)
        {

            animationDelay *= 2;
            animationTime += animationDelay;


            var animation = new DoubleAnimation
            {
                To = 0,
                BeginTime = TimeSpan.FromSeconds(0),
                FillBehavior = FillBehavior.Stop
            };
            if (animationTime < 3200)
            {
                animation.Duration = TimeSpan.FromMilliseconds(animationTime);
                animation.Completed += animation_Completed;
            }
            else
            { 
                animation.Duration = TimeSpan.FromMilliseconds(100);
                animation.AutoReverse = true;
                animation.RepeatBehavior = RepeatBehavior.Forever;
                Awardtimer = new DispatcherTimer();
                Awardtimer.Tick += Awardtimer_Tick;
                Awardtimer.Interval = TimeSpan.FromSeconds(4);
                Awardtimer.Start();
            }
            
            GenNext();
            mainImg.changeSource(lsImgs[next].imgPath);
            lsImgs[next].BeginAnimation(UIElement.OpacityProperty, animation);
            pre = next;
        }

        private void Awardtimer_Tick(object sender, EventArgs e)
        {
            lsImgs[next].BeginAnimation(UIElement.OpacityProperty, null);
            Awardtimer.Stop();
        }
    }
}
