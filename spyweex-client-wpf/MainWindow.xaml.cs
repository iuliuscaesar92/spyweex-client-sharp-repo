using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
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

namespace spyweex_client_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class Session
        {
            public string ID { get; set; }
            public string IP { get; set; }
            public string Username { get; set; }
            public string ComputerName { get; set; }
            public string Privileges { get; set; }
            public string OS { get; set; }
            public string Uptime { get; set; }
            public string Country { get; set; }
            public string Cam { get; set; }
            public string InstallDate { get; set; }
        }

        /// <summary>
        /// sessions source for listview.
        /// </summary>
        ObservableCollection<Session> sessions = new ObservableCollection<Session>();
        public Session SelectedSession { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SessionListView.ItemsSource = sessions;
        }

        private void btnStart_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnStop_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancel_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnIP_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnDesktopScreen_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnWebcamScreen_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddUser_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnRemoveUser_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnCmdPrompt_Clicked(object sender, RoutedEventArgs e)
        {

        }

        private void btnKeylogger_Clicked(object sender, ContextMenuEventArgs e)
        {

        }

        private void btnKeyLogger_Clicked(object sender, RoutedEventArgs e)
        {

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