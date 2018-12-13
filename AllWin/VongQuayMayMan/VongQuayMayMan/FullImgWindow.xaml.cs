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
        List<DoubleAnimation> lsAnimation;
        
        UserImg mainImg;
        UserImg luckyImg;
        int gridSize;
        public int RotateMode;
        public bool isRemove;

        DispatcherTimer rotateTimer;
        DispatcherTimer Awardtimer;

        bool isStopFade;

        int pre, next;
        public FullImgWindow(List<String> ls, int rMode = 2, bool rem = false)
        {
            InitializeComponent();
            imgSources = new List<string>();
            lsImgs = new List<UserImg>();
            lsAnimation = new List<DoubleAnimation>();

            isStopFade = true;
            isRemove = rem;
            RotateMode = rMode;

            rotateTimer = new DispatcherTimer();
            rotateTimer.Tick += RotateTimer_Tick;
            rotateTimer.Interval = TimeSpan.FromMilliseconds(30);
            foreach (var src in ls)
            {
                imgSources.Add(src);
            }
            InitGrid();
            
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
                    {
                        next = 0;
                    }
                    while (lsImgs[next] == null) {
                        next++;
                        if (next >= lsImgs.Count)
                        next = 0;
                    }
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

        private void fifo()
        {
            if (isStopFade)
                return;
            Random rand = new Random();
            int i = rand.Next(lsImgs.Count);
            while (lsImgs[i] == null || lsImgs[i].duringAnimation)
            {
                i = rand.Next(lsImgs.Count);
            }
            var animation = new DoubleAnimation
            {
                To = 0,
                BeginTime = TimeSpan.FromSeconds(rand.Next(3)),
                FillBehavior = FillBehavior.Stop,
                Duration = new Duration(TimeSpan.FromMilliseconds(3000)),
                AutoReverse = true
            };
            animation.Completed += (x, y) =>
            {
                lsImgs[i].duringAnimation = false;
                fifo();
            };
            lsImgs[i].duringAnimation = true;
            lsImgs[i].BeginAnimation(UIElement.OpacityProperty, animation);
        }
        //private void Timer_Tick(object sender, EventArgs e)
        //{
        //    Random rand = new Random();
        //    int trytime = 0;
        //    int i = rand.Next(lsImgs.Count);

        //    while (lsImgs[i] == null || lsImgs[i].duringAnimation) {
        //        trytime++;
        //        if (trytime > 100) return;
        //        i = rand.Next(lsImgs.Count);
        //    }
        //    trytime = 0;
        //    int j = rand.Next(lsImgs.Count);
        //    while (i == j || lsImgs[j] == null || lsImgs[j].duringAnimation) {
        //        trytime++;
        //        if (trytime > 100) return;
        //        j = rand.Next(lsImgs.Count);
        //    }
        //    String temp = lsImgs[i].imgPath;

        //    lsImgs[i].changeImgSource(lsImgs[j].imgPath, 0, 3000);
        //    lsImgs[j].changeImgSource(temp, 0, 3000);
        //}

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
                isStopFade = false;
                for (int i = 0; i < lsImgs.Count / 2; i++)
                {
                    fifo();
                }
            }
        }

        public void startRotate()
        {
            endRotation();
            isStopFade = true;
            foreach(var item in lsImgs)
            {
                if (item != null)
                {
                    item.BeginAnimation(UIElement.OpacityProperty, null);
                }
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
            if (!rotateTimer.IsEnabled)
            {
                return;
            }
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
                Awardtimer.Interval = TimeSpan.FromSeconds(2);
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
            
            luckyImg = new UserImg(lsImgs[next].imgPath, 1000);


            luckyImg.SetValue(Grid.RowProperty, 0);
            luckyImg.SetValue(Grid.ColumnProperty, 0);
            luckyImg.SetValue(Grid.RowSpanProperty, gridSize);
            luckyImg.SetValue(Grid.ColumnSpanProperty, gridSize);
            grid.Children.Add(luckyImg);
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void endRotation()
        {
            if (luckyImg == null)
            {
                return;
            }
            grid.Children.Remove(luckyImg);
            luckyImg = null;
            if (isRemove)
            {
                string removePath = lsImgs[next].imgPath;
                for (int i = 0; i < lsImgs.Count; i++)
                {
                    if (lsImgs[i] != null && lsImgs[i].imgPath.Equals(removePath))
                    {
                        lsImgs[i] = null;
                    }
                }
                Random ran = new Random();

                while (lsImgs[next] == null)
                    next = ran.Next(lsImgs.Count);
                pre = next;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                endRotation();
            }
        }
    }
}
