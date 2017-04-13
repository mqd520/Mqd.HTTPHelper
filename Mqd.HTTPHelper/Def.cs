using System;

/// <summary>
/// Mqd.HTTPHelper
/// </summary>
namespace Mqd.HTTPHelper
{
    /// <summary>
    /// 协议类型
    /// </summary>
    public enum EType
    {
        /// <summary>
        /// HTTP
        /// </summary>
        HTTP = 1,

        /// <summary>
        /// HTTPS
        /// </summary>
        HTTPS = 2
    }

    /// <summary>
    /// 协议版本
    /// </summary>
    public enum EVersion
    {
        /// <summary>
        /// HTTP 1.0
        /// </summary>
        HTTP_1 = 1,

        /// <summary>
        /// HTTP 1.1
        /// </summary>
        HTTP_11 = 2
    }

    public class HTTPMethod
    {

        public const string Post = "POST";

        public const string Get = "GET";
    }
}
