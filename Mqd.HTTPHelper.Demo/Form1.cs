using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mqd.HTTPHelper;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Mqd.HTTPHelper.Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = "127.0.0.1";
            //url = "127.0.0.1/";
            //url = "127.0.0.1/iisatart.htm";
            //url = "127.0.0.1:80";
            //url = "127.0.0.1:80/";
            //url = "127.0.0.1:80/iisatart.htm";
            //url = "http://127.0.0.1";
            //url = "http://127.0.0.1/";
            //url = "http://127.0.0.1/iisatart.htm";
            //url = "http://127.0.0.1:80";
            //url = "http://127.0.0.1:80/";
            //url = "http://127.0.0.1:80/iisatart.htm";

            //url = "www.baidu.com";
            //url = "www.baidu.com/";
            //url = "www.baidu.com/index.html";
            //url = "http://www.baidu.com";
            //url = "http://www.baidu.com/";
            //url = "http://www.baidu.com/index.html";
            //url = "http://www.baidu.com:80";
            //url = "http://www.baidu.com:80/";
            //url = "http://www.baidu.com:80/index.html";
            //url = "www.baidu.com:80";
            //url = "www.baidu.com:80/";
            //url = "www.baidu.com:80/index.html";

            //url = "bj.meituan.com";
            url = "http://bbs.csdn.net/topics/380243447";
            //HTTP http = new HTTP(url, System.Text.Encoding.Default);
            //Stream s = http.Send();
            //StreamReader sr = new StreamReader(s);
            //string response = sr.ReadToEnd();
            //Console.WriteLine(response);

            HttpWebRequest wq = (HttpWebRequest)WebRequest.Create(url);
            WebResponse response = wq.GetResponse();
            Stream s1 = response.GetResponseStream();
            //byte[] buffer=new byte[100000];
            //s1.Read(buffer, 0, buffer.Length);
            StreamReader sr = new StreamReader(s1);
            string result = sr.ReadToEnd();
        }
    }
}
