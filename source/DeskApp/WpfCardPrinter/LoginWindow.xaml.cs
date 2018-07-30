using DataLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfCardPrinter.Model;
using WpfCardPrinter.ModelAccess;
using WpfCardPrinter.ModelAccess.SqliteAccess;
using WpfQualityCertPrinter.Utils;

namespace WpfCardPrinter
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            InitData();
        }
        private void InitData()
        {
            ObservableCollection<LoginLog> userList = new ObservableCollection<LoginLog>();
            using (LoginLogSqliteAccess login = new LoginLogSqliteAccess())
            {
                var loginlist = login.LoginLogList();
                if (loginlist != null && loginlist.Count > 0)
                {
                    loginlist.ForEach(o =>
                    {
                        userList.Add(new LoginLog
                        {
                            Id = o.Id,
                            UserName = o.UserName
                        });
                    });
                    this.cbAccount.ItemsSource = userList;
                    this.cbAccount.SelectedIndex = 0;
                }
            }
        }
        public static string GetLocalIp()
        {
            IPAddress localIp = null;

            try
            {
                IPAddress[] ipArray;
                ipArray = Dns.GetHostAddresses(Dns.GetHostName());
                localIp = ipArray.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            if (localIp == null)
            {
                localIp = IPAddress.Parse("127.0.0.1");
            }
            return localIp.ToString();
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string account = cbAccount.Text;
            string password = pbPass.Password;

            if (string.IsNullOrEmpty(account))
            {
                MessageBox.Show("请先输入账号！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("请先输入密码", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //将密码加密
            password = MD5Util.GenerateMD5(password);
#if DEBUG
                    MainWindow mainwindow = new MainWindow(null, null);
                    mainwindow.Show();

                    this.Close();
#else
            try
            {
                using (MngAdminAccess access = new MngAdminAccess())
                {
                    var admin = access.Single(account, password);
                    if (admin != null)
                    {
                        string configshopcode = System.Configuration.ConfigurationManager.AppSettings["WorkshopProductLine"];

                        PdWorkshop workshop = null;
                        //判断是否有权限
                        using (PdWorkshopAccess paccess = new PdWorkshopAccess())
                        {
                            var shop = paccess.Single(configshopcode);
                            if (shop != null)
                            {
                                if (!string.IsNullOrEmpty(shop.Inputer))
                                {
                                    var inputers = shop.Inputer.Split(',');
                                    if (inputers.Contains(admin.Id.ToString()))
                                    {
                                        using (LoginLogSqliteAccess login = new LoginLogSqliteAccess())
                                        {
                                            //如果没有就添加
                                            if (login.LoginLogInfo(account).Id <= 0)
                                            {
                                                login.Insert(new LoginLog
                                                {
                                                    UserName = account,
                                                    RealName = admin.RealName,
                                                    Address = GetLocalIp(),
                                                    LoginTime = Utils.TimeUtils.GetUnixTimeFromDateTime(DateTime.Now)
                                                });
                                            }
                                            //存在就修改时间
                                            else
                                            {
                                                login.Update(new LoginLog
                                                {
                                                    UserName = account,
                                                    LoginTime = Utils.TimeUtils.GetUnixTimeFromDateTime(DateTime.Now)
                                                });
                                            }
                                        }
                                        MainWindow mainwindow = new MainWindow(admin, workshop);
                                        mainwindow.Show();

                                        this.Close();

                                        return;
                                    }
                                }
                            }

                            MessageBox.Show("抱歉，该账号没有车间操作权限！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("账号或密码不正确，请重新登录！", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("登录失败，请检查网络或配置是否正常！\n\n" + ex.Message, "操作提醒", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
#endif
        }


        private void FirstLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MngSettingAccess access = new MngSettingAccess())
                {
                    var setting = access.Single();
                    if (setting != null)
                    {
                        string registerurl = setting.Domain + "Login";
                        System.Diagnostics.Process.Start(registerurl);
                    }
                }
            }
            catch
            {

            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
