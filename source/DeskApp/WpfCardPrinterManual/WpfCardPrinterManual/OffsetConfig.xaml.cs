using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfCardPrinterManual
{
    /// <summary>
    /// OffsetConfig.xaml 的交互逻辑
    /// </summary>
    public partial class OffsetConfig : Window
    {
        public delegate void UpdataSelect();
        public event UpdataSelect updataSelectHandler;
        const string xname = "OffsetX";
        const string yname = "OffsetY";
        public OffsetConfig()
        {
            InitializeComponent();
            InitData();
        }
        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitData()
        {
            this.xpyl.Text = ConfigurationManager.AppSettings[xname].ToString();
            this.ypyl.Text = ConfigurationManager.AppSettings[yname].ToString();
        }
        
        public void Update_Config(object sender, RoutedEventArgs e)
        {
            string xpyl = this.xpyl.Text;
            string ypyl = this.ypyl.Text;
            if (string.IsNullOrEmpty(xpyl))
            {
                MessageBox.Show("请输入X轴偏移量!", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(ypyl))
            {
                MessageBox.Show("请输入X轴偏移量！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Regex.IsMatch(xpyl, @"[0-9]\d*$") || !Regex.IsMatch(ypyl, @"[0-9]\d*$"))
            {
                MessageBox.Show("请输入整型数字!", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            UpdateAppSetting(xname, xpyl);
            UpdateAppSetting(yname, ypyl);
            if (updataSelectHandler != null)
            {
                updataSelectHandler.Invoke();
            }
            this.DialogResult = false;
        }
        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        /// <summary>
        /// 更新appconfig里面的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void UpdateAppSetting(string key, string value)
        {
            bool isModified = false;
            foreach (string keys in ConfigurationManager.AppSettings)
            {
                if (keys == key)
                {
                    isModified = true;
                }
            }
            Configuration config =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (isModified)
            {
                config.AppSettings.Settings.Remove(key);
            }

            config.AppSettings.Settings.Add(key, value);

            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
