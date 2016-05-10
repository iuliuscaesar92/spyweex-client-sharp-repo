using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using static spyweex_client_wpf.WxhtpExceptions;

namespace spyweex_client_wpf
{
    public class WxhtpClient
    {

        /// <summary>
        /// the connected tcp client
        /// </summary>
        private readonly TcpClient _tcpClient;

        /// <summary>
        /// executes tasks we give
        /// </summary>        
        private readonly AsyncTaskExecutor _asyncTaskExecutor;

        /// <summary>
        /// network stream to maintain the connection
        /// </summary>
        public NetworkStream networkStream;

        /// <summary>
        /// all received results from commands
        /// </summary>
        public MemoryStream MemStreamCMDResults = new MemoryStream();

        /// <summary>
        /// The sequence provides the ability to subscribe on it.
        /// When a response is obtained, it will be pushed a notification
        /// to all subscribed entities.
        /// </summary>


        // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
        public WxhtpClient(ref TcpClient client)
        {
            #region commented code
            //// Объявим строку, в которой будет хранится запрос клиента
            //string request = "";
            //// Буфер для хранения принятых от клиента данных
            //byte[] buffer = new byte[1024];
            //// Переменная для хранения количества байт, принятых от клиента
            //int count;
            //// Читаем из потока клиента до тех пор, пока от него поступают данные
            //while ((count = client.GetStream().Read(buffer, 0, buffer.Length)) > 0)
            //{
            //    // Преобразуем эти данные в строку и добавим ее к переменной Request
            //    request += Encoding.ASCII.GetString(buffer, 0, count);
            //    // Запрос должен обрываться последовательностью \r\n\r\n
            //    // Либо обрываем прием данных сами, если длина строки Request превышает 4 килобайта
            //    // Нам не нужно получать данные из POST-запроса (и т. п.), а обычный запрос
            //    // по идее не должен быть больше 4 килобайт
            //    if (request.IndexOf("\r\n\r\n") >= 0 || request.Length > 4096)
            //    {
            //        break;
            //    }
            //}

            //// Парсим строку запроса с использованием регулярных выражений
            //// При этом отсекаем все переменные GET-запроса
            //Match reqMatch = Regex.Match(request, @"^\w+\s+([^\s\?]+)[^\s]*\s+WXHTP/.*|");

            //// Если запрос не удался
            //if (reqMatch == Match.Empty)
            //{
            //    // Передаем клиенту ошибку 400 - неверный запрос
            //    SendError(client, 400);
            //    return;
            //}

            //// Получаем строку запроса
            //string requestUri = reqMatch.Groups[1].Value;

            //// Приводим ее к изначальному виду, преобразуя экранированные символы
            //// Например, "%20" -> " "
            //requestUri = Uri.UnescapeDataString(requestUri);

            //// Если в строке содержится двоеточие, передадим ошибку 400
            //// Это нужно для защиты от URL типа http://example.com/../../file.txt
            //if (requestUri.IndexOf("..") >= 0)
            //{
            //    SendError(client, 400);
            //    return;
            //}

            //// Если строка запроса оканчивается на "/", то добавим к ней index.html
            //if (requestUri.EndsWith("/"))
            //{
            //    requestUri += "index.html";
            //}

            //string filePath = "www/" + requestUri;

            //// Если в папке www не существует данного файла, посылаем ошибку 404
            //if (!File.Exists(filePath))
            //{
            //    SendError(client, 404);
            //    return;
            //}

            //// Получаем расширение файла из строки запроса
            //string extension = requestUri.Substring(requestUri.LastIndexOf('.'));

            //// Тип содержимого
            //string contentType = "";

            //// Пытаемся определить тип содержимого по расширению файла
            //switch (extension)
            //{
            //    case ".htm":
            //    case ".html":
            //        contentType = "text/html";
            //        break;
            //    case ".css":
            //        contentType = "text/stylesheet";
            //        break;
            //    case ".js":
            //        contentType = "text/javascript";
            //        break;
            //    case ".jpg":
            //        contentType = "image/jpeg";
            //        break;
            //    case ".jpeg":
            //    case ".png":
            //    case ".gif":
            //        contentType = "image/" + extension.Substring(1);
            //        break;
            //    default:
            //        if (extension.Length > 1)
            //        {
            //            contentType = "application/" + extension.Substring(1);
            //        }
            //        else
            //        {
            //            contentType = "application/unknown";
            //        }
            //        break;
            //}

            //// Открываем файл, страхуясь на случай ошибки
            //FileStream fs;
            //try
            //{
            //    fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            //}
            //catch (Exception)
            //{
            //    // Если случилась ошибка, посылаем клиенту ошибку 500
            //    SendError(client, 500);
            //    return;
            //}

            //// Посылаем заголовки
            //string headers = "HTTP/1.1 200 OK\nContent-Type: " + contentType + "\nContent-Length: " + fs.Length + "\n\n";
            //byte[] headersBuffer = Encoding.ASCII.GetBytes(headers);
            //client.GetStream().Write(headersBuffer, 0, headersBuffer.Length);

            //// Пока не достигнут конец файла
            //while (fs.Position < fs.Length)
            //{
            //    // Читаем данные из файла
            //    count = fs.Read(buffer, 0, buffer.Length);
            //    // И передаем их клиенту
            //    client.GetStream().Write(buffer, 0, count);
            //}

            //// Закроем файл и соединение
            //fs.Close();
            //client.Close();
            #endregion
            _tcpClient = client;
            _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            networkStream = _tcpClient.GetStream();
            _asyncTaskExecutor = new AsyncTaskExecutor(this);
            _asyncTaskExecutor.Start();

        }

        public TcpClient getTcpClient()
        {
            return _tcpClient;
        }

        public AsyncTaskExecutor GetAsyncTaskExecutor()
        {
            return _asyncTaskExecutor;
        }

        public async Task ExecuteTask(CancellationToken ct, params string[] listOfParams)
        {
            await _asyncTaskExecutor.Execute(this, ct, listOfParams);
        }

        public void CloseClient()
        {
            _tcpClient.Close();
            _tcpClient.Dispose();
        }

    }

    public class AsyncTaskExecutor
    {
        private WxhtpClient client;
        private readonly Subject<Response> ObservableSequenceOfResponses;

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
            while (true)
            {

                MemoryStream memoryStream = new MemoryStream();
                byte[] bufferResult = new byte[1024];
                StringBuilder str = new StringBuilder();
                int pos = 0;
                try
                {
                    while (true)
                    {
                        int bytesRead = await client.networkStream.ReadAsync(bufferResult, 0, bufferResult.Length);
                        str.Append(Encoding.ASCII.GetString(bufferResult));
                        memoryStream.Write(bufferResult, 0, bytesRead);
                        bytesRead = 0;
                        if (client.networkStream.DataAvailable)
                        {
                            continue;
                        }
                        else break;
                        //if (bytesRead <= 0)
                        //{
                        //    break;
                        //}                   
                    }

                    Parser parser = new Parser(ref memoryStream);
                    Response response = parser.tryParse();
                    ObservableSequenceOfResponses.OnNext(response);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Client Dropped, maybe. " + ex);
                }


            }

        }

        public async Task Execute(WxhtpClient wxhtpClient, CancellationToken ct, params string[] listOfParams)
        {
            WxhtpMessageBuilder wmBuilder = new WxhtpMessageBuilder(listOfParams);
            string message = wmBuilder.getContent();
            byte[] bufferRequest = Encoding.ASCII.GetBytes(message);
            await wxhtpClient.networkStream.WriteAsync(bufferRequest, 0, bufferRequest.Length);
        }

    }

    internal static class CancellationExtension
    {
        public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            return task.IsCompleted
                ? task
                : task.ContinueWith(
                    completedTask => completedTask.GetAwaiter().GetResult(),
                    cancellationToken,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
        }
    }

}
