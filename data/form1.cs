using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Timers;

namespace VolumeController
{
    public partial class Form1 : Form
    {

        //private VolumeController vc = new VolumeController();
        private static System.Timers.Timer aTimer;
        public static bool enable_timer = false;

        MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
        public MMDevice device;
        int mintrack = 0;
        int initvalue = 0;

        // サンプル再生用の準備
        WaveOutEvent waveOut = new WaveOutEvent();
        AudioFileReader afr = new AudioFileReader("C:\\Users\\s192163\\Desktop\\卒業研究\\VolumeController-master\\VolumeController-master\\400hz-6db-20sec.wav");
        //AudioFileReader afr = new AudioFileReader("C:\\Users\\s192034.TSITCL\\OneDrive - Cyber University\\School\\卒業研究\\GraduationResearch\\VolumeController\\VolumeController\\5khz-6db-20sec.wav");

        public Form1()
        {
            InitializeComponent();

            device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            // 現在の音量を取得して、初期値として設定
            label2.Text = System.Convert.ToString(GetVolume());
            volume_trackBar.Value = GetVolume();
            // デバイスの一覧を取得
            comboBox1.Items.AddRange(GetDevices());
            root_maxtrackBar.Value = (int)100;

     
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                
                var device = (MMDevice)comboBox1.SelectedItem;
                // progressBar1の値を変更
                progressBar1.Value = (int)(Math.Round(device.AudioMeterInformation.MasterPeakValue * 100));
                progres_value.Text = System.Convert.ToString(progressBar1.Value);

                // 音量を取得
                var volume = GetVolume();
                // Debug.WriteLine(progressBar1.Value * volume);
                //Debug.WriteLine(progressBar1.Value + volume);
                //sum_trackBar.Value = (int)((GetVolume()/100.0d) * (progressBar1.Value / 100.0d));
                
                sum_trackBar.Value = (int)(GetVolume() * progressBar1.Value / 100);
                //sum_trackBar.Value = (int)(GetVolume() + progressBar1.Value / 200 / 2);
                label11.Text = System.Convert.ToString(GetVolume() * progressBar1.Value / 100);
                int sumtrack = (int)(GetVolume() * progressBar1.Value / 100);
                //max_trackBar.Value = (int)((GetVolume() / 100.0d) * (progressBar1.Value / 100.0d) * 100);
                //Debug.WriteLine(sum_trackBar.Value);

                // 最大の音量を制限
                if ((sum_trackBar.Value > root_maxtrackBar.Value) && progressBar1.Value != 0)
                {
                    // volumeの大きさによって下げる音量の幅を変更する
                    //SetVolume(volume - (int)Math.Ceiling((double)volume / 10.0d));
                    SetVolume(volume - 1);
                    volume_trackBar.Value = volume;

                    SetTimer();
                }

                // 最小の音量を制限
                if ((sum_trackBar.Value < root_mintrackBar.Value) && progressBar1.Value != 0 && enable_timer == false)
                //if (( sumtrack < mintrack))
                {
                    // volumeの大きさによって下げる音量の幅を変更する
                    //SetVolume(volume + (int)Math.Ceiling((double)volume / 10.0d));
                    SetVolume(volume + 2);
                    volume_trackBar.Value = volume;
                    //
                    SetTimer();
                }
                if ((progressBar1.Value == 0) && volume_trackBar.Value != 0)
                {
                    //initvalue = (int)(GetVolume() * progressBar1.Value / 100);
                    SetVolume(volume);


                }

                if (progressBar1.Value != 0 && volume_trackBar.Value == 0 && max_trackBar.Value != 100)
                {
                    //SetVolume(max_trackBar.Value);
                }
                

                // サンプル音源のループ
                afr.Position = 0;
            }
        }

        private void volume_trackBar_Scroll(object sender, EventArgs e)
        {
            SetVolume(volume_trackBar.Value);
        }

        private void max_trackBar_Scroll(object sender, EventArgs e)
        {
            max_volume.Text = System.Convert.ToString(max_trackBar.Value);
        }

        private void min_trackBar_Scroll(object sender, EventArgs e)
        {
            min_volume.Text = System.Convert.ToString(min_trackBar.Value);
        }

        private void media_btn_Click(object sender, EventArgs e)
        {
            if (waveOut.PlaybackState == PlaybackState.Stopped)
            {
                waveOut.Init(afr);
                waveOut.Play();
                media_btn.Text = "stop";
            }
            else if (waveOut.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Stop();
                media_btn.Text = "play";
            }
        }

        private void max_btn_Click(object sender, EventArgs e)
        {
            //max_trackBar.Value = (int)Math.Floor((double)(GetVolume() * (progressBar1.Value / 100)) * 100.0d);
            max_trackBar.Value = (int)(volume_trackBar.Value);
            max_volume.Text = System.Convert.ToString(max_trackBar.Value);

            root_maxtrackBar.Value = (int)(GetVolume() * progressBar1.Value / 100);
            label10.Text = System.Convert.ToString(GetVolume() * progressBar1.Value / 100);

            
        }

        private void min_btn_Click(object sender, EventArgs e)
        {
            //min_trackBar.Value = (int)Math.Floor((double)(GetVolume() * progressBar1.Value) / 100.0d);
            //min_trackBar.Value = (int)(GetVolume() * (progressBar1.Value / 100.0d)) * 100;
            //min_trackBar.Value = (int)((GetVolume() / 100.0d) * (progressBar1.Value / 100.0d) * 100);
            //min_trackBar.Value = (int)(GetVolume() * progressBar1.Value / 100);
            min_trackBar.Value = (int)(volume_trackBar.Value);
            min_volume.Text = System.Convert.ToString(min_trackBar.Value);

            root_mintrackBar.Value = (int)(GetVolume() * progressBar1.Value / 100);
            label12.Text = System.Convert.ToString(GetVolume() * progressBar1.Value / 100);

            mintrack = (int)(GetVolume() * progressBar1.Value / 100); 

           
        }

        // 音量の短時間連続上昇を防ぐ
        private static void SetTimer()
        {
            // Create a timer with a two second interval.
            // １秒
            aTimer = new System.Timers.Timer(500);
            Debug.WriteLine("start");
            // Hook up the Elapsed event for the timer.
            enable_timer = true;
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = false;
            aTimer.Enabled = true;

        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            enable_timer = false;
            Debug.WriteLine("stop");
        }

        /*
        public void update()
        {

        }
        */


        // メソッド:音量の変更
        public int SetVolume(int volume)
        {
            // 音量を変更（範囲：0.0～1.0）
            if (volume < 0)
            {
                volume = 0;
            }else if (volume > 100)
            {
                volume = 100;
            }
            device.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)volume / 100.0f);
            volume_trackBar.Value = volume;
            label2.Text = System.Convert.ToString(volume);


            return GetVolume();
        }

        // メソッド：音量の取得 return：0.00～1.00 
        public int GetVolume()
        {
            return (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
        }

        // メソッド：デバイスの一覧を取得
        public object[] GetDevices()
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
            return devices.ToArray();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
    }

    /*
    // 音量調整をする
    class VolumeController
    {
        public MMDevice device;

        // VolumeControllerのコンストラクター
        public VolumeController()
        {
            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
            device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }

        // メソッド:音量の変更
        public int SetVolume(int volume)
        {
            // 音量を変更（範囲：0.0～1.0）
            volume = volume < 0 ? 0 : volume;
            device.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)volume / 100.0f);

            return GetVolume();
        }

        // メソッド：音量の取得
        public int GetVolume()
        {
            return (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
        }

        // メソッド：デバイスの一覧を取得
        public object[] GetDevices()
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
            return devices.ToArray();
        }
    }
    */
}
