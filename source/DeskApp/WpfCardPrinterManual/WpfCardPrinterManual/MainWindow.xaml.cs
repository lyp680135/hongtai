using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
using WpfCardPrinter.Utils;
using WpfCardPrinterManual.Model;

namespace WpfCardPrinterManual
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string mLabelPageWidth = ConfigurationManager.AppSettings["PageWidth"];
        public string mLabelPageHeight = ConfigurationManager.AppSettings["PageHeight"];
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 更改配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Change_config(object sender, RoutedEventArgs e)
        {
            OffsetConfig cgw = new OffsetConfig();
            cgw.Title = "更改配置";
            cgw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            cgw.updataSelectHandler += InitLabel;
            cgw.Owner = this;
            cgw.ShowDialog();
        }
        /// <summary>
        /// 打印标牌
        /// </summary>
        /// <param name="product">产品</param>
        /// <param name="printing">打印队列位置</param>
        /// <param name="count">打印队列长度</param>
        public void DoPrint(PdProduct product, int printing, int count, bool shouldreload = false)
        {

        }
        public void InitLabel()
        {
            if (!string.IsNullOrEmpty(mLabelPageWidth) && !string.IsNullOrEmpty(mLabelPageHeight))
            {
                int w = 0, h = 0;
                int.TryParse(mLabelPageWidth, out w);
                int.TryParse(mLabelPageHeight, out h);

                double batcodeTop = Canvas.GetTop(this.lbLabelBatcode);

                double scale = 1;

                if (w > 0 && h > 0)
                {
                    this.panelLabel.Width = w;
                    this.panelLabel.Height = h;

                    if (w > this.panelLabel.MaxWidth)
                    {
                        scale = (this.panelLabel.MaxWidth / w);
                        this.panelLabel.Width = this.panelLabel.MaxWidth;
                        this.panelLabel.Height = h * scale;
                    }
                    else
                    {
                        if (h > this.panelLabel.MaxHeight)
                        {
                            scale = (this.panelLabel.MaxHeight / h);
                            this.panelLabel.Height = this.panelLabel.MaxHeight;
                            this.panelLabel.Width = w * scale;
                        }
                    }
                }

                int offsetX = 0, offsetY = 0, fontsize = 0;

                string OffsetX = ConfigurationManager.AppSettings["OffsetX_DEMO"];
                string OffsetY = ConfigurationManager.AppSettings["OffsetY_DEMO"];
                string FontSize = ConfigurationManager.AppSettings["FontSize"];
                string ProductClassPoint = ConfigurationManager.AppSettings["ProductClassPoint"];
                string GBStandardPoint = ConfigurationManager.AppSettings["GBStandardPoint"];
                string MaterialPoint = ConfigurationManager.AppSettings["MaterialPoint"];
                string BatcodePoint = ConfigurationManager.AppSettings["BatcodePoint"];
                string RandomcodePoint = ConfigurationManager.AppSettings["RandomcodePoint"];
                string SpecPoint = ConfigurationManager.AppSettings["SpecPoint"];
                string LengthPoint = ConfigurationManager.AppSettings["LengthPoint"];
                string WeightPoint = ConfigurationManager.AppSettings["WeightPoint"];
                string PiececountPoint = ConfigurationManager.AppSettings["PiececountPoint"];
                string DatePoint = ConfigurationManager.AppSettings["DatePoint"];
                string BundlecodePoint = ConfigurationManager.AppSettings["BundlecodePoint"];
                string WorkshiftPoint = ConfigurationManager.AppSettings["WorkshiftPoint"];
                string QAInspectorsPoint = ConfigurationManager.AppSettings["QAInspectorsPoint"];
                string QRCodePoint = ConfigurationManager.AppSettings["QRCodePoint"];
                string QRCodeWidth = ConfigurationManager.AppSettings["QRCodeWidth"];

                int.TryParse(OffsetX, out offsetX);
                int.TryParse(OffsetY, out offsetY);
                int.TryParse(FontSize, out fontsize);

                if (!string.IsNullOrEmpty(BatcodePoint))
                {
                    var bpoint = CommonUtils.GetPointFromSetting(BatcodePoint);
                    if (bpoint != null)
                    {
                        if (scale == 1)
                        {
                            scale = batcodeTop / bpoint.Y;
                        }
                    }
                }

                //是否显示品名
                if (!string.IsNullOrEmpty(ProductClassPoint))
                {
                    var point = CommonUtils.GetPointFromSetting(ProductClassPoint);
                    Canvas.SetTop(lbLabelProductClass, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelProductClass, (point.X + offsetX) * scale);
                }

                //是否显示国家生产标准
                if (!string.IsNullOrEmpty(GBStandardPoint))
                {
                    var point = CommonUtils.GetPointFromSetting(GBStandardPoint);
                    Canvas.SetTop(lbLabelGBStandard, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelGBStandard, (point.X + offsetX) * scale);
                }

                //是否显示材质（牌号）
                if (!string.IsNullOrEmpty(MaterialPoint))
                {
                    var point = CommonUtils.GetPointFromSetting(MaterialPoint);
                    Canvas.SetTop(lbLabelMaterial, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelMaterial, (point.X + offsetX) * scale);
                }

                //是否显示批号
                if (!string.IsNullOrEmpty(BatcodePoint))
                {
                    var point = CommonUtils.GetPointFromSetting(BatcodePoint);
                    Canvas.SetTop(lbLabelBatcode, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelBatcode, (point.X + offsetX) * scale);
                }

                //是否显示检验码
                if (!string.IsNullOrEmpty(RandomcodePoint))
                {
                    var point = CommonUtils.GetPointFromSetting(RandomcodePoint);
                    Canvas.SetTop(lbLabelRandomCode, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelRandomCode, (point.X + offsetX) * scale);
                }
                else
                {
                    lbLabelRandomCode.Visibility = System.Windows.Visibility.Hidden;
                }

                //是否显示规格
                if (!string.IsNullOrEmpty(SpecPoint))
                {
                    var point = CommonUtils.GetPointFromSetting(SpecPoint);
                    Canvas.SetTop(lbLabelSpec, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelSpec, (point.X + offsetX) * scale);
                }

                //是否显示长度
                if (!string.IsNullOrEmpty(LengthPoint))
                {
                    var point = CommonUtils.GetPointFromSetting(LengthPoint);
                    Canvas.SetTop(lbLabelLength, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelLength, (point.X + offsetX) * scale);
                }
                else
                {
                    lbLabelLength.Visibility = System.Windows.Visibility.Hidden;
                }

                //是否显示重量
                if (!string.IsNullOrEmpty(WeightPoint))
                {
                    var point = CommonUtils.GetPointFromSetting(WeightPoint);
                    Canvas.SetTop(lbLabelWeight, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelWeight, (point.X + offsetX) * scale);
                }

                //是否显示捆号
                if (!string.IsNullOrEmpty(BundlecodePoint))
                {
                    var point = CommonUtils.GetPointFromSetting(BundlecodePoint);
                    Canvas.SetTop(lbLabelBundleCode, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelBundleCode, (point.X + offsetX) * scale);
                }
                else
                {
                    lbLabelBundleCode.Visibility = System.Windows.Visibility.Hidden;
                }

                //是否显示支数
                if (!string.IsNullOrEmpty(PiececountPoint))
                {
                    var point = CommonUtils.GetPointFromSetting(PiececountPoint);
                    Canvas.SetTop(lbLabelPiececount, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelPiececount, (point.X + offsetX) * scale);
                }

                //是否显示时间
                if (!string.IsNullOrEmpty(DatePoint))
                {
                    var point = CommonUtils.GetPointFromSetting(DatePoint);
                    Canvas.SetTop(lbLabelTime, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelTime, (point.X + offsetX) * scale);
                }

                //是否显示班组
                if (!string.IsNullOrEmpty(WorkshiftPoint))
                {
                    var point = CommonUtils.GetPointFromSetting(WorkshiftPoint);
                    Canvas.SetTop(lbLabelWorkshift, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelWorkshift, (point.X + offsetX) * scale);
                }
                else
                {
                    lbLabelWorkshift.Visibility = System.Windows.Visibility.Hidden;
                }

                //是否显示质检员
                if (!string.IsNullOrEmpty(QAInspectorsPoint))
                {
                    var point = CommonUtils.GetPointFromSetting(QAInspectorsPoint);
                    Canvas.SetTop(lbLabelQAInspectors, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(lbLabelQAInspectors, (point.X + offsetX) * scale);
                }

                //是否显示二维码
                if (!string.IsNullOrEmpty(QRCodePoint))
                {
                    var point = CommonUtils.GetPointFromSetting(QRCodePoint);
                    Canvas.SetTop(imgLabelQRcode, (point.Y + offsetY) * scale);
                    Canvas.SetLeft(imgLabelQRcode, (point.X + offsetX) * scale);
                }
            }
            else
            {
                //把默认参数写进配置文件
                Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                cfa.AppSettings.Settings["PageWidth"].Value = string.Format("{0}", this.panelLabel.Width);
                cfa.AppSettings.Settings["PageHeight"].Value = string.Format("{0}", this.panelLabel.Height);
                cfa.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                mLabelPageWidth = ConfigurationManager.AppSettings["PageWidth"];
                mLabelPageHeight = ConfigurationManager.AppSettings["PageHeight"];
            }
        }
        /// <summary>
        /// 加载进度条
        /// </summary>
        public void ShowLoading()
        {
            lbError.Content = "";
            lbProc.Content = "";
            lbTip.Content = "正在加载数据，请稍等...";
            mask.Visibility = Visibility.Visible;
            loading.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 关闭进度条
        /// </summary>
        public void HideLoading()
        {
            mask.Visibility = Visibility.Hidden;
            loading.Visibility = Visibility.Hidden;
        }

        public void SetLoadingValue(double progvalue, string tips, string proc)
        {
            progress.Value = progvalue;
            lbProc.Content = proc;

            if (!string.IsNullOrEmpty(tips))
            {
                lbTip.Content = tips;
            }
        }

        public void SetLoadingErrorValue(double progvalue, string tips, string proc_err)
        {
            progress.Value = progvalue;
            lbError.Content += proc_err + " ";

            if (!string.IsNullOrEmpty(tips))
            {
                lbTip.Content = tips;
            }

        }
    }
}
