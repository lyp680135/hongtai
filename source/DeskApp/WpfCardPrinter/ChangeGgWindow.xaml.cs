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
    /// ChangeGgWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeGgWindow : Window
    {
        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="id"></param>
        public delegate void UpdataSelect(int id);
        public event UpdataSelect updataSelectHandler;
        public MainWindow main { get; set; }
        public ChangeGgWindow()
        {
            InitializeComponent();
            //初始化数据
            InitData();
        }

        public void InitData()
        {
            main = (MainWindow)Application.Current.MainWindow;
            using (BaseSpecificationsAccess bsa = new BaseSpecificationsAccess())
            {
                //数据库取数据
                var basList = bsa.GetListByClassAndMaterial(main.pubclassId, main.pubmaterialId);
                ObservableCollection<BaseSpecifications> mSpecList = new ObservableCollection<BaseSpecifications>();
                //先清空
                mSpecList.Clear();
                //如果有数据就填充
                if (basList != null && basList.Count > 0)
                    basList.ForEach(o =>
                    {
                        var specname = o.Callname + " x " + o.Referlength;
                        if (main.mMaterial != null)
                        {
                            //盘卷只显示规格直径
                            if (main.mMaterial.Deliverytype == (int)EnumList.DeliveryType.盘卷)
                            {
                                specname = o.Callname;
                            }
                        }

                        mSpecList.Add(new BaseSpecifications
                        {
                            Id = o.Id,
                            Specname =  o.Specname,
                            Callname = o.Callname,
                            FullSpecname = specname
                        });
                    });

                //绑定
                cbSpec.ItemsSource = mSpecList;
                if (main.mSelectedProduct != null && main.mSelectedProduct.Specid != null)
                {
                    var mSpecInfo=mSpecList.FirstOrDefault(w => w.Id == main.mSelectedProduct.Specid);
                    if ( mSpecInfo!= null)
                    {
                        cbSpec.SelectedValue = main.mSelectedProduct.Specid;
                        
                        lbCurrGg.Content = mSpecInfo.FullSpecname;
                        if (main.mMaterial != null)
                        {
                            //盘卷只显示规格直径
                            if (main.mMaterial.Deliverytype == (int)EnumList.DeliveryType.盘卷)
                            {
                                lbCurrGg.Content = mSpecInfo.Callname;
                            }
                        }
                    }                        
                    else
                        cbSpec.SelectedIndex = 0;
                }
                else
                {
                    cbSpec.SelectedIndex = 0;
                }
            }
        }
		
        public void Submit_Click(object sender, RoutedEventArgs e)
        {
            int Specid = 0;
            int.TryParse(cbSpec.SelectedValue.ToString(), out Specid);
            if (Specid == 0)
                MessageBox.Show("请选择规格", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
            //委托更新UI
            if (updataSelectHandler != null)
                updataSelectHandler.Invoke(Convert.ToInt32(cbSpec.SelectedValue));
            //同步更新
            //main.updateChangeSelect(Convert.ToInt32(cbSpec.SelectedValue));
            this.DialogResult = false;
        }
		
        private void SpecChanged(object sender, EventArgs e) 
        {
            this.lbCurrGg.Content = (this.cbSpec.SelectedItem as BaseSpecifications).FullSpecname;
        }
    }
}
