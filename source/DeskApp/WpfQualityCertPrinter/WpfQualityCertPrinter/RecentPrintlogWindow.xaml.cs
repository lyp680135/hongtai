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
    }
}
