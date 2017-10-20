using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Input;

namespace RDPSlave.Classes
{
    public class RDPFunctions
    {
        public class PDPConnections : RDPSlaveModel
        {
            SortedList<int, RDPConnection> connectionList = new SortedList<int, RDPConnection>();
            bool hasDefaultConnection = false;
            string lastErrorMessage = "";

            public SortedList<int, RDPConnection> ConnectionList {get { return connectionList; } set { connectionList = value; NotifyPropertyChanged("ConnectionList"); } }
            public bool HasDefaultConnection { get { return hasDefaultConnection; } set { hasDefaultConnection = value; NotifyPropertyChanged("HasDefaultConnection"); } }
            public string LastErrorMessage { get { return lastErrorMessage; } }

            public RDPConnection DefaultConnection { 
                get 
                {
                if (!this.hasDefaultConnection)
                	{
                        return null;
                	}
                    else
                    {
                        return connectionList.Values.Where(f => f.IsDefault = true).First();
                    }
                }
            }

            public bool LoadConnections()
            {
                return (LoadConnections(""));
            }
            public bool LoadConnections(string fileName)
            {
                bool result = false;

                if (fileName==null || fileName=="")
                {
                    fileName = AppDomain.CurrentDomain.BaseDirectory + "\\RDPConnections.xml";
                }

                try
                {
                    connectionList = new SortedList<int, RDPConnection>();

                    XDocument doc = XDocument.Load(fileName);
                    int i = 0;
                    foreach (var item in doc.Root.Elements())
                    {
                        int order = 0;

                        RDPFunctions.RDPConnection rdp = new RDPFunctions.RDPConnection();
                       
                        int.TryParse(Helpers.TryGetElementValue(item,"Order","0"), out order);
                       
                        rdp.Host = Helpers.TryGetElementValue(item, "Host","");
                        rdp.Name = Helpers.TryGetElementValue(item, "Name", "");
                        rdp.UserName = Helpers.TryGetElementValue(item, "UserName", "");
                        rdp.Password = Helpers.TryGetElementValue(item, "Password", "");
                        rdp.Group = Helpers.TryGetElementValue(item, "Group", "");

                        bool isdefault = false;
                        bool.TryParse(Helpers.TryGetElementValue(item, "Default","false"), out isdefault);
                        if (isdefault)
                        {
                            hasDefaultConnection = true;
                            rdp.IsDefault = true;
                        }

                        i++;
                        if (order>0)
                        {
                            connectionList.Add(order, rdp);
                        }
                        else
                        {
                            connectionList.Add(i, rdp);
                        }
                        
                    }

                    result = true;
                }
                catch (Exception e)
                {
                    lastErrorMessage = e.Message;
                    NotifyPropertyChanged("LastErrorMessage");
                }

                return result;
            }
            public bool SaveConnections()
            {
                return SaveConnections("");
            }
            public bool SaveConnections(string fileName)
            {
                bool result = false;
                try
                {

                    result = true;
                }
                catch (Exception e)
                {
                    lastErrorMessage = e.Message;
                    NotifyPropertyChanged("LastErrorMessage");
                }

                return result;
            }
        }
        public class RDPConnection : RDPSlaveModel
        {
            string name;
            string host;
            string userName;
            string password;
            string group;
            bool isDefault;

            RelayCommand startSessionCommand;

            public ICommand StartSession
            {
                get
                {
                    if (startSessionCommand == null)
                    {
                        startSessionCommand = new RelayCommand(param => StartRDPSession(this.host,this.userName,this.password),
                            param => (this.host!=null & this.host!=""));
                    }
                    return startSessionCommand;
                }
            }

            public string Name { get { return name; } set { name = value;NotifyPropertyChanged("Name"); } }
            public string Host { get { return host; } set { host = value; NotifyPropertyChanged("Host"); } }
            public string UserName { get { return userName; } set { userName = value; NotifyPropertyChanged("UserName"); } }
            public string Password { get { return password; } set { password = value; NotifyPropertyChanged("Password"); } }
            public string Group { get { return group; } set { group = value; NotifyPropertyChanged("Group"); } }
            public bool IsDefault { get { return isDefault; } set { isDefault = value; NotifyPropertyChanged("IsDefault"); } }

            public RDPConnection() { }
            public RDPConnection(string name, string host, string username, string password) : this(name,host,username,password,"") { }
            public RDPConnection(string name, string host, string username, string password, string group) : this(name, host, username, password, group, false) { }
            public RDPConnection(string name, string host, string username, string password, string group, bool isdefault)
            {
                this.name = name;
                this.host = host;
                this.userName = username;
                this.password = password;
                this.group = group;
                this.isDefault = isdefault;
            }
        }
        public static void StartRDPSession(RDPConnection rdp)
        {
            StartRDPSession(rdp.Host, rdp.UserName, rdp.Password);
        }
        public static void StartRDPSession(string serverIP, string userName, string password)
        {
            Process rdcProcess = new Process();
            rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\cmdkey.exe");
            rdcProcess.StartInfo.Arguments = "/generic:TERMSRV/" + serverIP + " /user:" + userName + " /pass:" + password;
            rdcProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            rdcProcess.Start();

            rdcProcess.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\system32\mstsc.exe");
            rdcProcess.StartInfo.Arguments = "/v " + serverIP; // ip or name of computer to connect
            rdcProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            rdcProcess.Start();
        }
    }
    
}
