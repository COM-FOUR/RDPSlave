using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shell;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Windows;

namespace RDPSlave
{
    public class RDPSlaveModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        public class RelayCommand : ICommand
        {
            #region Fields

            readonly Action<object> _execute;
            readonly Predicate<object> _canExecute;

            #endregion // Fields

            #region Constructors

            public RelayCommand(Action<object> execute)
                : this(execute, null)
            {
            }

            public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            {
                if (execute == null)
                    throw new ArgumentNullException("execute");

                _execute = execute;
                _canExecute = canExecute;
            }
            #endregion // Constructors

            #region ICommand Members


            public bool CanExecute(object parameter)
            {
                return _canExecute == null ? true : _canExecute(parameter);
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }

            #endregion // ICommand Members
        }
    }
    public class RDPSlaveViewModel : RDPSlaveModel
    {
        //private Application currApp;
        
        RDPFunctions.RDPConnections connections = new RDPFunctions.RDPConnections();

        RDPFunctions.RDPConnection selectedRDPConnection = new RDPFunctions.RDPConnection();

        public bool isSilentProcessing = true;

        RelayCommand startSessionCommand;
        RelayCommand deleteConnectionCommand;
        RelayCommand createConnectionCommand;
        RelayCommand saveConnectionsCommand;
        RelayCommand loadConnectionsCommand;

        public ICommand StartSessionCommand
        {
            get
            {
                if (startSessionCommand == null)
                {
                    startSessionCommand = new RelayCommand(param => selectedRDPConnection.StartSession(),
                        param => this.selectedRDPConnection!=null);
                }
                return startSessionCommand;
            }
        }
        public ICommand DeleteConnectionCommand
        {
            get
            {
                if (deleteConnectionCommand == null)
                {
                    deleteConnectionCommand = new RelayCommand(param => this.DeleteSelectedRDPConnection(),
                        param => this.selectedRDPConnection != null);
                }
                return deleteConnectionCommand;
            }
        }
        public ICommand CreateConnectionCommand
        {
            get
            {
                if (createConnectionCommand == null)
                {
                    createConnectionCommand = new RelayCommand(param => this.CreateRDPConnection(),
                        param => true);
                }
                return createConnectionCommand;
            }
        }
        public ICommand SaveConnectionsCommand
        {
            get
            {
                if (saveConnectionsCommand == null)
                {
                    saveConnectionsCommand = new RelayCommand(param => this.WriteRDPConnections(true),
                        param => this.connections.ConnectionList.Count>0);
                }
                return saveConnectionsCommand;
            }
        }
        public ICommand LoadConnectionsCommand
        {
            get
            {
                if (loadConnectionsCommand == null)
                {
                    loadConnectionsCommand = new RelayCommand(param => this.ReadRDPConnections(true),
                        param => true);
                }
                return loadConnectionsCommand;
            }
        }
        public RDPFunctions.RDPConnections Connections { get { return connections; } set { connections = value; NotifyPropertyChanged("Connections"); } }
        public ObservableCollection<RDPFunctions.RDPConnection> ConnectionList { get { return new ObservableCollection<RDPFunctions.RDPConnection>(connections.ConnectionList); } set { connections.ConnectionList = value.ToList<RDPFunctions.RDPConnection>(); NotifyPropertyChanged("ConnectionList"); } }
        public RDPFunctions.RDPConnection SelectedRDPConnection { get { return selectedRDPConnection; } set { selectedRDPConnection = value; NotifyPropertyChanged("SelectedRDPConnection"); } }

        public RDPSlaveViewModel() : this(new Application(),new string[0]) { }
        public RDPSlaveViewModel(Application app) : this(app, new string[0]) { }
        public RDPSlaveViewModel(Application app, string[] startupArgs)
        {
            //currApp = app;

            ReadRDPConnections();

            if (startupArgs.Length > 0)
            {
                ProcessStartupArgs(startupArgs);
            }
            else
            {
                if (connections.HasDefaultConnection)
                {
                    RDPFunctions.StartRDPSession(connections.DefaultConnection);
                }
            }

            CreateJumpList();

        }
        private void CreateJumpList()
        {
            JumpList jl = new JumpList();

            JumpTask jt = new JumpTask();
            jt.Title = "Hauptfenster";
            jt.Description = "Hauptfenster";
            jt.Arguments = "SHOWWINDOW";
            jt.ApplicationPath = Assembly.GetEntryAssembly().CodeBase;
            jt.IconResourcePath = Assembly.GetEntryAssembly().CodeBase;

            jl.JumpItems.Add(jt);

            foreach (RDPFunctions.RDPConnection item in connections.ConnectionList)
            {
                jt = new JumpTask();
                //jt.IconResourcePath = AppDomain.CurrentDomain.BaseDirectory + "\\Icons.dll";

                jt.Title = item.Name;
                jt.Description = item.Host;
                jt.Arguments = item.Name;
                jt.CustomCategory = item.Group;
                jt.ApplicationPath = Assembly.GetEntryAssembly().CodeBase;
                jt.IconResourcePath = Assembly.GetEntryAssembly().CodeBase;

                jl.JumpItems.Add(jt);
            }
            JumpList.SetJumpList(Application.Current, jl);
            //jl.Apply();
        }
        private void ReadRDPConnections()
        {
            ReadRDPConnections(false);
        }
        private void ReadRDPConnections(bool confirm)
        {
            if (confirm)
            {
                MessageBoxResult result = MessageBox.Show("RDP Verbindungen aus Datei laden?", "Laden", MessageBoxButton.YesNo);
                if (result!= MessageBoxResult.Yes)
                {
                    return;
                }
            }

            connections.LoadConnections();
            NotifyPropertyChanged("ConnectionList");
            if (connections.ConnectionList.Count>0)
            {
                selectedRDPConnection = connections.ConnectionList.First();
            }
        }
        private void WriteRDPConnections()
        {
            WriteRDPConnections(false);
        }
        private void WriteRDPConnections(bool confirm)
        {
            if (confirm)
            {
                MessageBoxResult result = MessageBox.Show("RDP Verbindungen in Datei speichern?", "Speichern", MessageBoxButton.YesNo);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            connections.SaveConnections();
            NotifyPropertyChanged("ConnectionList");
            this.CreateJumpList();
        }
        private void DeleteSelectedRDPConnection()
        {
            MessageBoxResult result = MessageBox.Show("Aktuelle RDP Verbindung entfernen?", "Löschen", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            connections.ConnectionList.Remove(selectedRDPConnection);
            
            NotifyPropertyChanged("ConnectionList");
        }
        private void CreateRDPConnection()
        {
            if (connections.ConnectionList.Any())
            {
                var lastItem = connections.ConnectionList.Last();
                connections.ConnectionList.Add(new RDPFunctions.RDPConnection(lastItem.Order + 1));
            }
            else
            {
                connections.ConnectionList.Add(new RDPFunctions.RDPConnection(1));
            }
            
            NotifyPropertyChanged("ConnectionList");
        }
        private void ProcessStartupArgs(string[] startupArgs)
        {
            foreach (string arg in startupArgs)
            {
                switch (arg.ToUpper())
                {
                    case "SHOWWINDOW": isSilentProcessing = false; break;
                    default:
                        if (connections.HostIsKnown(arg))
                        {
                            connections.StartSessionByHost(arg);
                        }
                        if (connections.HostNameIsKnown(arg))
                        {
                            connections.StartSessionByHostName(arg);
                        }
                        break;
                }
            }
        }
    }
}
