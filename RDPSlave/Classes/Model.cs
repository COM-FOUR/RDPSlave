﻿using System;
using System.Collections.Generic;
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
        public bool isSilentProcessing = true;

        RelayCommand startSession;
        public ICommand LoginCommand
        {
            get
            {
                if (startSession == null)
                {
                    startSession = new RelayCommand(param => RDPFunctions.StartRDPSession("", "", ""),
                        param => true);
                }
                return startSession;
            }
        }
        public RDPFunctions.RDPConnections Connections { get { return connections; } set { connections = value; NotifyPropertyChanged("Connections"); } }

        public RDPSlaveViewModel() : this(new Application(),new string[0]) { }
        public RDPSlaveViewModel(Application app) : this(app, new string[0]) { }
        public RDPSlaveViewModel(Application app, string[] startupArgs)
        {
            //currApp = app;

            ReadRDPconnections();

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

            foreach (KeyValuePair<int, RDPFunctions.RDPConnection> item in connections.ConnectionList)
            {
                jt = new JumpTask();
                //jt.IconResourcePath = AppDomain.CurrentDomain.BaseDirectory + "\\Icons.dll";

                jt.Title = item.Value.Name;
                jt.Description = item.Value.Host;
                jt.Arguments = item.Key.ToString();
                jt.CustomCategory = item.Value.Group;
                jt.ApplicationPath = Assembly.GetEntryAssembly().CodeBase;
                jt.IconResourcePath = Assembly.GetEntryAssembly().CodeBase;

                jl.JumpItems.Add(jt);
            }
            JumpList.SetJumpList(Application.Current, jl);
            //jl.Apply();
        }
        private void ReadRDPconnections()
        {
            connections.LoadConnections();
        }
        private void ProcessStartupArgs(string[] startupArgs)
        {
            foreach (string arg in startupArgs)
            {
                int i;
                if (int.TryParse(arg.ToUpper(), out i))
                {
                    if (connections.ConnectionList.ContainsKey(i))
                    {
                        connections.ConnectionList[i].StartSession();
                    }
                }
                else
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
}
