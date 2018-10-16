using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfQualityCertPrinter
{
    /// <summary>
    /// ImageViewerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ImageViewerWindow : Window
    {
        private bool mouseDown;
        private System.Windows.Point mouseXY;

        public ImageViewerWindow()
        {
            InitializeComponent();
        }

        public void SetSource(ImageSource source)
        {
            imgPreview.Source = source;
        }

        private void imgPreview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            img.CaptureMouse();
            mouseDown = true;
            mouseXY = e.GetPosition(img);
        }

        private void imgPreview_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            img.ReleaseMouseCapture();
            mouseDown = false;
        }

        private void imgPreview_MouseMove(object sender, MouseEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            if (mouseDown)
            {
                Domousemove(img, e);
            }
        }

        private void Domousemove(ContentControl img, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            var group = imgPreview.FindResource("Imageview") as TransformGroup;
            var transform = group.Children[1] as TranslateTransform;
            var position = e.GetPosition(img);
            transform.X -= mouseXY.X - position.X;
            transform.Y -= mouseXY.Y - position.Y;
            mouseXY = position;
        }

        private void imgPreview_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }
            var point = e.GetPosition(img);
            var group = imgPreview.FindResource("Imageview") as TransformGroup;
            var delta = e.Delta * 0.001;
            DowheelZoom(group, point, delta);
        }

        private void DowheelZoom(TransformGroup group, System.Windows.Point point, double delta)
        {
            var pointToContent = group.Inverse.Transform(point);
            var transform = group.Children[0] as ScaleTransform;
            if (transform.ScaleX + delta < 0.1) return;
            transform.ScaleX += delta;
            transform.ScaleY += delta;
            var transform1 = group.Children[1] as TranslateTransform;
            transform1.X = -1 * ((pointToContent.X * transform.ScaleX) - point.X);
            transform1.Y = -1 * ((pointToContent.Y * transform.ScaleY) - point.Y);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuBigger_Click(object sender, RoutedEventArgs e)
        {
            var point = new System.Windows.Point(this.Width / 2, this.Height / 2);
            var group = imgPreview.FindResource("Imageview") as TransformGroup;
            var delta = 2;
            DowheelZoom(group, point, delta);
        }

        private void MenuSmaller_Click(object sender, RoutedEventArgs e)
        {
            var point = new System.Windows.Point(this.Width / 2, this.Height / 2);
            var group = imgPreview.FindResource("Imageview") as TransformGroup;
            var delta = -2;
            DowheelZoom(group, point, delta);
        }

        private void MenuSaveTo_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "图像文件(*.png)|*.png|*.*|";
            dialog.FileName = "cert";
            dialog.DefaultExt = "png";
            dialog.RestoreDirectory = true;

            if (dialog.ShowDialog(this) == true)
            {
                //获得文件路径
                string filepath = dialog.FileName.ToString();

                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imgPreview.Source));
                encoder.Save(ms);

                Bitmap bmp = new Bitmap(ms);

                System.Drawing.Image bmd = System.Drawing.Image.FromStream(ms, true);
                bmd.Save(filepath, System.Drawing.Imaging.ImageFormat.Png);

                ms.Close();
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }  
    }
}
