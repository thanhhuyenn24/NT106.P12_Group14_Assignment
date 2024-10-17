using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SocketChat
{
    public partial class Form1 : Form
    {
        private Socket listener = null;
        private bool started = false;
        private int _port = 11000;
        private static int _buff_size = 8192;
        private byte[] _buffer = new byte[_buff_size];
        private Thread serverThread = null;
        private delegate void SafeCallDelegate(string text, Control obj);
        private List<Socket> clientSockets = new List<Socket>();
        private Dictionary<string, byte[]> fileStorage = new Dictionary<string, byte[]>();

        public Form1()
        {
            InitializeComponent();
            listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (started)
                {
                    started = false;
                    button2.Text = "Listen";
                    serverThread = null;
                    listener.Close();
                }
                else
                {
                    serverThread = new Thread(() => this.listen());
                    serverThread.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void listen()
        {
            listener.Bind(new IPEndPoint(IPAddress.Parse(textBox1.Text), _port));
            listener.Listen(10);
            started = true;
            UpdateTextThreadSafe("Stop", button2);
            UpdateTextThreadSafe("Start listening", richTextBox1);

            while (started)
            {
                Socket client = listener.Accept();
                clientSockets.Add(client);
                Thread clientThread = new Thread(() => this.readingClientSocket(client));
                clientThread.Start();
                UpdateTextThreadSafe("Accepted connection from " + client.RemoteEndPoint.ToString(), richTextBox1);
                UpdateTextThreadSafe(client.RemoteEndPoint.ToString() + " has joined the chat!", richTextBox1);
            }
        }

        private void readingClientSocket(Socket client)
        {
            byte[] buffer = new byte[_buff_size];
            while (client.Connected)
            {
                try
                {
                    if (client.Available > 0)
                    {
                        int bytesRead = client.Receive(buffer);
                        string header = Encoding.UTF8.GetString(buffer, 0, 5);

                        if (header == "FILE:")
                        {
                            // Parse file info
                            string fileInfo = Encoding.UTF8.GetString(buffer, 5, bytesRead - 5);
                            string[] parts = fileInfo.Split('|');
                            string fileName = parts[0];
                            long fileSize = long.Parse(parts[1]);

                            // Receive file data
                            byte[] fileData = new byte[fileSize];
                            int totalBytesRead = 0;

                            while (totalBytesRead < fileSize)
                            {
                                int currentRead = client.Receive(fileData, totalBytesRead,
                                    (int)fileSize - totalBytesRead, SocketFlags.None);
                                totalBytesRead += currentRead;
                            }

                            // Store file data
                            string fileKey = $"{DateTime.Now.Ticks}_{fileName}";
                            fileStorage[fileKey] = fileData;

                            // Notify all clients about the file
                            string notification = $"FILE_INFO|{client.RemoteEndPoint}|{fileName}|{fileKey}";
                            byte[] notificationData = Encoding.UTF8.GetBytes(notification);

                            foreach (Socket s in clientSockets)
                            {
                                s.Send(notificationData);
                            }

                            // Update server UI
                            UpdateTextThreadSafe($"{client.RemoteEndPoint} sent file: {fileName}", richTextBox1);
                        }
                        else if (header == "GETF:")
                        {
                            // Handle file download request
                            string fileKey = Encoding.UTF8.GetString(buffer, 5, bytesRead - 5);
                            if (fileStorage.ContainsKey(fileKey))
                            {
                                byte[] fileData = fileStorage[fileKey];
                                // Send file size first
                                byte[] sizeData = Encoding.UTF8.GetBytes($"SIZE:{fileData.Length}");
                                client.Send(sizeData);

                                // Wait a moment to ensure size data is received
                                Thread.Sleep(100);

                                // Send file data
                                client.Send(fileData);
                            }
                        }
                        else
                        {
                            // Handle normal text message
                            StringBuilder sb = new StringBuilder();
                            sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                            while (client.Available > 0)
                            {
                                bytesRead = client.Receive(buffer);
                                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                            }

                            string receivedStr = client.RemoteEndPoint + ": " + sb.ToString();
                            UpdateTextThreadSafe(receivedStr, richTextBox1);

                            foreach (Socket s in clientSockets)
                            {
                                s.Send(Encoding.UTF8.GetBytes(receivedStr));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateTextThreadSafe($"Error with client {client.RemoteEndPoint}: {ex.Message}", richTextBox1);
                    break;
                }
            }

            clientSockets.Remove(client);
            UpdateTextThreadSafe($"{client.RemoteEndPoint} has disconnected.", richTextBox1);
        }

        private void UpdateTextThreadSafe(string text, Control control)
        {
            if (control.InvokeRequired)
            {
                var d = new SafeCallDelegate(UpdateTextThreadSafe);
                control.Invoke(d, new object[] { text, control });
            }
            else
            {
                if (control is RichTextBox)
                {
                    ((RichTextBox)control).AppendText("\r\n" + text);
                    ((RichTextBox)control).ScrollToCaret();
                }
                else
                {
                    control.Text = text;
                }
            }
        }
    }
}