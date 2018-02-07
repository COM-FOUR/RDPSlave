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
        internal ObservableCollection<RDPFunctions.RDPConnection> connectionList = new ObservableCollection<RDPFunctions.RDPConnection>();
        #endregion

        #region Comamnds
        internal RelayCommand startSessionCommand;
        internal RelayCommand deleteConnectionCommand;
        internal RelayCommand createConnectionCommand;
        internal RelayCommand saveConnectionsCommand;
        internal RelayCommand loadConnectionsCommand;
        internal RelayCommand incMaxDisplTaskbarItemsCommand;
        internal RelayCommand moveUpInConnectionListCommand;
        internal RelayCommand moveDownInConnectionListCommand;

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
        /// <summary>
        /// increases the maximum number of displayed JumpListItems in WindowsTaskbar to current count of RDPConnections
        /// </summary>
        public ICommand IncMaxDisplTaskbarItemsCommand
        {
            get
            {
                if (incMaxDisplTaskbarItemsCommand == null)
                {
                    incMaxDisplTaskbarItemsCommand = new RelayCommand(param => this.IncreaseMaxDisplayedJumpListItems(this.ConnectionList.Count),
                        param => true);
                }
                return incMaxDisplTaskbarItemsCommand;
            }
        }
        /// <summary>
        /// moves selected RDPconnection up in list
        /// </summary>
        public ICommand MoveUpInConnectionListCommand
        {
            get
            {
                if (moveUpInConnectionListCommand == null)
                {
                    moveUpInConnectionListCommand = new RelayCommand(param => this.MoveItemInConnectionlist(-1),
                        param => true);
                }
                return moveUpInConnectionListCommand;
            }
        }
        /// <summary>
        /// moves selected RDPconnection down in list
        /// </summary>
        public ICommand MoveDownInConnectionListCommand
        {
            get
            {
                if (moveDownInConnectionListCommand == null)
                {
                    moveDownInConnectionListCommand = new RelayCommand(param => this.MoveItemInConnectionlist(+1),
                        param => true);
                }
                return moveDownInConnectionListCommand;
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
        //public ObservableCollection<RDPFunctions.RDPConnection> ConnectionList { get { return new ObservableCollection<RDPFunctions.RDPConnection>(connections.ConnectionList); } set { connections.ConnectionList = value.ToList<RDPFunctions.RDPConnection>(); NotifyPropertyChanged("ConnectionList"); } }
        public ObservableCollection<RDPFunctions.RDPConnection> ConnectionList { get { return connectionList; } set { connectionList = value; NotifyPropertyChanged("ConnectionList"); } }
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
            connectionList = new ObservableCollection<RDPFunctions.RDPConnection>(connections.ConnectionList);
            NotifyPropertyChanged("ConnectionList");
            //if (connections.ConnectionList.Count>0)
            //{
            //    selectedRDPConnection = connections.ConnectionList.First();
            //}
            if (connectionList.Count>0)
            {
                selectedRDPConnection = connectionList.First();
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
            connections.ConnectionList = connectionList.ToList();
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

            //connections.ConnectionList.Remove(selectedRDPConnection);
            connectionList.Remove(selectedRDPConnection);

            NotifyPropertyChanged("ConnectionList");
        }
        /// <summary>
        /// creates a new, empty RDPConnection at the end of ConnectionList
        /// </summary>
        private void CreateRDPConnection()
        {
            if (connectionList.Any())
            {
                var lastItem = connectionList.Last();
                connectionList.Add(new RDPFunctions.RDPConnection(lastItem.Order + 1));
            }
            else
            {
                connectionList.Add(new RDPFunctions.RDPConnection(1));
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
        /// <summary>
        /// increases the maximum number of displayed JumpListItems in WindowsTaskbar to current count of RDPConnections
        /// </summary>
        /// <param name="count">number to be set as maximum</param>
        private void IncreaseMaxDisplayedJumpListItems(int count)
        {
            

            if (count >=1 && count<10)
            {
                count = 15;
            }
            else if (count >= 10 && count < 20)
            {
                count = 25;
            }
            else if (count >= 20)
            {
                count = 35;
            }
            Helpers.SetMaxJumpListItems(count);
        }
        /// <summary>
        /// changes the index of currently selected RDPConnection in list
        /// </summary>
        /// <param name="steps">steps to move index</param>
        private void MoveItemInConnectionlist(int steps)
        {
            int currentIndex = ConnectionList.IndexOf(SelectedRDPConnection);
            int newIndex = currentIndex + steps;
            
            if (newIndex<0)
            {
                newIndex = 0;
            }
            else
            {
                int lastIndex = ConnectionList.IndexOf(ConnectionList.Last());
                if (newIndex>lastIndex)
                {
                    newIndex = lastIndex;
                }
            }
            
            if (currentIndex!=newIndex)
            {
                ConnectionList.Move(currentIndex, newIndex);
                NotifyPropertyChanged("ConnectionList");
            }
        }
        #endregion
    }
}
