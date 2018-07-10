using DataLibrary;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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

namespace WpfCardPrinter
{
    /// <summary>
    /// ChangeShiftWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeShiftWindow : Window
    {
        public ChangeShiftWindow()
        {
            InitializeComponent();

            InitData();
        }

        public void InitData()
        {
            MainWindow main = (MainWindow) Application.Current.MainWindow;

            PdWorkshopTeam currteam=main.GetCurrentWorkShift();

            lbCurrShift.Content = currteam.TeamName;
            cbShift.ItemsSource = main.mTeamList;

            if (main.mTeamList != null)
            {
                int index = 0;
                foreach (var item in main.mTeamList)
                {
                    if (item.Id == currteam.Id)
                    {
                        if (index+1 < main.mTeamList.Count)
                        {
                            cbShift.SelectedValue = main.mTeamList[index+1].Id; 
                        }
                        else
                        {
                            cbShift.SelectedValue = main.mTeamList[0].Id; 
                        }
                        break;
                    }
                    index++;
                }
            }
            
        }

        public void Submit_Click(object sender, RoutedEventArgs e)
        {
            //判断是否选择了一样的排班
            int teamid = 0;

            if (cbShift.SelectedValue != null)
            {
                int.TryParse(cbShift.SelectedValue.ToString(), out teamid);
            }

            MainWindow main = (MainWindow)Application.Current.MainWindow;
            if (teamid == main.GetCurrentWorkShift().Id)
            {
                MessageBox.Show("不能换到同一排班", "操作提醒", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                this.DialogResult = main.SetCurrentWorkShift(teamid);
            }
        }

        private void WorkShiftChanged(object sender, EventArgs e)
        {

        }

        public void Relogin_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogresult = MessageBox.Show("请确认是否要退出该账号？", "操作提醒", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (dialogresult == MessageBoxResult.OK)
            {
                LoginWindow lwin = new LoginWindow();
                lwin.Show();

                this.DialogResult = false;

                this.Close();
            }
        }
    }
}
