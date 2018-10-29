using System;
using System.Collections.Generic;
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
using DataLibrary;
using WpfQualityCertPrinter.ModelAccess;
using WpfQualityCertPrinter.ViewModel;
using System.Collections.ObjectModel;

namespace WpfQualityCertPrinter
{
    /// <summary>
    /// SelectProductWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SelectProductWindow : Window
    {
        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="list"></param>
        /// <param name="lpn"></param>
        /// <param name="consignor"></param>
        /// <param name="sellerid"></param>
        public delegate void SelectProductInfoConfirmed(ObservableCollection<SelectedProductInfoNew> list,
            string lpn, string consignor, int sellerid, int printid, DateTime OutDate);
        public event SelectProductInfoConfirmed SelectedProductInfoConfirmedHandler;

        private ObservableCollection<SelectedProductInfoNew> m_productinfolist = new ObservableCollection<SelectedProductInfoNew>();

        private int m_Printlogid = 0;


        /// <summary>
        /// 材质列表
        /// </summary>
        private List<MaterialList> materList;

        private List<BaseSpecifications> specList;


        public SelectProductWindow()
        {
            InitializeComponent();

            InitData();

            BindView();
        }

        private void InitData()
        {
            this.OutDate.SelectedDate = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            using (BaseProductMaterialAccess access = new BaseProductMaterialAccess())
            {
                materList = access.GetShowList();
                if (materList != null)
                {
                    cdMaterial.ItemsSource = materList;
                }
            }

            using (BaseSpecificationsAccess access = new BaseSpecificationsAccess())
            {
                specList = access.GetList();
                if (specList == null)
                {
                    specList = new List<BaseSpecifications>();
                }
            }

            using (SaleSellerAccess access = new SaleSellerAccess())
            {
                var list = access.GetTopSellerList();
                if (list != null)
                {
                    cbSeller.ItemsSource = list;
                    if (list.Count > 0)
                    {
                        cbSeller.SelectedIndex = 0;
                    }
                }
            }

            using (SalePrintlogNewAccess access = new SalePrintlogNewAccess())
            {
                //获取最近使用过的车牌号
                var list = access.GetRecentLpns();
                if (list != null)
                {
                    cbLpn.ItemsSource = list;
                }
            }

            //获取最近使用过的收货单位
            using (SalePrintlogNewAccess access = new SalePrintlogNewAccess())
            {
                //获取最近使用过的收货单位
                var list = access.GetRecentConsignor();
                if (list != null)
                {
                    cbConsignee.ItemsSource = list;
                }
            }

        }

        public void BindView()
        {
            if (m_productinfolist.Count > 0)
            {
                if (dgSelectProducts.ItemsSource == null)
                {
                    dgSelectProducts.ItemsSource = m_productinfolist;
                }
            }

            dgSelectProducts.Items.Refresh();
            dgSelectProducts.Focus();
        }


        private void Recent_Click(object sender, RoutedEventArgs e)
        {
            RecentPrintlogWindow rpw = new RecentPrintlogWindow();
            rpw.Title = "选择最近生成的质量证明书";
            rpw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            rpw.Owner = this;

            rpw.PrintlogSelectedHandler += OnPrintlogSelected;

            rpw.ShowDialog();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

            string lpn, consignor;
            int sellerid = 0;

            if (string.IsNullOrEmpty(cbLpn.Text))
            {
                MessageBox.Show("必须填写随车车牌号才能出库！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (cbSeller.SelectedValue == null)
            {
                MessageBox.Show("必须选择售达方才能出库！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (this.m_productinfolist.Count <= 0)
            {
                MessageBox.Show("必须输入好随车产品才能出库！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!this.OutDate.SelectedDate.HasValue)
            {
                MessageBox.Show("必需设置出证日期！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrEmpty(cbConsignee.Text))
            {
                consignor = cbConsignee.Text;
            }

            lpn = cbLpn.Text;
            consignor = cbConsignee.Text;

            int.TryParse(cbSeller.SelectedValue.ToString(), out sellerid);

            if (cbSeller.SelectedValue == null)
            {
                MessageBox.Show("无效的售达方数据！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //判断所选的货是否能打在一张质保书
            //判断数据是否合法（不能选不同材质的数据）
            bool finderr = false;
            SelectedProductInfoNew previnfo = null;
            for (var i = 0; i < this.m_productinfolist.Count; i++)
            {
                if (previnfo == null)
                {
                    previnfo = this.m_productinfolist[i];
                    continue;
                }

                var item = this.m_productinfolist[i];
                if (previnfo.Materialid != item.Materialid)
                {
                    finderr = true;
                    break;
                }

                previnfo = item;
            }

            if (finderr)
            {
                MessageBox.Show("不同牌号的产品不能打印在一张质保书中，请按要求选择！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {

                EnumList.MeteringMode meterMode = EnumList.MeteringMode.理计;
                #region 判断是否 是理计产品
                if (this.cdMaterial.SelectedValue != null && !string.IsNullOrWhiteSpace(this.cdMaterial.SelectedValue.ToString()))
                {
                    var mater = materList.FirstOrDefault(c => c.Materialid == (int)this.cdMaterial.SelectedValue);
                    if (mater == null)
                    {
                        MessageBox.Show("牌号选择有问题", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        meterMode = mater.Measurement;
                    }
                }
                #endregion

                if (this.m_productinfolist.Count(c => !string.IsNullOrWhiteSpace(c.Batcode)) < 1)
                {
                    MessageBox.Show("请输入批号", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                ObservableCollection<SelectedProductInfoNew> m_productinfolist_last = new ObservableCollection<SelectedProductInfoNew>();

                int i = 0;
                foreach (var mp in this.m_productinfolist)
                {
                    i++;
                    if (!string.IsNullOrWhiteSpace(mp.Batcode) && !string.IsNullOrWhiteSpace(mp.Spec))
                    {
                        #region 输入内容检查

                        if (this.m_productinfolist.Count(c => c.Batcode.Trim() == mp.Batcode.Trim() && c.Spec.Trim() == mp.Spec.Trim()) > 1)
                        {
                            MessageBox.Show(mp.Batcode + " Φ" + mp.Spec + "有两个一样，请写在同一行", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(mp.Spec))
                        {
                            MessageBox.Show("第" + i + "行，请输入正确的规格", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        else
                        {
                            var specCheck = specList.FirstOrDefault(c => c.Materialid == (int)this.cdMaterial.SelectedValue && (c.Specname == "Φ" + mp.Spec || c.Specname == "Ф" + mp.Spec));
                            if (specCheck == null)
                            {
                                MessageBox.Show("第" + i + "行，该牌号下找不到该规格，请联系管理员设置", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }

                        if (!mp.Printnumber.HasValue || mp.Printnumber.Value <= 0)
                        {
                            MessageBox.Show("第" + i + "行，请输入件数", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (meterMode != EnumList.MeteringMode.磅计)
                        {
                            if (!mp.Length.HasValue || mp.Length.Value < 0)
                            {
                                MessageBox.Show("第" + i + "行，请输入长度", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            if (!mp.SingleWeight.HasValue || mp.SingleWeight.Value < 0)
                            {
                                MessageBox.Show("第" + i + "行，请输入单重", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            var SinWeigth = specList.Count(c => c.Materialid == (int)this.cdMaterial.SelectedValue 
                                && (c.Specname == "Φ" + mp.Spec || c.Specname == "Ф" + mp.Spec)
                                && c.Referlength.Value == mp.Length);

                            if (SinWeigth <= 0)
                            {
                                MessageBox.Show("第" + i + "行，单重不正确，请检查", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                        #endregion

                        m_productinfolist_last.Add(mp);
                    }
                }

                if (this.SelectedProductInfoConfirmedHandler != null)
                {
                    this.SelectedProductInfoConfirmedHandler.Invoke(m_productinfolist_last, lpn, consignor, sellerid, m_Printlogid, OutDate.SelectedDate.Value);
                }

                this.DialogResult = true;
            }
        }

        private void ReSelect_Click(object sender, RoutedEventArgs e)
        {
            //先退出编辑模式
            dgSelectProducts.CommitEdit(DataGridEditingUnit.Row, true);

            //如果是从历史数据中选择的，不允许进行修改
            if (this.m_Printlogid > 0)
            {
                MessageBox.Show("历史生成记录不可更改，如果需要请先辙销历史记录！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var item = ((Button)sender).DataContext;

            if (item is SelectedProductInfo)
            {
                var info = (SelectedProductInfo)item;

                ProductListWindow plw = new ProductListWindow(info.Batcode);
                plw.Title = "重新选择随车产品";
                plw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                plw.Owner = this;

                plw.ProductSelectedHandler += OnProductSelected;

                plw.ShowDialog();
            }

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            //如果是从历史数据中选择的，不允许进行修改
            if (this.m_Printlogid > 0)
            {
                MessageBox.Show("历史生成记录不可删除，如果需要请先辙销历史记录！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var item = ((Button)sender).DataContext;

            if (item is SelectedProductInfoNew)
            {
                var info = (SelectedProductInfoNew)item;
                m_productinfolist.Remove(info);
            }

            BindView();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public void OnProductSelected(List<SelectedProductInfo> list)
        {
            BindView();
        }

        public void OnPrintlogSelected(SalePrintlog info, List<SalePrintLogDetail> list)
        {
            if (info != null && list != null)
            {
                BindView();
            }
        }

        private void cdMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int materId = (int)this.cdMaterial.SelectedValue;

            this.m_productinfolist.Clear();

            for (int i = 0; i < 5; i++)
            {
                this.m_productinfolist.Add(new SelectedProductInfoNew()
                {
                    Batcode = string.Empty,
                    Materialid = materId,
                    Spec = null,
                    Length = null,
                    SingleWeight = null,
                    Printnumber = null,
                });
            }

            BindView();
        }
    }
}
