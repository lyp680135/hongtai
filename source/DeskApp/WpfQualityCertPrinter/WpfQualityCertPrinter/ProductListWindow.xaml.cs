using DataLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfQualityCertPrinter.ModelAccess;
using WpfQualityCertPrinter.ViewModel;

namespace WpfQualityCertPrinter
{
    public partial class QualityLevelDataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }
            return (DataLibrary.EnumList.ProductQualityLevel)value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// ProductListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProductListWindow : Window
    {
        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="list"></param>
        public delegate void ProductSelected(List<SelectedProductInfo> list);
        public event ProductSelected ProductSelectedHandler;

        private string m_batcode = null;
        private ObservableCollection<ProductInfo> m_list = new ObservableCollection<ProductInfo>();

        public ProductListWindow(string batcode)
        {
            InitializeComponent();

            this.m_batcode = batcode;

            ShowAndLoad();
        }

        public void ShowAndLoad()
        {
            InitData();

            BindView();
        }

        private void InitData()
        {
            using (PdProductAccess access = new PdProductAccess())
            {
                var list = access.GetListByBatcode(this.m_batcode);
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        var pdinfo = new ProductInfo();
                        
                        pdinfo.Id = item.Id;
                        pdinfo.Batcode = item.Batcode;
                        pdinfo.Classid = item.Classid.Value;
                        pdinfo.Classname = item.Classname;
                        pdinfo.Materialid = item.Materialid.Value;
                        pdinfo.Materialname = item.Materialname;
                        pdinfo.Specid = item.Specid.Value;
                        pdinfo.Piececount = item.Piececount.Value;
                        pdinfo.Bundlecode = item.Bundlecode;
                        pdinfo.Workshiftid = item.WorkShift.Value;
                        pdinfo.Workshiftname = item.Shiftname;
                        pdinfo.Length = string.Format("{0}",item.Length);
                        pdinfo.Lengthtype = item.Lengthtype.Value;
                        pdinfo.Specfullname = string.Format("{0}x{1}",item.Callspecname,item.Length);

                        if (pdinfo.Lengthtype != (int)EnumList.ProductQualityLevel.定尺)
                        {
                            if (pdinfo.Lengthtype == (int)EnumList.ProductQualityLevel.短尺)
                            {
                                pdinfo.Length = "S";
                                pdinfo.Lengthnote = "非尺";
                            }
                            else if (pdinfo.Lengthtype == (int)EnumList.ProductQualityLevel.长尺)
                            {
                                pdinfo.Length = "L";
                                pdinfo.Lengthnote = "非尺";
                            }
                            else if (pdinfo.Lengthtype == (int)EnumList.ProductQualityLevel.标准)
                            {
                                pdinfo.Length = "-";
                                pdinfo.Lengthnote = "-";
                            }
                            else
                            {
                                pdinfo.Length = "非尺";
                                pdinfo.Lengthnote = "非尺";
                            }
                        }
                        else
                        {
                            pdinfo.Lengthnote = "定尺";
                        }

                        pdinfo.Checked = false;
                        
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

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            //开始组装数据,返回选中产品列表页
            List<SelectedProductInfo> selectedinfo = new List<SelectedProductInfo>();

            foreach (var item in m_list)
            {
                if (item.Checked)
                {
                    //判断是否按批号、材质、规格分组
                    bool cangroup = false;
                    for (var i = 0; i < selectedinfo.Count; i++ )
                    {
                        var sitem = selectedinfo[i];
                        if (item.Batcode == sitem.Batcode
                            && item.Classid == sitem.Classid
                            && item.Materialid == sitem.Materialid
                            && item.Specid == sitem.Specid
                            && item.Lengthnote == sitem.Lengthnote)
                        {
                            cangroup = true;
                            sitem.Number++;
                            sitem.Printnumber++;
                            break;
                        }
                    }

                    if (!cangroup)
                    {
                        selectedinfo.Add(new SelectedProductInfo()
                        {
                            Batcode = item.Batcode,
                            Classid = item.Classid,
                            Classname = item.Classname,
                            Materialid = item.Materialid,
                            Materialname = item.Materialname,
                            Specid = item.Specid,
                            Specfullname = item.Specfullname,
                            Length = item.Length,
                            Lengthtype = item.Lengthtype,
                            Lengthnote = item.Lengthnote,
                            Number = 1,
                            Printnumber = 1,
                        });
                    }
                }
            }

            //判断数据是否合法（不能选不同材质的数据）
            bool finderr = false;
            SelectedProductInfo previnfo = null;
            for (var i = 0; i < selectedinfo.Count; i++)
            {
                if (previnfo == null)
                {
                    previnfo = selectedinfo[i];
                    continue;
                }

                var item = selectedinfo[i];
                if( previnfo.Materialid != item.Materialid )
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
                if (ProductSelectedHandler != null)
                {
                    ProductSelectedHandler.Invoke(selectedinfo);
                }

                this.DialogResult = true;
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
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
    }
}
