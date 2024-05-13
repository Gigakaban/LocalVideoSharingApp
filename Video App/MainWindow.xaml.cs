using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

namespace Video_App
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event EventHandler<List<string>> ClientIPsUpdated;
        public event EventHandler<string> Name;
        private List<string> clientIPs = new List<string>();
        private TcpListener server;
        private TcpClient client;
        private NetworkStream stream;
        private bool isServer = false;
        private bool off = true;
        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindow_Closing;
            string hostName = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            
            foreach(var item in addresses)
            {
                if(item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    cboxip.Items.Add(item);
                }
            }
            
            
        }
        public void videoWindowClosed1()
        {
            mwindow.Visibility = Visibility.Visible;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(File.Exists(Path.Combine(Path.GetTempPath(), "tempvideo.mp4")))
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "tempvideo.mp4"));
            }
            Process[] processes = Process.GetProcesses();
            foreach(var item in processes)
            {
                Console.WriteLine(item.ProcessName);
                if(item.ProcessName== "Video App")
                {
                   item.Kill();
                }
            }
           
        }

        private void ConnectButton_CLick(object sender, RoutedEventArgs e)
        {
            if(IPAddress.TryParse(ServerIpTextBox.Text, out IPAddress a))
            {
                string serverIp = ServerIpTextBox.Text;
                int serverPort;

                if (int.TryParse(ServerPortTextBox.Text, out serverPort))
                {
                    if(serverPort>0&&serverPort<65534)
                    {

                        Thread clientThread = new Thread(() =>
                        {
                            isServer = false;
                            ConnectToServer(serverIp, serverPort);
                        });
                        clientThread.Start();
                    }
                    else
                    {
                        MessageBox.Show("Введіть коректний номер порту.");
                    }
                    
                }
                else
                {
                    MessageBox.Show("Введіть номер порту (цифри).");
                }
            }
            else
            {
                MessageBox.Show("Введіть ipv4 правильно.");
            }
        }

        private void HostButton_Click(object sender, RoutedEventArgs e)
        {
            if (cboxip.SelectedItem==null)
            {
                MessageBox.Show("Виберіть ip в випадаючому списці");
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Video Files | *.mp4; *.avi; *.mkv"; 
                if (openFileDialog.ShowDialog() == true)
                {
                    string videoFilePath = openFileDialog.FileName;

                    string[] tstr = videoFilePath.Split('\\');

                    Thread serverThread = new Thread(() =>
                    {
                        isServer = true;
                        StartServer(videoFilePath, tstr[tstr.Length - 1]);

                    });

                    serverThread.Start();
                    lgif.Visibility = Visibility.Visible;
                    lstatus.Visibility = Visibility.Visible;
                }
            }
            
        }

        private async void StartServer(string videoFilePath, string name)
        {
            

            try
            {
                int port = 12345;
                string cbox = "";
                await Dispatcher.InvokeAsync(() =>
                {
                    cbox = cboxip.SelectedItem.ToString();
                }, System.Windows.Threading.DispatcherPriority.Normal);
                IPAddress myIp = IPAddress.Parse(cbox);
                await Dispatcher.InvokeAsync(() =>
                {
                Clipboard.SetText($"{myIp} {port}");
                }, System.Windows.Threading.DispatcherPriority.Normal);

                


                server = new TcpListener(myIp, port);
                server.Start();
                MessageBox.Show("Сервер стартований на IP-адресі: " + myIp.ToString() + ", порт: " + port.ToString()+" Данні скопійовані в буфер обміну");

                bool isFirstClient = true;

                while (off)
                {

                    client = await server.AcceptTcpClientAsync();
                    await Dispatcher.InvokeAsync(() =>
                    {
                        lstatus.Content = "Передача данних...";
                    }, System.Windows.Threading.DispatcherPriority.Normal);
                    

                    stream = client.GetStream();
                    UpdateClientIPs(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());

                    using (FileStream fileStream = File.OpenRead(videoFilePath))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;

                        while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await stream.WriteAsync(buffer, 0, bytesRead);
                        }
                    }


                    if (isFirstClient)
                    {
                        isFirstClient = false;
                        await Dispatcher.InvokeAsync(() =>
                        {
                            Uri videoUri = new Uri(videoFilePath);
                            video vid = new video(videoUri,clientIPs,name);
                            vid.Show();
                            mwindow.Visibility = Visibility.Hidden;
                        }, System.Windows.Threading.DispatcherPriority.Normal);
                    }
                    await Dispatcher.InvokeAsync(() =>
                    {
                        Name?.Invoke(this, name);
                    }, System.Windows.Threading.DispatcherPriority.Normal);
                    

                    stream.Close();
                    client.Close();
                    MessageBox.Show("Відео успішно надіслано клієнту.");
                                                                                            

                    MessageBoxResult result = MessageBox.Show("Продовжити приймати нових клієнтів?", "Підключення нових клієнтів", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No)
                    {
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка сервера: " + ex.Message);
            }
            finally
            {

                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
                if (server != null)
                    server.Stop();
            }
        }

        private void UpdateClientIPs(string ipAddress)
        {
            clientIPs.Add(ipAddress);
            ClientIPsUpdated?.Invoke(this, clientIPs);
            
        }


        private async void ConnectToServer(string serverIp, int serverPort)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(serverIp, serverPort);
                stream = client.GetStream();
                Application.Current.Dispatcher.Invoke(() =>
                {

                    lgif.Visibility = Visibility.Visible;
                    lstatus.Content = "Триває отримання данних...";
                    lstatus.Visibility = Visibility.Visible;
                });
                
                
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        memoryStream.Write(buffer, 0, bytesRead);
                    }

                    byte[] receivedData = memoryStream.ToArray();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        
                        videoclient videoclient= new videoclient(Convert.ToBase64String(receivedData));
                        videoclient.Show();
                        
                        mwindow.Visibility = Visibility.Hidden;
                    });
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка підключення: " + ex.Message);
            }
            finally
            {
                stream.Close();
                client.Close();
            }

        }

        

    }
}
