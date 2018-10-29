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
using WpfQualityCertPrinter.ModelAccess;

namespace WpfQualityCertPrinter
{
    /// <summary>
    /// SelCondition.xaml 的交互逻辑
    /// </summary>
    public partial class SelCondition : Window
    {
        public SelCondition()
        {
            InitializeComponent();
            InitData();
        }
        private void InitData()
        {
            using (SaleSellerAuthAccess access = new SaleSellerAuthAccess())
            {
                //获取最近使用过的车牌号
                var list = access.GetRecentLpns();
                if (list != null)
                {
                    cbLpn.ItemsSource = list;
                }
            }
        }

        private void Seach(object sender, RoutedEventArgs e)
        {
            var cblpn = this.cbLpn.Text;
            Dictionary<string, object> parms;
            if (string.IsNullOrWhiteSpace(cblpn)) 
            {
                MessageBox.Show("请选择车牌号", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!this.startDate.SelectedDate.HasValue) 
            {
                MessageBox.Show("请选择时间", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string conditionStr = condition(cblpn, Convert.ToDateTime(this.startDate.SelectedDate).ToString("yyyy-MM-dd") +" "+ "00:00:00", Convert.ToDateTime(this.startDate.SelectedDate).ToString("yyyy-MM-dd") +" "+"23:59:59", out parms);
            using (GJSelectModelAccess gma = new GJSelectModelAccess())
            {
                var list = gma.GetList(conditionStr, parms);
                SelectGJ sj = new SelectGJ();              
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        sj.m_productinfolist.Add(item);
                    }
                }
                sj.BindView();
                sj.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                sj.Owner = this;
                sj.ShowDialog();
                this.Close();
            }
        }

        private string condition(string cbLpn, string startDate, string endDate, out Dictionary<string, object> parms)
        {
            parms = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(cbLpn))
            {
                parms.Add("@cbLpn", string.Format("%{0}%", cbLpn));
            }

            if (!string.IsNullOrWhiteSpace(startDate))
            {
                parms.Add("@startDate", XYNetCloud.Utils.TimeUtils.GetUnixTimeFromDateTime(Convert.ToDateTime(startDate)));
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                parms.Add("@endDate", XYNetCloud.Utils.TimeUtils.GetUnixTimeFromDateTime(Convert.ToDateTime(endDate)));
            }
            StringBuilder condition = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(cbLpn))
            {
                condition.Append(" and Lpn like @cbLpn ");
            }

            if (!string.IsNullOrWhiteSpace(startDate))
            {
                condition.Append(" and createtime>=@startDate ");
            }
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                condition.Append(" and createtime<=@endDate ");
            }
            return condition.ToString();
        }
    }
}
