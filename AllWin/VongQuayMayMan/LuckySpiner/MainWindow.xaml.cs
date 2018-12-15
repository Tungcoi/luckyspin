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
        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            bgPath = "";
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

                //testimg(imgs);
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
                    bgPath = e.UserState.ToString();
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
                    break;
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
            SpinerWindow wd = new SpinerWindow(bgPath, data);
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
            int imgSize = 1;
            while (imgSize * imgSize < imgsPth.Count) imgSize++;
            int tileWidth = imgWidth / imgSize;
            int tileHeight = imgHeight / imgSize;
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                for (int i = 0; i < imgSize; i++)
                {
                    for (int j = 0; j < imgSize; j++)
                    {
                        int pos = (i * imgSize + j) % imgsPth.Count;
                        if ((i * imgSize + j) < imgsPth.Count) 
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

            e.Result = imgsPth;
            //done
            worker.ReportProgress(0, mypicpath);
        }


    }
}
