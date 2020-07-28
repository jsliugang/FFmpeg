using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FFmpegDemo_RTMP_Pull
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private System.ComponentModel.BackgroundWorker bgWorker = new System.ComponentModel.BackgroundWorker();
        StringBuilder sb = new StringBuilder();
        TextBlock textBlock;
        ChartPlotter plotter;

        public  void setControls(TextBlock tb, ChartPlotter cp)
        {
            textBlock = tb;
            plotter = cp;

            InitializeBackgroundWorker();
        }

        private ObservableDataSource<Point> dataSource = new ObservableDataSource<Point>();
        private PerformanceCounter performanceCounter = new PerformanceCounter();
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private int currentSecond = 0;

        private void InitializeBackgroundWorker()
        {
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgWorker_ProgessChanged);
            bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgWorker_WorkerCompleted);
            Start();

            LineGraph line = new LineGraph(dataSource);
            line.LinePen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Green, 2);
            plotter.Children.Add(line);
            plotter.LegendVisible = false;
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += timer_Tick;
            dispatcherTimer.IsEnabled = true;
            plotter.Viewport.FitToView();
        }
        private void Start()
        {
            if (bgWorker.IsBusy)
                return;

            bgWorker.RunWorkerAsync("hello");
        }


        private void timer_Tick(object sender, EventArgs e)
        {
            performanceCounter.CategoryName = "Processor";
            performanceCounter.CounterName = "% Processor Time";
            performanceCounter.InstanceName = "_Total";

            double x = currentSecond;
            double y = performanceCounter.NextValue();

            Point point = new Point(x, y);
            if (dataSource.Collection.Count >= 120)
            {
                dataSource.Collection.RemoveAt(0);
            }
            dataSource.AppendAsync(plotter.Dispatcher, point);

            currentSecond++;
        }

        public void bgWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            for (int i = 0; i <= 180; i++)
            {
                if (bgWorker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    bgWorker.ReportProgress(i, "Working");
                    System.Threading.Thread.Sleep(1000);
                    int threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    this.textBlock.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        this.textBlock.Text += string.Format("\nDoWork线程ID：【{0}】\n{1}", threadid, FFmpegDemo.FFmpeg_Manager.ShowMessage);
                    });
                }
            }
        }

        public void bgWorker_ProgessChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            //string state = (string)e.UserState;//接收ReportProgress方法传递过来的userState
            //this.progressBar.Value = e.ProgressPercentage;
            this.textBlock.Text = sb.ToString() + "处理进度:" + Convert.ToString(e.ProgressPercentage) + "s";
            int threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            this.textBlock.Text += string.Format("\nProgessChanged线程ID：【{0}】", threadid);
        }

        public void bgWorker_WorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                System.Windows.MessageBox.Show(e.Error.ToString());
                return;
            }
            if (!e.Cancelled)
                this.textBlock.Text = "处理完毕!180s";
            else
                this.textBlock.Text = "处理终止!";

            System.Threading.Thread.Sleep(5000);
            Start();
        }
    }
}
