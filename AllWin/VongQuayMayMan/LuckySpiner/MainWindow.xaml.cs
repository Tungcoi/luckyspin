using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LuckySpiner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string bgPath;
        int imgSize;
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            bgPath = "";
            imgSize = -1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bgPath = "";
            var dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == true)
            {
                List<String> imgs = new List<string>();
                foreach (var path in dlg.FileNames)
                {
                    imgs.Add(path);
                }

                if (imgs.Count < 30)
                {
                    MessageBox.Show("Số lượng ảnh quá ít.", " Không thể khởi tạo chương trình!!!", MessageBoxButton.OK);
                    return;
                }
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += Worker_DoWork;
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                worker.ProgressChanged += Worker_ProgressChanged;
                worker.WorkerReportsProgress = true;
                worker.RunWorkerAsync(imgs);
            }
            
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker == null)
                return;
            switch (e.ProgressPercentage)
            {
                case 0:
                    btn.Content = "Vui lòng chọn ảnh";
                    
                    btn.IsEnabled = true;
                    break;
                case 1:
                    btn.IsEnabled = false;
                    btn.Content = "Đang phân tích ảnh đầu vào " + e.UserState.ToString();
                    break;
                case 2:
                    btn.IsEnabled = false;
                    btn.Content = "Đang ghép ảnh đầu vào thành ảnh nền.\nVui lòng đợi!!!";
                    break;
                case 3:
                    btn.Content = "Đang ghi ảnh nền. Vui lòng đợi!!!";
                    btn.IsEnabled = false;
                    break;
                case 4:
                    btn.Content = "Đang khởi tạo cửa sổ mới. Vui lòng đợi!!!";
                    btn.IsEnabled = false;
                    bgPath = e.UserState.ToString();
                    break;
                //case 4:
                //    btn.Content = "Đang khởi tạo cửa sổ mới. Vui lòng đợi!!!";
                //    btn.IsEnabled = false;
                //    bgPath = e.UserState.ToString();
                //    break;
                case 100:
                    MessageBox.Show("Vui lòng kiểm tra lại ảnh:" + e.UserState.ToString());
                    this.Close();
                    break;
            }
                
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (String.IsNullOrEmpty(bgPath))
                return;
            List<String> data = e.Result as List<String>;
            SpinerWindow wd = new SpinerWindow(logoPath,bgPath, data, imgSize);
            wd.Show();
            this.Close();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker == null)
                return;
            List<string> imgsPth = e.Argument as List<string>;
            //make background image
            int imgWidth = 3840;
            int imgHeight = 2160;
            imgSize = 4;
            while ((imgSize * imgSize * 3 / 4) < imgsPth.Count) imgSize+= 4;
            int tileWidth = imgWidth / imgSize;
            int tileHeight = imgHeight / imgSize;
            DrawingVisual drawingVisual = new DrawingVisual();
            int pos = 0;
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                for (int i = 0; i < imgSize; i++)
                {
                    for (int j = 0; j < imgSize; j++)
                    {
                        //int pos = (i * imgSize + j) % imgsPth.Count;
                        if (i >= (imgSize / 4) && i < (imgSize * 3 / 4) && j >= (imgSize / 4) && j < (imgSize * 3 / 4)) //mainImage 
                            continue;
                        if (pos < imgsPth.Count) 
                            worker.ReportProgress(1, String.Format(" - {0}({1}/{2})", System.IO.Path.GetFileName(imgsPth[pos]), pos + 1, imgSize * imgSize));
                        string pth = imgsPth[pos];
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.DecodePixelWidth = tileWidth;
                        bi.DecodePixelHeight = tileHeight;
                        try
                        {
                            bi.UriSource = new Uri(pth);
                            bi.EndInit();
                        }
                        catch (Exception ex)
                        {
                            string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                            // Append text to an existing file named "WriteLines.txt".
                            using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(mydocpath, "LuckySpinerLog.txt"), true))
                            {
                                outputFile.WriteLine(DateTime.Now.ToString());
                                outputFile.WriteLine(ex.Message);
                                drawingContext.Close();
                                worker.ReportProgress(100, pth);
                                return;
                            }
                        }
                        drawingContext.DrawImage(bi, new Rect(i * tileWidth + 1, j * tileHeight + 1, tileWidth - 2, tileHeight - 2));
                        GC.Collect();
                        pos = (pos  + 1) % imgsPth.Count;
                    }
                }
            }

            worker.ReportProgress(2);
            RenderTargetBitmap bmp = new RenderTargetBitmap(imgWidth, imgHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            worker.ReportProgress(3);
            string mypicpath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\LuckySpinerBackground.png";
            using (Stream stream = File.Create(mypicpath))
                encoder.Save(stream);
            
            worker.ReportProgress(4, mypicpath);
            //worker.ReportProgress(5, imgSize);
            //done
            e.Result = imgsPth;
        }

        private string logoPath;

        private void btnLogo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                lb.Foreground = Brushes.Black;
                lb.Content = "Logo file:" + dlg.FileName;
                logoPath = dlg.FileName;
                try
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(logoPath);
                    bmp.EndInit();
                    img.Source = bmp;
                    btn.IsEnabled = true;
                }catch (Exception ex)
                {
                    lb.Foreground = Brushes.Red;
                    lb.Content = "Không thể load logo!!!";
                    btn.IsEnabled = false;
                    string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    // Append text to an existing file named "WriteLines.txt".
                    using (StreamWriter outputFile = new StreamWriter(System.IO.Path.Combine(mydocpath, "LuckySpinerLog.txt"), true))
                    {
                        outputFile.WriteLine(DateTime.Now.ToString());
                        outputFile.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}
