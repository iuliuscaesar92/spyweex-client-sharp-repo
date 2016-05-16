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
    public class ViewModel : INotifyPropertyChanged
    {
        public ViewModel()
        {
            //_selectedSession = new Session(" ", " ");
        }

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
                    NotifyPropertyChanged("SelectedSession");
                }
            }
        }

        //public int Incoming
        //{
        //    get { return SelectedSession.Incoming; }
        //    set
        //    {
        //        if (SelectedSession.Incoming != value)
        //        {
        //            SelectedSession.Incoming = value;
        //            NotifyPropertyChanged("Incoming");
        //        }
        //    }
        //}

        //public int Outgoing
        //{
        //    get { return SelectedSession.Outgoing; }
        //    set
        //    {
        //        if (SelectedSession.Outgoing != value)
        //        {
        //            SelectedSession.Outgoing = value;
        //            NotifyPropertyChanged("Outgoing");
        //        }
        //    }
        //}
    }

    public class Session : INotifyPropertyChanged
    {
        public string ID { get; set; }
        public string IP { get; set; }
        public string Username { get; set; }
        public string ComputerName { get; set; }
        public string Privileges { get; set; }
        public string OS { get; set; }
        public string Uptime { get; set; }
        public string Cam { get; set; }
        public string InstallDate { get; set; }
        public string Country { get; set; }
        public string RegionName { get; set; }
        public string City { get; set; }
        public string Isp { get; set; }
        public string Coords { get; set; }
        public string Zip { get; set; }


        private int incoming;
        private int outgoing;

        public Session()
        {
            
        }

        public Session(string ID, string IP)
        {
            this.ID = ID;
            this.IP = IP;            
        }

        public int Incoming
        {
            get { return incoming; }
            set
            {
                if (incoming != value)
                {
                    incoming = value;
                    NotifyPropertyChanged("Incoming");
                }
            }
        }

        public int Outgoing
        {
            get { return outgoing; }
            set
            {
                if (outgoing != value)
                {
                    outgoing = value;
                    NotifyPropertyChanged("Outgoing");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

    }
}
