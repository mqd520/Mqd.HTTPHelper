using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// Mqd.HTTPHelper
/// </summary>
namespace Mqd.HTTPHelper
{
    /// <summary>
    /// HTTP助手
    /// </summary>
    public class HTTP1
    {
        /// <summary>
        /// 回车换行符
        /// </summary>
        private const string CRLF = "\r\n";
        /// <summary>
        /// HTTP协议标识
        /// </summary>
        private const string _http = "HTTP";
        /// <summary>
        /// 默认字符编码
        /// </summary>
        private readonly Encoding _encode;
        /// <summary>
        /// 请求方法
        /// </summary>
        private readonly string _method;
        /// <summary>
        /// 请求的虚拟路径
        /// </summary>
        private readonly string _virPath;
        /// <summary>
        /// 协议版本
        /// </summary>
        private readonly int _version;
        /// <summary>
        /// 协议类型HTTP/HTTPS
        /// </summary>
        private readonly int _type;
        /// <summary>
        /// 随机数
        /// </summary>
        private readonly string _boudary;
        /// <summary>
        /// 请求头数据
        /// </summary>
        private Dictionary<string, string> _requestHeadData = new Dictionary<string, string>();
        /// <summary>
        /// 表单数据
        /// </summary>
        private Dictionary<string, string> _formData = new Dictionary<string, string>();
        /// <summary>
        /// 目标IP
        /// </summary>
        private string _destIP;
        /// <summary>
        /// 目标端口
        /// </summary>
        private int _destPort = 0;

        /// <summary>
        /// 请求行协议头字节总长度
        /// </summary>
        private int _reqAndHeadLen = 0;
        /// <summary>
        /// 协议体长度
        /// </summary>
        private int _contentLen = 0;
        private List<byte> _buffer = new List<byte>();


        public HTTP1(string url, Encoding encode)
        {
            _encode = encode;
            _method = "GET";
            _type = (int)EType.HTTP;
            _version = (int)EVersion.HTTP_11;
            IPPort data = AnalysisHelper.AnalysisIPAndPort(url);
            _destIP = data.IP;
            _destPort = data.Port;
            _virPath = data.VirtualPath;
            _boudary = DateTime.Now.Ticks.ToString("x");
            _requestHeadData.Add("Host", data.Host);
            _requestHeadData.Add("User-Agent", "Mozilla / 5.0(Windows NT 10.0; WOW64; rv: 49.0) Gecko / 20100101 Firefox / 49.0");
            _requestHeadData.Add("Accept-Language", "zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
            _requestHeadData.Add("Accept-Encoding", "gzip, deflate");
            _requestHeadData.Add("Content-Type", "text/html");
            //_requestHeadData.Add("Connection", "Keep-Alive");
            //_requestHeadData.Add("Proxy-Connection", "Keep-Alive");
            PacketData();
        }

        /// <summary>
        /// 获取协议类型字符串
        /// </summary>
        /// <returns></returns>
        private string getTypeStr()
        {
            string type = "";
            if (_type == (int)EType.HTTP)
            {
                type = "HTTP";
            }
            else if (_type == (int)EType.HTTPS)
            {
                type = "HTTPS";
            }
            return type;
        }

        /// <summary>
        /// 获取协议版本字符串
        /// </summary>
        /// <returns></returns>
        private string getVerStr()
        {
            string version = "";
            if (_version == (int)EVersion.HTTP_1)
            {
                version = "1.0";
            }
            else if (_version == (int)EVersion.HTTP_11)
            {
                version = "1.1";
            }
            return version;
        }

        /// <summary>
        /// 封包
        /// </summary>
        private void PacketData()
        {
            // 创建请求行数据
            string head = string.Format("{0} {1} {2}/{3}{4}", _method, _virPath, getTypeStr(), getVerStr(), CRLF);
            byte[] buffer = _encode.GetBytes(head);
            _buffer.AddRange(buffer);

            // 创建协议体数据
            List<byte> list = new List<byte>();
            if (_method.Equals(HTTPMethod.Post, StringComparison.OrdinalIgnoreCase) && _formData.Count() > 0)
            {
                foreach (var item in _formData)
                {
                    list.AddRange(_encode.GetBytes(string.Format("--{0}{1}", _boudary, CRLF)));
                    list.AddRange(_encode.GetBytes(string.Format("Content-Disposition:form-data; name={0}{1}", item.Key, CRLF)));
                    list.AddRange(_encode.GetBytes(string.Format("{0}", CRLF)));
                    list.AddRange(_encode.GetBytes(string.Format("{0}{1}", item.Value, CRLF)));
                }
            }

            // 创建协议头数据
            foreach (var item in _requestHeadData)
            {
                _buffer.AddRange(_encode.GetBytes(string.Format("{0}: {1}{2}", item.Key, item.Value, CRLF)));
            }
            _buffer.AddRange(_encode.GetBytes(CRLF));
            if (_method.Equals(HTTPMethod.Get, StringComparison.OrdinalIgnoreCase))
            {
                _requestHeadData.Add("Content-Length", list.Count.ToString());
            }

            if (_method.Equals(HTTPMethod.Post, StringComparison.OrdinalIgnoreCase))
            {
                _buffer.AddRange(list.ToArray());
            }
        }

        public byte[] GetRequestData()
        {
            return _buffer.ToArray();
        }

        public Stream Send()
        {
            //TcpClient client = new TcpClient();
            //IPAddress ip = IPAddress.Parse(_destIP);
            //client.Connect(ip, _destPort);
            //NetworkStream ns = client.GetStream();
            //byte[] data = GetRequestData();
            //ns.Write(data, 0, data.Length);
            //MemoryStream ms = new MemoryStream();
            //byte[] buffer = new byte[1000];
            //while (true)
            //{
            //    int len = ns.Read(buffer, 0, buffer.Length);
            //    if (len > 0)
            //    {
            //        ms.Write(buffer, 0, len);
            //    }
            //    if (len < buffer.Length)
            //    {
            //        break;
            //    }
            //}
            //ms.Position = 0;
            //ns.Close();
            //client.Close();
            //return ms;

            TcpClient client = new TcpClient();
            IPAddress ip = IPAddress.Parse(_destIP);
            client.Connect(ip, _destPort);
            NetworkStream ns = client.GetStream();
            byte[] data = GetRequestData();
            ns.Write(data, 0, data.Length);
            MemoryStream ms = new MemoryStream();
            byte[] buffer = new byte[10000];
            int totalByte = 0;//需要总字节数
            int recvByte = 0;//已接收的总字节数
            while (true)
            {
                int len = ns.Read(buffer, 0, buffer.Length);
                Console.WriteLine("收到: " + len);
                if (len > 0)
                {
                    recvByte += len;
                    ms.Write(buffer, 0, len);
                    if (totalByte == 0)
                    {
                        totalByte = GetTotalByte(buffer);
                        Console.WriteLine("需要接收: " + totalByte);
                    }
                }
                if (recvByte == totalByte)
                {
                    Console.WriteLine("一共收到：" + totalByte);
                    break;
                }
            }
            ms.Position = 0;
            ns.Close();
            client.Close();
            return ms;
        }

        /// <summary>
        /// 计算需要接收的总字节数
        /// </summary>
        /// <param name="data">收到HTTP请求的第一段报文数据</param>
        /// <returns></returns>
        private int GetTotalByte(byte[] data)
        {
            int count = 0;
            string str = Encoding.Default.GetString(data);
            Regex reg = new Regex(@"Content-Length:\s?\d+\r\n", RegexOptions.IgnoreCase);
            Match match = reg.Match(str);
            bool chunked = false;
            int chunkLen = 0;
            if (match.Success)
            {
                _contentLen = Convert.ToInt32(match.Value.Substring(15).Trim());
            }
            else
            {
                reg = new Regex(@"Transfer-Encoding:\s?chunked", RegexOptions.IgnoreCase);
                match = reg.Match(str);
                if (match.Success)
                {
                    chunked = true;
                    reg = new Regex(@"\r\n\r\n\w{1,5}\r\n");
                    match = reg.Match(str);
                    if (match.Success)
                    {
                        _contentLen = Convert.ToInt32(match.Value.Trim(), 16);
                        chunkLen = match.Value.Trim().Length + 2;
                    }
                }
            }

            reg = new Regex(@"\r\n\r\n");
            match = reg.Match(str);
            if (match.Success)
            {
                _reqAndHeadLen = Encoding.Default.GetBytes(str.Substring(0, match.Index)).Length + 4;
                if (chunked)
                {
                    _reqAndHeadLen += chunkLen;
                }
            }

            count = _reqAndHeadLen + _contentLen;
            return count;
        }
    }
}
