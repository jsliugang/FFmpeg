using System;
using System.Threading;

namespace FFmpegDemo
{
    public class FFmpeg_Manager
    {

        private static FFmpeg_Manager instance;
        public static string ShowMessage = "";

        public static FFmpeg_Manager GetInstance()
        {
            if (instance == null)
            {
                instance = new FFmpeg_Manager();
            }
            return instance;
        }

        Pull_Rtmp.ShowBitmap Methodshow;
        string Url;
        Action<string> Meassage;
        Pull_Rtmp rtmp = new Pull_Rtmp();
        Thread thPlayer;
        public void Start(Pull_Rtmp.ShowBitmap show, string url, Action<string> action)
        {
            Url = url;
            Methodshow = show;
            Meassage = action;

            if (thPlayer != null)
            {
                rtmp.Stop();
                thPlayer = null;
            }
            else
            {
                thPlayer = new Thread(DeCoding);
                thPlayer.IsBackground = true;
                thPlayer.Start();
            }
        }

        /// <summary>
        /// 播放线程执行方法
        /// </summary>
        private unsafe void DeCoding()
        {
            try
            {
                Console.WriteLine("DeCoding run...");
                FFmpeg_Manager.ShowMessage="\nDeCoding run...";

                rtmp.Start(Methodshow, Url);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Meassage("异常：" + ex.Message); 
                
                instance = null;
            }
            finally
            {
                Console.WriteLine("DeCoding exit");
                Meassage("DeCoding exit");
                rtmp.Stop();

                thPlayer = null;
            }
        }

    }
}
