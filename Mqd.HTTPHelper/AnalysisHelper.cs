using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

namespace Mqd.HTTPHelper
{
    /// <summary>
    /// 解析助手
    /// </summary>
    internal class AnalysisHelper
    {
        /// <summary>
        /// IP正则表达式
        /// </summary>
        private const string _patternIP = @"((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)";

        /// <summary>
        /// 端口正则表达式
        /// </summary>
        private const string _patternPort = @":\d{1,5}";

        /// <summary>
        /// 域名正则表达式
        /// </summary>
        private const string _patternDomain = @"([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,6}";

        /// <summary>
        /// 解析IP
        /// </summary>
        /// <param name="url">地址</param>
        internal static IPPort AnalysisIPAndPort(string url)
        {
            IPPort data = new IPPort();
            string host = "";
            Regex reg = new Regex(_patternIP);
            Match match = reg.Match(url);
            bool appointPort = true;//url中是否指定端口
            int index = -1;//虚拟路径在url中起始索引
            if (match.Success)
            {
                //IP形式url
                data.IP = match.Value;
                host = data.IP;//IP作为主机
                index = match.Index + match.Value.Length;
                //匹配IP+端口
                reg = new Regex(_patternIP + _patternPort);
                match = reg.Match(url);
                if (match.Success)
                {
                    data.Port = Convert.ToInt32(match.Value.Substring(match.Value.IndexOf(":") + 1));
                    index += 1 + data.Port.ToString().Length;//: + 端口的总长度
                }
                else
                {
                    appointPort = false;
                }
            }
            else
            {
                //域名形式url
                reg = new Regex(_patternDomain);
                match = reg.Match(url);
                if (match.Success)
                {
                    host = match.Value;//域名作为主机
                    index = match.Index + match.Value.Length;
                    data.IP = AnalysisDomain(match.Value);
                    //匹配域名+端口
                    reg = new Regex(_patternDomain + _patternPort);
                    match = reg.Match(url);
                    if (match.Success)
                    {
                        data.Port = Convert.ToInt32(match.Value.Substring(match.Value.IndexOf(":") + 1));
                        index += 1 + data.Port.ToString().Length;//: + 端口的总长度
                    }
                    else
                    {
                        appointPort = false;
                    }
                }
            }

            //url中没有指定端口,采用默认端口
            if (!appointPort)
            {
                data.Port = 80;
                if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    data.Port = 443;
                }
                data.Host = host;
            }
            else
            {
                //url中指定了端口
                data.Host = string.Format("{0}:{1}", host, data.Port);
            }

            if (index < url.Length)
            {
                data.VirtualPath = url.Substring(index);
            }
            else
            {
                data.VirtualPath = "/";
            }

#if DEBUG
            Debug.WriteLine(string.Format("Mqd.HTTPHelper: 目标IP = {0} 目标端口 = {1}", data.IP, data.Port));
#endif
            return data;
        }

        /// <summary>
        /// 解析域名到IP
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns></returns>
        private static string AnalysisDomain(string domain)
        {
            string ip = "";

            //先从host文件匹配
            ip = AnalysisFromHostFile(domain);
            if (!string.IsNullOrEmpty(ip))
            {
                return ip;
            }

            string cmd = string.Format("nslookup {0}", domain);
            Process p = new Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = "cmd.exe";
            p.Start();
            p.StandardInput.WriteLine(cmd);
            p.StandardInput.WriteLine("exit");
            string result = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            p.Close();

            Regex reg = new Regex(string.Format(@"{0}\r\nAddress:\s\s{1}\r\n", domain, _patternIP));
            Match match = reg.Match(result);
            if (match.Success)
            {
                reg = new Regex(_patternIP);
                match = reg.Match(match.Value);
                if (match.Success)
                {
                    ip = match.Value;
                }
            }
            return ip;
        }

        /// <summary>
        /// 从本地host文件解析域名
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        private static string AnalysisFromHostFile(string domain)
        {
            string ip = "";
            string path = @"C:\Windows\System32\drivers\etc\hosts";
            if (File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                StreamReader sr = new StreamReader(fs);
                List<string> list = new List<string>();
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();
                    if (!line.StartsWith("#") && !string.IsNullOrEmpty(line))
                    {
                        //去除#以后的字符串(注释)
                        int index = line.IndexOf("#");
                        if (index > -1)
                        {
                            list.Add(line.Substring(0, index).Trim());
                        }
                        else
                        {
                            list.Add(line);
                        }
                    }
                }
                sr.Close();
                fs.Close();

                Regex reg = new Regex(_patternDomain);
                foreach (var item in list)
                {
                    Match match = reg.Match(item);
                    if (match.Success && match.Value.Equals(domain, StringComparison.OrdinalIgnoreCase))
                    {
                        ip = item.Substring(0, match.Index).Trim();
                        break;
                    }
                }
            }
            return ip;
        }
    }

    /// <summary>
    /// IP端口数据
    /// </summary>
    internal class IPPort
    {
        /// <summary>
        /// IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 主机
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 虚拟路径
        /// </summary>
        public string VirtualPath { get; set; }
    }
}
