using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Picture_viewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true; // Cho phép form nhận sự kiện phím trước khi truyền cho control
            this.KeyDown += Form1_KeyDown; // Đăng ký sự kiện KeyDown
        }
        //Định nghĩa KeyDown 
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right) // Kiểm tra nếu phím phải được nhấn
            {
                Next_Click(sender, e); // Gọi phương thức Next_Click
            }
            else if (e.KeyCode == Keys.Left) // Kiểm tra nếu phím trái được nhấn
            {
                Back_Click(sender, e); // Gọi phương thức Back_Click
            }
        }

        // Danh sách lưu đường dẫn các file ảnh
        private List<string> imageFiles = new List<string>();

        // Chỉ số của ảnh hiện tại đang hiển thị
        private int currentImageIndex = -1;
        private void Open_Click(object sender, EventArgs e)
        {
            // Open Folder Browser Dialog to select folder
            FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string selectedFolderPath = folderBrowserDialog1.SelectedPath;

                // Gán file ảnh vào biến toàn cục imageFiles
                imageFiles = Directory.GetFiles(selectedFolderPath)
                    .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                                f.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                    .ToList();  // Chuyển sang List để lưu vào biến toàn cục

                path.Text = selectedFolderPath;

                // Clear the panel before adding new pictures
                listPic.Controls.Clear();

                // Dynamically load images into PictureBox controls and add them to panel1
                int xOffset = 10;  // Horizontal spacing between images
                int yOffset = 10;  // Vertical spacing between images
                int imageWidth = 200;  // Set image width (adjust as needed)
                int imageHeight = 200; // Set image height (adjust as needed)

                foreach (string imageFile in imageFiles)
                {
                    // Create a new PictureBox
                    PictureBox pictureBox = new PictureBox
                    {
                        Image = Image.FromFile(imageFile),   // Load image
                        SizeMode = PictureBoxSizeMode.Zoom,  // Adjust image to fit
                        Width = imageWidth,
                        Height = imageHeight,
                        Location = new Point(xOffset, yOffset), // Set position in panel
                        Cursor = Cursors.Hand  // Thay đổi con trỏ chuột khi di chuyển lên hình
                    };
                    string fileName = System.IO.Path.GetFileName(imageFile);

                    // Tạo Label để hiển thị tên file
                    Label label = new Label
                    {
                        Text = fileName,
                        AutoSize = true,
                        ForeColor = Color.White,
                        Location = new Point(xOffset, yOffset + imageHeight + 5), // Đặt dưới ảnh
                        Cursor = Cursors.Hand  // Thay đổi con trỏ chuột khi di chuyển lên label
                    };
                    // Add PictureBox to panel1
                    listPic.Controls.Add(pictureBox);
                    listPic.Controls.Add(label);
                    // Thêm sự kiện Click cho PictureBox và Label

                    pictureBox.Click += (s, ev) => DisplayImageInMainPicView(imageFile);
                    label.Click += (s, ev) => DisplayImageInMainPicView(imageFile);

                    // Update offset for the next image (arrange them horizontally or vertically)
                    xOffset += imageWidth + 10;  // Adjust for next image's position
                }
            }
        }
        // Hàm để hiển thị hình ảnh trong MainPicView
        private void DisplayImageInMainPicView(string imageFilePath)
        {
            MainPicView.Image = Image.FromFile(imageFilePath); // Nạp ảnh vào MainPicView
            // Cập nhật chỉ số của ảnh hiện tại trong danh sách
            currentImageIndex = imageFiles.IndexOf(imageFilePath);
        }

        private void Close_Click(object sender, EventArgs e)
        {
            MainPicView.Image = null;
        }

        private void Back_Click(object sender, EventArgs e)
        {
            if (currentImageIndex > 0)
            {
                currentImageIndex--;  // Lùi lại ảnh trước đó
                DisplayImageInMainPicView(imageFiles[currentImageIndex]);
            }
        }

        private void Next_Click(object sender, EventArgs e)
        {
            if ((currentImageIndex < imageFiles.Count - 1)&&(currentImageIndex > -1))
            {
                currentImageIndex++;  // Tiến tới ảnh tiếp theo
                DisplayImageInMainPicView(imageFiles[currentImageIndex]);
            }
        }
    }
}
