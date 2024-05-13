using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Input;


namespace Video_App
{
    /// <summary>
    /// Логика взаимодействия для videoclient.xaml
    /// </summary>
    public partial class videoclient : Window
    {
        static private IPAddress ipAddress = IPAddress.Any;
        static private int port = 12346;
        static private TcpListener listener = new TcpListener(ipAddress, port);

        public videoclient(string str)
        {
            InitializeComponent();
            byte[] bytes = Convert.FromBase64String(str);

            string tempFilePath = Path.Combine(Path.GetTempPath(), "tempvideo.mp4");

            File.WriteAllBytes(tempFilePath, bytes);

            Uri uri = new Uri(tempFilePath);

            vido.LoadedBehavior = MediaState.Manual;

            vido.Pause();

            StartListener();

            Closing += videoclient_Closing;

            this.KeyDown += Window_KeyDown;

            vido.Source = uri;
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
        }
        private void OnFullScreen()
        {
            this.WindowStyle = WindowStyle.None;
        }
        private void OffFullScreen()
        {
            this.WindowStyle = WindowStyle.ToolWindow;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (volum.Visibility == Visibility.Hidden)
            {
                volum.Visibility = Visibility.Visible;
            }
            else
            {
                volum.Visibility = Visibility.Hidden;
            }
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sliderValue = volum.Value;

            vido.Volume = sliderValue;
        }
        private void videoclient_Closing(object sender, CancelEventArgs e)
        {
            listener.Stop();

            MainWindow v = new MainWindow();
            v.videoWindowClosed1();
        }

        async void StartListener()
        {
            try
            {
                listener.Start();
                Console.WriteLine("Очікування підключень...");

                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    Console.WriteLine("Підключений клієнт!");

                    Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Помилка: " + e.Message);
            }
        }
        async void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Отримано от клієнта: {data}");

                    if (data == "pause")
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            vido.Pause();
                        });
                    }
                    if (data.Contains("time"))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            data = data.Remove(0, 4);

                            TimeSpan position = TimeSpan.Parse(data);
                            vido.Position = position;

                        });
                    }
                    if (data.Contains("name"))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            data = data.Remove(0, 4);
                            vid.Title = data;

                        });
                    }
                    if (data == "play")
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            vido.Play();
                        });
                    }
                    if (data == "+10")
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            TimeSpan currentPosition = vido.Position;
                            TimeSpan newPosition = currentPosition.Add(TimeSpan.FromSeconds(10));

                            if (newPosition < vido.NaturalDuration.TimeSpan)
                            {
                                vido.Position = newPosition;
                            }
                        });
                    }
                    if (data == "-10")
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            TimeSpan currentPosition = vido.Position;
                            TimeSpan newPosition = currentPosition.Subtract(TimeSpan.FromSeconds(10));

                            if (newPosition >= TimeSpan.Zero)
                            {
                                vido.Position = newPosition;
                            }
                        });
                    }
                    if (data == "exit")
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Close();
                        });
                    }
                    byte[] response = Encoding.UTF8.GetBytes("Данні отримані успішно!");
                    await stream.WriteAsync(response, 0, response.Length);
                }

                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Помилка при обробці клієнта: " + e.Message);
            }
        }

    }
}
