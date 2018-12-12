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

namespace LuckySpin
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            Uri uri = new Uri("file:///" + @"C:\Temp\logo.png");
            BitmapImage bm = new BitmapImage(uri);
            img.Source = bm;

            var animation = new DoubleAnimation
            {
                To = 0,
                BeginTime = TimeSpan.FromSeconds(3),
                FillBehavior = FillBehavior.Stop,
                Duration = new Duration(TimeSpan.FromMilliseconds(5000))
            };
            animation.Completed += (x, y) =>
            {
                Uri uri1 = new Uri("file:///" + @"C:\Images\face.png");
                BitmapImage bm1 = new BitmapImage(uri1);
                img.Source = bm1;
                img.Opacity = 1;
            };
            img.BeginAnimation(UIElement.OpacityProperty, animation);

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            img.BeginAnimation(UIElement.OpacityProperty, null);
        }
    }
}
