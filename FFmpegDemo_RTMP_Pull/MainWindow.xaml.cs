using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FFmpegDemo_RTMP_Pull
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {
            Dictionary<string, string> valuePairs = new Dictionary<string, string>()
            {   { "CCTV1高清","http://ivi.bupt.edu.cn/hls/cctv1hd.m3u8"},
                { "CCTV3高清","http://ivi.bupt.edu.cn/hls/cctv3hd.m3u8"},
                { "CCTV6高清","http://ivi.bupt.edu.cn/hls/cctv6hd.m3u8"},
                { "湖南卫视","rtmp://58.200.131.2:1935/livetv/hunantv" },
                { "广西卫视","rtmp://58.200.131.2:1935/livetv/gxtv" },
                { "东方卫视","rtmp://58.200.131.2:1935/livetv/dftv" },
                //{ "柏欣妤","https://h5.48.cn/2019appshare/memberLiveShare/?id=494974326296875008" }
            };

            cb_PullList.ItemsSource = valuePairs;
            cb_PullList.DisplayMemberPath = "Key";
            cb_PullList.SelectedValuePath = "Value";
            cb_PullList.SelectedIndex = 3;

            (App.Current as App).setControls(tb_Info, plotter);
        }

        private void cb_PullList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tb_Address.Text = cb_PullList.SelectedValue.ToString();
        }
        Border border;
        int Index = 0;
        private void bt_Pull_Click(object sender, RoutedEventArgs e)
        {
            if (border is null)
            {
                border = bd_Play;
            }
            string rtmp = tb_Address.Text;
            FFmpegDemo.FFmpeg_Manager demo = FFmpegDemo.FFmpeg_Manager.GetInstance();
            System.Drawing.Bitmap oldBmp = null;
            Image image = new Image();
            border.Child = image;
            image.SizeChanged += Image_SizeChanged;

            // 更新图片显示
            FFmpegDemo.Pull_Rtmp.ShowBitmap show = (bmp) =>
            {
                int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
                this.Dispatcher.Invoke(() =>
                {
                    image.Source = GetWriteableBitmap(bmp);

                    if (oldBmp != null)
                    {
                        oldBmp.Dispose();
                    }
                    oldBmp = bmp;
                });
            };
            Action<string> action = (String msg) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    FFmpegDemo.FFmpeg_Manager.ShowMessage += string.Format("\n【{0}】{1}", ++Index, msg);
                });
            };

            demo.Start(show, rtmp, action);
            if (bt_Pull.Content.ToString() == "停止播放")
            {
                bt_Pull.Content = "开始播放";
            }
            else
            {
                bt_Pull.Content = "停止播放";
            }
        }
        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScaleTransform stf = new ScaleTransform();
            stf.ScaleY = -1;
            stf.CenterY = e.NewSize.Height / 2;
            border.RenderTransform = stf;
        }

        WriteableBitmap _drawBitmap;
        public WriteableBitmap GetWriteableBitmap(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null) { return _drawBitmap; }

            byte[] bytes;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                bytes = ms.GetBuffer(); // byte[] bytes = ms.ToArray(); 这两句都可以
            }

            if (_drawBitmap == null || _drawBitmap.Width != bitmap.Width || _drawBitmap.Height != bitmap.Height)
            {
                _drawBitmap = new WriteableBitmap(bitmap.Width, bitmap.Height, 96, 96, PixelFormats.Bgr24, null);
                _drawBitmap.WritePixels(new Int32Rect(0, 0, bitmap.Width, bitmap.Height),
                    bytes, _drawBitmap.BackBufferStride, 0);
                return _drawBitmap;
            }
            else
            {
                _drawBitmap.WritePixels(new Int32Rect(0, 0, bitmap.Width, bitmap.Height),
                   bytes, _drawBitmap.BackBufferStride, 0);
                return _drawBitmap;
            }
        }

        private void tb_Address_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (tb_Address.Text.StartsWith("https://") && !tb_Address.Text.EndsWith("m3u8"))
            //{
            //    System.Windows.Forms.WebBrowser wb_Video = new System.Windows.Forms.WebBrowser();
            //    string address = tb_Address.Text;
            //    wb_Video.Navigate(address);
            //    wb_Video.Navigated += Wb_Video_Navigated;
            //}
        }

        private void Wb_Video_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            //System.Windows.Forms.WebBrowser wb_Video = sender as System.Windows.Forms.WebBrowser;
            //foreach (HtmlElement htmlElement in wb_Video.Document.GetElementsByTagName("video"))
            //{
            //    string address = htmlElement.GetAttribute("src");
            //    if (address.StartsWith("rtmp://"))
            //        tb_Address.Text = address;
            //}
        }
    }
}
