using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

using System.IO;

namespace KcciImageEditor
{
    public partial class imgEditor : Form
    {
        public imgEditor()
        {
            InitializeComponent();

            BindDomainIUpDown();
        }

        private Image Img;
        private Size OriginalImageSize;
        private Size ModifiedImageSize;

        public bool Makeselection = false;

        int cropX;
        int cropY;
        int cropWidth;

        int cropHeight;
        const int pictureboxMax_x = 800;
        const int pictureboxMax_y = 700;
        public Pen cropPen;

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            Img = null;

            labSize.Text = "0Kb";
            label8.Text = "-";
            btnCrop.Enabled = false;
            Makeselection = false;

            qualityPer.Text = "100";

            OpenFileDialog Dlg = new OpenFileDialog();
            Dlg.Filter = "";
            Dlg.Title = "Select image";
            if (Dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //FromFile을 이용할 경우 파일을 프로그램 이용시 사용중으로 지속됨.(삭제 및 변경 불가시 사용)
                //Img = Image.FromFile(Dlg.FileName);
                using(FileStream fsIn = new FileStream(Dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Img = Image.FromStream(fsIn);
                }

                //Image.FromFile(String) method creates an image from the specifed file, here dlg.Filename contains the name of the file from which to create the image
                LoadImage();

                FileInfo fInfo = new FileInfo(Dlg.FileName);
                string strFileSize = GetFileSize(fInfo.Length);

                labSize.Text = strFileSize;
            }
            if (PictureBox1.Image != null)
            {
                btnSave.Enabled = true;
                btnResize.Enabled = true;
                btnRotateLeft.Enabled = true;
                btnRotateRight.Enabled = true;
                btnRotatevertical.Enabled = true;
                btnRotateHorizantal.Enabled = true;
                btnMakeSelection.Enabled = true;
            }
            else {
                btnSave.Enabled = false;
                btnSave.Enabled = false;
                btnResize.Enabled = false;
                btnRotateLeft.Enabled = false;
                btnRotateRight.Enabled = false;
                btnRotatevertical.Enabled = false;
                btnRotateHorizantal.Enabled = false;
                btnMakeSelection.Enabled = false;
            }

        }

        private string GetFileSize(double byteCount)
        {
            string size = "0 Bytes";
            if (byteCount >= 1073741824.0)
                size = String.Format("{0:##.##}", byteCount / 1073741824.0) + " GB";
            else if (byteCount >= 1048576.0)
                size = String.Format("{0:##.##}", byteCount / 1048576.0) + " MB";
            else if (byteCount >= 1024.0)
                size = String.Format("{0:##.##}", byteCount / 1024.0) + " KB";
            else if (byteCount > 0 && byteCount < 1024.0)
                size = byteCount.ToString() + " Bytes";

            return size;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private void LoadImage()
        {
            //we set the picturebox size according to image, we can get image width and height with the help of Image.Width and Image.height properties.
            int imgWidth = Img.Width;
            int imghieght = Img.Height;
            PictureBox1.Width = imgWidth;
            PictureBox1.Height = imghieght;
            PictureBox1.Image = Img;
            OriginalImageSize = new Size(imgWidth, imghieght);

            label8.Text = imgWidth + " X " + imghieght;

            SetResizeInfo();

            CenterPictureBox(PictureBox1);
        }

    
        private void CenterPictureBox(PictureBox picBox)
        {  
            picBox.Location = new Point((picBox.Parent.ClientSize.Width / 2) - (picBox.Width / 2),
                                        (picBox.Parent.ClientSize.Height / 2) - (picBox.Height / 2));
            picBox.Refresh();
        }

        private void SetResizeInfo()
        {
            //lbloriginalSize.Text = OriginalImageSize.ToString();
            //lblModifiedSize.Text = ModifiedImageSize.ToString();
            // picturebox보다 사진 크기가 크면 picturesize에 비율대로 맞춘다.
            double resizeWidth;
            double resizeHeight;
            if (pictureboxMax_x < OriginalImageSize.Width || pictureboxMax_y < OriginalImageSize.Height)
            {
                //가로가 긴 사진일 경우
                if(OriginalImageSize.Width > OriginalImageSize.Height)
                {
                    resizeWidth = pictureboxMax_x;
                    resizeHeight = Math.Round(((double)OriginalImageSize.Height * resizeWidth) / (double)OriginalImageSize.Width);
                }
                else//세로가 긴 사진일 경우
                {
                    resizeHeight = pictureboxMax_y;
                    resizeWidth = Math.Round(((double)OriginalImageSize.Width * resizeHeight) / (double)OriginalImageSize.Height);
                }
            }
            else // 최대사이즈보다 작으면 원본 그대로
            {
                resizeWidth = (double)OriginalImageSize.Width;
                resizeHeight = (double)OriginalImageSize.Height;
            }

            ModifiedImageSize = new Size((int)resizeWidth, (int)resizeHeight);
            Bitmap bm_source = new Bitmap(PictureBox1.Image);
            // Make a bitmap for the result.
            Bitmap bm_dest = new Bitmap(Convert.ToInt32(resizeWidth), Convert.ToInt32(resizeHeight));
            // Make a Graphics object for the result Bitmap.
            Graphics gr_dest = Graphics.FromImage(bm_dest);
            // Copy the source image into the destination bitmap.
            gr_dest.DrawImage(bm_source, 0, 0, bm_dest.Width, bm_dest.Height);
            // Display the result.
            PictureBox1.Image = bm_dest;
            PictureBox1.Width = bm_dest.Width;
            PictureBox1.Height = bm_dest.Height;
        }

        private void btnResize_Click(object sender, EventArgs e)
        {
            Bitmap bm_source = new Bitmap(PictureBox1.Image);
            // Make a bitmap for the result.
            Bitmap bm_dest = new Bitmap(Convert.ToInt32(ModifiedImageSize.Width), Convert.ToInt32(ModifiedImageSize.Height));
            // Make a Graphics object for the result Bitmap.
            Graphics gr_dest = Graphics.FromImage(bm_dest);
            // Copy the source image into the destination bitmap.
            gr_dest.DrawImage(bm_source, 0, 0, bm_dest.Width, bm_dest.Height);
            // Display the result.
            PictureBox1.Image = bm_dest;
            PictureBox1.Width = bm_dest.Width;
            PictureBox1.Height = bm_dest.Height;
        }

        private void BindDomainIUpDown()
        {
            for (int i = 1; i <= 999; i++)
            {
                DomainUpDown1.Items.Add(i);
            }
            DomainUpDown1.Text = "100";
        }

        private void DomainUpDown1_SelectedItemChanged(object sender, EventArgs e)
        {
            int percentage = 0;
            try
            {
                percentage = Convert.ToInt32(DomainUpDown1.Text);
                ModifiedImageSize = new Size((OriginalImageSize.Width * percentage) / 100, (OriginalImageSize.Height * percentage) / 100);
                //SetResizeInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid Percentage");
                return;
            }

        }
        private void btnRotateLeft_Click(object sender, EventArgs e)
        {
            PictureBox1.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            PictureBox1.Size = PictureBox1.Image.Size;
            PictureBox1.Refresh();
            CenterPictureBox(PictureBox1);
        }

        private void btnRotateRight_Click(object sender, EventArgs e)
        {
            PictureBox1.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            PictureBox1.Size = PictureBox1.Image.Size;
            PictureBox1.Refresh();
            CenterPictureBox(PictureBox1);
        }

        private void btnRotateHorizantal_Click(object sender, EventArgs e)
        {
            PictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipX);
            PictureBox1.Size = PictureBox1.Image.Size;
            PictureBox1.Refresh();
            CenterPictureBox(PictureBox1);
        }

        private void btnRotatevertical_Click(object sender, EventArgs e)
        {
            PictureBox1.Image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            PictureBox1.Size = PictureBox1.Image.Size;
            PictureBox1.Refresh();
            CenterPictureBox(PictureBox1);
        }

        private void btnMakeSelection_Click(object sender, EventArgs e)
        {
            Makeselection = true;
            btnCrop.Enabled = true;
        }

        private void btnCrop_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;

            try
            {
                if (cropWidth < 1)
                {
                    return;
                }
                Rectangle rect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
                //First we define a rectangle with the help of already calculated points
                Bitmap OriginalImage = new Bitmap(PictureBox1.Image, PictureBox1.Width, PictureBox1.Height);
                //Original image
                Bitmap _img = new Bitmap(cropWidth, cropHeight);
                // for cropinf image
                Graphics g = Graphics.FromImage(_img);
                // create graphics
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                //set image attributes
                g.DrawImage(OriginalImage, 0, 0, rect, GraphicsUnit.Pixel);

                PictureBox1.Image = _img;
                PictureBox1.Width = _img.Width;
                PictureBox1.Height = _img.Height;
                btnCrop.Enabled = false;
                Makeselection = false;

                CenterPictureBox(PictureBox1);
            }
            catch (Exception ex)
            {
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Default;
            if (Makeselection)
            {
                try
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        Cursor = Cursors.Cross;
                        cropX = e.X;
                        cropY = e.Y;

                        cropPen = new Pen(Color.Black, 1);
                        cropPen.DashStyle = DashStyle.DashDotDot;
                    }
                    PictureBox1.Refresh();
                }
                catch (Exception ex)
                {
                }
            }            
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (Makeselection)
            {
                Cursor = Cursors.Default;
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {   
            Cursor = Cursors.Default;
            if (Makeselection)
            {

                try
                {
                    if (PictureBox1.Image == null)
                        return;

                    if (e.Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        PictureBox1.Refresh();
                        cropWidth = e.X - cropX;
                        cropHeight = e.Y - cropY;
                        PictureBox1.CreateGraphics().DrawRectangle(cropPen, cropX, cropY, cropWidth, cropHeight);
                    }
                }
                catch (Exception ex)
                {
                    //if (ex.Number == 5)
                    //    return;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfdlg = new SaveFileDialog())
            {
                sfdlg.Title = "Save Dialog";
                sfdlg.DefaultExt = "jpg";
                sfdlg.Filter = "JPG images (*.jpg)|*.jpg";
                sfdlg.FileName = "default.jpg";

                if (sfdlg.ShowDialog(this) == DialogResult.OK)
                {
                    using (Bitmap bmp = new Bitmap(PictureBox1.Width, PictureBox1.Height))
                    {
                        PictureBox1.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                        PictureBox1.Image = new Bitmap(PictureBox1.Width, PictureBox1.Height);

                        var fileName = sfdlg.FileName;
                        if (!System.IO.Path.HasExtension(fileName) || System.IO.Path.GetExtension(fileName) != ".jpg")
                            fileName = fileName + ".jpg";

                        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

                        // Create an Encoder object based on the GUID  
                        // for the Quality parameter category.  
                        System.Drawing.Imaging.Encoder myEncoder =
                            System.Drawing.Imaging.Encoder.Quality;

                        EncoderParameters myEncoderParameters = new EncoderParameters(1);

                        long _value = 100L;
                        _value = long.Parse(qualityPer.Text);

                        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, _value);
                        myEncoderParameters.Param[0] = myEncoderParameter;
                      
                        bmp.Save(fileName, jpgEncoder, myEncoderParameters);
                        MessageBox.Show("저장이 완료되었습니다.");

                        btnSave.Enabled = false;
                        Makeselection = false;
                        btnCrop.Enabled = false;

                        btnResize.Enabled = false;
                        btnRotateLeft.Enabled = false;
                        btnRotateRight.Enabled = false;
                        btnRotatevertical.Enabled = false;
                        btnRotateHorizantal.Enabled = false;
                        btnMakeSelection.Enabled = false;

                        labSize.Text = "0Kb";
                        label8.Text = "-";

                        qualityPer.Text = "100";

                        PictureBox1.Image = null;

                        CenterPictureBox(PictureBox1);
                    }
                }
            }
        }
        private void qualityPer_SelectedItemChanged(object sender, EventArgs e)
        {
            if(PictureBox1.Image == null)
            {
                return;
            }
            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            System.Drawing.Imaging.Encoder myEncoder =
                            System.Drawing.Imaging.Encoder.Quality;

            Image img = PictureBox1.Image;

            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            long _value = 100L;

            if (long.TryParse(qualityPer.Text, out _value))
            {
                if(_value > 100)
                {
                    qualityPer.Text = "100";
                }

                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, _value);
                myEncoderParameters.Param[0] = myEncoderParameter;

                using (MemoryStream jpgStream = new MemoryStream())
                {
                    img.Save(jpgStream, jpgEncoder, myEncoderParameters);
                    labSize.Text = GetFileSize(jpgStream.ToArray().Length);
                }
            }
        }
    }
}
