using System;
using System.Collections.Generic;
using System.IO.Ports;
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

namespace WpfCardPrinter
{
    /// <summary>
    /// ComDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ComDialog : Window
    {
        public ComDialog()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            //串口
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                cbPort.Items.Add(port);
            }
            cbPort.SelectedIndex = 0;

            //波特率
            cbBaudRate.Items.Add("110");
            cbBaudRate.Items.Add("300");
            cbBaudRate.Items.Add("600");
            cbBaudRate.Items.Add("1200");
            cbBaudRate.Items.Add("2400");
            cbBaudRate.Items.Add("4800");
            cbBaudRate.Items.Add("9600");
            cbBaudRate.Items.Add("19200");
            cbBaudRate.Items.Add("38400");
            cbBaudRate.Items.Add("57600");
            cbBaudRate.Items.Add("115200");
            cbBaudRate.Items.Add("230400");
            cbBaudRate.Items.Add("460800");
            cbBaudRate.Items.Add("921600");
            cbBaudRate.SelectedIndex = 5;

            //数据位
            cbDataBits.Items.Add("5");
            cbDataBits.Items.Add("6");
            cbDataBits.Items.Add("7");
            cbDataBits.Items.Add("8");
            cbDataBits.SelectedIndex = 3;

            //停止位
            cbStopBit.Items.Add("1");
            cbStopBit.SelectedIndex = 0;

            //校验位
            cbParity.Items.Add("无");
            cbParity.SelectedIndex = 0;
        }

        public void OpenPort_Click(object sender,RoutedEventArgs e)
        {
            //以下4个参数都是从窗体MainForm传入的
            MainWindow main = (MainWindow)Application.Current.MainWindow;

            main.mComPortConfig.ComName = cbPort.Text;
            main.mComPortConfig.BaudRate = int.Parse(cbBaudRate.Text);
            main.mComPortConfig.DataBits = int.Parse(cbDataBits.Text);
            main.mComPortConfig.StopBits = int.Parse(cbStopBit.Text);
            main.mComPortConfig.TimeOut = 500;

            DialogResult = true;
        }

        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }

    public class ComPortConfig
    {
        public string ComName;
        public int BaudRate;
        public int DataBits;
        public int StopBits;
        public int TimeOut;
    }
}
