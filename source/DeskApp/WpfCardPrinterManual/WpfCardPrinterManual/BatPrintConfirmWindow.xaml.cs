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
using WpfCardPrinterManual.Model;

namespace WpfCardPrinterManual
{
    /// <summary>
    /// BatPrintConfirmWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BatPrintConfirmWindow : Window
    {
        private PdProduct pdProduct { get; set; }
        /// <summary>
        /// 批量打印委托
        /// </summary>
        /// <param name="productInfos"></param>
        public delegate void DoPrint(List<PdProduct> productInfos);
        public event DoPrint doPrintHandler;
        public BatPrintConfirmWindow(PdProduct pdProduct)
        {
            InitializeComponent();
            this.pdProduct = pdProduct;
            InitLable(pdProduct);
        }

        public void InitLable(PdProduct pdProduct)
        {
            txtBundle.Text = pdProduct.Bundlecode;
            txtPrintNum.Text = "1";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int printNum;
            if (!int.TryParse(txtPrintNum.Text, out printNum))
            {
                MessageBox.Show("打印件数不能小于1", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            List<PdProduct> pdList = new List<PdProduct>();
            for (int i = 0; i < printNum; i++)
            {
                PdProduct product = new PdProduct();
                int Bundlecode;
                int.TryParse(txtBundle.Text, out Bundlecode);
                Bundlecode += i;
                product.Batcode = pdProduct.Batcode;
                product.ClassName = pdProduct.ClassName;
                product.SpecName = pdProduct.SpecName;
                product.MaterialName = pdProduct.MaterialName;
                product.Piececount = pdProduct.Piececount;
                product.Randomcode = pdProduct.Randomcode;
                product.Length = pdProduct.Length;
                product.Meterweight = pdProduct.Meterweight;
                product.Weight = pdProduct.Weight;
                product.GBStandard = pdProduct.GBStandard;
                product.Createtime = pdProduct.Createtime;
                product.Bundlecode = Bundlecode <= 9 ? Bundlecode.ToString("D2") : Bundlecode.ToString();
                pdList.Add(product);
            }

            if (doPrintHandler != null)
            {
                if (pdList != null && pdList.Any())
                {
                    doPrintHandler.Invoke(pdList.OrderBy(o => o.Bundlecode).ToList());
                    this.DialogResult = false;
                }
            }
        }
    }
}
