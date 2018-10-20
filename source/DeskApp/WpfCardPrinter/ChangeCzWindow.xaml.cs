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
        public delegate void UpdataSelect(int classid, int materialid, string batcode);
        public event UpdataSelect updataSelectHandler;
        public int ClassId { get; set; }

        public PdBatcode mCurrentBatCode { get; set; }
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
            
        }

        public void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (updataSelectHandler != null)
            {
                int mid = (cbMaterial.SelectedItem as BaseProductMaterial).Id;
                int cid = (cbMaterial.SelectedItem as BaseProductMaterial).Classid;
                using (PdProductAccess pdaccess = new PdProductAccess())
                {
                    var productList = pdaccess.GetListByBatcode(mCurrentBatCode.Batcode);
                    if (productList != null && productList.Count > 0)
                    {
                        MessageBoxResult result = MessageBox.Show("此操作将会清空该批号下的产品数据,确认操作？", "操作提醒", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                        if (result == MessageBoxResult.OK)
                        {
                            //不需要判断同名规格是否存在，直接清空
                            productList.ForEach(o =>
                            {
                                 //将数据备份到删除记录表
                                if (pdaccess.InsertDeleted(o) > 0)
                                {
                                    pdaccess.DeleteProductById(o.Id);
                                }
                            });
                        }
                    }
                }
                updataSelectHandler.Invoke(cid, mid, mCurrentBatCode.Batcode);
                this.DialogResult = false;
            }
        }
    }
}
