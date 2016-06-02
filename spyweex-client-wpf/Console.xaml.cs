using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Mm.Wpf.Controls;
using spyweex_client_wpf.StaticStrings;
using spyweex_client_wpf.Subscribers;

namespace spyweex_client_wpf
{


    /// <summary>
    /// Interaction logic for Console.xaml
    /// </summary>
    public partial class Console : CustomWindow
    {
        WxhtpClient _wxhtpClient;
        WxhtpServiceServer _wxhtpServiceServer;
        CmdExecListener cmdexecListener;
        ConsoleContent dc = new ConsoleContent();
        CancellationToken _ct;

        public Console(WxhtpClient client, WxhtpServiceServer wxhtpServiceServer, CancellationToken ct)
        {
            InitializeComponent();
            
            _wxhtpClient = client;
            _wxhtpServiceServer = wxhtpServiceServer;
            _ct = ct;
            DataContext = dc;
            Loaded += MainWindow_Loaded;
            Title = Title + " " + _wxhtpClient.getTcpClient().Client.RemoteEndPoint.ToString();
            cmdexecListener = new CmdExecListener();
            Closing += new CancelEventHandler(MainWindow_Closing);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InputBlock.KeyDown += InputBlock_KeyDown;
            InputBlock.Focus();
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            _wxhtpClient.isConsoleAttached = false;
            cmdexecListener.UnsubscribeAsync();
        }

        async void InputBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (cmdexecListener.isSubscribed) return;
                    
                string cmd = InputBlock.Text;
                await _wxhtpClient.ExecuteTask(
                    _ct, _wxhtpClient.getTcpClient().Client.RemoteEndPoint.ToString(), METHOD_TYPE.GET, ACTION_TYPE.COMMAND_PROMPT, cmd);
                cmdexecListener.Subscribe(_wxhtpClient, dc);

                dc.ConsoleInput = InputBlock.Text;
                dc.RunCommand();
                InputBlock.IsReadOnly = true;
                InputBlock.Focus();
                Task r = Task.Delay(TimeSpan.FromSeconds(1.5));
                r.Wait();
                InputBlock.IsReadOnly = false;
                Scroller.ScrollToBottom();
                
            }
        }
    }

    public class ConsoleContent : INotifyPropertyChanged
    {
        string consoleInput = string.Empty;
        ObservableCollection<string> consoleOutput = new ObservableCollection<string>(){
        #region ASCII ART
        "Spy-weex console session started...\n" +
        "************************************************************\n"+
        "   _____ _______     __  __          ________ ________   __"+
        "\n  / ____|  __ \\ \\   / /  \\ \\        / /  ____|  ____\\ \\ / /"+
        "\n | (___ | |__) \\ \\_/ /____\\ \\  /\\  / /| |__  | |__   \\ V /"+
        "\n  \\___ \\|  ___/ \\   /______\\ \\/  \\/ / |  __| |  __|   > <"+
        "\n  ____) | |      | |        \\  /\\  /  | |____| |____ / . \\ "+
        "\n |_____/|_|      |_|         \\/  \\/   |______|______/_/ \\_\\\n\n"+
        "************************************************************" 
        #endregion ASCII ART
        };

        public string ConsoleInput
        {
            get
            {
                return consoleInput;
            }
            set
            {
                consoleInput = value;
                OnPropertyChanged("ConsoleInput");
            }
        }

        public ObservableCollection<string> ConsoleOutput
        {
            get
            {
                return consoleOutput;
            }
            set
            {
                consoleOutput = value;
                OnPropertyChanged("ConsoleOutput");
            }
        }

        public void RunCommand()
        {
            ConsoleOutput.Add(ConsoleInput);
            // do your stuff here.
            ConsoleInput = String.Empty;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
