using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RDPSlave
{
    /// <summary>
    /// provides methods an classes to connect via rdp
    /// </summary>
    public class RDPFunctions
    {
        /// <summary>
        /// wrapper class for list of multipple connections
        /// </summary>
        public class RDPConnections : RDPSlaveModel
        {
            #region Locals
            List<RDPConnection> connectionList = new List<RDPConnection>();
            bool hasDefaultConnection = false;
            string lastErrorMessage = "";
            #endregion

            #region Globals
            public List<RDPConnection> ConnectionList {get { return connectionList; } set { connectionList = value; NotifyPropertyChanged("ConnectionList"); } }
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
                        return connectionList.Where(f => f.IsDefault == true).First();
                    }
                }
            }
            #endregion

            #region Methods
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
                    connectionList = new List<RDPConnection>();
                    
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
                            rdp.Order = order;
                        }
                        else
                        {
                            rdp.Order = i;
                        }

                        connectionList.Add(rdp);
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

                if (fileName == null || fileName == "")
                {
                    fileName = AppDomain.CurrentDomain.BaseDirectory + "RDPConnections.xml";
                }

                try
                {
                    using (XmlWriter writer = XmlWriter.Create(fileName))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("RDPConnections");
                        foreach (var item in connectionList)
                        {
                            writer.WriteStartElement("RDPConnection");
                            writer.WriteElementString("Name", item.Name);
                            writer.WriteElementString("Host", item.Host);
                            writer.WriteElementString("UserName", item.UserName);
                            writer.WriteElementString("Password", item.Password);
                            writer.WriteElementString("Group", item.Group);
                            writer.WriteElementString("Default", item.IsDefault.ToString());
                            writer.WriteElementString("Order", item.Order.ToString());
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }

                    if (RDPSlave.Properties.Settings.Default.EncryptConnectionFile)
                    {
                        File.Encrypt(fileName);
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
            public bool HostIsKnown(string host)
            {
                bool result = false;

                if (connectionList != null)
                {
                    result = connectionList.Any(f => f.Host == host);
                }

                return result;
            }
            public void StartSessionByHost(string host)
            {
                if (connectionList != null && connectionList.Any(f => f.Host == host))
                {

                    RDPConnection rdp = connectionList.Where(f => f.Host == host).First();
                    
                    rdp.StartSession();
                }
            }
            public bool HostNameIsKnown(string hostname)
            {
                bool result = false;

                if (connectionList!=null)
                {
                    result = connectionList.Any(f => f.Name == hostname);
                }
                
                return result;
            }
            public void StartSessionByHostName(string hostname)
            {
                if (connectionList != null && connectionList.Any(f => f.Name == hostname))
                {
                    RDPConnection rdp = connectionList.Where(f => f.Name == hostname).First();
                    rdp.StartSession();
                }
            }
            #endregion

        }
        /// <summary>
        /// Contains necessary fields for rdp conection
        /// </summary>
        public class RDPConnection : RDPSlaveModel
        {
            #region Locals
            internal int order;
            internal string name;
            internal string host;
            internal string userName;
            internal string password;
            internal string group;
            internal bool isDefault;
            #endregion
            
            #region Globals
            public int Order { get { return order; } set { order = value; NotifyPropertyChanged("Order"); } }
            public string Name { get { return name; } set { name = value;NotifyPropertyChanged("Name"); } }
            public string Host { get { return host; } set { host = value; NotifyPropertyChanged("Host"); } }
            public string UserName { get { return userName; } set { userName = value; NotifyPropertyChanged("UserName"); } }
            public string Password { get { return password; } set { password = value; NotifyPropertyChanged("Password"); } }
            public string Group { get { return group; } set { group = value; NotifyPropertyChanged("Group"); } }
            public bool IsDefault { get { return isDefault; } set { isDefault = value; NotifyPropertyChanged("IsDefault"); } }
            #endregion

            #region Constructors
            public RDPConnection() { }
            public RDPConnection(int order) : this(order, "", "", "", "", "") { }
            public RDPConnection(int order, string name, string host, string username, string password) : this(order,name, host,username,password,"") { }
            public RDPConnection(int order, string name, string host, string username, string password, string group) : this(order,name, host, username, password, group, false) { }
            public RDPConnection(int order, string name, string host, string username, string password, string group, bool isdefault)
            {
                this.name = name;
                this.host = host;
                this.userName = username;
                this.password = password;
                this.group = group;
                this.isDefault = isdefault;
            }
            #endregion

            #region Methods
            public void StartSession()
            {
                StartRDPSession(this.host, this.userName, this.password);
            }
            #endregion
        }

        #region Static Methods
        /// <summary>
        /// starts a rdp session with provided RDPConnection
        /// </summary>
        /// <param name="rdp">RDPConnection to connect to</param>
        public static void StartRDPSession(RDPConnection rdp)
        {
            StartRDPSession(rdp.Host, rdp.UserName, rdp.Password);
        }
        /// <summary>
        /// starts a rdp session with provided credentials
        /// </summary>
        /// <param name="serverIP">name or ip of rdp host</param>
        /// <param name="userName">username for rdp session</param>
        /// <param name="password">password for rdp session</param>
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
        #endregion
    }
}
