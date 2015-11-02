using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;
using Starksoft.Aspen.Proxy;

namespace TorConnection__GET_and_POST_
{
   
    internal class TORLibrary
    {
        internal static TorClient Tor_obj;
        internal static List<TorProxy> proxy_socks;

        public delegate void DelegateLibraryInputMessage(string message);
        public event DelegateLibraryInputMessage LibraryInputMessage;
        private int index = -1;
        public TORLibrary()
        {
            if (Tor_obj == null || proxy_socks == null)
            {
                if (Tor_obj == null)
                {
                    Tor_obj = TorClient.Instance;
                    Tor_obj.InpuClientMessage += Tor_obj_InputMessage;
                    Tor_obj.tor_start();
                }
                if (proxy_socks == null)
                {
                    proxy_socks = new List<TorProxy>();
                }
            }
        }

        public int addTor(string type, string host,int port)
        {
            
            if (type == "proxy")
            {
                proxy_socks.Add(new TorProxy(host, port, "127.0.0.1", 9050, ProxyType.Socks4a));
                index = proxy_socks.Count() - 1;
               proxy_socks[index].InputProxyMessage += ProxySocks;
                proxy_socks[index].Connect();
                return index;
            }
            return index;
        }

        void ProxySocks(string message)
        {
            if (LibraryInputMessage != null) LibraryInputMessage.Invoke(message); 
        }
        void Tor_obj_InputMessage(string message)
        {
            if (LibraryInputMessage != null) LibraryInputMessage.Invoke(message); 
        }
    }

    internal class TorProxy
    {
       

        public struct connection
        {
            internal TcpClient client;
            internal IProxyClient Proxy;
            public ProxyType proxy_type;

            public string proxy_host;
            public string ProxyHost
            {
                get { return proxy_host; }
                set { proxy_host = value; }
            }
            public int proxy_port;
            public int ProxyPort
            {
                get { return proxy_port; }
                set { proxy_port = value; }
            }
            public string target_host;

            public string TargetHost
            {
                get { return target_host; }
                set { target_host = value; }
            }
            public int target_port;
            public int TargetPort
            {
                get { return target_port; }
                set { target_port = value; }
            }

            public void proxy_config(string target_host, int target_port, string proxy_host, int proxy_port, ProxyType type)
            {
                this.target_host = target_host; this.target_port = target_port;
                this.proxy_host = proxy_host; this.proxy_port = proxy_port;
                this.proxy_type = type;
            }
        }

        public connection proxy_connection;

        public delegate void DelegateProxyInputMessage(string message);
        public event DelegateProxyInputMessage InputProxyMessage;

        public TorProxy(string target_host, int target_port, string proxy_host, int proxy_port, ProxyType type)
        {
            if (InputProxyMessage != null) InputProxyMessage.Invoke("Socket allocated.");
            proxy_connection = new connection();
            proxy_connection.proxy_config(target_host, target_port, proxy_host, proxy_port, type);
            ProxyClientFactory factory = new ProxyClientFactory();
            proxy_connection.Proxy = factory.CreateProxyClient(type, proxy_host, proxy_port);
            proxy_connection.Proxy.CreateConnectionAsyncCompleted += new EventHandler<CreateConnectionAsyncCompletedEventArgs>(proxy_connected);
        }

        internal void Connect()
        {
            proxy_connection.Proxy.CreateConnectionAsync(proxy_connection.target_host, proxy_connection.target_port);
        }

        private void proxy_connected(object sender, CreateConnectionAsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (InputProxyMessage != null) InputProxyMessage.Invoke("Connection Error!");
                if (InputProxyMessage != null) InputProxyMessage.Invoke(e.Error.Message);
                return;
            }
            else if (e.Error == null)
            {
                if (InputProxyMessage != null) InputProxyMessage.Invoke("Connected to Tor!");
                this.proxy_connection.client = e.ProxyConnection;
                if (InputProxyMessage != null) InputProxyMessage.Invoke("Proxy referenced.");
            }
        }

        public void send(string data)
        {
            proxy_connection.client.Client.Send(ASCIIEncoding.ASCII.GetBytes(data));
        }

        public string receive()
        {

            byte[] bytes = new byte[proxy_connection.client.ReceiveBufferSize];
            NetworkStream netStream = proxy_connection.client.GetStream();
            Thread.Sleep(1000);
            if (InputProxyMessage != null) InputProxyMessage.Invoke("Finalizing Data 50%..");
            netStream.Read(bytes, 0, (int)proxy_connection.client.ReceiveBufferSize);
            Thread.Sleep(1000);
            if (InputProxyMessage != null) InputProxyMessage.Invoke("Finalizing Data 100%..");
            return Encoding.UTF8.GetString(bytes);
        }
    }

    internal class TorClient
    {

        public delegate void DelegateClientInputMessage(string message);
        public event DelegateClientInputMessage InpuClientMessage;

        private static TorClient instance;

        private Process tor;

        public TorClient() { }

        internal static TorClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TorClient();
                }
                return instance;
            }
        }

        public bool IsProcessOpen(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Contains(name))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        internal void tor_start()
        {
            if (IsProcessOpen("tor"))
            {
                if (InpuClientMessage != null) InpuClientMessage.Invoke("A Tor process already exists.");
                tor_stop();
               
            }
            else
            {
                if (InpuClientMessage != null) InpuClientMessage.Invoke("Tor init...");
                this.tor = new Process();
                this.tor.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\tor.exe";

                if (InpuClientMessage != null) InpuClientMessage.Invoke("Loaded file " + Directory.GetCurrentDirectory());
                this.tor.StartInfo.CreateNoWindow = false;
                this.tor.StartInfo.UseShellExecute = false;
                this.tor.StartInfo.RedirectStandardOutput = true;
                this.tor.StartInfo.RedirectStandardInput = true;
                this.tor.StartInfo.RedirectStandardError = true;
                this.tor.OutputDataReceived += new DataReceivedEventHandler((sender, args) => { stdout__tor(sender, args); });
                this.tor.ErrorDataReceived += new DataReceivedEventHandler((sender, args) => { stderr__tor(sender, args); });
                this.tor.Start();
                Console.WriteLine("Strapping input...");
                this.tor.BeginOutputReadLine();
                this.tor.BeginErrorReadLine();
            }
        }

        internal void tor_stop()
        {
            if (InpuClientMessage != null) InpuClientMessage.Invoke("Attempting to open another Tor process.");
            Process[] processes = Process.GetProcessesByName("tor");
            if (processes.Length > 0)
                processes[0].Kill();

        }

        internal void tor_restart()
        {
            tor_stop();
            tor_start();
        }

        private void stdout__tor(object sender, DataReceivedEventArgs pipe)
        {
            try
            {
                if (pipe.Data.Contains("Bootstrapped 100%: Done."))
                {
                    if (InpuClientMessage != null) InpuClientMessage.Invoke("Tor had been initialized.");
                }
                if (InpuClientMessage != null) InpuClientMessage.Invoke(pipe.Data);
            }
            catch
            {
                tor_restart();
            }
        }

        private void stderr__tor(object sender, DataReceivedEventArgs pipe)
        {
            if (InpuClientMessage != null) InpuClientMessage.Invoke("[Erroe]: " + pipe.Data);
        }
    }
}
