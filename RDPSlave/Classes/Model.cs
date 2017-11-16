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
    /// <summary>
    /// model for mvvm pattern
    /// </summary>
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
    /// <summary>
    /// viewmodel for mvvm pattern
    /// </summary>
    public class RDPSlaveViewModel : RDPSlaveModel
    {
        //private Application currApp;

        #region Locals
        internal RDPFunctions.RDPConnections connections = new RDPFunctions.RDPConnections();
        internal RDPFunctions.RDPConnection selectedRDPConnection = new RDPFunctions.RDPConnection();
        internal bool silentProcessing = true;
        #endregion

        #region Comamnds
        internal RelayCommand startSessionCommand;
        internal RelayCommand deleteConnectionCommand;
        internal RelayCommand createConnectionCommand;
        internal RelayCommand saveConnectionsCommand;
        internal RelayCommand loadConnectionsCommand;

        /// <summary>
        /// starts session for currently selected RDPConnection
        /// </summary>
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
        /// <summary>
        /// delete currently selected RDPConnection
        /// </summary>
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
        /// <summary>
        /// create new, empty RDPConnection in ConnectionList
        /// </summary>
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
        /// <summary>
        /// save current ConnectionList to file
        /// </summary>
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
        /// <summary>
        /// load ConnectionList from file
        /// </summary>
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
        #endregion

        #region Globals
        /// <summary>
        /// wrapper including a list of rdp connections
        /// </summary>
        public RDPFunctions.RDPConnections Connections { get { return connections; } set { connections = value; NotifyPropertyChanged("Connections"); } }
        /// <summary>
        /// observable list of RDPConnection for binding purpose
        /// </summary>
        public ObservableCollection<RDPFunctions.RDPConnection> ConnectionList { get { return new ObservableCollection<RDPFunctions.RDPConnection>(connections.ConnectionList); } set { connections.ConnectionList = value.ToList<RDPFunctions.RDPConnection>(); NotifyPropertyChanged("ConnectionList"); } }
        /// <summary>
        /// currently ,via binding selected RDPCOnnection
        /// </summary>
        public RDPFunctions.RDPConnection SelectedRDPConnection { get { return selectedRDPConnection; } set { selectedRDPConnection = value; NotifyPropertyChanged("SelectedRDPConnection"); } }
        /// <summary>
        /// indicator if mainwindow should be shown, or not
        /// </summary>
        public bool isSilentProcessing { get { return silentProcessing; } }

        #endregion

        #region Constructors
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
        #endregion

        #region Methods
        /// <summary>
        /// creates windows jumplist
        /// </summary>
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
        /// <summary>
        /// load rdp connections from file
        /// </summary>
        private void ReadRDPConnections()
        {
            ReadRDPConnections(false);
        }
        /// <summary>
        /// load rdp connections from file
        /// </summary>
        /// <param name="confirm">show confirmation dialog</param>
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
        /// <summary>
        /// save rdp connections to file
        /// </summary>
        private void WriteRDPConnections()
        {
            WriteRDPConnections(false);
        }
        /// <summary>
        /// save rdp connections to file
        /// </summary>
        /// <param name="confirm">show confirmation dialog</param>
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
        /// <summary>
        /// deletes currently selected RDPConnection from ConnectionList
        /// </summary>
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
        /// <summary>
        /// creates a new, empty RDPConnection at the end of ConnectionList
        /// </summary>
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
        /// <summary>
        /// processes the provided startup parameters
        /// </summary>
        /// <param name="startupArgs">startup parameters</param>
        private void ProcessStartupArgs(string[] startupArgs)
        {
            foreach (string arg in startupArgs)
            {
                switch (arg.ToUpper())
                {
                    case "SHOWWINDOW": silentProcessing = false; break;
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
        #endregion
    }
}
