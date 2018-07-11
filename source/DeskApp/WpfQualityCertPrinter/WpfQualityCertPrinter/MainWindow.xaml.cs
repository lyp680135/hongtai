using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Configuration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.IO.Ports;
using System.Threading;
using System.IO;
using WpfQualityCertPrinter.ModelAccess;
using DataLibrary;
using WpfQualityCertPrinter.Common;
using XYNetCloud.Utils;
using WpfQualityCertPrinter.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Printing;
using System.Drawing.Printing;
using System.Drawing;
using WpfQualityCertPrinter.Utils;

namespace WpfQualityCertPrinter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage m_template = null;
        private BitmapImage m_notfind = null;
        private PrintQueue mDefaultPrinter = null;

        private string mCurrentPrintno = string.Empty;

        private MngAdmin mUser = null;

        public string mLabelPageWidth = ConfigurationManager.AppSettings["PageWidth"];
        public string mLabelPageHeight = ConfigurationManager.AppSettings["PageHeight"];

        string DefaultPrinter = ConfigurationManager.AppSettings["DefaultPrinter"];


        public MainWindow(MngAdmin user)
        {
            InitializeComponent();

            LogHelper.SetConfig();

            Application.Current.MainWindow = this;

            this.mUser = user;

            m_notfind = new BitmapImage(new Uri("/WpfQualityCertPrinter;component/Resources/notfind.jpg", UriKind.RelativeOrAbsolute));
            m_template = new BitmapImage(new Uri("/WpfQualityCertPrinter;component/Resources/cert_template.png", UriKind.RelativeOrAbsolute));

            ShowLoading();

            Task.Factory.StartNew(InitWork);

        }

        public void InitWork()
        {
            Task task = new Task(() => BeginInitData());
            task.Start();

            Task.WaitAll(task);
        }

        private void BeginInitData()
        {
            try
            {
                using (var access = new SalePrintlogAccess())
                {
                    SalePrintlog latestlog = access.SingleById();
                    if (latestlog != null)
                    {
                        this.mCurrentPrintno = latestlog.Printno;

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            HideLoading();

                            imgPreview.Source = null;
                            ImageLoadingControl.Visibility = Visibility.Visible;
                        }));

                        string path = string.Format(CommonHouse.CERT_PATH_PREFIX + "{0}{1}.jpg", latestlog.Id, latestlog.Checkcode);
                        var bmp = HttpUtils.GetHttpBitmap(path);
                        if (bmp != null)
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                var imgsource = ImageUtils.ChangeBitmapToImageSource(bmp);
                                imgPreview.Source = imgsource;

                                ImageLoadingControl.Visibility = Visibility.Hidden;
                            }));
                        }
                        else
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                imgPreview.Source = m_template;

                                ImageLoadingControl.Visibility = Visibility.Hidden;
                            }));
                        }
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            HideLoading();
                        }));
                    }
                }
            }
            catch (Exception e)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    SetLoadingErrorValue(100, "", "程序初始化失败，请检查网络！");
                }));

                LogHelper.WriteLog("初始化失败：" + e.Message);
            }
        }

        public void SearchWork(string printno)
        {
            Task task = new Task(() => BeginSearchData(printno));
            task.Start();

            Task.WaitAll(task);
        }

        private void BeginSearchData(string printno)
        {
            var access = new SalePrintlogAccess();
            SalePrintlog latestlog = access.SingleByPrintno(printno);
            if (latestlog != null)
            {
                mCurrentPrintno = null;

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    HideLoading();

                    imgPreview.Source = null;
                    ImageLoadingControl.Visibility = Visibility.Visible;
                }));

                string path = string.Format(CommonHouse.CERT_PATH_PREFIX + "{0}{1}.jpg", latestlog.Id, latestlog.Checkcode);
                var bmp = HttpUtils.GetHttpBitmap(path);
                if (bmp != null)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var imgsource = ImageUtils.ChangeBitmapToImageSource(bmp);
                        imgPreview.Source = imgsource;

                        mCurrentPrintno = printno;

                        ImageLoadingControl.Visibility = Visibility.Hidden;
                    }));
                }
                else
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        imgPreview.Source = m_notfind;

                        ImageLoadingControl.Visibility = Visibility.Hidden;
                    }));
                }
            }
            else
            {
                mCurrentPrintno = null;

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    imgPreview.Source = m_notfind;
                    HideLoading();
                }));
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

        public void On_Closeing(object sender, CancelEventArgs e)
        {

        }

        public void Search_Click(object sender, RoutedEventArgs e)
        {
            string printno = txtPrintno.Text;
            if (!string.IsNullOrEmpty(printno))
            {
                ShowLoading();

                Task.Factory.StartNew(() =>
                {
                    SearchWork(printno);
                });
            }
        }
		
        public void GJSearch_Click(object sender, RoutedEventArgs e)
        {           
            //订阅事件
            SelectGJ.updataSelectHandler += SearchWork;
            SelCondition sc = new SelCondition();
            sc.Title = "高级查询";
            sc.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            sc.Owner = this;
            sc.ShowDialog();
        }
		
        public void Generate_Click(object sender, RoutedEventArgs e)
        {
            SelectProductWindow spw = new SelectProductWindow();
            spw.Title = "随车生成质量证明书";
            spw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            spw.Owner = this;

            spw.SelectedProductInfoConfirmedHandler += OnSelectedProductInfoConfirmed;

            spw.ShowDialog();
        }

        public void OnSelectedProductInfoConfirmed(ObservableCollection<SelectedProductInfo> list,
            string lpn, string consignor, int sellerid, int printid)
        {
            var postlist = JsonConvert.SerializeObject(list);

            JObject postobj = new JObject();
            postobj["printid"] = printid;
            postobj["list"] = postlist;
            postobj["lpn"] = lpn;
            postobj["sellerid"] = sellerid;
            postobj["consignor"] = consignor;

            if (mUser != null)
            {
                postobj["userid"] = mUser.Id;
            }
            else
            {
                postobj["userid"] = 0;
            }

            var postdata = JsonConvert.SerializeObject(postobj);

            List<Tuple<string, string>> paramslist = new List<Tuple<string, string>>();
            paramslist.Add(new Tuple<string, string>("requestData", postdata));

            ShowLoading();

            SetLoadingValue(0, "正在生成证书，请稍候...", "");

            Task.Factory.StartNew(() =>
            {
                try
                {
                    string result = new HttpUtils().Post(CommonHouse.GENERATE_CERT_URL, paramslist, false);
                    if (!string.IsNullOrEmpty(result))
                    {
                        try
                        {
                            JObject responsedata = (JObject)JsonConvert.DeserializeObject(result);
                            if (responsedata != null)
                            {
                                if (responsedata["status"].ToString() == "1")
                                {
                                    var datastr = responsedata["data"].ToString();
                                    JObject dataobj = (JObject)JsonConvert.DeserializeObject(datastr);
                                    if (dataobj != null)
                                    {
                                        string printno = dataobj["printno"].ToString();
                                        string realprintid = dataobj["printid"].ToString();
                                        string printcode = dataobj["printcode"].ToString();

                                        this.mCurrentPrintno = printno;

                                        //查看质保书
                                        string path = string.Format(CommonHouse.CERT_PATH_PREFIX + "{0}{1}p.jpg", realprintid, printcode);
                                        var bmp = HttpUtils.GetHttpBitmap(path);
                                        if (bmp != null)
                                        {
                                            this.Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                var imgsource = ImageUtils.ChangeBitmapToImageSource(bmp);
                                                imgPreview.Source = imgsource;

                                                ImageLoadingControl.Visibility = Visibility.Hidden;
                                            }));
                                        }
                                        else
                                        {
                                            this.Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                imgPreview.Source = m_notfind;

                                                ImageLoadingControl.Visibility = Visibility.Hidden;
                                            }));
                                        }
                                    }
                                }
                                else
                                {
                                    this.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        string msg = responsedata["msg"].ToString();
                                        MessageBox.Show("请求生成质量证明书返回生成失败：\n\n" + msg, "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);

                                        LogHelper.WriteLog("请求生成质量证明书返回生成失败：" + msg);
                                    }));

                                }
                            }
                        }
                        catch (Exception e)
                        {
                            LogHelper.WriteLog("请求生成质量证明书返回数据解析出错：" + e.Message);
                        }
                        finally
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                HideLoading();
                            }));
                        }

                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("请求生成质量证明书生成失败：\n\n 服务器返回为空", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
                    }
                }
                catch (Exception e)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("请求生成质量证明书生成失败：\n\n" + e.Message, "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                    LogHelper.WriteLog("请求生成质量证明书出错：" + e.Message);
                }
                finally
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        HideLoading();
                    }));
                }
            });
        }

        public void Print_Click(object sender, RoutedEventArgs e)
        {
            if (imgPreview.Source == null)
            {
                MessageBox.Show("质量证明书还没有生成，请生成后重试！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            btnPrint.IsEnabled = false;

            //去生成真正的质保书
            GenerateRealCert();
        }

        private void GenerateRealCert()
        {
            //判断该质量证明书是否生成打印过
            using (SalePrintlogAccess access = new SalePrintlogAccess())
            {
                var log = access.SingleByPrintno(mCurrentPrintno);
                if (log != null)
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            //直接返回图片，然后通知打印！！！
                            string path = string.Format(CommonHouse.CERT_PATH_PREFIX + "{0}{1}.jpg", log.Id, log.Checkcode);
                            var bmp = HttpUtils.GetHttpBitmap(path);
                            if (bmp != null)
                            {
                                this.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    var imgsource = ImageUtils.ChangeBitmapToImageSource(bmp);
                                    imgPreview.Source = imgsource;

                                    ImageLoadingControl.Visibility = Visibility.Hidden;

                                    //开始打印
                                    DoPrint();

                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                MessageBox.Show("获取质量证明书时失败：\n\n" + ex.Message, "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                            }));
                        }
                        finally
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                btnPrint.IsEnabled = true;
                                HideLoading();
                            }));
                        }
                    });

                    return;
                }
            }

            JObject postobj = new JObject();
            postobj["printno"] = mCurrentPrintno;
            postobj["iswater"] = 1;

            var postdata = JsonConvert.SerializeObject(postobj);

            List<Tuple<string, string>> paramslist = new List<Tuple<string, string>>();
            paramslist.Add(new Tuple<string, string>("requestData", postdata));


            ShowLoading();

            SetLoadingValue(0, "正在准备打印证书，请稍候...", "");

            Task.Factory.StartNew(() =>
            {
                try
                {
                    string result = new HttpUtils().Post(CommonHouse.GENERATE_CERT_URL, paramslist, false);
                    if (!string.IsNullOrEmpty(result))
                    {
                        try
                        {
                            JObject responsedata = (JObject)JsonConvert.DeserializeObject(result);
                            if (responsedata != null)
                            {
                                if (responsedata["status"].ToString() == "1")
                                {
                                    var datastr = responsedata["data"].ToString();
                                    JObject dataobj = (JObject)JsonConvert.DeserializeObject(datastr);
                                    if (dataobj != null)
                                    {
                                        string printno = dataobj["printno"].ToString();
                                        string realprintid = dataobj["printid"].ToString();
                                        string printcode = dataobj["printcode"].ToString();

                                        this.mCurrentPrintno = printno;

                                        //查看质保书
                                        string path = string.Format(CommonHouse.CERT_PATH_PREFIX + "{0}{1}.jpg", realprintid, printcode);
                                        var bmp = HttpUtils.GetHttpBitmap(path);
                                        if (bmp != null)
                                        {
                                            this.Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                var imgsource = ImageUtils.ChangeBitmapToImageSource(bmp);
                                                imgPreview.Source = imgsource;

                                                ImageLoadingControl.Visibility = Visibility.Hidden;

                                                //开始打印
                                                DoPrint();

                                            }));
                                        }
                                        else
                                        {
                                            this.Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                imgPreview.Source = m_notfind;

                                                ImageLoadingControl.Visibility = Visibility.Hidden;
                                            }));
                                        }
                                    }
                                }
                                else
                                {
                                    string msg = responsedata["msg"].ToString();
                                    LogHelper.WriteLog("请求打印质量证明书返回生成失败：" + msg);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                MessageBox.Show("请求打印质量证明书返回数据解析出错：\n\n" + e.Message, "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                            }));
                            LogHelper.WriteLog("请求打印质量证明书返回数据解析出错：" + e.Message);
                        }
                        finally
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                btnPrint.IsEnabled = true;
                                HideLoading();
                            }));
                        }

                    }
                    else
                    {
                        MessageBox.Show("请求打印质量证明书返回生成失败：\n\n 服务器返回为空", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception e)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        btnPrint.IsEnabled = true;
                        MessageBox.Show("请求打印质量证明书出错：\n\n" + e.Message, "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                    }));
                    LogHelper.WriteLog("请求打印质量证明书出错：" + e.Message);
                }
                finally
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        btnPrint.IsEnabled = true;
                        HideLoading();
                    }));
                }
            });
        }

        private void DoPrint()
        {
            int w = 0, h = 0;
            int.TryParse(mLabelPageWidth, out w);
            int.TryParse(mLabelPageHeight, out h);

            //从本地计算机中获取所有打印机对象(PrintQueue)
            var printers = new LocalPrintServer().GetPrintQueues();
            //选择一个打印机
            var selectedPrinter = printers.FirstOrDefault(p => p.Name.Contains(DefaultPrinter));
            if (selectedPrinter == null)
            {
                if (mDefaultPrinter == null)
                {
                    MessageBox.Show("没有找到默认打印机，请选择！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (Externs.SetDefaultPrinter(DefaultPrinter))
                {

                }
            }

            PrintDocument pd = new PrintDocument();
            PrintController controller = new StandardPrintController();
            pd.PrintController = controller;

            PaperSize pp = new PaperSize();
            foreach (PaperSize ps in pd.PrinterSettings.PaperSizes)
            {
                if (ps.PaperName.Equals("A4"))
                {
                    pp = ps;
                }
            }

            //将缺省的纸张设置为新建的自定义纸张
            pd.DefaultPageSettings.PaperSize = pp;
            pd.DefaultPageSettings.Landscape = true;
            pd.PrintPage += onPrintDocumentPrintPage;
            pd.EndPrint += onPrintDocumentPrintEnd;

            pd.Print();
            return;

            //DrawingVisual vis = new DrawingVisual();
            //using (DrawingContext dc = vis.RenderOpen())
            //{
            //    var bmprect = new Rect(20, 60, imgPreview.Source.Width-20, imgPreview.Source.Height);
            //    dc.DrawImage(imgPreview.Source, bmprect);

            //    dc.Close();

            //    //保存到本地
            //    RenderTargetBitmap bitmap = new RenderTargetBitmap(
            //               (int)(w), (int)(h), 96, 96, PixelFormats.Pbgra32);
            //    bitmap.Render(vis);

            //    PngBitmapEncoder encode = new PngBitmapEncoder();
            //    encode.Frames.Add(BitmapFrame.Create(bitmap));
            //    var ms = new System.IO.MemoryStream();
            //    encode.Save(ms);

            //    System.Drawing.Image bmd = System.Drawing.Image.FromStream(ms, true);
            //    bmd.Save("abc.png", System.Drawing.Imaging.ImageFormat.Png);

            //    PrintDialog dialog = new PrintDialog();
            //    PrintTicket pt = dialog.PrintTicket;
            //    pt.PageOrientation = PageOrientation.Landscape;
            //    pt.CopyCount = 1;

            //    //从本地计算机中获取所有打印机对象(PrintQueue)
            //    var printers = new LocalPrintServer().GetPrintQueues();
            //    //选择一个打印机
            //    var selectedPrinter = printers.FirstOrDefault(p => p.Name.Contains(DefaultPrinter));
            //    if (selectedPrinter == null)
            //    {
            //        if (mDefaultPrinter == null)
            //        {
            //            MessageBox.Show("没有找到默认打印机，请选择！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Information);
            //        }
            //    }
            //    else
            //    {
            //        dialog.PrintQueue = selectedPrinter;

            //        mDefaultPrinter = dialog.PrintQueue;
            //    }

            //    PageMediaSize pagesize = new PageMediaSize(PageMediaSizeName.NorthAmericaLetter, bitmap.Width, bitmap.Height);
            //    pt.PageMediaSize = pagesize;

            //    if (mDefaultPrinter == null)
            //    {
            //        if (dialog.ShowDialog() == true)
            //        {
            //            //自动缩放图片到一张纸张上
            //            var capabilities = dialog.PrintQueue.GetPrintCapabilities(dialog.PrintTicket);
            //            var scale = Math.Max(capabilities.PageImageableArea.ExtentWidth / w, capabilities.PageImageableArea.ExtentHeight / h);
            //            vis.Transform = new ScaleTransform(scale, scale);

            //            dialog.PrintVisual(vis, "质量证明书打印");

            //            mDefaultPrinter = dialog.PrintQueue;
            //        }
            //    }
            //    else
            //    {
            //        //自动缩放图片到一张纸张上
            //        var capabilities = dialog.PrintQueue.GetPrintCapabilities(dialog.PrintTicket);
            //        var scale = Math.Max(capabilities.PageImageableArea.ExtentWidth / w, capabilities.PageImageableArea.ExtentHeight / h);
            //        vis.Transform = new ScaleTransform(scale, scale);

            //        dialog.PrintVisual(vis, "质量证明书打印");
            //    }
            //}
        }

        void onPrintDocumentPrintPage(object sender, PrintPageEventArgs e)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)imgPreview.Source));
            encoder.Save(ms);

            Bitmap bmp = new Bitmap(ms);
            ms.Close();

            e.Graphics.PageScale = 0.725f;

            e.Graphics.DrawImage(bmp, 0, 0, (float)(imgPreview.Source.Width), (float)imgPreview.Source.Height);
        }

        /// <summary>
        /// 结束打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void onPrintDocumentPrintEnd(object sender, PrintEventArgs e)
        {
            btnPrint.IsEnabled = true;
        }

        private void imgPreview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var img = sender as ContentControl;
            if (img == null)
            {
                return;
            }

            if (imgPreview.Source == m_notfind)
            {
                return;
            }

            double width = imgPreview.Source.Width;
            double height = imgPreview.Source.Height;
            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;
            ImageViewerWindow viewer = new ImageViewerWindow();
            viewer.Width = screenWidth;
            viewer.Height = screenHeight;

            viewer.SetSource(imgPreview.Source);

            viewer.Owner = this;

            viewer.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            viewer.ShowDialog();

        }


    }
}
