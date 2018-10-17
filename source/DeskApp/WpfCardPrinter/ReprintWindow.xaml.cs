using DataLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfCardPrinter.ModelAccess;
using WpfCardPrinter.Utils;

namespace WpfCardPrinter
{
    /// <summary>
    /// Reprint.xaml 的交互逻辑
    /// </summary>
    public partial class ReprintWindow : Window
    {
        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="id"></param>
        public delegate void DoPrint(List<ProductInfo> productInfos);
        public event DoPrint doPrintHandler;
        private PdBatcode mCurrentBatCode { get; set; }
        public BaseProductMaterial MaterialInfo { get; set; }
        private ObservableCollection<ProductInfo> m_list = new ObservableCollection<ProductInfo>();
        public ReprintWindow(PdBatcode mCurrentBatCode)
        {
            InitializeComponent();
            this.mCurrentBatCode = mCurrentBatCode;
            ShowLoading();
            InitWork();
            BindView();
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
                SetLoadingValue(50, "", "正在装载标牌资源...");
            }));

            InitData("initdata");

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

        private void dgProduct_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
        /// <summary>
        /// 打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (m_list.Count > 0 && m_list.Any())
            {
                var selectList = m_list.Where(w => w.Checked).ToList();
                if (selectList.Count <= 0 || !selectList.Any())
                {
                    MessageBox.Show("请选择要补打的产品!", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {

                    if (doPrintHandler != null)
                    {
                        doPrintHandler.Invoke(selectList);
                        this.DialogResult = false;
                    }
                }
            }
            else
            {
                MessageBox.Show("没有可补打的产品,请重新选择轧制批号!", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void InitData(string type)
        {
            using (PdProductAccess access = new PdProductAccess())
            {
                var list = new List<PdProduct>();
                if (type == "initdata")
                {
                    list = access.Query(mCurrentBatCode.Batcode, null, mCurrentBatCode.Workshopid);
                }
                else
                {
                    list = access.Query(this.txtQueryBatcode.Text.Trim(), null, mCurrentBatCode.Workshopid);
                    if (list == null)
                    {
                        MessageBox.Show("没有找到满足条件的产品记录！", "查询结果", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        using (PdBatcodeAccess batcode_access = new PdBatcodeAccess())
                        {
                            var code = batcode_access.SingleByBatcode(this.txtQueryBatcode.Text.Trim());
                            if (code != null)
                            {
                                PdBatcode searched_batcode = code;
                                mCurrentBatCode = searched_batcode;
                            }
                            else
                            {
                                MessageBox.Show("数据异常，无法找到批号！", "查询结果", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }

                }
                if (list != null && list.Any())
                {
                    foreach (var item in list)
                    {
                        var pdinfo = new ProductInfo();
                        pdinfo.Id = item.Id;
                        pdinfo.Adder = item.Adder;
                        pdinfo.Batcode = item.Batcode;
                        pdinfo.Bundlecode = item.Bundlecode;
                        pdinfo.BundlecodeValue = item.BundlecodeValue;
                        pdinfo.Checked = false;
                        pdinfo.Classid = item.Classid;
                        pdinfo.Classname = item.Classname;
                        pdinfo.Createtime = item.Createtime;
                        pdinfo.Length = item.Length;
                        pdinfo.Lengthtype = item.Lengthtype;
                        pdinfo.Materialid = item.Materialid;
                        pdinfo.Materialname = item.Materialname;
                        pdinfo.Meterweight = item.Meterweight;
                        pdinfo.Piececount = item.Piececount;
                        pdinfo.Randomcode = item.Randomcode;
                        pdinfo.ReferWeight = item.ReferWeight;
                        pdinfo.Shiftname = item.Shiftname;
                        pdinfo.Specid = item.Specid;
                        pdinfo.Specname = item.Specname;
                        pdinfo.Weight = item.Weight;
                        pdinfo.WorkShift = item.WorkShift;
                        m_list.Add(pdinfo);
                    }
                }
            }
        }
        private void BindView()
        {
            if (m_list.Count > 0)
            {
                if (dgProduct.ItemsSource == null)
                {
                    dgProduct.ItemsSource = m_list;
                }
            }

            dgProduct.Items.Refresh();
            dgProduct.Focus();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedlist = dgProduct.SelectedItems;

            var count = selectedlist.Count;
            foreach (var selecteditem in selectedlist)
            {
                if (selecteditem is ProductInfo)
                {
                    ProductInfo product = (ProductInfo)selecteditem;

                    for (var i = 0; i < m_list.Count; i++)
                    {
                        if (m_list[i].Id == product.Id)
                        {
                            if (count > 1)
                            {
                                m_list[i].Checked = true;
                            }
                            else
                            {
                                if (product.Checked)
                                {
                                    m_list[i].Checked = false;
                                }
                                else
                                {
                                    m_list[i].Checked = true;
                                }
                            }

                            break;
                        }
                    }
                }
            }

            BindView();
        }
        /// 全选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ckbSelectedAll_Checked(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < m_list.Count; i++)
            {
                m_list[i].Checked = true;
            }

            BindView();
        }

        /// <summary>
        /// 全不选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ckbSelectedAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < m_list.Count; i++)
            {
                m_list[i].Checked = false;
            }

            BindView();
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
        /// 查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (m_list.Count > 0 && m_list.Any())
            {
                m_list.Clear();
            }
            InitData("");
            BindView();
        }
    }
}