using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;

namespace TorConnection__GET_and_POST_
{
    public partial class Form1 : Form
    {
        Thread TorWorker;
        TORLibrary library;
        public Form1()
        {
            InitializeComponent();
           
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            TorWorker = new Thread(new ThreadStart(DoTor));
            TorWorker.Start();
        }

        void DoTor()
        {
            library = new TORLibrary();
            library.LibraryInputMessage += library_LibraryInputMessage;
            Thread.Sleep(20000);
            int connect_index = library.addTor("proxy", "app.swagbucks.com", 80);
            Thread.Sleep(10000);
            TORLibrary.proxy_socks[connect_index].send("GET /?cmd=apm-1&emailAddress=geekables@yahoo.com&pswd=Haha9406 HTTP/1.1\r\n" +
            "Host: app.swagbucks.com\r\n" +
            "User-Agent: Mozilla/5.0 (Windows NT 6.1; rv:31.0) Gecko/20100101 Firefox/31.0\r\n\r\n");
            Regex regex = new Regex("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}", RegexOptions.Multiline);
            var request = TORLibrary.proxy_socks[connect_index].receive();
            //textBox1.Invoke(new MethodInvoker(() => textBox1.Text = regex.Match(request).Groups[0].Value));
            textBox2.Invoke(new MethodInvoker(() => textBox2.Text = textBox2.Text + request + "\r\n"));
        }

        void library_LibraryInputMessage(string message)
        {
            textBox2.Invoke(new MethodInvoker(() => textBox2.Text = textBox2.Text + message + "\r\n"));
        }
    }

   
}
