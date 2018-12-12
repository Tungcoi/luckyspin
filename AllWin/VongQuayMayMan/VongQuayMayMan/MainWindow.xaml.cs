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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VongQuayMayMan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FullImgWindow fWd;
        public MainWindow()
        {
            InitializeComponent();
            fWd = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (fWd != null)
            {
                var result = MessageBox.Show("Bạn muốn chọn số ảnh khác?", "Màn hình quay thưởng đang thực thi", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    fWd.Close();
                    fWd = null;
                } else
                {
                    return;
                }
            }

            var dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == true)
            {
                List<String> imgs = new List<string>();
                foreach (var path in dlg.FileNames)
                {
                    imgs.Add(path);
                }
                fWd = new FullImgWindow(imgs);
                fWd.Show();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (fWd != null)
            {
                fWd.Close();
                fWd = null;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (fWd != null)
            {
                fWd.startRotate();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (fWd != null)
            {
                fWd.StopRotate();
            }
        }
    }
}
