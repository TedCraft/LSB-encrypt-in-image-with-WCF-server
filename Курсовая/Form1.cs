using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel;
using System.IO;

namespace Курсовая
{
    public partial class Form1 : Form, ServiceImgSend.IServiceLSBCallback
    {
        Bitmap img;
        string fileName;
        bool isConnected = false;
        ServiceImgSend.ServiceLSBClient client;
        int id;
        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 1;
        }

        private void Connect()
        {
            if(!isConnected)
            {
                client = new ServiceImgSend.ServiceLSBClient(new InstanceContext(this));
                try
                {
                    id = client.Connect(textBox2.Text);
                }
                catch (Exception)
                {
                    MessageBox.Show("Сервер недоступен!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                textBox3.Text = "Ваш ID: " + id.ToString();
                textBox2.Enabled = false;
                isConnected = true;
            }
        }

        private void Disconnect()
        {
            if (isConnected)
            {
                try
                {
                    client.Disconnect(id);
                }
                catch (Exception)
                {
                }

                client = null;
                textBox3.Text = "";
                textBox2.Enabled = true;
                isConnected = false;
            }
        }

        private string getBytes(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in Encoding.Unicode.GetBytes(text))
                sb.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
            return sb.ToString();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                img = new Bitmap(ofd.FileName);
                fileName = ofd.FileName;
                pictureBox1.Image = img;
            }
        }

        private byte replaceColor(byte color, ref List<string> bytes, int bytesNum)
        {
            if (bytes.Count != 0)
            {
                string newColor = Convert.ToString(color, 2).PadLeft(8, '0').Substring(0, 8 - bytesNum) + bytes[0];
                bytes.RemoveAt(0);
                return Convert.ToByte(newColor, 2);
            }
            else return color;
        }

        private void Encode(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                if (textBox1.Text == "") return;
                string byteText = getBytes(textBox1.Text.ToString());
                int bytes = Convert.ToInt32(comboBox1.Text);
                while (byteText.Length % bytes != 0)
                    byteText += "0";
                string bytesCount = Convert.ToString(Convert.ToInt32(Math.Ceiling(byteText.Length/(double)bytes)), 2).PadLeft(16, '0');
                byteText = bytesCount + byteText;

                List<string> byteMas = Enumerable.Range(0, byteText.Length / bytes)
                    .Select(i => byteText.Substring(i * bytes, bytes)).ToList();

                if (img.Width * img.Height * 3 < byteMas.Count)
                {
                    MessageBox.Show("Изображение недостаточного размера (попробуйте уменьшить количество бит!)");
                    return;
                }

                bool mark = false;
                for(int i = 0; i < img.Height; i++)
                {
                    if (mark) break;
                    for(int j = 0; j < img.Width; j++)
                    {
                        if (byteMas.Count != 0)
                        {
                            Color color = img.GetPixel(j, i);
                            byte r = replaceColor(color.R, ref byteMas, bytes);
                            img.SetPixel(j, i, Color.FromArgb(color.A, r, color.G, color.B));
                        }
                        else
                        {
                            mark = true;
                            break;
                        }
                    }
                }
                pictureBox1.Image = img;
            }
            else
            {
                MessageBox.Show("Выберите изображение!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileName != "" && fileName != null) {
                img.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img != null)
            {
                var ofd = new SaveFileDialog();
                ofd.Filter = "Image Files|*.png;";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    img.Save(ofd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    fileName = ofd.FileName;
                    pictureBox1.Image = img;
                }
            }
        }

        private void Decode(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                int bytes = Convert.ToInt32(comboBox1.Text);
                string bytesCount = "";
                for(int i = 0; i < Math.Ceiling(16.0/bytes); i++)
                {
                    bytesCount += Convert.ToString(img.GetPixel(i, 0).R, 2).PadLeft(8, '0').Substring(8-bytes);
                }

                string byteStroke = "";
                while (bytesCount.Length > 16)
                {
                    byteStroke = bytesCount[bytesCount.Length - 1] + byteStroke;
                    bytesCount = bytesCount.Remove(bytesCount.Length - 1, 1);
                }
                int count = Convert.ToInt32(bytesCount, 2);

                bool mark = false;
                for (int i = 0; i < img.Height; i++)
                {
                    if (mark) break;
                    for (int j = i > 0 ? 0 : Convert.ToInt32(Math.Ceiling(16.0 / bytes)); j < img.Width; j++)
                    {
                        if (count > 0)
                        {
                            byteStroke += Convert.ToString(img.GetPixel(j, i).R, 2).PadLeft(8, '0').Substring(8 - bytes);
                            count--;
                        }
                        else
                        {
                            mark = true;
                            break;
                        }
                    }
                }

                while (byteStroke.Length % 16 != 0)
                    byteStroke = byteStroke.Remove(byteStroke.Length - 1, 1);

                List<string> strByteMas = Enumerable.Range(0, byteStroke.Length / 8)
                    .Select(i => byteStroke.Substring(i * 8, 8)).ToList();

                byte[] byteMas = strByteMas.Select(i => Convert.ToByte(i, 2)).ToArray();
                textBox1.Text = Encoding.Unicode.GetString(byteMas);
            }
            else
            {
                MessageBox.Show("Выберите изображение!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox2.Text != "")
            {
                Connect();
            }
            else
            {
                MessageBox.Show("Введите имя пользователя!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        public void ImgCallback(string msg, byte[] image)
        {
            DialogResult dialogResult = MessageBox.Show(msg, "Получено изображение!", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                MemoryStream ms = new MemoryStream(image);
                img = (Bitmap)Bitmap.FromStream(ms);
                pictureBox1.Image = img;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }

        private void отправитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(client == null || client.State == CommunicationState.Faulted)
            {
                MessageBox.Show("Вы были отключены от сервера! Попробуйте подключиться снова!");
                client = null;
                textBox3.Text = "";
                textBox2.Enabled = true;
                isConnected = false;
                return;
            }
            if (isConnected)
            {
                if(img == null)
                {
                    MessageBox.Show("Выберите изображение!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                SelectUser form = new SelectUser();
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        int to = Convert.ToInt32(form.textBox1.Text);
                        if (to != 0)
                        {
                            MemoryStream ms = new MemoryStream();
                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                            //get the buffer 
                            byte[] bitmapBytes = ms.GetBuffer();
                            MessageBox.Show(client.SendImg(form.textBox2.Text, bitmapBytes, id, to));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                form.Dispose();
            }
            else {
                MessageBox.Show("Вы не подключены к серверу!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}