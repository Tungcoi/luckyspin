using System;
using System.Collections.Generic;
using System.IO;
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

namespace VongQuayMayMan
{
    /// <summary>
    /// Interaction logic for UserImg.xaml
    /// </summary>
    public partial class UserImg : UserControl
    {
        public int xPos;
        public int yPos;
        public bool duringAnimation;
        public String imgPath;
        int imgpxl;

        public UserImg(String path, int pxw = 200)
        {
            InitializeComponent();
            imgPath = path;
            imgpxl = pxw;
            duringAnimation = false;
        }

        public void changeImgSource(String path, int startTime, int duration)
        {
            //if (String.IsNullOrEmpty(path))
            //    return;
            duringAnimation = true;
            var animation = new DoubleAnimation
            {
                To = 0,
                BeginTime = TimeSpan.FromSeconds(startTime),
                FillBehavior = FillBehavior.Stop,
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                AutoReverse = true
            };
            //animation.Completed += (x, y) =>
            //{
            //    var uri = new Uri("file:///" + path);
            //    var bitmap = new BitmapImage();
            //    bitmap.BeginInit();
            //    bitmap.UriSource = uri;
            //    bitmap.DecodePixelWidth = imgpxl;
            //    bitmap.EndInit();
            //    img.Opacity = 0;
            //    img.Source = bitmap;
            //    imgPath = path;

            //    var animationback = new DoubleAnimation
            //    {
            //        To = 1,
            //        BeginTime = TimeSpan.FromSeconds(0),
            //        FillBehavior = FillBehavior.Stop,
            //        Duration = new Duration(TimeSpan.FromMilliseconds(duration))
            //    };

            //    animationback.Completed += (i, j) =>
            //    {
            //        img.Opacity = 1;
            //        duringAnimation = false;
            //    };
            //    img.BeginAnimation(UIElement.OpacityProperty, animationback);

            //};
            animation.Completed += (x, y) =>
            {
                img.Opacity = 1;
            };

            img.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(imgPath))
                return;
            try
            {
                var uri = new Uri("file:///" + imgPath);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.DecodePixelWidth = imgpxl;
                bitmap.EndInit();
                bitmap.Freeze();
                img.Source = bitmap;
            } catch (Exception ex)
            {
                MessageBox.Show("Không thể load ảnh " + imgPath);
                string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string[] lines = { DateTime.Now.ToString(), ex.Message };
                File.AppendAllLines(System.IO.Path.Combine(mydocpath, "LuckySprinErrorLog.txt"), lines);
            }
           
        }

        public void changeSource(String path)
        {
            try
            {
                var uri = new Uri("file:///" + path);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.DecodePixelWidth = imgpxl;
                bitmap.EndInit();
                img.Source = bitmap;
                imgPath = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể load ảnh " + imgPath);
                string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string[] lines = { DateTime.Now.ToString(), ex.Message };
                File.AppendAllLines(System.IO.Path.Combine(mydocpath, "LuckySprinErrorLog.txt"), lines);
            }
        }

        public void stopAnimation()
        {
            img.BeginAnimation(UIElement.OpacityProperty, null);
        }
    }
}
