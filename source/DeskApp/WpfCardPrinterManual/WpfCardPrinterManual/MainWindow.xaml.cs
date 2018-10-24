using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
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
using WpfCardPrinterManual.ModelAccess.SqliteAccess;

namespace WpfCardPrinterManual
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string mLabelPageWidth = ConfigurationManager.AppSettings["PageWidth"];
        public string mLabelPageHeight = ConfigurationManager.AppSettings["PageHeight"];
        public string myQRCodeUrlString = System.Configuration.ConfigurationManager.AppSettings["QRCodeUrlString"];
        public string mPrintNumber = ConfigurationManager.AppSettings["PrintNumber"];
        private PrintQueue mDefaultPrinter = null;
        int batprinted = 0;
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
            //获取配置
            int offsetX = 0, offsetY = 0, fontsize = 12;
            int w = 0, h = 0;
            int qrcodeWidth = 0;

            string DefaultPrinter = ConfigurationManager.AppSettings["DefaultPrinter"];
            string OffsetX = ConfigurationManager.AppSettings["OffsetX"];
            string OffsetY = ConfigurationManager.AppSettings["OffsetY"];
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

            int.TryParse(mLabelPageWidth, out w);
            int.TryParse(mLabelPageHeight, out h);

            int.TryParse(QRCodeWidth, out qrcodeWidth);
            this.Dispatcher.Invoke(new Action(() =>
            {
                DrawingVisual vis = new DrawingVisual();
                using (DrawingContext dc = vis.RenderOpen())
                {
                    var url = QRCodeUtils.GetShortUrl(myQRCodeUrlString);
                    System.Drawing.Bitmap qrcodebmp = QRCodeUtils.Generate(url, 300);
                    var pen = new Pen(Brushes.Black, 1);
                    Rect rect = new Rect(0, 0, w, h);
                    #region 质量标准
                    if (!string.IsNullOrEmpty(GBStandardPoint))
                    {
                        var point = CommonUtils.GetPointFromSetting(GBStandardPoint);

                        FormattedText formattedText = new FormattedText(
                        product.GBStandard,
                        CultureInfo.GetCultureInfo("zh-CHS"),
                        FlowDirection.LeftToRight,
                        new Typeface("微软雅黑"),
                        fontsize - 1,
                        Brushes.Black);
                        formattedText.SetFontWeight(FontWeight.FromOpenTypeWeight(900));

                        var position = new Point(point.X + offsetX, point.Y + offsetY);
                        dc.DrawText(formattedText, position);
                    }
                    #endregion

                    #region 牌号
                    if (!string.IsNullOrEmpty(MaterialPoint))
                    {
                        var point = CommonUtils.GetPointFromSetting(MaterialPoint);

                        FormattedText formattedText = new FormattedText(
                        product.MaterialName,
                        CultureInfo.GetCultureInfo("zh-CHS"),
                        FlowDirection.LeftToRight,
                        new Typeface("微软雅黑"),
                        fontsize,
                        Brushes.Black);
                        formattedText.SetFontWeight(FontWeight.FromOpenTypeWeight(900));

                        var position = new Point(point.X + offsetX, point.Y + offsetY);
                        dc.DrawText(formattedText, position);
                    }
                    #endregion

                    #region 轧制批号
                    if (!string.IsNullOrEmpty(BatcodePoint))
                    {
                        var point = CommonUtils.GetPointFromSetting(BatcodePoint);

                        FormattedText formattedText = new FormattedText(
                        product.Batcode,
                        CultureInfo.GetCultureInfo("zh-CHS"),
                        FlowDirection.LeftToRight,
                        new Typeface("微软雅黑"),
                        fontsize,
                        Brushes.Black);
                        formattedText.SetFontWeight(FontWeight.FromOpenTypeWeight(900));

                        var position = new Point(point.X + offsetX, point.Y + offsetY);
                        dc.DrawText(formattedText, position);
                    }
                    #endregion

                    #region 校验码
                    if (!string.IsNullOrEmpty(RandomcodePoint))
                    {
                        var point = CommonUtils.GetPointFromSetting(RandomcodePoint);

                        FormattedText formattedText = new FormattedText(
                        product.Randomcode,
                        CultureInfo.GetCultureInfo("zh-CHS"),
                        FlowDirection.LeftToRight,
                        new Typeface("微软雅黑"),
                        fontsize,
                        Brushes.Black);
                        formattedText.SetFontWeight(FontWeight.FromOpenTypeWeight(900));

                        var position = new Point(point.X + offsetX, point.Y + offsetY);
                        dc.DrawText(formattedText, position);
                    }
                    #endregion

                    #region 规格
                    if (!string.IsNullOrEmpty(SpecPoint))
                    {
                        var point = CommonUtils.GetPointFromSetting(SpecPoint);

                        FormattedText formattedText = new FormattedText(
                        product.SpecName,
                        CultureInfo.GetCultureInfo("zh-CHS"),
                        FlowDirection.LeftToRight,
                        new Typeface("微软雅黑"),
                        fontsize,
                        Brushes.Black);
                        formattedText.SetFontWeight(FontWeight.FromOpenTypeWeight(900));

                        var position = new Point(point.X + offsetX, point.Y + offsetY);
                        dc.DrawText(formattedText, position);
                    }
                    #endregion

                    #region 定尺
                    if (!string.IsNullOrEmpty(LengthPoint))
                    {
                        var point = CommonUtils.GetPointFromSetting(LengthPoint);
                        string lengthtxt = string.Format("{0}", product.Length);
                        FormattedText formattedText = new FormattedText(
                        lengthtxt,
                        CultureInfo.GetCultureInfo("zh-CHS"),
                        FlowDirection.LeftToRight,
                        new Typeface("微软雅黑"),
                        fontsize,
                        Brushes.Black);
                        formattedText.SetFontWeight(FontWeight.FromOpenTypeWeight(900));

                        var position = new Point(point.X + offsetX, point.Y + offsetY);
                        dc.DrawText(formattedText, position);
                    }
                    #endregion

                    #region 重量
                    if (!string.IsNullOrEmpty(WeightPoint))
                    {
                        //TODO：如果只显示理重
                        var point = CommonUtils.GetPointFromSetting(WeightPoint);
                        FormattedText formattedText = new FormattedText(
                        string.Format("{0}", product.Weight),
                        CultureInfo.GetCultureInfo("zh-CHS"),
                        FlowDirection.LeftToRight,
                        new Typeface("微软雅黑"),
                        fontsize,
                        Brushes.Black);
                        formattedText.SetFontWeight(FontWeight.FromOpenTypeWeight(900));

                        var position = new Point(point.X + offsetX, point.Y + offsetY);
                        dc.DrawText(formattedText, position);
                    }
                    #endregion

                    #region 支数
                    if (!string.IsNullOrEmpty(PiececountPoint))
                    {
                        var point = CommonUtils.GetPointFromSetting(PiececountPoint);
                        string piececounttxt = string.Format("{0}", product.Piececount);
                        FormattedText formattedText = new FormattedText(
                        piececounttxt,
                        CultureInfo.GetCultureInfo("zh-CHS"),
                        FlowDirection.LeftToRight,
                        new Typeface("微软雅黑"),
                        fontsize,
                        Brushes.Black);
                        formattedText.SetFontWeight(FontWeight.FromOpenTypeWeight(900));

                        var position = new Point(point.X + offsetX, point.Y + offsetY);
                        dc.DrawText(formattedText, position);
                    }

                    #endregion

                    #region 日期
                    if (!string.IsNullOrEmpty(DatePoint))
                    {
                        var point = CommonUtils.GetPointFromSetting(DatePoint);

                        FormattedText formattedText = new FormattedText(
                        TimeUtils.GetDateTimeFromUnixTime(product.Createtime.Value).ToString("yyyyMMdd"),
                        CultureInfo.GetCultureInfo("zh-CHS"),
                        FlowDirection.LeftToRight,
                        new Typeface("微软雅黑"),
                        fontsize,
                        Brushes.Black);
                        formattedText.SetFontWeight(FontWeight.FromOpenTypeWeight(900));

                        var position = new Point(point.X + offsetX, point.Y + offsetY);
                        dc.DrawText(formattedText, position);
                    }
                    #endregion

                    #region 捆号
                    if (!string.IsNullOrEmpty(BundlecodePoint))
                    {
                        var point = CommonUtils.GetPointFromSetting(BundlecodePoint);

                        FormattedText formattedText = new FormattedText(
                            product.Bundlecode,
                            CultureInfo.GetCultureInfo("zh-CHS"),
                            FlowDirection.LeftToRight,
                            new Typeface("微软雅黑"),
                            fontsize,
                            Brushes.Black);
                        formattedText.SetFontWeight(FontWeight.FromOpenTypeWeight(900));

                        var position = new Point(point.X + offsetX, point.Y + offsetY);
                        dc.DrawText(formattedText, position);
                    }
                    #endregion

                    #region 二维码
                    if (!string.IsNullOrEmpty(QRCodePoint))
                    {
                        var point = CommonUtils.GetPointFromSetting(QRCodePoint);

                        var bmprect = new Rect(point.X + offsetX, point.Y + offsetY, qrcodeWidth, qrcodeWidth);
                        dc.DrawImage(ImageUtils.ChangeBitmapToImageSource(qrcodebmp), bmprect);
                    }
                    #endregion

                    dc.Close();

                    //保存到本地
                    RenderTargetBitmap bitmap = new RenderTargetBitmap(
                    (int)(w * 1.3), (int)(h * 1.3), 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(vis);

                    int printcount = 0;
                    int.TryParse(mPrintNumber, out printcount);
                    if (printcount <= 0) printcount = 1;

                    PrintDialog dialog = new PrintDialog();
                    PrintTicket pt = dialog.PrintTicket;
                    pt.PageOrientation = PageOrientation.Landscape;
                    pt.CopyCount = printcount;

                    //从本地计算机中获取所有打印机对象(PrintQueue)
                    var printers = new LocalPrintServer().GetPrintQueues();
                    //选择一个打印机
                    var selectedPrinter = printers.FirstOrDefault(p => p.Name.Contains(DefaultPrinter));
                    if (selectedPrinter == null)
                    {
                        if (mDefaultPrinter == null)
                        {
                            LogHelper.WriteLog("没有找到默认打印机");
                        }
                    }
                    else
                    {
                        dialog.PrintQueue = selectedPrinter;
                    }

                    PageMediaSize pagesize = new PageMediaSize(PageMediaSizeName.Unknown, h, w);
                    pt.PageMediaSize = pagesize;
                    if (mDefaultPrinter == null)
                    {

                        if (dialog.ShowDialog() == true)
                        {
                            dialog.PrintVisual(vis, "标牌打印");

                            mDefaultPrinter = selectedPrinter;

                            Task.Factory.StartNew(() =>
                            {
                                Task task = new Task(() =>
                                {
                                    if (batprinted == count)
                                    {
                                        this.Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            HideLoading();
                                            batprinted = 0;
                                        }));
                                    }
                                    else
                                    {
                                        this.Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            batprinted++;

                                            if (batprinted <= count)
                                            {
                                                string formatstr = "正在批量打印第{0}件，共{1}件";
                                                SetLoadingValue(batprinted / count * 100, "正在批量打印中...", string.Format(formatstr, batprinted + 1, count));

                                                if (batprinted == count)
                                                {
                                                    this.Dispatcher.BeginInvoke(new Action(() =>
                                                    {
                                                        HideLoading();
                                                        batprinted = 0;

                                                    }));
                                                }
                                            }
                                        }));
                                    }
                                });

                                task.Start();
                                Task.WaitAll(task);
                            });
                        }
                    }
                    else
                    {
                        dialog.PrintVisual(vis, "标牌打印");

                        Task.Factory.StartNew(() =>
                        {
                            Task task = new Task(() =>
                            {
                                if (batprinted == count)
                                {
                                    this.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        HideLoading();
                                        batprinted = 0;
                                    }));
                                }
                                else
                                {
                                    this.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        batprinted++;

                                        if (batprinted <= count)
                                        {
                                            string formatstr = "正在批量打印第{0}件，共{1}件";
                                            SetLoadingValue(batprinted / count * 100, "正在批量打印中...", string.Format(formatstr, batprinted, count));

                                            if (batprinted == count)
                                            {
                                                this.Dispatcher.BeginInvoke(new Action(() =>
                                                {
                                                    HideLoading();
                                                    batprinted = 0;
                                                }));
                                            }
                                        }
                                    }));
                                }
                            });

                            task.Start();
                            Task.WaitAll(task);

                        });
                    }
                }
            }));
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

        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            Print();
        }
        /// <summary>
        /// 获取产品对象
        /// </summary>
        /// <returns></returns>
        public PdProduct GetPdProduct()
        {
            PdProduct pdProduct = new PdProduct();
            pdProduct.GBStandard = txtGBStandard.Text;
            pdProduct.Batcode = txtBatCode.Text;
            pdProduct.ClassName = txtCbClass.Text;
            pdProduct.MaterialName = txtMaterial.Text;
            int Piececount;
            int.TryParse(txtPiececount.Text, out Piececount);
            pdProduct.Piececount = Piececount;
            double Weight;
            double.TryParse(txtWeight.Text, out Weight);
            pdProduct.Weight = Weight;
            double MeterWeight;
            double.TryParse(txtMeterWeight.Text, out MeterWeight);
            pdProduct.Meterweight = MeterWeight;
            pdProduct.SpecName = txtSpec.Text;
            int Bundlecode;
            int.TryParse(txtBundle.Text, out Bundlecode);
            pdProduct.Bundlecode = Bundlecode <= 9 ? Bundlecode.ToString("D2") : Bundlecode.ToString();
            pdProduct.Randomcode = GenerateRandomCode();
            double Length;
            double.TryParse(txtLength.Text, out Length);
            pdProduct.Length = Length;
            pdProduct.Createtime = (int)TimeUtils.GetCurrentUnixTime();
            return pdProduct;
        }

        public void Print()
        {

            using (PdProductAccess pdProductAccess = new PdProductAccess())
            {
                pdProductAccess.InsertProduct(GetPdProduct());
            }
            DoPrint(GetPdProduct(), 1, 1);
        }
        public string GenerateRandomCode()
        {
            StringBuilder codebuilder = new StringBuilder();

            long tick = DateTime.Now.Ticks;
            Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));

            for (int i = 0; i < 3; i++)
            {
                int r = ran.Next(0, 10);
                codebuilder.Append(r);
            }

            return codebuilder.ToString();
        }

        private void btnPrints_Click(object sender, RoutedEventArgs e)
        {
            //弹出批量打印确认表单窗口，确认打印产品捆数
            BatPrintConfirmWindow window = new BatPrintConfirmWindow(GetPdProduct());
            window.Title = "开始批量打印";
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;
            window.doPrintHandler += btnReprintList;
            window.ShowDialog();
        }

        /// <summary>
        /// 批量打印
        /// </summary>
        /// <param name="productList"></param>
        public void btnReprintList(List<PdProduct> productList)
        {
            //统计打印进度
            ShowLoading();

            string DefaultPrinter = ConfigurationManager.AppSettings["DefaultPrinter"];
            var printers = new LocalPrintServer().GetPrintQueues();

            //如果没有设置打印机，应该先弹出打印机选择界面
            if (mDefaultPrinter == null)
            {
                //选择一个打印机
                var selectedPrinter = printers.FirstOrDefault(p => p.Name.Contains(DefaultPrinter));
                if (selectedPrinter == null)
                {
                    MessageBox.Show("在批量打印前请先设置好打印机！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);

                    PrintDialog dialog = new PrintDialog();
                    if (dialog.ShowDialog() == true)
                    {
                        mDefaultPrinter = dialog.PrintQueue;
                    }
                }
                else
                {
                    mDefaultPrinter = selectedPrinter;
                }
            }
            int len = productList.Count;
            int printing = 1;
            batprinted = 0;
            string formatstr = "正在批量打印第{0}件，共{1}件";

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                SetLoadingValue(printing / len * 100, "正在批量打印中...", string.Format(formatstr, printing, len));
                progress.IsIndeterminate = true;
            }));
            Task.Factory.StartNew(() =>
            {
                using (PdProductAccess pdProductAccess = new PdProductAccess())
                {
                    foreach (var item in productList)
                    {
                        pdProductAccess.InsertProduct(item);
                        DoPrint(item, printing, len);
                    }
                }
            });

        }
    }
}
