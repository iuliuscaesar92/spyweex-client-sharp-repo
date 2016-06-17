using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace spyweex_client_wpf
{
    public class AsyncTaskExecutor
    {
        private Object thisLock = new Object();
        /// <summary>
        /// Reference to the client instance
        /// </summary>
        private WxhtpClient client;
        /// <summary>
        /// A collection of responses on which to subscribe
        /// </summary>
        private readonly Subject<Response> ObservableSequenceOfResponses;
        /// <summary>
        /// bool testing if taskexecutor reads from stream continuously.
        /// Also used in infinite loop and to stop continously reading.
        /// </summary>
        public bool isWorking = false;
        /// <summary>
        /// Cancellation token to stop task reading from source.
        /// </summary>
        CancellationTokenSource cts = new CancellationTokenSource();
        private CancellationToken CancelTokenReadAsync;

        public Subject<Response> getObservableSequenceOfReponses()
        {
            return ObservableSequenceOfResponses;
        }

        public AsyncTaskExecutor(WxhtpClient client)
        {
            this.client = client;
            ObservableSequenceOfResponses = new Subject<Response>();
        }

        public async void Start()
        {
            if (isWorking) return;
            int counter = 0;
            CancelTokenReadAsync = cts.Token;
            isWorking = true;

            while (isWorking)
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] bufferResult = new byte[1024];

                try
                {
                    while (isWorking)
                    {

                        int bytesRead = 0;
                        try
                        {

                            bytesRead = await client.networkStream.ReadAsync(bufferResult, 0, bufferResult.Length);

                            if (bytesRead <= 0)
                            {
                                isWorking = false;
                                break;
                            }
                            if (bytesRead > 0)
                            {
                                byte[] endingBytes = new byte[9];
                                Array.Copy(bufferResult, bytesRead - 10, endingBytes, 0, 9);

                                var encoding = System.Text.Encoding.GetEncoding("iso-8859-1");
                                string ending_magic_word = encoding.GetString(endingBytes);

                                if (ending_magic_word.Equals("end_wxhtp"))
                                {
                                    break;
                                }
                                else
                                {
                                    lock (thisLock)
                                    {
                                        memoryStream.Write(bufferResult, 0, bytesRead);
                                    }
                                    continue;
                                }
                            }
                            else
                            {
                              break;
                            }

                        }

                        catch (Exception ex)
                        {
                            Debug.WriteLine((Exception)ex);
                            Stop();
                            client.Close();
                            MessageBoxResult result = MessageBox.Show("Connection {0} Dropped ", client.getTcpClient().Client.RemoteEndPoint.ToString());
                        }
                    }
                    if (isWorking == false)
                        break;

                    lock (thisLock)
                    {
                        MemoryStream toBeParsed = memoryStream;
                        Parser parser = new Parser(toBeParsed);
                        Response response = parser.tryParse();
                        ObservableSequenceOfResponses.OnNext(response);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Client Dropped, maybe. \n" + ex);
                    Stop();
                }
            }                     
        }

        public async Task Execute(CancellationToken ct, params string[] listOfParams)
        {
            WxhtpMessageBuilder wmBuilder = new WxhtpMessageBuilder(listOfParams);
            string message = wmBuilder.getContent();
            byte[] bufferRequest = Encoding.ASCII.GetBytes(message);
            await client.networkStream.WriteAsync(bufferRequest, 0, bufferRequest.Length);
        }

        public void Stop()
        {
            cts?.Cancel();
            isWorking = false;
        }
    }

}