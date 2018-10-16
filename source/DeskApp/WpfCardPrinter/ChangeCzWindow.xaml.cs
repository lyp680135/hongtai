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
using System.Windows.Shapes;
using WpfCardPrinter.ModelAccess;

namespace WpfCardPrinter
{
    /// <summary>
    /// ChangeCzWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeCzWindow : Window
    {
        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="id"></param>
        public delegate void UpdataSelect(int classid, int materialid, string mcurrbatcode);
        public event UpdataSelect updataSelectHandler;
        public int ClassId { get; set; }

        public string mCurrentBatCode { get; set; }
        public int MaterialId { get; set; }
        public ObservableCollection<BaseProductMaterial> mMaterialList = new ObservableCollection<BaseProductMaterial>();
        public ListCollectionView mGroupedMaterialList = null;
        public ChangeCzWindow()
        {
            InitializeComponent();
            //初始化数据
            InitData();
        }

        private void InitData()
        {
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
                cbMaterial.ItemsSource = mGroupedMaterialList;
            }
        }

        private void MaterialChanged(object sender, EventArgs e)
        {
            var currMaterial = (this.cbMaterial.SelectedItem as BaseProductMaterial);
            this.lbCurrCz.Content = string.Format("{0}->{1}", currMaterial.Classname, currMaterial.Name);
        }

        public void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (updataSelectHandler != null)
            {
                int mid = (cbMaterial.SelectedItem as BaseProductMaterial).Id;
                int cid = (cbMaterial.SelectedItem as BaseProductMaterial).Classid;
                using (PdProductAccess pdaccess = new PdProductAccess())
                {
                    var productList = pdaccess.GetListByBatcode(mCurrentBatCode);
                    if (productList != null && productList.Count > 0)
                    {
                        MessageBoxResult result = MessageBox.Show("此操作将会重置数据,确认操作？", "操作提醒", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        if (result == MessageBoxResult.OK)
                        {
                            using (BaseSpecificationsAccess specaccess = new BaseSpecificationsAccess())
                            {
                                var objList = specaccess.GetListByClassAndMaterial(cid, mid);
                                productList.ForEach(o =>
                                {
                                    var obj = specaccess.GetListById(Convert.ToInt16(o.Specid)).FirstOrDefault();
                                    if (cid != ClassId || !objList.Any(x => x.Callname == obj.Callname && x.Referlength == obj.Referlength))
                                    {
                                        pdaccess.DeleteProductById(o.Id);
                                    }
                                    else
                                    {
                                        o.Materialid = mid;
                                        pdaccess.Update(o);
                                    }
                                });
                            }
                        }
                    }
                }
                updataSelectHandler.Invoke(cid, mid, mCurrentBatCode);
                this.DialogResult = false;
            }
        }
    }
}
