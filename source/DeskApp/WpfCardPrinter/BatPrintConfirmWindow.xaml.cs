using DataLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using WpfCardPrinter.ModelAccess;

namespace WpfCardPrinter
{

    public class BatPrintProductSpec
    {
        public int Specid { get; set; }
        public string Specname { get; set; }
        public double? Speclength { get; set; }
        public string FullSpecname { get; set; }
        public int Printnums { get; set; }
        public string Workshift { get; set; }
        public int Lengthtype { get; set; }
    }

    /// <summary>
    /// BatPrintConfirmWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BatPrintConfirmWindow : Window
    {
        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="list"></param>
        public delegate void BatPrintConfirmed(List<DataLibrary.PdProduct> list);
        public event BatPrintConfirmed BatPrintConfirmedHandler;

        public MainWindow main { get; set; }

        public PdBatcode batcode = null;
        public BaseProductClass mPdClass = null;
        public BaseProductMaterial material = null;
        public BaseSpecifications defaultSpec = null;

        public static ObservableCollection<BatPrintProductSpec> SpecList = new ObservableCollection<BatPrintProductSpec>();

        public static ObservableCollection<BatPrintProductSpec> allspecs = new ObservableCollection<BatPrintProductSpec>();
        public ObservableCollection<BatPrintProductSpec> CanSelectSpecs
        {
            get { return allspecs; }
        }


        public BatPrintConfirmWindow()
        {
            InitializeComponent();
        }

        public BatPrintConfirmWindow(BaseProductClass pdclass, BaseSpecifications spec,PdBatcode batcode,BaseProductMaterial material)
        {
            InitializeComponent();

            this.mPdClass = pdclass;
            this.defaultSpec = spec;
            this.batcode = batcode;
            this.material = material;

            InitSpecList();
            BindView();
        }

        public void InitSpecList(){

            main = (MainWindow)Application.Current.MainWindow;

            allspecs.Clear();
            SpecList.Clear();

            if (defaultSpec != null)
            {
                BatPrintProductSpec spec = new BatPrintProductSpec();
                spec.Specid = defaultSpec.Id;
                spec.Specname = defaultSpec.Specname;
                spec.Speclength = defaultSpec.Referlength;
                spec.Printnums = 20;
                spec.FullSpecname = defaultSpec.FullSpecname;
                spec.Workshift = main.GetCurrentWorkShift().TeamName;

                if(this.material.Deliverytype == (int)EnumList.DeliveryType.盘卷)
                {
                    spec.Lengthtype = (int)EnumList.ProductQualityLevel.标准;
                }
                else
                {
                    spec.Lengthtype = (int)EnumList.ProductQualityLevel.定尺;
                }

                allspecs.Add(spec);

                SpecList.Add(spec);
            }
            
            foreach (var item in main.mSpecList)
            {
                if (item.Id != defaultSpec.Id)
                {
                    BatPrintProductSpec spec = new BatPrintProductSpec();
                    spec.Specid = item.Id;
                    spec.Specname = item.Specname;
                    spec.Speclength = item.Referlength;
                    spec.Printnums = 0;
                    spec.FullSpecname = item.FullSpecname;

                    if (this.material.Deliverytype == (int)EnumList.DeliveryType.盘卷)
                    {
                        spec.Lengthtype = (int)EnumList.ProductQualityLevel.标准;
                    }
                    else
                    {
                        spec.Lengthtype = (int)EnumList.ProductQualityLevel.定尺;
                    }

                    allspecs.Add(spec);
                }
            }

        }

        public void BindView()
        {
            dgSpec.ItemsSource = SpecList;

            txtBatcode.Text = batcode.Batcode;
            txtMaterial.Text = material.Name;
        }


        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            //获取到所有的规格和打印数量
            var list = SpecList;

            var products = new List<PdProduct>();

            StringBuilder confirmmsg = new StringBuilder();
            confirmmsg.Append("准备打印以下产品的标牌：\r\n\r\n");

            int start = 1;
            if (int.TryParse(main.mStartBundle, out start))
            {

            }
            else
            {
                start = 1;
            }

            foreach(BatPrintProductSpec item in list)
            {
                //没有选中规格的直接提示，然后跳出
                if ((item.Specid <= 0) && item.Printnums > 0)
                {
                    MessageBox.Show("输入有误，还有没指定规格的行！！！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //根据选中的Specid，补全Specname，没有选规格的行忽略掉
                var spec=main.findSpecById(item.Specid);
                if (spec == null)
                {
                    continue;
                }

                string msg = string.Format("{0}－{1}件\r\n", spec.FullSpecname, item.Printnums);

                confirmmsg.Append(msg);

                int length = item.Printnums;
                
                try
                {
                    if (!main.isOffline())
                    {
                        using (PdProductAccess access = new PdProductAccess())
                        {
                            for (int i = 0; i < length; i++)
                            {
                                string bundlecode = null;
                                //判断捆号是否存在，一直找到不存在的捆号为止
                                while (bundlecode == null)
                                {
                                    var bundle = start.ToString("00");
                                    var p = access.SingleByBatcodeAndBundle(batcode.Batcode, bundle);
                                    if (p == null)
                                    {
                                        bundlecode = bundle;
                                        start++;
                                    }
                                    else
                                    {
                                        start++;
                                    }
                                }

                                //生成并保存产品列表，然后通知打印
                                PdProduct product = new PdProduct();
                                product.Classid = mPdClass.Id;
                                product.Batcode = batcode.Batcode;
                                product.Materialid = material.Id;
                                product.Specid = spec.Id;
                                product.Lengthtype = item.Lengthtype;
                                product.Length = spec.Referlength;
                                product.Bundlecode = bundlecode;
                                product.Piececount = spec.Referpiececount;
                                product.Meterweight = spec.Refermeterweight;
                                product.ReferWeight = spec.Referpieceweight;
                                product.WorkShift = main.GetCurrentWorkShift().Id;
                                product.Shiftname = main.GetCurrentWorkShift().TeamName;
                                product.Weight = spec.Referpieceweight;         //批量打印默认打理重

                                product.Specname = spec.Specname;

                                //时间以输入框的时候为准
                                product.Createtime = (int)((main.dpProductionDate.SelectedDate != null) ? Utils.TimeUtils.GetUnixTimeFromDateTime(main.dpProductionDate.SelectedDate.Value) : 0);

                                products.Add(product);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.IndexOf("Connection must be valid and open") != -1)
                    {
                        //通知主界面，现在已是离线状态
                        main.setOffline();
                    }
                }

                if (main.isOffline())
                {
                    //如果是离线状态，则与本地窗口中的数据进行比对
                    for (int i = 0; i < length; i++)
                    {
                        string bundlecode = null;
                        //判断捆号是否存在，一直找到不存在的捆号为止
                        while (bundlecode == null)
                        {
                            var bundle = start.ToString("00");
                            var p = main.findRealProductByBundlecode(bundle);
                            if (p < 0)
                            {
                                bundlecode = bundle;
                                start++;
                            }
                            else
                            {
                                start++;
                            }
                        }

                        //生成并保存产品列表，然后通知打印
                        PdProduct product = new PdProduct();
                        product.Classid = mPdClass.Id;
                        product.Batcode = batcode.Batcode;
                        product.Materialid = material.Id;
                        product.Specid = spec.Id;
                        product.Lengthtype = item.Lengthtype;
                        product.Length = spec.Referlength;
                        product.Bundlecode = bundlecode;
                        product.Piececount = spec.Referpiececount;
                        product.Meterweight = spec.Refermeterweight;
                        product.ReferWeight = spec.Referpieceweight;
                        product.Createtime = (int)Utils.TimeUtils.GetCurrentUnixTime();
                        product.WorkShift = main.GetCurrentWorkShift().Id;
                        product.Shiftname = main.GetCurrentWorkShift().TeamName;
                        product.Weight = spec.Referpieceweight;             //批量打印默认打理重

                        product.Specname = spec.Specname;

                        products.Add(product);
                    }
                }
            }

            confirmmsg.Append("\r\n请确认打印机已经准备好，然后点击“确定”按钮开始打印！");

            MessageBoxResult mbr = MessageBox.Show(confirmmsg.ToString(), "操作确认", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK,MessageBoxOptions.DefaultDesktopOnly);
            if (mbr == MessageBoxResult.OK)
            {
                this.DialogResult = false;

                if (BatPrintConfirmedHandler != null)
                    BatPrintConfirmedHandler.Invoke(products);

                
            }
        }

    }
}
