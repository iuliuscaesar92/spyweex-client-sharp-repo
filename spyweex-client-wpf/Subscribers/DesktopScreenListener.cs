﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

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

}