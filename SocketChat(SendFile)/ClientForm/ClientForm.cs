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

namespace ClientForm
{
    public partial class ClientForm : Form
    {
        private Socket clientSocket = null;
        private static int _buff_size = 8192;
        private delegate void SafeCallDelegate(string text, Control obj);
        private delegate void AddFileButtonDelegate(string clientInfo, string fileName, string fileKey);
        private Thread recvThread = null;
        private Dictionary<string, string> fileKeys = new Dictionary<string, string>();

        public ClientForm()
        {
            InitializeComponent();
            clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                IPAddress serverIp = IPAddress.Parse(textBox1.Text);
                int serverPort = int.Parse(textBox2.Text);
                IPEndPoint serverEp = new IPEndPoint(serverIp, serverPort);
                clientSocket.Connect(serverEp);
                richTextBox1.Text += "Connected to " + serverEp.ToString();
                this.Text = "Connected to " + serverEp.ToString();
                recvThread = new Thread(() => this.readingClientSocket());
                recvThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(richTextBox2.Text))
                {
                    clientSocket.Send(Encoding.UTF8.GetBytes(richTextBox2.Text));
                    richTextBox2.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SendFilebtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string fileName = Path.GetFileName(ofd.FileName);
                        byte[] fileData = File.ReadAllBytes(ofd.FileName);

                        // Send file header
                        string header = $"FILE:{fileName}|{fileData.Length}";
                        clientSocket.Send(Encoding.UTF8.GetBytes(header));

                        // Send file data
                        clientSocket.Send(fileData);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddFileButton(string clientInfo, string fileName, string fileKey)
        {
            if (flowLayoutPanel1.InvokeRequired)
            {
                flowLayoutPanel1.Invoke(new AddFileButtonDelegate(AddFileButton), new object[] { clientInfo, fileName, fileKey });
                return;
            }

            Button fileButton = new Button();
            fileButton.Text = fileName;
            fileButton.Tag = fileKey;
            fileButton.AutoSize = true;
            fileButton.Click += FileButton_Click;
            flowLayoutPanel1.Controls.Add(fileButton);

            UpdateTextThreadSafe($"{clientInfo} sent file: {fileName}", richTextBox1);
        }

        private async void FileButton_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                string fileKey = btn.Tag.ToString();
                string fileName = btn.Text;
                string fileExtension = Path.GetExtension(fileName);

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    if (!string.IsNullOrEmpty(fileExtension))
                    {
                        string filterName = fileExtension.TrimStart('.').ToUpper() + " files";
                        sfd.Filter = $"{filterName} (*{fileExtension})|*{fileExtension}|All files (*.*)|*.*";
                    }
                    else
                    {
                        sfd.Filter = "All files (*.*)|*.*";
                    }

                    sfd.FileName = fileName;
                    sfd.DefaultExt = fileExtension;

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        // Request file from server
                        clientSocket.Send(Encoding.UTF8.GetBytes($"GETF:{fileKey}"));

                        // Receive size information first
                        byte[] sizeBuffer = new byte[_buff_size];
                        int bytesRead = clientSocket.Receive(sizeBuffer);
                        string sizeResponse = Encoding.UTF8.GetString(sizeBuffer, 0, bytesRead);

                        if (!sizeResponse.StartsWith("SIZE:"))
                        {
                            throw new Exception("Invalid response from server");
                        }

                        string sizeStr = sizeResponse.Substring(5);
                        if (!int.TryParse(sizeStr, out int fileSize))
                        {
                            throw new Exception("Invalid file size format");
                        }

                        // Receive file data
                        byte[] fileData = new byte[fileSize];
                        int totalReceived = 0;

                        while (totalReceived < fileSize)
                        {
                            int currentReceived = clientSocket.Receive(fileData, totalReceived,
                                fileSize - totalReceived, SocketFlags.None);

                            if (currentReceived == 0)
                            {
                                throw new Exception("Connection closed by server");
                            }

                            totalReceived += currentReceived;
                        }

                        // Save file
                        File.WriteAllBytes(sfd.FileName, fileData);
                        MessageBox.Show("File downloaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void readingClientSocket()
        {
            byte[] buffer = new byte[_buff_size];
            while (clientSocket != null && clientSocket.Connected)
            {
                try
                {
                    if (clientSocket.Available > 0)
                    {
                        int bytesRead = clientSocket.Receive(buffer);
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        if (message.StartsWith("FILE_INFO|"))
                        {
                            string[] parts = message.Split('|');
                            string clientInfo = parts[1];
                            string fileName = parts[2];
                            string fileKey = parts[3];
                            AddFileButton(clientInfo, fileName, fileKey);
                        }
                        else
                        {
                            UpdateTextThreadSafe("Message forwarded by server from " + message, richTextBox1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    UpdateTextThreadSafe($"Error receiving data: {ex.Message}", richTextBox1);
                    break;
                }
            }
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