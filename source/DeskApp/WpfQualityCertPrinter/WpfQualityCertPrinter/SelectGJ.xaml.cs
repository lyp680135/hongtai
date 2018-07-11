using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using System.Windows.Shapes;
using WpfQualityCertPrinter.Model;
using XYNetCloud.Utils;

namespace WpfQualityCertPrinter
{
    /// <summary>
    /// SelectGJ.xaml 的交互逻辑
    /// </summary>
    public partial class SelectGJ : Window
    {
        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="id"></param>
        public delegate void UpdataSelect(string printno);
        public static event UpdataSelect updataSelectHandler;
        public ObservableCollection<GJSelectModel> m_productinfolist = new ObservableCollection<GJSelectModel>();
        public SelectGJ()
        {
            InitializeComponent();
            //BindView();
        }

        public void BindView()
        {
            if (m_productinfolist.Count > 0)
            {
                if (dgPrintlog.ItemsSource == null)
                {
                    dgPrintlog.ItemsSource = m_productinfolist;
                }
            }

            dgPrintlog.Items.Refresh();
            dgPrintlog.Focus();
        }

        private void dgProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgPrintlog.SelectedIndex < 0)
            {
                return;
            }
            var selectitem = dgPrintlog.SelectedItem;
            if (selectitem is GJSelectModel)
            {
                var gjsm = (GJSelectModel)selectitem;
                if (updataSelectHandler != null)
                {
                    Task.Factory.StartNew(() =>
                    {
                        updataSelectHandler.Invoke(gjsm.printno);
                    });
                    this.Close();
                    // 静态变量会缓存,用完事件一定要清空否则会多次查询
                    updataSelectHandler = null;
                }
            }
        }
    }
}
