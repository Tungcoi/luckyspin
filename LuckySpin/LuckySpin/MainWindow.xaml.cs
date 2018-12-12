using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LuckySpin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        int animationTime = 500;
        int animationDelay = 100;
        int pos;
        List<Image> ls;
        List<String> imgs;
        Image mainImg;
        bool stop;
        public MainWindow()
        {
            InitializeComponent();
            ls = new List<Image>();
            ls.Clear();
            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(30);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            var mainUri = new Uri("file:///" + imgs[pos]);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = mainUri;
            bitmap.DecodePixelWidth = 600;
            bitmap.EndInit();
            mainImg.Source = bitmap;
            int pre = pos - 1;
            if (pre < 0)
                pre = ls.Count - 1;
            ls[pre].Visibility = System.Windows.Visibility.Visible;
            ls[pos].Visibility = System.Windows.Visibility.Collapsed;
            pos++;
            if (pos >= ls.Count)
                pos = 0;
           
            
        }

      

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == true)
            {
                imgs = new List<string>();
                foreach (var path in dlg.FileNames)
                {
                    imgs.Add(path);
                    //var uri = new Uri("file:///" + path);
                    //var bitmap = new BitmapImage();
                    //bitmap.BeginInit();
                    //bitmap.UriSource = uri;
                    //bitmap.DecodePixelWidth = 200;
                    //bitmap.EndInit();
                    //Image img = new Image();
                    //img.Source = bitmap;
                    //img.SetValue(Grid.RowProperty, count / 15);
                    //img.SetValue(Grid.ColumnProperty, count % 15);
                    //img.SetValue(Grid.RowSpanProperty, 1);
                    //img.SetValue(Grid.ColumnSpanProperty, 1);
                    //Thickness mg = new Thickness();
                    //mg.Bottom = 1;
                    //mg.Top = 1;
                    //mg.Left = 1;
                    //mg.Right = 1;
                    //img.Margin = mg;
                    //count++;
                    //grid.Children.Add(img);
                    //ls.Add(img);
                    //if (count > 200) break;
                }
                calculateGrid(imgs.Count);
            }
            

        }

        private void calculateGrid(int imgc)
        {
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            grid.Children.Clear();
            int i = 3;
            while ((i * i * 8 / 9) < imgc) i += 3;
            for (int j = 0; j < i; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
            }

            //add main img
            mainImg = new Image();
            mainImg.SetValue(Grid.RowProperty, i / 3);
            mainImg.SetValue(Grid.ColumnProperty, i / 3);
            mainImg.SetValue(Grid.RowSpanProperty, i / 3);
            mainImg.SetValue(Grid.ColumnSpanProperty, i / 3);
            Thickness mg = new Thickness();
            mg.Bottom = 1;
            mg.Top = 1;
            mg.Left = 1;
            mg.Right = 1;
            mainImg.Margin = mg;
            grid.Children.Add(mainImg);
            int fillpos = 0;
            for (int r = 0; r < i; r++)
            {
                for (int c = 0; c < i; c++)
                {
                    if (!(((r >= i / 3) && (r < i / 3 * 2)) && ((c >= i / 3) && (c < i / 3 * 2))))
                    {
                       

                        Uri uri;
                        if (fillpos < imgs.Count)
                        {
                            uri = new Uri("file:///" + imgs[fillpos]);
                        }
                        else
                        {
                            uri = new Uri("file:///" + "C:\\Images\\face.png");
                        }
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = uri;
                        bitmap.DecodePixelWidth = 100;
                        bitmap.EndInit();
                        Image img = new Image();
                        img.Source = bitmap;
                        img.SetValue(Grid.RowProperty, r);
                        img.SetValue(Grid.ColumnProperty, c);
                        img.SetValue(Grid.RowSpanProperty, 1);
                        img.SetValue(Grid.ColumnSpanProperty, 1);
                        Thickness magin = new Thickness();
                        magin.Bottom = 1;
                        magin.Top = 1;
                        magin.Left = 1;
                        magin.Right = 1;
                        img.Margin = magin;
                        grid.Children.Add(img);
                        if (fillpos < imgs.Count)
                        {
                            ls.Add(img);
                        }
                        fillpos++;
                        
                    }
                }
            }
        }

        

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            pos = 0;
            stop = false;
            timer.Start();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            stop = true;
            timer.Stop();
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
                pos--;
                if (pos < 0)
                    pos = ls.Count - 1;
                animation.Duration = TimeSpan.FromMilliseconds(100);
                animation.AutoReverse = true;
                animation.RepeatBehavior = RepeatBehavior.Forever;
            }
            
            if (pos >= ls.Count)
                pos = 0;
            var mainUri = new Uri("file:///" + imgs[pos]);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = mainUri;
            bitmap.DecodePixelWidth = 600;
            bitmap.EndInit();
            mainImg.Source = bitmap;
            ls[pos].BeginAnimation(UIElement.OpacityProperty, animation);
            pos++;
        }
    }
}
