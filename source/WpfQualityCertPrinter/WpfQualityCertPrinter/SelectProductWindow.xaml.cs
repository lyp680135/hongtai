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
        public delegate void SelectProductInfoConfirmed(ObservableCollection<SelectedProductInfo> list,
            string lpn, string consignor, int sellerid, int printid);
        public event SelectProductInfoConfirmed SelectedProductInfoConfirmedHandler;

        private ObservableCollection<SelectedProductInfo> m_productinfolist = new ObservableCollection<SelectedProductInfo>();

        private int m_Printlogid = 0;

        public SelectProductWindow()
        {
            InitializeComponent();

            InitData();

            BindView();
        }

        private void InitData()
        {
            //获取最近10条轧制批号列表
            using (PdBatcodeAccess access = new PdBatcodeAccess())
            {
                var list = access.GetList();
                if (list != null)
                {
                    cbBatcode.ItemsSource = list;
                }
            }

            using (SaleSellerAccess access = new SaleSellerAccess())
            {
                var list = access.GetTopSellerList();
                if (list != null)
                {
                    cbSeller.ItemsSource = list;
                }
            }

            using (SaleSellerAuthAccess access = new SaleSellerAuthAccess())
            {
                //获取最近使用过的车牌号
                var list = access.GetRecentLpns();
                if (list != null)
                {
                    cbLpn.ItemsSource = list;
                }
            }

            //获取最近使用过的收货单位
            using (SalePrintlogAccess access = new SalePrintlogAccess())
            {
                //获取最近使用过的车牌号
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
                MessageBox.Show("必须选择好随车产品才能出库！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show( "无效的售达方数据！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //判断所选的货是否能打在一张质保书
            //判断数据是否合法（不能选不同材质的数据）
            bool finderr = false;
            SelectedProductInfo previnfo = null;
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
                if (this.SelectedProductInfoConfirmedHandler != null)
                {
                    this.SelectedProductInfoConfirmedHandler.Invoke(m_productinfolist, lpn, consignor, sellerid, m_Printlogid);
                }

                this.DialogResult = true;
            }
        }

        private void ReSelect_Click(object sender, RoutedEventArgs e)
        {
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

            if (item is SelectedProductInfo)
            {
                var info = (SelectedProductInfo)item;
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

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            string batcode = cbBatcode.Text;
            if (!string.IsNullOrEmpty(batcode))
            {
                ProductListWindow plw = new ProductListWindow(batcode);
                plw.Title = "选择随车产品";
                plw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                plw.Owner = this;

                plw.ProductSelectedHandler += OnProductSelected;

                plw.ShowDialog();
            }
        }

        public void OnProductSelected(List<SelectedProductInfo> list)
        {
            foreach (var item in list)
            {
                bool find = false;
                //判断是否有重复的，如果有重复的则更新
                for (var i = 0; i < this.m_productinfolist.Count; i++)
                {
                    var sitem = this.m_productinfolist[i];
                    if (item.Batcode == sitem.Batcode
                            && item.Classid == sitem.Classid
                            && item.Materialid == sitem.Materialid
                            && item.Specid == sitem.Specid
                            && item.Lengthnote == sitem.Lengthnote)
                    {
                        this.m_productinfolist[i].Number = item.Number;
                        find = true;
                        break;
                    }
                }

                if (!find)
                {
                    this.m_productinfolist.Add(item);
                }
            }

            BindView();
        }

        public void OnPrintlogSelected(SalePrintlog info, List<SalePrintLogDetail> list)
        {
            if (info != null && list != null)
            {
                this.m_Printlogid = info.Id;

                //禁用查询功能
                this.btnSearch.IsEnabled = false;

                cbLpn.Text = info.Lpn;
                cbSeller.SelectedValue = info.Sellerid;
                cbSeller.IsEnabled = false;

                cbConsignee.Text = info.Consignor;

                this.m_productinfolist.Clear();
                foreach (var item in list)
                {
                    using(SaleSellerAuthAccess access = new SaleSellerAuthAccess())
                    {
                        var selecteditem = access.GetProductInfoById(item.Authid);
                        if (selecteditem != null)
                        {
                            this.m_productinfolist.Add(selecteditem);
                        }
                    }
                }

                BindView();
            }
        }
    }
}
