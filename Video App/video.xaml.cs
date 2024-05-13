using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Video_App
{
    /// <summary>
    /// Логика взаимодействия для video.xaml
    /// </summary>
    public partial class video : Window
    {
        private List<string> clientIPs = new List<string>();
        public event EventHandler<bool> WindowClosed;
        private string _name;
        public video(Uri str, List<string> ClientIPs, string name)
        {
            InitializeComponent();

            vido.LoadedBehavior = MediaState.Manual;

            _name= name;

            vid.Title= name;

            vido.Volume = 0.5;

            vido.Source = str;

            vido.Pause();

            clientIPs = ClientIPs;

            ((MainWindow)Application.Current.MainWindow).ClientIPsUpdated += MainWindow_ClientIPsUpdated;

            ((MainWindow)Application.Current.MainWindow).Name += NameTransport;

            Closing += WindowClosed1;

            this.KeyDown += Window_KeyDown;

        }
        private void NameTransport(object sender, string name)
        {
            try
            {
                foreach (var item in clientIPs)
                {
                    SendName(item, 12346, name);
                }
            }
            catch
            {

            }

        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F)
            {
                OnFullScreen();
            }
            if (e.Key == Key.Escape)
            {
                OffFullScreen();
            }
            if (e.Key == Key.F5)
            {
                try
                {
                    TimeSpan currentPosition = vido.Position;
                    foreach (var item in clientIPs)
                    {
                        SyncTime(item, 12346,currentPosition);
                    }
                }
                catch { }
                
            }
        }
        private void SyncTime(string ip, int port, TimeSpan currentPosition)
        {
            try
            {

                using (TcpClient client = new TcpClient(ip, port))
                {

                    using (NetworkStream stream = client.GetStream())
                    {

                        string request = $"time{currentPosition}"; 
                        byte[] data = Encoding.UTF8.GetBytes(request);
                        stream.Write(data, 0, data.Length);

                        Console.WriteLine("Запит відправлений успішно!");
                    }
                }
            }
            catch (Exception e)
            {
                
            }

        }
        private void OnFullScreen()
        {
            this.WindowStyle = WindowStyle.None;
        }
        private void OffFullScreen()
        {
            this.WindowStyle = WindowStyle.ToolWindow;
        }
        private void WindowClosed1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow v = new MainWindow();
            v.videoWindowClosed1();
            try
            {
                foreach (var item in clientIPs)
                {
                    SendExit(item, 12346);
                }
            }
            catch { }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(lessvid.Visibility==Visibility.Hidden)
            {
                lessvid.Visibility = Visibility.Visible;
                pause.Visibility = Visibility.Visible;
                incrvid.Visibility = Visibility.Visible;
                volum.Visibility = Visibility.Visible;
            }
            else
            {
                lessvid.Visibility = Visibility.Hidden;
                pause.Visibility = Visibility.Hidden;
                incrvid.Visibility = Visibility.Hidden;
                volum.Visibility = Visibility.Hidden;
            }
        }

        private void Minus10second_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan currentPosition = vido.Position;
            TimeSpan newPosition = currentPosition.Subtract(TimeSpan.FromSeconds(10));

            if (newPosition >= TimeSpan.Zero)
            {
                vido.Position = newPosition;
            }
            try
            {
                foreach (var item in clientIPs)
                {
                    SendMinus10(item, 12346);
                }
            }
            catch
            { }
            
        }
        private void Plus10second_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan currentPosition = vido.Position;
            TimeSpan newPosition = currentPosition.Add(TimeSpan.FromSeconds(10));

            if (newPosition < vido.NaturalDuration.TimeSpan)
            {
                vido.Position = newPosition;
            }
            try
            {
                foreach (var item in clientIPs)
                {
                    SendPlus10(item, 12346);
                }
            }
            catch { }

        }
        private bool isPaused = false;
        
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            if (isPaused)
            {
                vido.Play();
                try
                {
                    foreach (var item in clientIPs)
                    {
                        SendPlay(item, 12346);
                        SendName(item, 12346, _name);
                    }
                }
                catch { }
                
            }
            else
            {
                vido.Pause();
                try
                {
                    foreach (var item in clientIPs)
                    {
                        SendPause(item, 12346);

                    }
                }
                catch { }
                
            }

            isPaused = !isPaused;
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sliderValue = volum.Value;   
            vido.Volume=sliderValue;
        }


        private void MainWindow_ClientIPsUpdated(object sender, List<string> e)
        {
            clientIPs = e;
            
        }

        private void SendPause(string ip,int port)
        {
            try
            {
                
                using (TcpClient client = new TcpClient(ip, port))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        string request = "pause"; 
                        byte[] data = Encoding.UTF8.GetBytes(request);
                        stream.Write(data, 0, data.Length);

                        Console.WriteLine("Запит відправлений успішно!");
                    }
                }
            }
            catch (Exception e)
            {
                
            }

        }

        private void SendPlay (string ip, int port)
        {
            try
            {
                
                using (TcpClient client = new TcpClient(ip, port))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        string request = "play"; 
                        byte[] data = Encoding.UTF8.GetBytes(request);
                        stream.Write(data, 0, data.Length);

                        Console.WriteLine("Запит відправлений успішно!");
                    }
                }
            }
            catch (Exception e)
            {
                
            }
        }

        private void SendName(string ip, int port,string name)
        {
            try
            {

                using (TcpClient client = new TcpClient(ip, port))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        string request = $"name{name}"; // Запрос на паузу
                        byte[] data = Encoding.UTF8.GetBytes(request);
                        stream.Write(data, 0, data.Length);

                        Console.WriteLine("Запит відправлений успішно!");
                    }
                }
            }
            catch (Exception e)
            {
                
            }
        }

        private void SendPlus10(string ip, int port)
        {
            try
            {

                using (TcpClient client = new TcpClient(ip, port))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        string request = "+10"; 
                        byte[] data = Encoding.UTF8.GetBytes(request);
                        stream.Write(data, 0, data.Length);

                        Console.WriteLine("Запит відправлений успішно!");
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private void SendMinus10(string ip, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient(ip, port))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        string request = "-10"; 
                        byte[] data = Encoding.UTF8.GetBytes(request);
                        stream.Write(data, 0, data.Length);

                        Console.WriteLine("Запит відправлений успішно!");
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private void SendExit(string ip, int port)
        {
            try
            {

                using (TcpClient client = new TcpClient(ip, port))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        string request = "exit"; 
                        byte[] data = Encoding.UTF8.GetBytes(request);
                        stream.Write(data, 0, data.Length);

                        Console.WriteLine("Запит відправлений успішно!");
                    }
                }
            }
            catch (Exception e)
            {

            }
        }
    }
}
