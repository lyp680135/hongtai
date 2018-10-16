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
using XYNetCloud.Utils;
using System.Globalization;

namespace WpfQualityCertPrinter
{
    public partial class DateTimeDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }

            int timestamp = 0;
            DateTime date = TimeUtils.GetUnixStartTime();
            int.TryParse(value.ToString(), out timestamp);

            if (timestamp > 0)
            {
                date = TimeUtils.GetDateTimeFromUnixTime(timestamp);
            }

            if (date == TimeUtils.GetUnixStartTime())
            {
                return "";
            }

            return date.ToString("yyyy-MM-dd");
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// RecentPrintlogWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RecentPrintlogWindow : Window
    {
        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="info"></param>
        public delegate void PrintlogSelected(SalePrintlog info, List<SalePrintLogDetail> list);
        public event PrintlogSelected PrintlogSelectedHandler;


        public RecentPrintlogWindow()
        {
            InitializeComponent();

            InitData();

            BindView();
        }

        private void InitData()
        {
            using (SalePrintlogAccess access = new SalePrintlogAccess())
            {
                var list = access.GetListRecent();
                if (list != null)
                {
                    dgPrintlog.ItemsSource = list;
                }
            }
        }

        public void BindView()
        {
            
        }


        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            var selecteditem = dgPrintlog.SelectedItem;

            if (selecteditem != null)
            {
                if (selecteditem is SalePrintlog)
                {
                    SalePrintlog log = (SalePrintlog)selecteditem;
                    
                    using(SalePrintLogDetaillAccess access = new SalePrintLogDetaillAccess())
                    {
                        var details = access.GetListByPrintid(log.Id);
                        if (details != null)
                        {
                            if (this.PrintlogSelectedHandler != null)
                            {
                                this.PrintlogSelectedHandler.Invoke(log, details);

                                this.DialogResult = true;
                            }
                            else
                            {
                                throw new Exception("没有设置回调方法");
                            }
                        }
                        else
                        {
                            MessageBox.Show("数据异常，查询不到质量证明书明细！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                            this.DialogResult = false;
                        }
                    }
                }
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var selecteditem = dgPrintlog.SelectedItem;

            if (selecteditem != null)
            {
                if (selecteditem is SalePrintlog)
                {
                    SalePrintlog log = (SalePrintlog)selecteditem;

                    using(SalePrintlogAccess logaccess = new SalePrintlogAccess())
                    {
                        var reallog = logaccess.SingleAllById(log.Id);
                        if (reallog != null)
                        {
                            var conn=logaccess.GetConnection();
                            using (var trans = conn.BeginTransaction())
                            {
                                try
                                {
                                    SalePrintLogDetaillAccess access = new SalePrintLogDetaillAccess(conn);
                                    SaleSellerAuthAccess authaccess = new SaleSellerAuthAccess(conn);
                                    SaleSellerAuthDetailAccess authdetailaccess = new SaleSellerAuthDetailAccess(conn);
                                    PdStockoutAccess pdaccess = new PdStockoutAccess(conn);

                                    var list = access.GetListByPrintid(reallog.Id);
                                    foreach (var item in list)
                                    {
                                        var auth = authaccess.SingleById(item.Authid);

                                        // 查找授权详情纪录 并纪录 产品ID列表
                                        var authdetails = authdetailaccess.GetListByAuthid(auth.Id, auth.Number.Value);
                                        List<int> authdetailsIds = authdetails.Select(c => c.Id).ToList();
                                        List<int> productIds = authdetails.Select(c => c.Productid).ToList();

                                        // 删除授权详情纪录
                                        if (authdetailaccess.DeleteByIds(authdetailsIds) <= 0)
                                        {
                                            trans.Rollback();
                                            MessageBox.Show("授权详情操作异常，撤销失败！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                                            return;
                                        }
                                        else
                                        {
                                            // 更新授权记录
                                            if (auth != null)
                                            {
                                                int authNum = auth.Number.Value;
                                                if (authNum > auth.Number)
                                                {
                                                    auth.Number = auth.Number - auth.Number;
                                                    if (authaccess.UpdateNums(auth) <= 0)
                                                    {
                                                        trans.Rollback();
                                                        MessageBox.Show("授权信息操作异常，撤销失败！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    if (authaccess.Delete(auth.Id) <= 0)
                                                    {
                                                        trans.Rollback();
                                                        MessageBox.Show("授权信息操作异常，撤销失败！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                                                        return;
                                                    }
                                                }
                                            }

                                            //根据产品ID删除出库记录
                                            if (pdaccess.DeleteByProductIds(productIds, auth.Sellerid) <= 0)
                                            {
                                                trans.Rollback();
                                                MessageBox.Show("出库记录操作异常，撤销失败！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                                                return;
                                            }
                                        }
                                    }

                                    // 删除打印详情记录
                                    List<int> printdetailids = list.Select(c => c.Id).ToList();
                                    if (access.DeleteByIds(printdetailids) <= 0)
                                    {
                                        trans.Rollback();
                                        MessageBox.Show("打印详情记录操作异常，撤销失败！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }

                                    if (logaccess.Delete(reallog.Id) <= 0)
                                    {
                                        trans.Rollback();
                                        MessageBox.Show("打印记录操作异常，撤销失败！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }

                                    trans.Commit();
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.WriteLog("撤销最近生成质保书失败！"+ex.ToString());

                                    trans.Rollback();
                                    MessageBox.Show("操作异常，撤销失败！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                        }


                        MessageBox.Show("撤销成功！", "操作提醒", MessageBoxButton.OK,MessageBoxImage.Information);

                        //重新加载数据
                        InitData();

                        BindView();
                    }
                }
            }
        }
    }
}
