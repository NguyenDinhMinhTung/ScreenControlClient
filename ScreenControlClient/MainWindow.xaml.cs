using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Microsoft.Win32;

namespace ScreenControlClient
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int cButtons, uint dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern uint keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const int MOUSEEVENTF_WHEEL = 0x0800;

        private const int KEYEVENTF_KEYUP = 0x02;

        public delegate void Action();

        Thread thread;

        DispatcherTimer dispatcherTimer;

        bool isConnected = false;
        int timeCount = 5;

        Socket socket;
        IPEndPoint ipep;

        int screenWidth;
        int screenHeight;
        float scale;

        public MainWindow()
        {
            InitializeComponent();

            var currentDPI = (int)Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Desktop\\WindowMetrics", "AppliedDPI", 96);
            scale = (float)currentDPI / 96;

            screenWidth = (int)(System.Windows.SystemParameters.PrimaryScreenWidth * Math.Round(scale));
            screenHeight = (int)(System.Windows.SystemParameters.PrimaryScreenHeight * Math.Round(scale));

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Tick += (_sender, _e) =>
            {
                if (timeCount > 0)
                {
                    timeCount--;
                    btnConnect.Content = "CONNECT " + timeCount;
                }
                else
                {
                    if (!isConnected)
                    {
                        showNotConnect();
                    }
                    else
                    {
                        showConnected();
                    }
                }
            };
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        byte[] getByte(byte[] data, int start, uint count)
        {
            byte[] result = new byte[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = data[start + i];
            }

            return result;
        }

        public void client()
        {
            var port = 6000;
            var buffer = Encoding.UTF8.GetBytes("IP" + GetLocalIPAddress());

            var server = new UdpClient(port);
            server.EnableBroadcast = true;
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, port);
            server.Connect(ip);

            server.Send(buffer, buffer.Length);
            server.Close();

            int recv;
            byte[] data;
            ipep = new IPEndPoint(IPAddress.Any, port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(ipep);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            data = new byte[17];
            recv = socket.ReceiveFrom(data, ref Remote);
            showConnected();

            while (true)
            {
                data = new byte[17];
                recv = socket.ReceiveFrom(data, ref Remote);

                if (data[0] == 9) break;
                switch (data[0])
                {
                    case 0:
                        double dx = BitConverter.ToDouble(getByte(data, 1, 8), 0);
                        double dy = BitConverter.ToDouble(getByte(data, 9, 8), 0);

                        int x = (int)Math.Round(dx * screenWidth);
                        int y = (int)Math.Round(dy * screenHeight);

                        SetCursorPos(x, y);
                        break;

                    case 1:
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        break;

                    case 2:
                        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                        break;

                    case 3:
                        mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                        break;

                    case 4:
                        mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                        break;

                    case 5:
                        mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                        break;

                    case 6:
                        mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                        break;

                    case 7:
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 120, 0);
                        break;

                    case 8:
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -120, 0);
                        break;

                    case 10:
                        //MessageBox.Show(data[1].ToString());
                        keybd_event(data[1], 0, 0, (UIntPtr)0);
                        break;

                    case 11:
                        keybd_event(data[1], 0, KEYEVENTF_KEYUP, (UIntPtr)0);
                        break;
                }
            }

            socket.Close();
            showNotConnect();
        }

        public void sendData(byte[] data)
        {
            socket.SendTo(data, data.Length, SocketFlags.None, ipep);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            thread?.Abort();

            Environment.Exit(0);
        }

        void showConnected()
        {
            isConnected = true;

            btnConnect.Dispatcher.Invoke(new Action(() =>
            {
                btnConnect.IsEnabled = false;
                btnConnect.Content = "CONNECT";
            }));

            dispatcherTimer.IsEnabled = false;

            txtStatus.Dispatcher.Invoke(new Action(() =>
            {
                txtStatus.Text = "KET NOI THANH CONG";
            }));

            timeCount = 5;
        }

        void showNotConnect()
        {
            isConnected = false;

            socket?.Close();

            btnConnect.Dispatcher.Invoke(new Action(() =>
            {
                btnConnect.IsEnabled = true;
                btnConnect.Content = "CONNECT";
            }));
            dispatcherTimer.IsEnabled = false;
            txtStatus.Dispatcher.Invoke(new Action(() =>
            {
                txtStatus.Text = "CHUA KET NOI";
            }));
            timeCount = 5;

            thread?.Abort();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
            }

            thread = new Thread(client);
            thread.IsBackground = true;

            thread.Start();

            btnConnect.IsEnabled = false;
            txtStatus.Text = "DANG DOI KET NOI";

            dispatcherTimer.IsEnabled = true;
        }
    }
}
