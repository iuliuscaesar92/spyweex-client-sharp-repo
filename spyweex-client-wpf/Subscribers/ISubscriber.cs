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

namespace spyweex_client_wpf.Subscribers
{
    public abstract class ISubscriber
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

        public async void UnsubscribeByTimeOut(TimeSpan ts)
        {
            await Task.Delay(ts);
            if(isSubscribed) UnsubscribeAsync();
        }

    }
}