using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Xml.XPath;

namespace spyweex_client_wpf
{
    public static class Utils
    {
        public static string[] GetAllLocalIPv4(NetworkInterfaceType _type)
        {
            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return ipAddrList.ToArray();
        }

        public static void SaveToBmp(FrameworkElement visual, string fileName)
        {
            var encoder = new BmpBitmapEncoder();
            SaveUsingEncoder(visual, fileName, encoder);
        }

        public static void SaveToPng(FrameworkElement visual, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            SaveUsingEncoder(visual, fileName, encoder);
        }

        public static void SaveUsingEncoder(FrameworkElement visual, string fileName, BitmapEncoder encoder)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap((int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            encoder.Frames.Add(frame);

            using (var stream = File.Create(fileName))
            {
                encoder.Save(stream);
            }
        }

        public static IPEndPoint ParseIPEndpoint(string ipEndPoint)
        {
            int ipAddressLength = ipEndPoint.LastIndexOf(':');
            return new IPEndPoint(
                IPAddress.Parse(ipEndPoint.Substring(0, ipAddressLength)),
                Convert.ToInt32(ipEndPoint.Substring(ipAddressLength + 1)));
        }

        public static Tuple<string, string, string, string, string, string> GetGeoInfo(string ipAddress)
        {
            try
            {
                WebClient webClient = new WebClient();
                string jsonResult = webClient.DownloadString("http://ip-api.com/json/" + ipAddress);

                var jsonReader = JsonReaderWriterFactory.CreateJsonReader(
                    Encoding.Default.GetBytes(jsonResult),
                    new System.Xml.XmlDictionaryReaderQuotas());

                var root = XElement.Load(jsonReader);
                if (!root.XPathSelectElement("status").Value.Equals("success"))
                {
                    return Tuple.Create(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                        string.Empty);
                }
                string country = root.XPathSelectElement("country").Value;
                string regionName = root.XPathSelectElement("regionName").Value;
                string city = root.XPathSelectElement("city").Value;
                string isp = root.XPathSelectElement("isp").Value;
                string coords = $"{root.XPathSelectElement("lat").Value},{root.XPathSelectElement("lon").Value}";
                string zip = root.XPathSelectElement("zip").Value;

                return Tuple.Create(country, regionName, city, isp, coords, zip);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception occured: {0}", ex.ToString());
                return Tuple.Create(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            }
        }
    }
}
