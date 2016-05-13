using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace spyweex_client_wpf
{
    class ViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Session> sessions = new ObservableCollection<Session>();

        private Session _selectedSession;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public Session SelectedSession
        {
            get
            { return this._selectedSession; }
            set
            {
                if (value != this._selectedSession)
                {
                    this._selectedSession = value;
                    NotifyPropertyChanged("SelectedSessionProperty");
                }
            }
        }
    }

    public class Session
    {
        public string ID { get; set; }
        public string IP { get; set; }
        public string Username { get; set; }
        public string ComputerName { get; set; }
        public string Privileges { get; set; }
        public string OS { get; set; }
        public string Uptime { get; set; }
        public string Country { get; set; }
        public string Cam { get; set; }
        public string InstallDate { get; set; }
        public IPEndPoint ipEndPoint { get; set; }
        public int incoming { get; set; }
        public int outgoing { get; set; }

        public Session(string ID, string IP)
        {
            this.ID = ID;
            this.IP = IP;
        }

    }
}
