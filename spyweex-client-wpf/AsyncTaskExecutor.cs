using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace spyweex_client_wpf
{
    public class AsyncTaskExecutor
    {
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

            CancelTokenReadAsync = cts.Token;
            isWorking = true;
            while (isWorking)
            {
                MemoryStream memoryStream = new MemoryStream();
                byte[] bufferResult = new byte[512];

                try
                {
                    while (isWorking)
                    {
                        int bytesRead;
                        try
                        {
                            bytesRead = await client.networkStream.ReadAsync(bufferResult, 0, bufferResult.Length);
                            memoryStream.Write(bufferResult, 0, bytesRead);
                            bytesRead = 0;
                            if (client.networkStream.DataAvailable)
                            {
                                continue;
                            }
                            else break;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine((Exception)ex);
                            client.Close();
                            MessageBoxResult result = MessageBox.Show("Connection {0} Dropped ", client.getTcpClient().Client.RemoteEndPoint.ToString());
                        }          
                    }

                    Parser parser = new Parser(memoryStream);
                    Response response = parser.tryParse();
                    ObservableSequenceOfResponses.OnNext(response);
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