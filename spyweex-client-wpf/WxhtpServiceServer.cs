using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;


namespace spyweex_client_wpf
{
    public class WxhtpServiceServer
    {
        private TcpListener _listener; // Объект, принимающий TCP-клиентов
        private readonly IPAddress _localAddr;
        private readonly int _port;
        private volatile bool _shouldStop;
        private readonly object _syncLock = new object();
        private readonly ConcurrentDictionary<IPEndPoint, WxhtpClient> _dictionaryWxhtpClients;

        // Запуск сервера
        public WxhtpServiceServer(string ip, int port)
        {
            _localAddr = IPAddress.Parse(ip);
            _port = port;
            _shouldStop = false;
            _dictionaryWxhtpClients = new ConcurrentDictionary<IPEndPoint, WxhtpClient>();
        }

        public void Start()
        {
            _listener = new TcpListener(_localAddr, _port); // Создаем "слушателя" для указанного порта
            _listener.Start();
            // Запускаем его
            Debug.WriteLine("WxhtpServiceServer Started Succesfully");
        }

        public async Task Stop()
        {
            await RequestStop();
        }

        public async Task Run()
        {
            // В бесконечном цикле
            while (!_shouldStop)
            {
                //Принимаем новых клиентов. После того, как клиент был принят, он передается в новый поток (ClientThread)
                //с использованием пула потоков.
                try
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync();

                    // tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    // add new WxhtpClient(tcpClient) to dictionary
                    var ipEndPoint = ((IPEndPoint)tcpClient.Client.RemoteEndPoint);
                    WxhtpClient cl = new WxhtpClient(ref tcpClient);
                    _dictionaryWxhtpClients.TryAdd(ipEndPoint, cl);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception in Run " + e.Message);
                }

            }
            return;
        }

        private async Task RequestStop()
        {
            await Task.Run(() =>
            {
                try
                {
                    _shouldStop = true;
                    if (_listener != null)
                    {
                        _listener.Stop();
                    }
                    if (!_dictionaryWxhtpClients.IsEmpty)
                    {
                        CloseRemoveClients();
                    }
                    Debug.WriteLine("WxhtpServiceServer Stopped Succesfully");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            });

        }

        private void CloseRemoveClients()
        {
            try
            {
                foreach (var item in _dictionaryWxhtpClients)
                {
                    item.Value.CloseClient();
                }
                _dictionaryWxhtpClients.Clear();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in CloseRemoveClients " + e.Message);
            }

        }

        // Остановка сервера
        ~WxhtpServiceServer()
        {
            lock (_syncLock)
            {
                // Если "слушатель" был создан
                // Остановим его
                _listener?.Stop();
                if (!_dictionaryWxhtpClients.IsEmpty)
                {
                    CloseRemoveClients();
                }
            }

        }

        public WxhtpClient TryGetClientByIpEndpoint(IPEndPoint endPoint)
        {
            WxhtpClient wxhtpClient;
            _dictionaryWxhtpClients.TryGetValue(endPoint, out wxhtpClient);
            return wxhtpClient;
        }

        public WxhtpClient TryGetLastOrDefaultClient()
        {
            WxhtpClient wxhtpClient;
            if (!_dictionaryWxhtpClients.IsEmpty)
                wxhtpClient = _dictionaryWxhtpClients.LastOrDefault().Value;
            else
            {
                throw new EmptyCollectionException("Empty Collection");
            }
            return wxhtpClient;
        }

        public void TryRemoveClientByIpEndpoint(IPEndPoint endPoint)
        {
            WxhtpClient wxhtpClient;
            if (_dictionaryWxhtpClients.TryRemove(endPoint, out wxhtpClient))
            {
                //WxhtpClient.CancelAllTasks();
                //destroy WxhtpClient
            }

        }
    }

}
