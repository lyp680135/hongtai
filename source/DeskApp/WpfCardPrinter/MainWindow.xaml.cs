using MySql.Data.MySqlClient;
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
using DataLibrary;
using System.Globalization;
using WpfCardPrinter.ModelAccess;
using WpfCardPrinter.Utils;
using System.Reflection;
using System.Drawing.Printing;
using System.Printing;
using System.IO.Ports;
using System.Threading;
using System.IO;
using WpfCardPrinter.ModelAccess.SqliteAccess;

namespace WpfCardPrinter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string myWorkshopProductLine = LoginWindow.shopCode;
        public string myQRCodeUrlString = System.Configuration.ConfigurationManager.AppSettings["QRCodeUrlString"];
        public int nunlength = string.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["UnfixedLengthNumber"]) ? 0 : Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["UnfixedLengthNumber"]);
        public string mLabelPageWidth = ConfigurationManager.AppSettings["PageWidth"];
        public string mLabelPageHeight = ConfigurationManager.AppSettings["PageHeight"];

        public string mStartBundle = ConfigurationManager.AppSettings["StartBundle"];

        public string mPrintNumber = ConfigurationManager.AppSettings["PrintNumber"];

        public MngSetting mConfig = new MngSetting();
        public MngAdmin mUser = null;
        public PdWorkshop mWorkshop = null;

        public ComPortConfig mComPortConfig = new ComPortConfig();
        private SerialPort serialport = new SerialPort();

        public ObservableCollection<BaseProductClass> mClassList = new ObservableCollection<BaseProductClass>();
        public ObservableCollection<BaseProductMaterial> mMaterialList = new ObservableCollection<BaseProductMaterial>();
        public ListCollectionView mGroupedMaterialList = null;
        public ObservableCollection<PdProduct> mProductList = new ObservableCollection<PdProduct>();
        public ObservableCollection<BaseSpecifications> mSpecList = new ObservableCollection<BaseSpecifications>();
        public ObservableCollection<PdWorkshopTeam> mTeamList = new ObservableCollection<PdWorkshopTeam>();
        public ObservableCollection<CommonItem> mLengthTypeList = new ObservableCollection<CommonItem>();

        public PdProduct mSelectedProduct = null;
        private PdWorkshopTeam mCurrentTeam = null;
        public BaseProductMaterial mMaterial = null;


        private PrintQueue mDefaultPrinter = null;

        private int mSelectedProductIndex = -1;
        private string mCurrentBatCode = "";
        private string mTitle = "";

        private bool mbShouldShowTip = true;

        public int pubclassId { get; set; }
        public int pubmaterialId { get; set; }

        public bool mInitFailed = false;

        private BitmapImage zhi = null;
        private BitmapImage zhi_e = null;
        private BitmapImage pan = null;
        private BitmapImage pan_e = null;

        //是否离线
        private bool mbOffline = false;

        public MainWindow(MngAdmin user, PdWorkshop shop)
        {
            InitializeComponent();

            LogHelper.SetConfig();

            Application.Current.MainWindow = this;

            //显示增加材质的点击事件
            cbMaterial.AddHandler(ComboBox.MouseLeftButtonDownEvent, new MouseButtonEventHandler(MaterialClick), true);

            this.mUser = user;
            this.mWorkshop = shop;

            zhi = new BitmapImage(new Uri("pack://application:,,,/WpfCardPrinter;component/Resources/zhi.jpg", UriKind.RelativeOrAbsolute));
            zhi_e = new BitmapImage(new Uri("pack://application:,,,/WpfCardPrinter;component/Resources/zhi_e.jpg", UriKind.RelativeOrAbsolute));
            pan = new BitmapImage(new Uri("pack://application:,,,/WpfCardPrinter;component/Resources/pan.jpg", UriKind.RelativeOrAbsolute));
            pan_e = new BitmapImage(new Uri("pack://application:,,,/WpfCardPrinter;component/Resources/pan_e.jpg", UriKind.RelativeOrAbsolute));

            ShowLoading();

            Task.Factory.StartNew(InitWork);
        }

        public bool isOffline()
        {
            return this.mbOffline;
        }

        public void setOffline()
        {
            this.mbOffline = true;
        }

        private void CheckOfflineCache()
        {
            try
            {
                ProductSqliteAccess.CreateDB();
                ProductSqliteAccess.CreateTable();

                using (ProductSqliteAccess sqliteaccess = new ProductSqliteAccess())
                {
                    var list = sqliteaccess.GetList();
                    if (list != null)
                    {
                        using (PdProductAccess access = new PdProductAccess())
                        {
                            foreach (var item in list)
                            {
                                var oldid = item.Id;
                                long result = 0;

                                try
                                {
                                    result = access.Insert(item);
                                    if (result > 0)
                                    {
                                        if (mConfig.SystemVersion == EnumList.SystemVersion.简单版本)
                                        {
                                            SaveDataPreset(item);
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    //如果服务器已经有该捆号的产品，则当成已经上传成功
                                    if (e.Message.IndexOf("Duplicate entry") != -1)
                                    {
                                        result = 1;
                                    }
                                }

                                if (result > 0)
                                {
                                    var deleteresult = sqliteaccess.UpdateUploaded(oldid);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mInitFailed = true;

                LogHelper.WriteLog("上传缓存数据时发生错误：" + e.ToString());

                throw new Exception("检查缓存数据时发生错误..." + e.Message);
            }
        }

        private void InitWork()
        {
            Task task = new Task(() => BeginInitData());
            task.Start();

            Task.WaitAll(task);
        }

        private void BeginInitData()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                SetLoadingValue(2, "", "正在装载标牌资源...");
                //初始化标牌显示
                InitLabel();
            }));

            this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(5, "", "正在装载标牌资源...完成！"); }));
            this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(6, "", "正在加载初始数据..."); }));

            InitData();

            if (!mInitFailed)
            {
                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(70, "", "正在加载初始数据...完成！"); }));
                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(72, "", "正在初始化界面..."); }));

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    BindView();
                }));
            }

            if (!mInitFailed)
            {
                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(76, "", "正在检查缓存数据...完成"); }));
                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(81, "", "正在初始化表单数据..."); }));

                //获取生产线上的最后一条产品，以此为产品模板
                InitProductInfo();
            }


            if (!mInitFailed)
            {
                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(90, "", "正在初始化表单数据...完成"); }));

                //去除称重连接模块
                //this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(91, "", "正在连接称重设备..."); }));

                //this.Dispatcher.BeginInvoke(new Action(() =>
                //{
                //    InitWeightAutoInput();
                //    SetLoadingValue(100, "", "正在连接称重设备...完成");
                //}));
            }

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (lbError.Content == null || string.IsNullOrEmpty(lbError.Content.ToString()))
                {
                    Thread t = new Thread(() =>
                     {
                         Thread.Sleep(200);
                         this.Dispatcher.BeginInvoke(new Action(() =>
                         {
                             HideLoading();
                         }));
                     });
                    t.Start();
                }
            }));
        }

        private void StartReloadProduct(string batcode, bool should_reload_material = true)
        {

            if (mbOffline)
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    HideLoading();
                }));
                return;
            }

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ShowLoading();
            }));

            // 重新设置选中的材质
            using (PdProductAccess access = new PdProductAccess())
            {
                var products = access.GetListByBatcode(batcode);
                if (products != null)
                {
                    var product = products.Last();
                    if (product != null)
                    {
                        flag = false;

                        mMaterial = findMaterialById(product.Materialid.Value);
                        cbMaterial.SelectedValue = findMaterialById(product.Materialid.Value);

                        //将时间重置为当时最后一件打印的时间
                        dpProductionDate.SelectedDate = Utils.TimeUtils.GetDateTimeFromUnixTime(product.Createtime.Value);
                    }
                }
                else
                {
                    if (should_reload_material)
                    {
	                    var lastproduct = access.SingleLastProductByWorkshopid(mWorkshop.Id);
	                    if (lastproduct != null)
	                    {
	                        cbClass.SelectedValue = lastproduct.Classid;
	                        mMaterial = findMaterialById(lastproduct.Materialid.Value);
	                        cbMaterial.SelectedValue = mMaterial;

                            cbSpec.SelectedValue = lastproduct.Specid;
                        }
                        else
                        {
                            dpProductionDate.SelectedDate = Utils.TimeUtils.GetDateTimeFromUnixTime(mCurrentTeam.CurrTeamlog.CreateTime);
                        }
                    }
                }
            }

            Task.Factory.StartNew(() =>
            {
                ReloadProductWork(batcode);
            });
        }

        private void ReloadProductWork(string batcode)
        {
            Task task = new Task(() => BeginReload(batcode));
            task.Start();

            Task.WaitAll(task);
        }

        private void BeginReload(string batcode)
        {
            GetProductList(batcode);

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                BindView();

                Thread.Sleep(200);
                HideLoading();
            }));
        }


        public void On_Closeing(object sender, CancelEventArgs e)
        {
            if (serialport.IsOpen)
            {
                serialport.Close();
            }

            e.Cancel = false;
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

                    if (mMaterial != null && mMaterial.Deliverytype == (int)EnumList.DeliveryType.盘卷)
                    {
                        lbLabelProductClass.Visibility = System.Windows.Visibility.Visible;
                    }
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
                    if (mMaterial != null && mMaterial.Deliverytype == (int)EnumList.DeliveryType.盘卷)
                    {
                        lbLabelBundleCode.Visibility = System.Windows.Visibility.Visible;
                    }
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

        public PdWorkshopTeam GetCurrentWorkShift()
        {
            return mCurrentTeam;
        }

        public bool SetCurrentWorkShift(int shiftid)
        {
            if (shiftid > 0)
            {
                var workshift = findWorkShiftById(shiftid);

                using (PdWorkshopTeamLogAccess access = new PdWorkshopTeamLogAccess())
                {
                    //查询该车间最后一班换班记录
                    PdWorkshopTeamLog lastlog = access.SingleLastByWorkshopid(workshift.WorkshopId);
                    if (lastlog != null)
                    {
                        if (lastlog.TeamId == shiftid)
                        {
                            MessageBox.Show("数据错误，请重新打开程序！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }

                        //如果当前选中的行不是本班记录，则选到下一个空白行
                        if (mSelectedProduct != null)
                        {
                            if (mSelectedProduct.WorkShift != lastlog.TeamId)
                            {
                                dgProduct.SelectedIndex = mSelectedProductIndex + 1;
                            }
                        }
                    }

                    PdWorkshopTeamLog log = new PdWorkshopTeamLog();
                    log.WorkshopId = workshift.WorkshopId;
                    log.TeamId = shiftid;
                    log.Batcode = mCurrentBatCode;
                    log.CreateTime = (int)TimeUtils.GetCurrentUnixTime();

                    //写入换班记录到数据库
                    long newid = access.Insert(log);
                    if (newid > 0)
                    {
                        log.Id = (int)newid;
                        cbWorkShift.SelectedValue = shiftid;
                        mCurrentTeam = findWorkShiftById(shiftid);
                        mCurrentTeam.CurrTeamlog = log;

                        txtTeam.Text = mCurrentTeam.TeamName;

                        CountTeamProducted();

                        return true;
                    }
                }
            }

            return false;
        }

        public string GetWorkShiftName(int id)
        {
            foreach (var workshift in mTeamList)
            {
                if (workshift.Id == id)
                {
                    return workshift.TeamName;
                }
            }
            return "";
        }

        public void BindView()
        {
            lbTitle.Content = mTitle;
            txtBatCode.Text = mCurrentBatCode;

            if (mCurrentTeam != null)
            {
                txtTeam.Text = mCurrentTeam.TeamName;
            }

            cbClass.ItemsSource = mClassList;

            cbWorkShift.ItemsSource = mTeamList;

            cbLength.ItemsSource = mLengthTypeList;

            dgProduct.ItemsSource = mProductList;

            CountTeamProducted();

            //判断该炉号是否允许换班
            CheckTeamChangeable();

            //如果选中行在该批号，则应该重新选定行
            if (mSelectedProductIndex > 0)
            {
                dgProduct.SelectedIndex = mSelectedProductIndex;
                dgProduct.ScrollIntoView(dgProduct.SelectedItem);
            }
            else
            {
                //如果没选，则跳到最后一条产品位置
                int lastproduct_index = findProductLastIndex();
                dgProduct.SelectedIndex = lastproduct_index++;
                if (dgProduct.SelectedItem != null)
                {
                    dgProduct.ScrollIntoView(dgProduct.SelectedItem);
                }
            }
        }

        protected void InitData()
        {

            this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(10, "", "正在加载系统配置..."); }));

            try
            {
                using (MngSettingAccess access = new MngSettingAccess())
                {
                    mConfig = access.Single();

                    if (mConfig != null)
                    {
                        mTitle = mConfig.Client + "称重/标牌打印";
                    }
                    else
                    {
                        mConfig = new MngSetting();
                        mTitle = "小羽网络科技（杭州）" + "称重/标牌打印";
                    }
                }

                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(15, "", "正在检查缓存数据..."); }));

                CheckOfflineCache();

                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(20, "", "正在加载班组信息..."); }));

                int workshopid = 0;
                if (this.mWorkshop != null)
                {
                    workshopid = this.mWorkshop.Id;
                }

                //获取班别
                using (PdWorkshopTeamAccess access = new PdWorkshopTeamAccess())
                {
                    var list = access.GetListByWorkshop(myWorkshopProductLine);
                    if (list != null)
                    {
                        mTeamList.Clear();

                        foreach (var item in list)
                        {
                            mTeamList.Add(item);
                            workshopid = item.WorkshopId;
                        }
                    }
                    else
                    {
                        throw new Exception("找不到车间的班组信息...");
                    }
                }

                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(24, "", "正在加载班组信息...完成！"); }));

                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(28, "", "正在加载换班信息..."); }));

                if (workshopid > 0)
                {
                    //获取最后一个班别
                    using (PdWorkshopTeamLogAccess access = new PdWorkshopTeamLogAccess())
                    {
                        var lastlog = access.SingleLastByWorkshopid(workshopid);
                        if (lastlog != null)
                        {
                            var lastteam = findWorkShiftById(lastlog.TeamId);
                            if (lastteam != null)
                            {
                                mCurrentTeam = lastteam;
                                mCurrentTeam.CurrTeamlog = lastlog;
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("找不到指定车间信息...");
                }

                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(35, "", "正在加载换班信息...完成！"); }));

                if (mCurrentTeam == null)
                {
                    if (mTeamList.Count > 0)
                    {
                        mCurrentTeam = mTeamList[0];

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //当第一次使用系统时，把默认的排班写入库中
                            SetCurrentWorkShift(mCurrentTeam.Id);
                        }));
                    }
                    else
                    {
                        mCurrentTeam = new PdWorkshopTeam();
                        this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingErrorValue(36, "", "生产车间配置数据异常..."); }));
                        //MessageBox.Show("生产车间配置数据异常！", "系统提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(40, "", "正在加载系统配置...完成！"); }));

                mCurrentBatCode = GetBatCode("", 0);
                if (string.IsNullOrEmpty(mCurrentBatCode))
                {
                    mCurrentBatCode = "";

                    return;
                }

                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(45, "", "正在加载品名材质信息..."); }));

                //获取品名
                using (BaseProductClassAccess access = new BaseProductClassAccess())
                {
                    var list = access.GetList();

                    if (list != null)
                    {
                        mClassList.Clear();

                        mMaterialList.Clear();

                        foreach (var item in list)
                        {
                            mClassList.Add(item);
                        }
                    }
                    else
                    {
                        throw new Exception("材质没有设置好！");
                    }
                }

                using (BaseProductMaterialAccess maccess = new BaseProductMaterialAccess())
                {
                    var mtlist = maccess.GetList();
                    if (mtlist != null)
                    {
                        foreach (var mitem in mtlist)
                        {
                            mMaterialList.Add(mitem);
                        }
                    }

                    mGroupedMaterialList = new ListCollectionView(mMaterialList);
                    mGroupedMaterialList.GroupDescriptions.Add(new PropertyGroupDescription("Classname"));

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        flag = false;
                        cbMaterial.ItemsSource = mGroupedMaterialList;

                        if (mSelectedProduct != null
                            && mSelectedProduct.Materialid != null
                            && mSelectedProduct.Materialid > 0)
                        {
                            cbMaterial.SelectedValue = findMaterialById(mSelectedProduct.Materialid.Value);
                            if (cbMaterial.SelectedItem is BaseProductMaterial)
                                mSelectedProduct.Materialname = (cbMaterial.SelectedItem as BaseProductMaterial).Name;
                        }
                        else
                        {
                            cbMaterial.SelectedIndex = 0;
                        }
                    }));
                }

                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(51, "", "正在加载品名材质信息...完成！"); }));


                //长度
                var commonlist = new ObservableCollection<CommonItem>();
                foreach (EnumList.ProductQualityLevel eitem in Enum.GetValues(typeof(EnumList.ProductQualityLevel)))
                {
                    CommonItem item = new CommonItem();
                    item.Id = (int)eitem;
                    item.Name = eitem.ToString();

                    commonlist.Add(item);
                }
                mLengthTypeList = commonlist;


                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingValue(52, "", "正在加载产品列表信息..."); }));

                //根据炉号读取或生成产品列表
                if (!string.IsNullOrEmpty(mCurrentBatCode))
                {
                    GetProductList(mCurrentBatCode);
                }
            }
            catch (Exception e)
            {
                mTitle = "小羽网络科技（杭州）" + "称重/标牌打印";
                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingErrorValue(100, "", "数据加载失败..." + e.Message); }));

                mInitFailed = true;

                LogHelper.WriteLog("数据加载失败..." + e.ToString());
            }
        }

        public void InitProductInfo()
        {
            try
            {
                using (PdProductAccess access = new PdProductAccess())
                {
                    var lastproduct = access.SingleLastProductByWorkshopid(mCurrentTeam.WorkshopId);
                    if (lastproduct != null)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            cbClass.SelectedIndex = findClassIndexById(lastproduct.Classid.Value);

                            flag = false;
                            cbMaterial.SelectedValue = findMaterialById(lastproduct.Materialid.Value);
                            cbSpec.SelectedValue = lastproduct.Specid;

                            flag = false;
                            int index = findProductIndexById(lastproduct.Id);
                            if (index >= 0)
                            {
                                dgProduct.SelectedIndex = index;
                            }
                            else
                            {
                                dgProduct.SelectedIndex = 0;
                            }

                            //输入框默认值
                            dpProductionDate.SelectedDate = DateTime.Now;
                        }));
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //下拉框的默认选择
                            cbClass.SelectedIndex = 0;
                            cbMaterial.SelectedIndex = 0;
                            cbSpec.SelectedIndex = 0;

                            cbWorkShift.SelectedIndex = 0;
                            cbLength.SelectedIndex = 0;

                            dgProduct.SelectedIndex = 0;

                            //输入框默认值
                            dpProductionDate.SelectedDate = DateTime.Now;
                            txtBundle.Text = "1";
                        }));
                    }

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (dgProduct.SelectedItem is PdProduct)
                        {
                            mSelectedProduct = (PdProduct)dgProduct.SelectedItem;

                            //如果当前选中的行不是本班记录，则选到下一个空白行
                            if (mSelectedProduct.Id > 0)
                            {
                                this.mbShouldShowTip = false;
                                dgProduct.SelectedIndex = mSelectedProductIndex + 1;
                            }
                        }
                    }));
                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (dgProduct.SelectedItem != null)
                    {
                        dgProduct.ScrollIntoView(dgProduct.SelectedItem);
                    }
                }));
            }
            catch (Exception e)
            {
                this.Dispatcher.BeginInvoke(new Action(() => { SetLoadingErrorValue(100, "", "数据加载失败..." + e.Message); }));

                mInitFailed = true;
            }
        }

        public void CountTeamProducted()
        {
            using (PdWorkshopTeamLogAccess access = new PdWorkshopTeamLogAccess())
            {
                using (PdBatcodeAccess baccess = new PdBatcodeAccess(access.GetConnection()))
                {
                    var batcode = baccess.SingleByBatcode(mCurrentBatCode);
                    if (batcode == null)
                    {
                        return;
                    }

                    double totalweight = 0;
                    int count = access.CountCurrTeamProducted(mCurrentTeam, batcode.Id, out totalweight);

                    txtTeamCount.Content = string.Format("已产({0})", count);
                }
            }
        }

        public void CheckTeamChangeable()
        {
            using (PdWorkshopTeamAccess access = new PdWorkshopTeamAccess())
            {
                bool enable = access.CheckTeamChangeable(mCurrentBatCode, mCurrentTeam);
                if (enable)
                {
                    btnChangeShift.IsEnabled = true;
                }
                else
                {
                    btnChangeShift.IsEnabled = false;
                }
            }
        }

        public void GetProductList(string batcode)
        {
            using (PdProductAccess access = new PdProductAccess())
            {
                var list = access.GetListByBatcode(batcode);
                //如果有数据就输出
                if (list != null)
                {
                    //获取产品的品名和材质，设置成当前品名和材质
                    var product = list.Last();
                    if (product.Materialid > 0)
                    {
                        using (BaseProductMaterialAccess maccess = new BaseProductMaterialAccess())
                        {
                            mMaterial = maccess.Single(product.Materialid.Value);
                        }
                    }



                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //补全列表
                        mProductList = ProductAutoComplete(batcode, list);

                        //将所有未保存的产品重置规格
                        for (var i = 0; i < mProductList.Count; i++)
                        {
                            if (mProductList[i].Id <= 0)
                            {
                                mProductList[i].Specid = product.Specid;
                                mProductList[i].Specname = product.Specname;
                            }
                        }

                        flag = false;
                        int index = findProductIndexById(product.Id);
                        if (index >= 0)
                        {
                            mSelectedProductIndex = index + 1;
                        }
                        else
                        {
                            mSelectedProductIndex = 0;
                        }

                        SetLoadingValue(70, "", "正在加载产品列表信息...完成！");
                    }));
                }
                else
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //调用自动生成方法
                        mProductList = ProductAutoCreate(batcode);

                        SetLoadingValue(70, "", "正在加载产品列表信息...完成！");
                    }));
                }
            }
        }

        /// <summary>
        /// 自动生成产品列表
        /// </summary>
        /// <param name="batcode"></param>
        /// <returns></returns>
        public ObservableCollection<PdProduct> ProductAutoCreate(string batcode)
        {
            var list = new ObservableCollection<PdProduct>();

            int classid = 0, materialid = 0;

            var materialitem = (cbMaterial.SelectedItem as BaseProductMaterial);
            if (cbClass.SelectedValue != null)
            {
                classid = materialitem.Classid;
            }

            if (cbMaterial.SelectedValue != null)
            {
                materialid = materialitem.Id;
            }

            //int nunlength = 5;
            int strict_lengthtype = (int)DataLibrary.EnumList.ProductQualityLevel.定尺;
            if (mMaterial != null)
            {
                //盘卷没有非尺
                if (mMaterial.Deliverytype == (int)EnumList.DeliveryType.盘卷)
                {
                    nunlength = 0;
                    strict_lengthtype = (int)DataLibrary.EnumList.ProductQualityLevel.标准;
                }
            }

            for (var i = 1; i <= nunlength; i++)
            {
                var obj = new PdProduct();
                if (i <= 3)
                {
                    obj.Id = 0;
                    obj.Batcode = batcode;
                    obj.Classid = classid;
                    obj.Materialid = materialid;
                    obj.Lengthtype = (int)DataLibrary.EnumList.ProductQualityLevel.长尺;
                    obj.Length = null;
                    obj.Bundlecode = "F" + i.ToString("00");
                    obj.Piececount = null;
                    obj.Weight = null;
                    obj.Meterweight = null;
                    obj.WorkShift = null;

                    list.Add(obj);
                }

                if (i > 3 && i <= nunlength)
                {
                    obj.Id = 0;
                    obj.Batcode = batcode;
                    obj.Classid = classid;
                    obj.Materialid = materialid;
                    obj.Lengthtype = (int)DataLibrary.EnumList.ProductQualityLevel.短尺;
                    obj.Length = null;
                    obj.Bundlecode = "F" + i.ToString("00");
                    obj.Piececount = null;
                    obj.Weight = null;
                    obj.Meterweight = null;
                    obj.WorkShift = null;

                    list.Add(obj);
                }
            }


            int start = 1;

            if (int.TryParse(mStartBundle, out start))
            {

            }
            else
            {
                start = 1;
            }

            //逐行生成数据
            for (var i = start; i <= 100; i++)
            {
                var obj = new PdProduct();

                obj.Id = 0;
                obj.Batcode = batcode;
                obj.Classid = classid;
                obj.Materialid = materialid;
                obj.Lengthtype = strict_lengthtype;
                obj.Length = null;
                obj.Bundlecode = i.ToString("00");
                obj.Piececount = null;
                obj.Weight = null;
                obj.Meterweight = null;
                obj.WorkShift = null;

                list.Add(obj);
            }

            return list;
        }

        /// <summary>
        /// 补全产品列表
        /// </summary>
        /// <param name="batcode"></param>
        /// <param name="inlist"></param>
        /// <returns></returns>
        public ObservableCollection<PdProduct> ProductAutoComplete(string batcode, List<PdProduct> inlist)
        {
            var list = inlist;

            int classid = 0, materialid = 0;

            var materialitem = (cbMaterial.SelectedItem as BaseProductMaterial);
            if (materialitem != null)
            {
                classid = materialitem.Classid;
                materialid = materialitem.Id;
            }

            if (mMaterial != null)
            {
                classid = mMaterial.Classid;
                materialid = mMaterial.Id;
            }

            // int nunlength = 5;
            int strict_lengthtype = (int)DataLibrary.EnumList.ProductQualityLevel.定尺;
            if (mMaterial != null)
            {
                //盘卷没有非尺
                if (mMaterial.Deliverytype == (int)EnumList.DeliveryType.盘卷)
                {
                    nunlength = 0;
                    strict_lengthtype = (int)DataLibrary.EnumList.ProductQualityLevel.标准;
                }
            }

            for (var i = 1; i <= nunlength; i++)
            {
                var obj = new PdProduct();
                if (i <= 3)
                {
                    obj.Id = 0;
                    obj.Batcode = batcode;
                    obj.Classid = classid;
                    obj.Materialid = materialid;
                    obj.Lengthtype = (int)DataLibrary.EnumList.ProductQualityLevel.长尺;
                    obj.Length = null;
                    obj.Bundlecode = "F" + i.ToString("00");
                    obj.Piececount = null;
                    obj.Weight = null;
                    obj.Meterweight = null;
                    obj.WorkShift = null;

                    obj.BundlecodeValue = -(5 - i);

                    int index = findProduct(inlist, obj.Batcode, obj.Lengthtype.Value, obj.Bundlecode);
                    if (index < 0)
                    {
                        list.Add(obj);
                    }
                }

                if (i > 3 && i <= nunlength)
                {
                    obj.Id = 0;
                    obj.Batcode = batcode;
                    obj.Classid = classid;
                    obj.Materialid = materialid;
                    obj.Lengthtype = (int)DataLibrary.EnumList.ProductQualityLevel.短尺;
                    obj.Length = null;
                    obj.Bundlecode = "F" + i.ToString("00");
                    obj.Piececount = null;
                    obj.Weight = null;
                    obj.Meterweight = null;
                    obj.WorkShift = null;

                    obj.BundlecodeValue = -(5 - i);

                    int index = findProduct(inlist, obj.Batcode, obj.Lengthtype.Value, obj.Bundlecode);
                    if (index < 0)
                    {
                        list.Add(obj);
                    }
                }
            }


            int start = 1;

            if (int.TryParse(mStartBundle, out start))
            {

            }
            else
            {
                start = 1;
            }

            //逐行生成数据
            for (var i = start; i <= 100; i++)
            {
                var obj = new PdProduct();

                obj.Id = 0;
                obj.Batcode = batcode;
                obj.Classid = classid;
                obj.Materialid = materialid;
                obj.Lengthtype = strict_lengthtype;
                obj.Length = null;
                obj.Bundlecode = i.ToString("00");
                obj.Piececount = null;
                obj.Weight = null;
                obj.Meterweight = null;
                obj.WorkShift = null;

                obj.BundlecodeValue = i;

                int index = findProduct(inlist, obj.Batcode, obj.Lengthtype.Value, obj.Bundlecode);
                if (index < 0)
                {
                    list.Add(obj);
                }
            }

            //排序
            var sortlist = list.OrderBy(p => p.BundlecodeValue).ToList();

            ObservableCollection<PdProduct> outlist = new ObservableCollection<PdProduct>();
            foreach (var item in sortlist)
            {
                if (item.Lengthtype != (int)EnumList.ProductQualityLevel.定尺)
                {
                    outlist.Add(item);
                }
            }

            foreach (var item in sortlist)
            {
                if (item.Lengthtype == (int)EnumList.ProductQualityLevel.定尺)
                {
                    outlist.Add(item);
                }
            }

            return outlist;
        }

        private int findProduct(List<PdProduct> inlist, string batcode, int lengthtype, string bundlecode)
        {
            int retindex = -1;

            int index = 0;
            foreach (var item in inlist)
            {
                if (item.Batcode == batcode
                    && item.Bundlecode == bundlecode
                    // && item.Lengthtype == lengthtype
                    )
                {
                    retindex = index;
                    break;
                }
                index++;
            }

            return retindex;
        }

        private int findProductIndexById(int id)
        {
            int retindex = -1;

            if (mProductList != null)
            {
                int index = 0;
                foreach (var item in mProductList)
                {
                    if (item.Id == id)
                    {
                        retindex = index;
                        break;
                    }
                    index++;
                }
            }

            return retindex;
        }

        private void dgProduct_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void dgProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgProduct.SelectedIndex < 0)
            {
                mSelectedProduct = null;
                return;
            }

            if (mSelectedProductIndex != dgProduct.SelectedIndex)
            {
                //把称赋值到重量文本框
                txtWeight.Text = txtCurrWeight.Text;

                //取消选中变化时的保存操作
                //保存原来的数据
                //SaveProduct();
            }

            var selectitem = dgProduct.SelectedItem;
            if (selectitem is PdProduct)
            {
                PdProduct pd = (PdProduct)dgProduct.SelectedItem;

                if (string.IsNullOrEmpty(pd.Batcode))
                {
                    //为新增的行
                    pd.Batcode = txtBatCode.Text;
                }

                mSelectedProduct = pd;
                mSelectedProductIndex = dgProduct.SelectedIndex;

                UpdateForm();

                ShowLabelDemo();
            }
            else
            {
                var product = new PdProduct();
                product.Batcode = mCurrentBatCode;

                mSelectedProduct = product;
                mSelectedProductIndex = dgProduct.SelectedIndex;

                UpdateForm();
            }
        }

        /// <summary>
        /// 屏蔽鼠标拖拉多选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void dbProduct_PreviewMouseMove(object sender, EventArgs e)
        {
            var property = typeof(DataGrid).GetField("_isDraggingSelection",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            if (property != null)
            {
                property.SetValue(dgProduct, false);
            }
        }

        public void UpdateForm()
        {
            if (mSelectedProduct != null)
            {
                txtBatCode.Text = mSelectedProduct.Batcode;
                if (mSelectedProduct.Materialid > 0)
                {
                    int oldclass = 0;
                    int oldmaterial = 0;
                    if (cbMaterial.SelectedValue != null)
                    {
                        var mitem = (cbMaterial.SelectedItem as BaseProductMaterial);
                        oldclass = mitem.Classid;
                        oldmaterial = mitem.Id;
                    }

                    var materialitem = findMaterialById(mSelectedProduct.Materialid.Value);
                    cbMaterial.SelectedValue = materialitem;
                    cbClass.SelectedValue = materialitem.Classid;

                    //如果和原来的一样，手动触发控件刷新规格等数据
                    if (materialitem.Id == oldmaterial && materialitem.Classid == oldclass)
                    {
                        if (!mbOffline)
                        {
                            try
                            {
                                DoMaterialChanged(materialitem.Classid, materialitem.Id);
                            }
                            catch (Exception e)
                            {
                                if (e.Message.IndexOf("Connection must be valid and open") != -1)
                                {
                                    this.mbOffline = true;
                                }
                            }
                        }
                    }
                }
                if (mSelectedProduct.Specid > 0)
                {
                    cbSpec.SelectedValue = mSelectedProduct.Specid;
                }
                if (mSelectedProduct.WorkShift != null)
                {
                    cbWorkShift.SelectedValue = mSelectedProduct.WorkShift;
                }
                else
                {
                    cbWorkShift.SelectedValue = mCurrentTeam.Id;
                }

                if (mSelectedProduct.Lengthtype == null)    //新增行时
                {
                    cbLength.IsEnabled = true;
                    cbLength.SelectedIndex = 0;

                    mSelectedProduct.Lengthtype = (int)EnumList.ProductQualityLevel.定尺;
                    DoLengthTypeChanged();
                }
                else
                {
                    cbLength.IsEnabled = false;
                    cbLength.SelectedValue = mSelectedProduct.Lengthtype;
                    if (mSelectedProduct.Length > 0)
                    {
                        txtLength.Text = string.Format("{0}", mSelectedProduct.Length);
                    }
                    DoLengthTypeChanged();
                }

                //非尺不需要输入长度和支数
                if (mSelectedProduct.Lengthtype != (int)EnumList.ProductQualityLevel.定尺)
                {
                    //txtLength.Visibility = Visibility.Visible;
                    //txtPiececount.IsReadOnly = false;
                    //txtPiececount.Background = new SolidColorBrush(Colors.White);
                }
                else
                {
                    //txtLength.Visibility = Visibility.Hidden;
                    //txtPiececount.IsReadOnly = true;
                    //txtPiececount.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0"));
                }

                if (mSelectedProduct.Meterweight > 0)
                {
                    txtMeterWeight.Text = string.Format("{0:#,#.000}", mSelectedProduct.Meterweight);
                }
                if (mSelectedProduct.Piececount > 0)
                {
                    txtPiececount.Text = string.Format("{0}", mSelectedProduct.Piececount);
                }

                if (mSelectedProduct.Weight > 0)
                {
                    txtWeight.Text = string.Format("{0:#,#.000}", mSelectedProduct.Weight);
                    txtCurrWeight.Text = txtWeight.Text;
                }
                else
                {
                    if (mSelectedProduct.Id <= 0)
                    {
                        txtCurrWeight.Text = "";
                        DoSpecChanged();
                    }
                }

                if (mSelectedProduct.Bundlecode == null)
                {
                    txtBundle.IsReadOnly = false;

                    if (mProductList.Count > 0)
                    {
                        var lastpd = mProductList[(mProductList.Count - 1)];
                        int bundle = 0;
                        int.TryParse(lastpd.Bundlecode, out bundle);

                        if (bundle > 0)
                        {
                            txtBundle.Text = string.Format("{0}", bundle + 1);
                        }
                    }
                }
                else
                {
                    txtBundle.IsReadOnly = true;
                    txtBundle.Background = new SolidColorBrush(Colors.WhiteSmoke);
                    if (mMaterial != null)
                    {
                        if (mMaterial.Deliverytype == (int)EnumList.DeliveryType.盘卷)
                        {
                            txtBundle.IsReadOnly = false;
                            txtBundle.Background = new SolidColorBrush(Colors.LawnGreen);
                        }
                    }
                    txtBundle.Text = string.Format("{0}", mSelectedProduct.Bundlecode);
                }

                //如果是新增的，可以改时间，否则不能改
                if (mSelectedProduct.Id > 0)
                {
                    dpProductionDate.IsEnabled = false;
                }
                else
                {
                    dpProductionDate.IsEnabled = true;
                }
            }
        }
        public void Change_config(object sender, RoutedEventArgs e)
        {
            OffsetConfig cgw = new OffsetConfig();
            cgw.Title = "更改配置";
            cgw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            cgw.updataSelectHandler += InitLabel;
            cgw.Owner = this;
            cgw.ShowDialog();
        }
        public void MaterialClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                using (PdProductAccess access = new PdProductAccess())
                {
                    var products = access.GetListByBatcode(mCurrentBatCode);
                    if (products != null && products.Count > 0)
                    {

                        MessageBoxResult mbr = MessageBox.Show("当前批号下已经生产了其他材质，不能更换！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (mbr == MessageBoxResult.OK)
                        {
                            flag = true;
                        }
                    }
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 材质切换下拉事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool flag = true;
        public void MaterialChanged(object sender, EventArgs e)
        {
            int classid = 0;
            int materialid = 0;

            var materialitem = (cbMaterial.SelectedItem as BaseProductMaterial);
            if (materialitem != null)
            {
                classid = materialitem.Classid;
                materialid = materialitem.Id;
            }

            using (PdProductAccess access = new PdProductAccess())
            {
                var products = access.GetListByBatcode(mCurrentBatCode);
                if (products != null && products.Count > 0)
                {
                    var lastproduct = products.Last();
                    if (materialitem != null
                        && lastproduct.Materialid.Value != materialitem.Id)
                    {
                        if (flag == false)
                        {
                            cbClass.SelectedValue = lastproduct.Classid;
                            flag = true;
                        }
                        else
                        {
                            MessageBoxResult mbr = MessageBox.Show("当前批号下已经生产了其他材质，不能更换！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                            if (mbr == MessageBoxResult.OK)
                            {
                                flag = false;
                                cbMaterial.SelectedValue = findMaterialById(lastproduct.Materialid.Value);

                                return;
                            }
                        }
                    }
                }
            }

            if (materialid == 0)
            {
                return;
            }

            if (cbMaterial.SelectedValue == null)
            {
                //清空规格等绑定
                mSpecList.Clear();
                cbSpec.ItemsSource = mSpecList;

                return;
            }

            DoMaterialChanged(classid, materialid);

            //重新加载产品
            ReloadProductWork(mCurrentBatCode);

            flag = true;
        }

        public void DoMaterialChanged(int classid, int materialid)
        {
            //更换标牌！！！
            using (BaseProductMaterialAccess access = new BaseProductMaterialAccess())
            {
                mMaterial = access.Single(materialid);
                if (mMaterial != null)
                {
                    BitmapImage background = null;
                    if (mMaterial.Deliverytype == (int)DataLibrary.EnumList.DeliveryType.盘卷)
                    {
                        if (mMaterial.Name.EndsWith("E"))
                        {
                            background = pan_e;
                        }
                        else
                        {
                            background = pan;
                        }
                    }
                    else    //其他的默认当成直条处理
                    {
                        if (mMaterial.Name.EndsWith("E"))
                        {
                            background = zhi_e;
                        }
                        else
                        {
                            background = zhi;
                        }
                    }

                    ImageBrush imageBrush = new ImageBrush();
                    imageBrush.ImageSource = background;
                    panelLabel.Background = imageBrush;
                }
            }

            using (BaseSpecificationsAccess access = new BaseSpecificationsAccess())
            {
                var list = access.GetListByClassAndMaterial(classid, materialid);

                mSpecList.Clear();

                //如果有数据就输出
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        item.FullSpecname = item.Callname + " x " + item.Referlength;

                        //如果是盘卷，只显示规格，不显示长度
                        if (mMaterial != null)
                        {
                            if (mMaterial.Deliverytype == (int)EnumList.DeliveryType.盘卷)
                            {
                                item.FullSpecname = item.Callname;
                            }
                        }
                        mSpecList.Add(item);
                    }

                }

                cbSpec.ItemsSource = mSpecList;
                if (mSelectedProduct != null && mSelectedProduct.Specid != null)
                {
                    //if (mSpecList.FirstOrDefault(w => w.Id == mSelectedProduct.Specid) != null)
                    //    cbSpec.SelectedValue = mSelectedProduct.Specid;
                    //else
                    cbSpec.SelectedIndex = 0;
                    if (cbSpec.SelectedItem is BaseSpecifications)
                        mSelectedProduct.Specname = (cbSpec.SelectedItem as BaseSpecifications).Specname;
                }
                else
                {
                    using (PdProductAccess pdaccess = new PdProductAccess())
                    {
                        //同材质的产品以车间最后一个产品的规格为准
                        var lastproduct = pdaccess.SingleLastProductByWorkshopid(mWorkshop.Id);
                        if (lastproduct != null)
                        {
                            if (lastproduct.Materialid == mMaterial.Id)
                            {
                                cbSpec.SelectedValue = lastproduct.Specid;
                            }
                            else
                            {
                                cbSpec.SelectedIndex = 0;
                            }
                        }
                        else
                        {
                            cbSpec.SelectedIndex = 0;
                        }
                    }
                }
            }

            //成功更换之后，更新当前的材质变量
            mMaterial = findMaterialById(materialid);

            lbBundle.Content = "捆号：";
            bundleHeader.Header = "捆号";
            lbBundle.Height = 25;
            lengthPanel.Visibility = Visibility.Visible;
            piececountPanel.Visibility = Visibility.Visible;
            btnPrints.Visibility = Visibility.Visible;
            txtBundle.IsReadOnly = true;
            lbLabelBundleCode.Visibility = System.Windows.Visibility.Hidden;
            lbLabelProductClass.Visibility = System.Windows.Visibility.Hidden;

            //如果是盘卷，捆号变成勾号
            if (mMaterial != null)
            {
                if (mMaterial.Deliverytype == (int)EnumList.DeliveryType.盘卷)
                {
                    lbBundle.Content = "勾号：";
                    lbBundle.Height = 40;
                    bundleHeader.Header = "勾号";
                    btnPrints.Visibility = Visibility.Hidden;
                    txtBundle.IsReadOnly = false;
                    lengthPanel.Visibility = Visibility.Collapsed;
                    piececountPanel.Visibility = Visibility.Collapsed;

                    lbLabelBundleCode.Visibility = System.Windows.Visibility.Visible;
                    lbLabelProductClass.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        public void WorkShiftChanged(object sender, EventArgs e)
        {

        }

        public void SpecChanged(object sender, EventArgs e)
        {
            DoSpecChanged();
        }

        public void DoSpecChanged()
        {
            int specid = 0;

            object val = cbSpec.SelectedItem;

            if (val == null)
            {
                //清空规格理论属性
                txtMeterWeight.Text = "";
                txtLength.Text = "";
                txtPiececount.Text = "";
                txtWeight.Text = "";

                return;
            }

            if (val is BaseSpecifications)
            {
                specid = ((BaseSpecifications)val).Id;
            }

            if (specid > 0)
            {
                BaseSpecifications specitem = findSpecById(specid);
                if (specitem != null)
                {
                    txtMeterWeight.Text = string.Format("{0:#,#.000}", specitem.Refermeterweight);

                    if (mSelectedProduct == null || mSelectedProduct.Length == null || mSelectedProduct.Length <= 0)
                    {
                        txtLength.Text = string.Format("{0}", specitem.Referlength);
                    }

                    txtPiececount.Text = string.Format("{0}", specitem.Referpiececount);
                    txtWeight.Text = string.Format("{0:#,#.000}", specitem.Referpieceweight);

                    if (mSelectedProduct != null)
                    {
                        if (mSelectedProduct.Id > 0)
                        {
                            mSelectedProduct.Meterweight = specitem.Refermeterweight;
                            mSelectedProduct.Length = specitem.Referlength;
                            mSelectedProduct.Piececount = specitem.Referpiececount;
                            mSelectedProduct.ReferWeight = specitem.Referpieceweight;
                        }
                    }
                }
            }
        }

        public void LengthTypeChanged(object sender, EventArgs e)
        {
            DoLengthTypeChanged();
        }

        public void DoLengthTypeChanged()
        {
            int lentype = 0;

            object val = cbLength.SelectedValue;

            if (val == null)
            {
                return;
            }

            int.TryParse(val.ToString(), out lentype);

            if (lentype != (int)EnumList.ProductQualityLevel.定尺)
            {
                //txtLength.Visibility = Visibility.Visible;
                //txtPiececount.IsReadOnly = false;
                //txtPiececount.Background = new SolidColorBrush(Colors.White);

                var spec = cbSpec.SelectedItem;
                if (spec is BaseSpecifications)
                {
                    if (mSelectedProduct.Length == null)
                    {
                        txtLength.Text = string.Format("{0}", ((BaseSpecifications)spec).Referlength);
                    }
                }
            }
            else
            {
                //txtLength.Visibility = Visibility.Hidden;
                //txtPiececount.IsReadOnly = true;
                //txtPiececount.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0"));

                var spec = cbSpec.SelectedItem;
                if (spec is BaseSpecifications)
                {

                    if (mSelectedProduct == null || mSelectedProduct.Length == null)
                    {
                        txtLength.Text = string.Format("{0}", ((BaseSpecifications)spec).Referlength);
                    }
                }
            }
        }

        private void NextBatCode_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ShowLoading();
            }));

            if (mbOffline)
            {
                MessageBoxResult dialogresult = MessageBox.Show("离线状态下切换批号将不能再返回当前批号，请确认是否继续操作？", "操作提醒", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (dialogresult == MessageBoxResult.Cancel)
                {
                    return;
                }
            }


            bool check = false;
            try
            {
                if (!mbOffline)
                {
            		check = CheckBatCode(mCurrentBatCode);
                }
                else
                {
                    check = true;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("Connection must be valid and open") != -1)
                {
                    this.setOffline();
                    check = true;
                }
            }

            Task.Factory.StartNew(new Action(() =>
            {
                if (check)
                {
                    string nextcode = GetBatCode(mCurrentBatCode, 1);
                    if (!string.IsNullOrEmpty(nextcode))
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            txtBatCode.Text = nextcode;

                            if (!mbOffline)
                            {
                                mCurrentBatCode = nextcode;

                                //切换轧制批号后，重置选中行
                                mSelectedProductIndex = -1;

                                //重新加载新的产品列表
                                StartReloadProduct(nextcode);
                            }
                            else
                            {
                                mSelectedProductIndex = -1;

                                if (nextcode != mCurrentBatCode)
                                {
                                    //调用自动生成方法
                                    mProductList = ProductAutoCreate(nextcode);

                                    mCurrentBatCode = nextcode;

                                    BindView();

                                    Thread.Sleep(200);
                                    HideLoading();
                                }
                            }

                        }));
                    }
                    else
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            BindView();

                            Thread.Sleep(200);
                            HideLoading();
                        }));
                    }
                }
                else
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Thread.Sleep(200);
                        HideLoading();
                    }));
                }
            }));
        }

        private void CurrWeight_Changed(object sender, TextChangedEventArgs e)
        {
            double curweight = 0;

            string txt = txtCurrWeight.Text;

            if (string.IsNullOrEmpty(txt))
            {
                return;
            }

            //判断第一位是否是逗号
            if (txt.Substring(0, 1) == ",")
            {
                txt = txt.Remove(0, 1);
            }

            if (!double.TryParse(txt, out curweight))
            {
                TextChange[] change = new TextChange[e.Changes.Count];
                e.Changes.CopyTo(change, 0);
                int offset = change[0].Offset;

                txtCurrWeight.Text = txtCurrWeight.Text.Remove(offset, change[0].AddedLength);

                txtCurrWeight.Select(offset, 0);

                return;
            }

            //获取光标位置
            var pos = txtCurrWeight.SelectionStart;
            var dotpos = txtCurrWeight.Text.IndexOf(".");
            var originlength = txtCurrWeight.Text.Length;

            if (curweight < 1)
            {
                txtCurrWeight.Text = string.Format("{0:0.000}", curweight);
            }
            else
            {
                //输入框格式化
                txtCurrWeight.Text = string.Format("{0:#,#.000}", curweight);
            }

            //如果是小数位数上，每输入一个数，光标后移
            if (pos > dotpos)
            {
                if (dotpos > 0)
                {
                    txtCurrWeight.Select(pos, 1);
                }
                else
                {
                    txtCurrWeight.Select(pos, 0);
                }
            }
            else
            {
                int formatlength = txtCurrWeight.Text.Length;
                if (formatlength - originlength == 1)
                {
                    txtCurrWeight.Select(pos + 1, 0);
                }
                else if (formatlength - originlength == -1)
                {
                    if (pos > 0)
                    {
                        txtCurrWeight.Select(pos - 1, 0);
                    }
                    else
                    {
                        txtCurrWeight.Select(0, 0);
                    }
                }
                else
                {
                    txtCurrWeight.Select(pos, 0);
                }
            }


            if (curweight <= 0)
            {
                return;
            }

            //计算偏移量
            int specid = 0;

            object val = cbSpec.SelectedValue;

            if (val == null)
            {
                return;
            }

            int.TryParse(val.ToString(), out specid);

            if (specid > 0)
            {
                BaseSpecifications specitem = findSpecById(specid);
                if (specitem != null)
                {
                    double referweight = specitem.Referpieceweight.Value;

                    double offset = (curweight - referweight) / referweight;

                    offset = offset * 100;

                    txtOffset.Text = offset.ToString("#0.00");

                    if (offset > 0)
                    {
                        txtOffset.VerticalContentAlignment = System.Windows.VerticalAlignment.Top;
                        txtOffset.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        txtOffset.VerticalContentAlignment = System.Windows.VerticalAlignment.Bottom;
                        txtOffset.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }
            }
        }

        private void CurrWeight_KeyDown(object sender, KeyEventArgs e)
        {
            //判断是否在小数位数
            var curpos = txtCurrWeight.SelectionStart;
            var dotpost = txtCurrWeight.Text.IndexOf(".");

            if (curpos > dotpost)
            {
                if (e.Key == Key.Back)
                {
                    var prevtxt = txtCurrWeight.Text.Substring(curpos - 1, 1);
                    if (prevtxt == ".")
                    {
                        txtCurrWeight.Select(curpos - 1, 0);
                    }
                    else
                    {
                        txtCurrWeight.Select(curpos - 1, 1);
                    }
                }
                else if (e.Key == Key.Left)
                {
                    if (curpos - 1 > dotpost)
                    {
                        txtCurrWeight.Select(curpos - 1, 1);
                        e.Handled = true;
                    }
                    else
                    {
                        txtCurrWeight.Select(curpos - 1, 0);
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.Right)
                {
                    txtCurrWeight.Select(curpos + 1, 1);
                    e.Handled = true;
                }
                else
                {
                    txtCurrWeight.Select(curpos, 1);
                }
            }
            else
            {
                if (e.Key == Key.Decimal || e.Key == Key.OemPeriod)
                {
                    var pos = txtCurrWeight.Text.IndexOf(".");
                    txtCurrWeight.Select(pos + 1, 0);
                }
                else if (e.Key == Key.Back)
                {

                    if (curpos > 0)
                    {
                        var prevtxt = txtCurrWeight.Text.Substring(curpos - 1, 1);
                        if (prevtxt == "," || prevtxt == ".")
                        {
                            txtCurrWeight.Select(curpos - 1, 0);
                        }
                    }
                }
                else if (e.Key == Key.Right)
                {
                    if (curpos == dotpost)
                    {
                        txtCurrWeight.Select(curpos + 1, 1);
                        e.Handled = true;
                    }
                }
            }

        }
        /// <summary>
        /// 刷新主线程UI
        /// </summary>
        /// <param name="classid"></param>
        /// <param name="materialid"></param>
        /// <param name="mCurrentBatCode"></param>
        public void UpdateMaterial(int classid, int materialid, string batcode)
        {

            cbClass.SelectedValue = findClassIndexById(classid);
            cbMaterial.SelectedValue = findMaterialById(materialid);
            DoMaterialChanged(classid, materialid);

            //重新加载产品
            StartReloadProduct(batcode,false);
        }
		
        /// <summary>
        /// 切换材质
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeCz_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("确认切换材质？", "操作提醒", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                if (cbMaterial.SelectedValue == null)
                    return;

                using (PdBatcodeAccess baccess = new PdBatcodeAccess())
                {
                    var batcode = baccess.SingleByBatcode(mCurrentBatCode);
                    if (batcode == null)
                    {
                        return;
                    }

                    ChangeCzWindow cgw = new ChangeCzWindow();
                    cgw.Title = "切换材质";
                    cgw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    cgw.mCurrentBatCode = batcode;
                    if (cbMaterial.SelectedItem != null)
                    {
                        var materialitem = (cbMaterial.SelectedItem as BaseProductMaterial);
                        cgw.MaterialId = materialitem.Id;
                        cgw.ClassId = materialitem.Classid;
                    }
                    // 订阅事件
                    cgw.updataSelectHandler += UpdateMaterial;
                    cgw.lbCurrCz.Content = string.Format("{0}/{1}", this.cbClass.Text, (this.cbMaterial.SelectedItem as BaseProductMaterial).Name);
                    cgw.Owner = this;
                    cgw.ShowDialog();

                }
            }
        }
		
        /// <summary>
        /// 切换规格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeGg_Click(object sender, RoutedEventArgs e)
        {
            if (cbSpec.ItemsSource == null)
                return;

            int materialId = 0;
            int classId = 0;

            if (cbMaterial.SelectedItem != null)
            {
                var materialitem = (cbMaterial.SelectedItem as BaseProductMaterial);
                materialId = materialitem.Id;
                classId = materialitem.Classid;
            }

            pubclassId = classId;
            pubmaterialId = materialId;
            if (cbMaterial.SelectedValue == null || materialId == 0)
                MessageBox.Show("请先选择材质", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);

            ChangeGgWindow cgw = new ChangeGgWindow();
            cgw.Title = "换规格操作";
            cgw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //订阅事件
            cgw.updataSelectHandler += updateChangeSelect;
            cgw.lbCurrGg.Content = this.cbSpec.Text;
            cgw.Owner = this;
            cgw.ShowDialog();
        }
        /// <summary>
        /// 更新规格
        /// </summary>
        /// <param name="id"></param>
        public void updateChangeSelect(int id)
        {
            cbSpec.SelectedValue = id;

            if (id > 0 && mSelectedProduct != null)
            {
                var selectedspec = findSpecById(id);

                if (selectedspec != null)
                {
                    mSelectedProduct.Specid = id;

                    mSelectedProduct.Specname = selectedspec.Specname;
                    mProductList[mSelectedProductIndex] = mSelectedProduct;

                    if (mSelectedProductIndex > 0)
                    {
                        dgProduct.SelectedIndex = mSelectedProductIndex;
                        dgProduct.ScrollIntoView(dgProduct.SelectedItem);
                    }

                    //将所有未保存的产品重置规格
                    for (var i = 0; i < mProductList.Count; i++)
                    {
                        if (mProductList[i].Id <= 0)
                        {
                            mProductList[i].Specid = id;
                            mProductList[i].Specname = selectedspec.Specname;
                        }
                    }

                    //更新界面
                    dgProduct.Items.Refresh();
                }
            }

            DoSpecChanged();
        }

        private void ChangeShift_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogresult = MessageBox.Show("请确认是否要注销登录？", "操作提醒", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (dialogresult == MessageBoxResult.OK)
            {
                LoginWindow lwin = new LoginWindow();
                lwin.Show();
                this.Close();
            }
            //ChangeShiftWindow csw = new ChangeShiftWindow();
            //csw.Title = "换班操作";

            //csw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            //csw.Owner = this;

            //bool? result = csw.ShowDialog();
            //if (result.HasValue && result.Value)
            //{
            //    var workshift = cbWorkShift.SelectedItem;
            //    if (workshift is PdWorkshopTeam)
            //    {
            //        MessageBox.Show("已经成功换班到[" + ((PdWorkshopTeam)workshift).TeamName + "]", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Information);
            //    }
            //}
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            DateTime? querytime = dpQueryTime.SelectedDate;
            string querybatcode = txtQueryBatcode.Text;

            if (querytime == null && string.IsNullOrEmpty(querybatcode))
            {

            }
            else
            {
                using (PdProductAccess access = new PdProductAccess())
                {
                    var list = access.Query(querybatcode, querytime);
                    if (list == null)
                    {
                        MessageBox.Show("没有找到满足条件的产品记录！", "查询结果", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        if (querytime == null && !string.IsNullOrEmpty(querybatcode))
                        {
                            mCurrentBatCode = querybatcode;
                          
                            StartReloadProduct(mCurrentBatCode);
                                
                            return;
                        }

                        mSelectedProductIndex = -1;

                        mProductList.Clear();
                        foreach (var item in list)
                            mProductList.Add(item);

                        BindView();
                    }

                }
            }

        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            DoSaveProduct();
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (mSelectedProduct != null)
            {
                if (mSelectedProduct.Id > 0)
                {
                    var product = DoSaveProduct();
                    if (product != null)
                    {
                        //获取Specname
                        var spec = findSpecById(product.Specid.Value);
                        if (spec != null)
                        {
                            product.Specname = spec.Specname;
                            product.ReferWeight = spec.Referpieceweight;

                            DoPrint(product, 1, 1);
                        }
                    }
                }
                else
                {
                    var product = DoSaveProduct();
                    if (product != null)
                    {
                        //获取Specname
                        var spec = findSpecById(product.Specid.Value);
                        if (spec != null)
                        {
                            product.Specname = spec.Specname;
                            product.ReferWeight = spec.Referpieceweight;

                            MessageBoxResult dialogresult = MessageBox.Show("产品记录已保存，是否立即开始打印？", "操作提醒", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                            if (dialogresult == MessageBoxResult.OK)
                            {
                                DoPrint(product, 1, 1);

                                //打印完了根据计划的成材率判断是否需要提示更换批号
                                if (!mbOffline)
                                {
                                    using (PdBatcodeAccess access = new PdBatcodeAccess())
                                    {
                                        if (!access.CheckBatcodeRate(product.Batcode))
                                        {
                                            MessageBox.Show("当前批号已经超出生产计划的成材率，\r\n请及时切换到下一个批号！！！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        }
                                    }
                                }
	                            else
	                            {
	                                MessageBox.Show("你目前正在离线状态，将不能生成下一个批号！\r\n请尽快恢复网络！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
	                            }
							}
                        }
                        else
                        {
                            MessageBox.Show("获取不到规格号", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("请称重保存后再进行标牌打印！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("请先选中要打印标牌的产品", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private PdProduct DoSaveProduct()
        {
            if (mSelectedProduct != null)
            {
                //判断是否是本班
                if (mSelectedProduct.WorkShift > 0 && mCurrentTeam.Id != mSelectedProduct.WorkShift)
                {
                    MessageBoxResult dialogresult = MessageBox.Show(string.Format("该生产信息是由[{0}]录入的，请确认是否要修改别班的生产信息？", mSelectedProduct.Shiftname), "操作提醒", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (dialogresult != MessageBoxResult.OK)
                    {
                        dgProduct.SelectedIndex = -1;
                        return null;
                    }
                }
            }

            //把称赋值到重量文本框
            txtWeight.Text = txtCurrWeight.Text;

            var product = SaveProduct();
            if (product != null)
            {
                if (mSelectedProductIndex > 0)
                {
                    //保存完成之后，自动选中下一条
                    dgProduct.SelectedIndex = mSelectedProductIndex + 1;

                    dgProduct.ScrollIntoView(dgProduct.SelectedItem);
                }

                return product;
            }

            return product;
        }

        private PdProduct SaveProduct()
        {

            //当数据都还没有初始化的时候，不进行保存
            if (mSelectedProduct == null)
            {
                return null;
            }

            //判断是否是本班
            if (mSelectedProduct.WorkShift > 0 && mCurrentTeam.Id != mSelectedProduct.WorkShift)
            {
                if (this.mbShouldShowTip)
                {
                    MessageBoxResult dialogresult = MessageBox.Show(string.Format("该生产信息是由[{0}]录入的，请确认是否要修改别班的生产信息？", mSelectedProduct.Shiftname), "操作提醒", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (dialogresult != MessageBoxResult.OK)
                    {
                        return null;
                    }
                }
            }

            this.mbShouldShowTip = true;

            int classid = 0, materialid = 0, specid = 0, teamid = 0;
            double meterweight = 0, weight = 0, length = 0;
            int piececount = 0, bundlecode = 0;
            int lengthtype = (int)EnumList.ProductQualityLevel.定尺;
            PdProduct product = new PdProduct();

            product.Batcode = txtBatCode.Text;

            if (cbMaterial.SelectedItem != null)
            {
                var materialitem = (cbMaterial.SelectedItem as BaseProductMaterial);
                classid = materialitem.Classid;
                materialid = materialitem.Id;
            }

            if (classid > 0)
            {
                product.Classid = classid;
            }

            if (materialid > 0)
            {
                product.Materialid = materialid;
            }

            if (cbWorkShift.SelectedValue != null)
            {
                int.TryParse(cbWorkShift.SelectedValue.ToString(), out teamid);
            }

            if (teamid > 0)
            {
                product.WorkShift = teamid;
                product.Shiftname = ((PdWorkshopTeam)cbWorkShift.SelectedItem).TeamName;
            }

            if (cbSpec.SelectedValue != null)
            {
                int.TryParse(cbSpec.SelectedValue.ToString(), out specid);
            }

            if (specid > 0)
            {
                product.Specid = specid;
            }

            if (cbLength.SelectedValue != null)
            {
                int.TryParse(cbLength.SelectedValue.ToString(), out lengthtype);
            }

            product.Lengthtype = lengthtype;

            if (mSelectedProduct.Id <= 0)        //新记录用当前时间，老记录不改时间
            {
                product.Createtime = (int)((dpProductionDate.SelectedDate != null) ? ((dpProductionDate.SelectedDate.Value.ToUniversalTime().Ticks - 621355968000000000) / 10000000) : 0);
            }
            else
            {
                product.Createtime = mSelectedProduct.Createtime;
            }

            if (specid > 0)
            {
                foreach (var spec in mSpecList)
                {
                    if (spec.Id == specid)
                    {
                        product.Length = spec.Referlength;
                        product.Meterweight = spec.Refermeterweight;
                        product.Piececount = spec.Referpiececount;
                        break;
                    }
                }
            }
            else
            {
                double.TryParse(txtMeterWeight.Text, out meterweight);
                if (meterweight > 0)
                {
                    product.Meterweight = meterweight;
                }

                int.TryParse(txtPiececount.Text, out piececount);
                if (piececount > 0)
                {
                    product.Piececount = piececount;
                }
            }

            if (lengthtype != (int)EnumList.ProductQualityLevel.定尺)
            {
                double.TryParse(txtLength.Text, out length);
                if (length > 0)
                {
                    product.Length = length;
                }
            }

            double.TryParse(txtWeight.Text, out weight);
            if (weight > 0)
            {
                product.Weight = weight;
            }

            if (!string.IsNullOrEmpty(txtBundle.Text))
            {
                product.Bundlecode = txtBundle.Text;
            }

            if (this.mUser != null)
            {
                product.Adder = this.mUser.Id;
            }
            else
            {
                product.Adder = 0;
            }

            if (mSelectedProduct.Id > 0)        //更新数据库
            {
                product.Id = mSelectedProduct.Id;
                product.Randomcode = mSelectedProduct.Randomcode;
                product.Bundlecode = mSelectedProduct.Bundlecode;
                product.Specname = mSelectedProduct.Specname;

                using (PdProductAccess access = new PdProductAccess())
                {
                    //如果是离线时不允许更新！！！
                    if (!mbOffline)
                    {
                        access.Update(product);
                    }
                    else
                    {
                        MessageBox.Show("离线状态下不允许更新！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                        return null;
                    }
                }
            }
            else   //插入新记录
            {

                //如果是新增,并且重量为0,则忽略
                if (product.Id <= 0 && (product.Weight == null || product.Weight <= 0))
                {
                    if (mMaterial.Measurement == (int)EnumList.MeteringMode.理计)
                    {
                        var spec = findSpecById(product.Specid.Value);
                        if (spec != null)
                            product.Weight = spec.Referpieceweight;
                        else
                            return null;
                    }
                    else
                    {
                        return null;
                    }
                }

                //获取规格名称
                foreach (var item in mSpecList)
                {
                    if (item.Id == product.Specid)
                    {
                        product.Specname = item.Specname;
                        break;
                    }
                }
                using (PdProductAccess access = new PdProductAccess())
                {
                    if (!string.IsNullOrEmpty(product.Batcode) && product.Bundlecode != null)
                    {
                        //判断是否有重复
                        //var dup = access.SingleByBatcodeAndBundle(product.Batcode, product.Bundlecode);
                        //if (dup != null)
                        //{
                        //    MessageBox.Show("出现重复记录，保存失败！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);

                        //    //通知刷新
                        //    StartReloadProduct(mCurrentBatCode);
                        //    return null;
                        //}

                        //生成随机校验码
                        product.Randomcode = GenerateRandomCode();

                        try
                        {
                            product.Id = (int)access.Insert(product);
                        }
                        catch (Exception e)
                        {
                            LogHelper.WriteLog("保存产品时发生错误：" + e.ToString());

                            //并发错误
                            if (e.Message.IndexOf("Duplicate entry") != -1)
                            {
                                int curcode = 0;

                                //如果是并行插入同一条失败，则更新捆号重新插入
                                if (product.Lengthtype != (int)EnumList.ProductQualityLevel.定尺
                                    && product.Lengthtype != (int)EnumList.ProductQualityLevel.标准)
                                {
                                    var num = product.Bundlecode.Replace("F", "");

                                    int.TryParse(num, out curcode);

                                    if (curcode > 0)
                                    {
                                        curcode++;
                                        product.Bundlecode = "F" + curcode.ToString("00");
                                    }
                                }
                                else
                                {
                                    int.TryParse(product.Bundlecode, out curcode);

                                    curcode++;
                                    product.Bundlecode = curcode.ToString("00");
                                }

                                try
                                {
                                    product.Id = (int)access.Insert(product);
                                    //通知刷新
                                    StartReloadProduct(mCurrentBatCode);
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.WriteLog("再次保存产品时发生错误：" + ex.ToString());

                                    //通知刷新
                                    StartReloadProduct(mCurrentBatCode);

                                    return null;
                                }
                            }
                            else if (e.Message.IndexOf("Connection must be valid and open") != -1)
                            {
                                mbOffline = true;

                                //如果是离线状态，则保存到本地
                                ProductSqliteAccess.CreateDB();
                                ProductSqliteAccess.CreateTable();

                                using (ProductSqliteAccess sqliteAccess = new ProductSqliteAccess())
                                {
                                    var result = sqliteAccess.Insert(product);
                                    if (result > 0)
                                    {
                                        product.Id = (int)result;
                                    }
                                }
                            }
                        }

                        if (!mbOffline)
                        {
                            //如果系统是随机调取预置质量数据，则在此处预先生成质量！！！
                            SaveDataPreset(product);
                        }

                        mSelectedProduct.Id = product.Id;
                    }
                }
            }

            if (product.Id > 0)
            {
                if (mSelectedProductIndex > mProductList.Count - 1)
                {
                    mProductList.Add(product);
                }
                else
                {
                    if (mSelectedProductIndex >= 0)
                    {
                        //把数据反写回选中的那条数据中
                        mProductList[mSelectedProductIndex] = product;
                    }
                }

                if (!mbOffline)
                {
                    CountTeamProducted();
                }

                txtCurrWeight.Focus();

                return product;
            }

            return null;
        }

        /// <summary>
        /// 添加关系数据
        /// </summary>
        private void SaveDataPreset(PdProduct product)
        {
            //炉号
            var batcode = product.Batcode;
            int materialid = product.Materialid.Value;
            using (PdQualityAccess pdAccess = new PdQualityAccess())
            {
                var qualitys = pdAccess.GetList(materialid);

                if (mConfig.SystemVersion == DataLibrary.EnumList.SystemVersion.简单版本)
                {
                    if (qualitys == null)
                    {
                        return;
                    }

                    using (DataPresetAccess dpAccess = new DataPresetAccess())
                    {
                        // 不存在关系数据才添加
                        var dpInfo = dpAccess.GetDpList(materialid, batcode);

                        // 取同一种材质最后一条关系数据
                        var lastInfo = dpAccess.GetDpList(materialid);
                        int qid = 0;

                        if (dpInfo == null)
                        {
                            if (lastInfo != null)
                            {
                                qid = lastInfo.Qid != null ? lastInfo.Qid.Value : 0;
                            }

                            // 按照算法取下一条质量数据
                            var pdInfo = pdAccess.FindNext(materialid, qid);
                            if (pdInfo.Id == 0)
                            {
                                if (qid > 0)
                                {
                                    // 如果是最后一条数据就取第一条
                                    pdInfo = pdAccess.FindNext(materialid, 0);

                                    if (pdInfo == null)
                                    {
                                        return;
                                    }
                                }
                            }

                            // 添加数据
                            dpAccess.InsertDataPreset(new PdQualityProductPreset
                            {
                                BatCode = batcode,
                                Materialid = materialid,
                                Qid = pdInfo.Id,
                                CreateTime = (int)Utils.TimeUtils.GetCurrentUnixTime()
                            });
                        }
                    }
                }
            }
        }

        private void PrevBatCode_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ShowLoading();
            }));

            Task.Factory.StartNew(new Action(() =>
            {
                string prevcode = null;
                try
                {
                    if (!mbOffline)
                    {
                        prevcode = GetBatCode(mCurrentBatCode, -1);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.IndexOf("Connection must be valid and open") != -1)
                    {
                        this.setOffline();
                    }

                    LogHelper.WriteLog("获取上一个批号时失败：" + ex.ToString());
                }

                if (!string.IsNullOrEmpty(prevcode))
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        txtBatCode.Text = prevcode;

                        mCurrentBatCode = prevcode;

                        //切换轧制批号后，重置选中行
                        mSelectedProductIndex = -1;

                        StartReloadProduct(prevcode);
                    }));
                }
                else
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Thread.Sleep(200);
                        HideLoading();
                    }));
                }
            }));
        }

        public bool CheckBatCode(string curcode)
        {
            PdBatcode curritem = new PdBatcode();

            using (PdBatcodeAccess access = new PdBatcodeAccess())
            {
                curritem = access.SingleByBatcode(curcode);
                if (curritem == null)
                {
                    return true;
                }
            }

            if (curritem.Status != 1)
            {
                MessageBoxResult result = MessageBox.Show("在进行下一个炉号时，请确认当前炉号已经生产录入完成？", "操作提醒", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.OK)
                {
                    //解锁当前炉号
                    using (PdBatcodeAccess access = new PdBatcodeAccess())
                    {
                        int ret = access.UnLockStatus(curcode);
                        if (ret > 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            return true;
        }

        public string GetBatCode(string curcode, int offset)
        {
            string batcode = "";
            int serialno = 0;
            //第一位为生产线
            string FIRST_WORD = myWorkshopProductLine;
            string DATE_CODE = DateTime.Now.ToString("yy");

            PdBatcode curritem = new PdBatcode();

            //判断如果是离线状态，并且点击下一个，则自动生成下一个批号（后两位数字+1）
            if (mbOffline)
            {
                if (!string.IsNullOrEmpty(curcode) && offset > 0)
                {
                    int number = 0;

                    if (curcode.Length > 2)
                    {
                        string numberstr = curcode.Substring(curcode.Length - 2, 2);
                        int.TryParse(numberstr, out number);

                        if (number > 0)
                        {
                            number++;
                            string prefix = curcode.Replace(numberstr, "");
                            batcode = prefix + number.ToString("00");
                        }
                    }

                    return batcode;
                }
            }

            using (PdWorkshopAccess waccess = new PdWorkshopAccess())
            {
                //查询数据库里最后一条批号
                using (PdBatcodeAccess access = new PdBatcodeAccess())
                {
                    if (this.mWorkshop == null)
                    {
                        this.mWorkshop = waccess.Single(myWorkshopProductLine);
                        if (this.mWorkshop != null)
                        {
                            curritem = access.SingleLastById(this.mWorkshop.Id);
                        }

                        if (this.mWorkshop == null)
                        {
                            return batcode;
                        }
                    }

                    //如果是刚打开程序初始读取批号
                    if (string.IsNullOrEmpty(batcode) && offset == 0)
                    {
                        if (curritem != null)
                        {
                            batcode = curritem.Batcode;
                            return batcode;
                        }
                    }

                    if (!string.IsNullOrEmpty(curcode))    //根据当前批号从数据库获取批号记录标识，以方便找上一条下一条
                    {
                        curritem = access.SingleByBatcode(curcode);
                    }

                    if (curritem == null || curritem.Id <= 0)     //如果当前批号为空并且数据库里也没有
                    {
                        batcode = string.Format("{0}{1}{2}{3}", DATE_CODE, FIRST_WORD, 1.ToString("D5"), myWorkshopProductLine);
                        serialno = Convert.ToInt32(string.Format("{0}{1}", DATE_CODE, 1.ToString("D5")));
                        PdBatcode pdcode = new PdBatcode();
                        pdcode.Batcode = batcode;
                        pdcode.Adder = "标牌打印程序";
                        pdcode.Status = 0;
                        pdcode.Workshopid = mCurrentTeam.WorkshopId;
                        pdcode.Serialno = serialno;
                        access.Insert(pdcode);

                        if (curritem == null)
                        {
                            return batcode;
                        }
                    }
                    else
                    {
                        //下一个
                        if (offset > 0)
                        {
                            if (curritem.Status == 0)
                            {
                                return curritem.Batcode;
                            }
                            var pdcode = access.SingleNextById(curritem.Serialno, this.mWorkshop.Id);
                            if (pdcode != null && pdcode.Id > 0)
                            {
                                return pdcode.Batcode;
                            }
                            else
                            {
                                int serNo = Convert.ToInt32(curritem.Serialno.ToString().Substring(DATE_CODE.Length)) + offset;
                                var nextBatcode = curcode.Substring((DATE_CODE + FIRST_WORD).Length, 5);
                                var currSerNo = curritem.Serialno.ToString().Substring(0, 2);
                                int number = Convert.ToInt32(nextBatcode) + offset;
                                if (currSerNo != DATE_CODE)
                                {
                                    number = 1;
                                }
                                serialno = Convert.ToInt32(string.Format("{0}{1}", DATE_CODE, number.ToString("D5")));
                                batcode = string.Format("{0}{1}{2}{3}", DATE_CODE, FIRST_WORD, number.ToString("D5"), myWorkshopProductLine);
                                access.Insert(new PdBatcode
                                {
                                    Batcode = batcode,
                                    Adder = "标牌打印程序",
                                    Status = 0,
                                    Workshopid = mCurrentTeam.WorkshopId,
                                    Serialno = serialno
                                });
                            }
                        }
                        //上一个
                        else
                        {

                            var pdcode = access.SingleByPrefixCode(curritem.Serialno, this.mWorkshop.Id);
                            if (pdcode != null)
                            {
                                return pdcode.Batcode;
                            }
                            else
                            {
                                return curritem.Batcode;
                            }
                        }
                    }
                }
            }

            return batcode;
        }

        public void ShowLabelDemo()
        {
            //先清空
            lbLabelProductClass.Content = null;
            lbLabelBatcode.Content = null;
            lbLabelRandomCode.Content = null;
            lbLabelSpec.Content = null;
            lbLabelWeight.Content = null;
            lbLabelLength.Content = null;
            lbLabelPiececount.Content = null;
            imgLabelQRcode.Source = null;
            lbLabelTime.Content = null;
            lbLabelBundleCode.Content = null;

            if (mSelectedProduct != null)
            {
                lbLabelGBStandard.Content = mMaterial.Gbdocument;
                lbLabelMaterial.Content = mMaterial.Name;
                lbLabelBatcode.Content = mSelectedProduct.Batcode;
                lbLabelRandomCode.Content = mSelectedProduct.Randomcode;
                lbLabelProductClass.Content = mMaterial.GbClassname;

                if (mSelectedProduct.Specid != null)
                {
                    var spec = findSpecById(mSelectedProduct.Specid.Value);
                    if (spec != null)
                    {
                        lbLabelSpec.Content = spec.Specname;

                        //如果标牌只打印理重
                        lbLabelWeight.Content = spec.Referpieceweight;
                        lbLabelLength.Content = spec.Referlength;
                        lbLabelPiececount.Content = spec.Referpiececount;

                        mSelectedProduct.Specname = spec.Specname;
                        mSelectedProduct.ReferWeight = spec.Referpieceweight;
                    }
                }
                else
                {
                    var item = cbSpec.SelectedItem;
                    if (item is BaseSpecifications)
                    {
                        var spec = (item as BaseSpecifications);
                        lbLabelSpec.Content = spec.Specname;

                        //如果标牌只打印理重
                        lbLabelWeight.Content = spec.Referpieceweight;
                        lbLabelLength.Content = spec.Referlength;
                        lbLabelPiececount.Content = spec.Referpiececount;

                        mSelectedProduct.Specid = spec.Id;
                        mSelectedProduct.Specname = spec.Specname;
                        mSelectedProduct.ReferWeight = spec.Referpieceweight;
                    }
                }

                if (mSelectedProduct.Lengthtype != (int)EnumList.ProductQualityLevel.定尺)
                {
                    lbLabelWeight.Content = mSelectedProduct.Weight;
                    lbLabelPiececount.Content = "";

                    //短尺和长尺不显示长度，用字母代替
                    if (mSelectedProduct.Lengthtype == (int)EnumList.ProductQualityLevel.短尺)
                    {
                        lbLabelLength.Content = "S";
                    }
                    else if (mSelectedProduct.Lengthtype == (int)EnumList.ProductQualityLevel.长尺)
                    {
                        lbLabelLength.Content = "L";
                    }
                }

                lbLabelTime.Content = mSelectedProduct.Createtime != null ? TimeUtils.GetDateTimeFromUnixTime(mSelectedProduct.Createtime.Value).ToString("yyyyMMdd") : DateTime.Now.ToString("yyyyMMdd");
                lbLabelBundleCode.Content = mSelectedProduct.Bundlecode;

                if (mSelectedProduct.Id > 0)
                {
                    var url = QRCodeUtils.GetShortUrl(myQRCodeUrlString + "?c="
                        + mSelectedProduct.Batcode + "&r=" + mSelectedProduct.Randomcode);
                    System.Drawing.Bitmap qrcodebmp = QRCodeUtils.Generate(url, 300);

                    imgLabelQRcode.Source = ImageUtils.ChangeBitmapToImageSource(qrcodebmp);
                }
            }

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

            if (!mbOffline)
            {
                //判断该产品是否还可以打印
                using (PdQRCodePrintedLogAccess access = new PdQRCodePrintedLogAccess())
                {
                    var canprintnumber = access.GetCanPrintNumber(mCurrentTeam.WorkshopId, product.Specid.Value);
                    if (canprintnumber <= 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("当月二维码授权额度已用完，请申请增加授权额度！", "操作提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
                        return;
                    }
                }
            }

            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    DrawingVisual vis = new DrawingVisual();
                    using (DrawingContext dc = vis.RenderOpen())
                    {
                        string batcode = product.Batcode;
                        if (mSelectedProduct != null)
                        {
                            batcode = mSelectedProduct.Batcode;
                        }
                        var url = QRCodeUtils.GetShortUrl(myQRCodeUrlString + "?c="
                            + batcode + "&r=" + product.Randomcode);
                        System.Drawing.Bitmap qrcodebmp = QRCodeUtils.Generate(url, 300);

                        var pen = new Pen(Brushes.Black, 1);
                        Rect rect = new Rect(0, 0, w, h);

                        #region 产品国标名
                        if (!string.IsNullOrEmpty(ProductClassPoint))
                        {
                            var point = CommonUtils.GetPointFromSetting(ProductClassPoint);

                            if (mMaterial.Deliverytype == (int)EnumList.DeliveryType.盘卷)
                            {
                                FormattedText formattedText = new FormattedText(
                                (mMaterial.GbClassname == null) ? "" : mMaterial.GbClassname,
                                CultureInfo.GetCultureInfo("zh-CHS"),
                                FlowDirection.LeftToRight,
                                new Typeface("微软雅黑"),
                                fontsize,
                                Brushes.Black);
                                formattedText.SetFontWeight(FontWeight.FromOpenTypeWeight(900));

                                var position = new Point(point.X + offsetX, point.Y + offsetY);
                                dc.DrawText(formattedText, position);
                            }
                        }
                        #endregion

                        #region 质量标准
                        if (!string.IsNullOrEmpty(GBStandardPoint))
                        {
                            var point = CommonUtils.GetPointFromSetting(GBStandardPoint);

                            FormattedText formattedText = new FormattedText(
                            (mMaterial.Gbdocument == null) ? "" : mMaterial.Gbdocument,
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

                        #region 牌号
                        if (!string.IsNullOrEmpty(MaterialPoint))
                        {
                            var point = CommonUtils.GetPointFromSetting(MaterialPoint);

                            FormattedText formattedText = new FormattedText(
                            mMaterial.Name,
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
                            product.Specname,
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

                            if (product.Lengthtype == (int)EnumList.ProductQualityLevel.短尺)
                            {
                                lengthtxt = "S";
                            }
                            else if (product.Lengthtype == (int)EnumList.ProductQualityLevel.长尺)
                            {
                                lengthtxt = "L";
                            }

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

                            string weighttxt = string.Format("{0}", product.ReferWeight);
                            if (mMaterial.Measurement == (int)EnumList.MeteringMode.磅计)
                            {
                                weighttxt = string.Format("{0}", product.Weight);
                            }
                            else
                            {
                                if (product.Lengthtype == (int)EnumList.ProductQualityLevel.长尺
                                    || product.Lengthtype == (int)EnumList.ProductQualityLevel.短尺)
                                {
                                    weighttxt = string.Format("{0}", product.Weight);
                                }
                            }

                            FormattedText formattedText = new FormattedText(
                            weighttxt,
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
                            if (mMaterial.Deliverytype != (int)EnumList.DeliveryType.盘卷
                                && product.Lengthtype == (int)EnumList.ProductQualityLevel.定尺)
                            {
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
                                        if (!mbOffline)
                                        {
                                            using (PdQRCodePrintedLogAccess taccess = new PdQRCodePrintedLogAccess())
                                            {
                                                var printedlog = taccess.SingleByProductid(product.Id);

                                                if (printedlog != null)
                                                {
                                                    printedlog.Number++;

                                                    taccess.Update(printedlog);
                                                }
                                                else
                                                {
                                                    PdQRCodePrintedLog log = new PdQRCodePrintedLog();
                                                    log.ProductId = product.Id;
                                                    log.SpecId = product.Specid.Value;
                                                    log.WorkshopId = mCurrentTeam.WorkshopId;
                                                    log.Number = 1;
                                                    log.Status = 1;

                                                    if (this.mUser != null)
                                                    {
                                                        log.Adder = this.mUser.Id;
                                                    }
                                                    else
                                                    {
                                                        log.Adder = 0;
                                                    }

                                                    taccess.Insert(log);
                                                }
                                            }
                                        }

                                        if (batprinted == count)
                                        {
                                            //重新读取
                                            StartReloadProduct(mCurrentBatCode);

                                            this.Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                HideLoading();
                                                batprinted = 0;

                                                if (this.mbOffline)
                                                {
                                                    MessageBox.Show("你目前正在离线状态，将不能生成下一个批号！\r\n请尽快恢复网络！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                                                }
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
                                                        if (shouldreload)
                                                        {
	                                                        //重新读取
                                                            StartReloadProduct(mCurrentBatCode);
                                                        }

                                                        this.Dispatcher.BeginInvoke(new Action(() =>
                                                        {
                                                            HideLoading();
                                                            batprinted = 0;

                                                            if (this.mbOffline)
                                                            {
                                                                MessageBox.Show("你目前正在离线状态，将不能生成下一个批号！\r\n请尽快恢复网络！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                                                            }
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
                                        if (!this.mbOffline)
                                        {
                                            using (PdQRCodePrintedLogAccess taccess = new PdQRCodePrintedLogAccess())
                                            {
                                                PdQRCodePrintedLog log = new PdQRCodePrintedLog();
                                                log.ProductId = product.Id;
                                                log.SpecId = product.Specid.Value;
                                                log.WorkshopId = mCurrentTeam.WorkshopId;
                                                log.Number = 2;
                                                log.Status = 1;

                                                if (this.mUser != null)
                                                {
                                                    log.Adder = this.mUser.Id;
                                                }
                                                else
                                                {
                                                    log.Adder = 0;
                                                }

                                                taccess.Insert(log);
                                            }
                                        }

                                        if (batprinted == count)
                                        {
                                            if (shouldreload)
                                            {
	                                            //重新读取
	                                            StartReloadProduct(mCurrentBatCode);
                                            }

                                            this.Dispatcher.BeginInvoke(new Action(() =>
                                            {
                                                HideLoading();
                                                batprinted = 0;

                                                if (this.mbOffline)
                                                {
                                                    MessageBox.Show("你目前正在离线状态，将不能生成下一个批号！\r\n请尽快恢复网络！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                                                }
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
                                                        if (shouldreload)
                                                        {
                                                        	//重新读取
                                                            StartReloadProduct(mCurrentBatCode);
                                                        }

                                                        this.Dispatcher.BeginInvoke(new Action(() =>
                                                        {
                                                            HideLoading();
                                                            batprinted = 0;

                                                            if (this.mbOffline)
                                                            {
                                                                MessageBox.Show("你目前正在离线状态，将不能生成下一个批号！\r\n请尽快恢复网络！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                                                            }
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

        public void BatPrint_Click(object sender, RoutedEventArgs e)
        {
            BaseProductClass classitem = null;
            BaseProductMaterial materialitem = null;
            int materialid = 0;
            int classid = 0;
            if (cbMaterial.SelectedItem != null)
            {
                materialitem = (cbMaterial.SelectedItem as BaseProductMaterial);
                materialid = materialitem.Id;

                classitem = new BaseProductClass();
                classid = materialitem.Classid;
                classitem.Id = materialitem.Classid;
                classitem.Name = materialitem.Classname;
            }
            else
            {
                //没有指定品名材质
                MessageBox.Show("没有指定品名材质，不能批量打印！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            BaseSpecifications specitem = null;
            int specid = 0;
            if (cbSpec.SelectedValue != null)
            {
                int.TryParse(cbSpec.SelectedValue.ToString(), out specid);
            }

            foreach (var item in mSpecList)
            {
                if (item.Id == specid)
                {
                    specitem = item;
                    break;
                }
            }

            PdBatcode batcode = new PdBatcode();
            batcode.Batcode = mCurrentBatCode;

            //弹出批量打印确认表单窗口，确认打印产品捆数
            BatPrintConfirmWindow window = new BatPrintConfirmWindow(classitem, specitem, batcode, materialitem);
            window.Title = "开始批量打印";
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = this;

            window.BatPrintConfirmedHandler += onBatPrintConfirmed;

            window.ShowDialog();

        }

        /// <summary>
        /// 确定批量打印的产品列表后
        /// </summary>
        /// <param name="list"></param>
        public void onBatPrintConfirmed(List<PdProduct> list)
        {
            if (list == null || list.Count <= 0)
            {
                return;
            }

            if (!mbOffline)
            {
                using (PdQRCodePrintedLogAccess access = new PdQRCodePrintedLogAccess())
                {
                    var canprintnumber = access.GetCanPrintNumber(mCurrentTeam.WorkshopId, list[0].Specid.Value);
                    if (canprintnumber <= 0)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("当月二维码授权额度已用完，请申请增加授权额度！", "操作提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
                        return;
                    }
                }
            }

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

            Task.Factory.StartNew(() =>
            {
                try
                {
                    DoBatPrint(list);
                }
                catch (Exception ex)
                {
                    if (ex.Message.IndexOf("Connection must be valid and open") != -1)
                    {
                        this.setOffline();

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show("网络异常，进入离线打印模式，请重新开始批量打印！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);

                            this.BindView();
                            txtCurrWeight.Focus();
                        }));

                    }
                }

                //打印完了根据计划的成材率判断是否需要提示更换批号
                if (!mbOffline)
                {
                    using (PdBatcodeAccess access = new PdBatcodeAccess())
                    {
                        if (!access.CheckBatcodeRate(list.First().Batcode))
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                MessageBox.Show("当前批号已经超出生产计划的成材率，\r\n请及时切换到下一个批号！！！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }));
                        }
                    }
                }
            });

        }

        int batprinted = 0;

        public void DoBatPrint(List<PdProduct> list)
        {
            int len = list.Count;
            int printing = 1;
            batprinted = 0;
            string formatstr = "正在批量打印第{0}件，共{1}件";

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                SetLoadingValue(printing / len * 100, "正在批量打印中...", string.Format(formatstr, printing, len));
                progress.IsIndeterminate = true;
            }));

            if (!mbOffline)
            {
                try
                {
                    //先保存产品，然后打印产品
                    using (PdProductAccess access = new PdProductAccess())
                    {
                        foreach (PdProduct pd in list)
                        {
                            //生成随机校验码
                            pd.Randomcode = GenerateRandomCode();
                            if (this.mUser != null)
                            {
                                pd.Adder = this.mUser.Id;
                            }

                            pd.Id = (int)access.Insert(pd);
                            if (pd.Id > 0)
                            {
                                DoPrint(pd, printing, len, true);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e.Message.IndexOf("Connection must be valid and open") != -1)
                    {
                        this.mbOffline = true;
                    }

                    LogHelper.WriteLog("批量打印时发生错误：" + e.ToString());

                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show("批量打印出错中断：" + e.Message, "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);

                        HideLoading();
                        batprinted = 0;

                        this.BindView();
                        txtCurrWeight.Focus();
                    }));
                }
            }
            else
            {
                using (ProductSqliteAccess sqliteaccess = new ProductSqliteAccess())
                {
                    foreach (PdProduct pd in list)
                    {
                        //生成随机校验码
                        pd.Randomcode = GenerateRandomCode();
                        if (this.mUser != null)
                        {
                            pd.Adder = this.mUser.Id;
                        }

                        //保存到本地
                        pd.Id = (int)sqliteaccess.Insert(pd);

                        DoPrint(pd, printing, len, true);

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (pd.Id > 0)
                            {
                                if (mSelectedProductIndex > mProductList.Count - 1)
                                {
                                    mProductList.Add(pd);
                                }
                                else
                                {
                                    if (mSelectedProductIndex >= 0)
                                    {
                                        //把数据反写回选中的那条数据中
                                        mProductList[mSelectedProductIndex] = pd;
                                    }

                                    mSelectedProductIndex++;
                                }
                            }
                        }));
                    }

                    this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            this.BindView();
                            txtCurrWeight.Focus();
                        }));
                }
            }
        }

        /// <summary>
        /// 补打
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            using (PdBatcodeAccess access = new PdBatcodeAccess())
            {
                var batcode = access.SingleByBatcode(this.mCurrentBatCode);

                ReprintWindow rw = new ReprintWindow(batcode);
                rw.Title = "标牌补打";
                rw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (cbMaterial.SelectedItem != null)
                {
                    var materialitem = (cbMaterial.SelectedItem as BaseProductMaterial);
                    rw.MaterialInfo = materialitem;
                }
                rw.doPrintHandler += btnReprintList;
                rw.Owner = this;
                rw.ShowDialog();
            }
        }

        public void btnReprintList(List<ProductInfo> productList)
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
                foreach (var item in productList)
                {
                    DoPrint(item, printing, len);
                }
            });

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

        public BaseSpecifications findSpecById(int id)
        {
            if (mSpecList != null)
            {
                foreach (var item in mSpecList)
                {
                    if (item.Id == id)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        public PdWorkshopTeam findWorkShiftById(int id)
        {
            if (mTeamList != null)
            {
                foreach (var item in mTeamList)
                {
                    if (item.Id == id)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 检查当前列表中是否有该捆号的有ID产品
        /// </summary>
        /// <param name="bundlecode"></param>
        /// <returns></returns>
        public int findRealProductByBundlecode(string bundlecode)
        {
            int retindex = -1;

            if (mProductList != null)
            {
                int index = 0;
                foreach (var item in mProductList)
                {
                    if (item.Bundlecode == bundlecode && item.Id > 0)
                    {
                        retindex = index;
                        break;
                    }
                    index++;
                }
            }

            return retindex;
        }

        private int findClassIndexById(int id)
        {
            int retindex = -1;

            if (mClassList != null)
            {
                int index = 0;
                foreach (var item in mClassList)
                {
                    if (item.Id == id)
                    {
                        retindex = index;
                        break;
                    }
                    index++;
                }
            }

            return retindex;
        }

        public BaseProductMaterial findMaterialById(int id)
        {
            if (mMaterialList != null)
            {
                foreach (var item in mMaterialList)
                {
                    if (item.Id == id)
                    {
                        return item;
                    }
                }
            }

            return null;
        }

        private int findProductLastIndex()
        {
            if (mProductList == null || mProductList.Count == 0)
            {
                return -1;
            }
            else
            {
                var prodcut = mProductList.First();
                int index = 0;
                for (var i = 0; i < mProductList.Count; i++)
                {
                    if (mProductList[i].Createtime > prodcut.Createtime)
                    {
                        prodcut = mProductList[i];
                        index = i;
                    }
                }

                return index;
            }
        }

        #region 称重设备相关代码

        private void InitWeightAutoInput()
        {

            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0)
            {

                ComDialog dlg = new ComDialog();
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dlg.Owner = this;

                bool? result = dlg.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    if (!string.IsNullOrEmpty(mComPortConfig.ComName))
                    {
                        serialport.PortName = mComPortConfig.ComName;//串口号
                        serialport.BaudRate = mComPortConfig.BaudRate;//波特率
                        serialport.DataBits = mComPortConfig.DataBits;//数据位
                        serialport.StopBits = (StopBits)mComPortConfig.StopBits;//停止位
                        serialport.ReadTimeout = mComPortConfig.TimeOut;//读取数据的超时时间，引发ReadExisting异常

                        ConnectComPort();
                    }
                }
            }
        }

        public void ConnectComPort()
        {
            if (!string.IsNullOrEmpty(mComPortConfig.ComName))
            {
                try
                {
                    if (serialport.IsOpen)
                    {
                        serialport.Close();
                        serialport.Open();//打开串口
                    }
                    else
                    {
                        serialport.Open();//打开串口
                    }

                    ReceiveComPortData();

                    lbStatus.Content = "称重设备已连接";

                }
                catch (Exception ex)
                {
                    MessageBox.Show("错误：" + ex.Message, "串口通信");
                }
            }
            else
            {
                MessageBox.Show("请先设置串口！", "RS232串口通信");
            }
        }

        private Thread ComPortReceviceThread;
        protected Boolean stop = false;
        protected Boolean conState = false;
        bool bAccpet = false;

        private List<byte> mReceivedBytes = new List<byte>();

        private Byte XON = 0x02;
        private Byte XOFF = 0x03;

        public void ReceiveComPortData()
        {
            if (serialport.IsOpen)
            {
                //使用委托以及多线程进行
                bAccpet = true;
                ComPortReceviceThread = new Thread(new ThreadStart(ComPortReceviceThreadDelegate));
                ComPortReceviceThread.Start();
            }
            else
            {
                MessageBox.Show("请先打开串口");
            }
        }

        private void ComPortReceviceThreadDelegate()
        {
            reaction r = new reaction(fun);
            r();
        }

        //下面用到了接收信息的代理功能，此为设计的要点之一
        delegate void DelegateAcceptData();
        void fun()
        {
            while (bAccpet)
            {
                Thread.Sleep(1000);
                AcceptData();
            }
        }

        delegate void reaction();
        void AcceptData()
        {
            if (!Dispatcher.CheckAccess())
            {
                try
                {
                    DelegateAcceptData ddd = new DelegateAcceptData(AcceptData);
                    this.Dispatcher.BeginInvoke(ddd, new object[] { });
                }
                catch { }
            }
            else
            {
                try
                {
                    byte[] buffer = new byte[serialport.ReadBufferSize + 1];
                    int size = serialport.Read(buffer, 0, serialport.ReadBufferSize);

                    LogHelper.WriteLog(ByteUtils.byteToHexStr(buffer, size));

                    for (var i = 0; i < size; i++)
                    {
                        mReceivedBytes.Add(buffer[i]);
                        if (buffer[i] == XOFF)
                        {
                            //在接收数据中获取最后这帧数据
                            var buf = new List<byte>();
                            for (var j = 0; j < mReceivedBytes.Count; j++)
                            {
                                if (mReceivedBytes[j] == XON && buf.Count == 0)
                                {
                                    buf.Add(mReceivedBytes[j]);
                                }
                                else
                                {
                                    if (mReceivedBytes.Count > 0)
                                    {
                                        buf.Add(mReceivedBytes[j]);
                                        if (mReceivedBytes[j] == XOFF)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }

                            if (buf.Count == 12)
                            {
                                LogHelper.WriteLog("ReadA1Buffer");

                                DelegateReadA1Buffer ddd = new DelegateReadA1Buffer(AcceptData);
                                this.Dispatcher.BeginInvoke(ddd, buf);

                            }
                            else if (buf.Count == 9)
                            {
                                LogHelper.WriteLog("ReadD2Buffer");
                                DelegateReadD2Buffer ddd = new DelegateReadD2Buffer(AcceptData);
                                this.Dispatcher.BeginInvoke(ddd, buf);
                            }

                            //读完即清空
                            mReceivedBytes.Clear();
                        }
                    }

                }
                catch (Exception ex) { }
            }
        }

        delegate void DelegateReadA1Buffer();
        public void ReadA1Buffer(byte[] buf)
        {

            //byte[] buf = new byte[]{
            //                        0x02,0x2b,0x30,0x31,0x32,0x30,0x30,0x35,0x02,0x32,0x46,0x03
            //                    };
            //开始解析数据
            //XON｜符号（+/-）｜3~8重量数据（高位…低位）|小数点位数|异或校验高位|异或校验低位|XOFF

            //符号
            string flag = string.Format("{0}", (char)buf[1]);

            //重量
            string weight = string.Format("{0}", (char)buf[2])
                + string.Format("{0}", (char)buf[3])
                + string.Format("{0}", (char)buf[4])
                + string.Format("{0}", (char)buf[5])
                + string.Format("{0}", (char)buf[6])
                + string.Format("{0}", (char)buf[7]);

            //小数点位置
            int dot = buf[8];

            string front = weight.Substring(0, weight.Length - dot);
            string end = weight.Substring(front.Length, weight.Length - front.Length);

            int bflag = buf[9];     //D4~D7
            int sflag = buf[10];    //D0~D3

            string realweight = weight;
            if (dot > 0)
            {
                realweight = front + "." + end;
            }

            txtCurrWeight.Text = realweight;
        }

        delegate void DelegateReadD2Buffer();
        public void ReadD2Buffer(byte[] buf)
        {
            //byte[] buf = new byte[]{
            //                        0x3D,0x35,0x30,0x2E,0x32,0x31,0x30,0x30,0x00
            //                    };
            //开始解析数据
            //=｜2~8重量数据（高位…低位）|符号(0/-)

            //符号
            string flag = string.Format("{0}", (char)buf[8]);

            //重量
            string weight = string.Format("{0}", (char)buf[7])
                + string.Format("{0}", (char)buf[6])
                + string.Format("{0}", (char)buf[5])
                + string.Format("{0}", (char)buf[4])
                + string.Format("{0}", (char)buf[3])
                + string.Format("{0}", (char)buf[2])
                + string.Format("{0}", (char)buf[1]);

            string realweight = weight;
            if (flag != "\0")
            {
                realweight = flag + realweight;
            }

            txtCurrWeight.Text = realweight;
        }
        
    }
    #endregion

}
