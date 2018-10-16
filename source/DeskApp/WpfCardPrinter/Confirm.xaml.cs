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
using WpfCardPrinter.ModelAccess;
using WpfQualityCertPrinter.Utils;

namespace WpfCardPrinter
{
    /// <summary>
    /// Confirm.xaml 的交互逻辑
    /// </summary>
    public partial class Confirm : Window
    {
        public string mCurrentBatCode { get; set; }
        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="id"></param>
        public delegate void UpdataSelect(string batcode);
        public event UpdataSelect updataSelectHandler;
        public Confirm()
        {
            InitializeComponent();
        }
        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            var password = txtQueryBatcode.Password;
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("请先输口令！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (MngAdminAccess access = new MngAdminAccess())
            {
                var admin = access.Single("admin", MD5Util.GenerateMD5(password));
                if (admin != null)
                {
                    using (PdProductAccess pdAccess = new PdProductAccess())
                    {
                        var pdList = pdAccess.GetListByBatcode(mCurrentBatCode);
                        if (pdList != null && pdList.Count > 0)
                        {
                            using (PdStockOutAccess psAccess = new PdStockOutAccess())
                            {
                                var psOut = psAccess.SingleByProductid(string.Join(",", pdList.Select(s => s.Id).ToList()));
                                if (psOut != null && psOut.Id > 0)
                                {
                                    MessageBox.Show("该炉下已有产品出库，不能清空", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);                                  
                                }
                                else
                                {
                                    //删除
                                    pdAccess.DeleteProduct(mCurrentBatCode);
                                    if (updataSelectHandler != null)
                                        updataSelectHandler.Invoke(mCurrentBatCode);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("口令错误,重新输入!", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                this.DialogResult = false;
            }

        }
    }
}
