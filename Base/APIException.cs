using System;
using System.Runtime.Serialization;

namespace Achievement.Base
{
    [Serializable]
    public class APIException : Exception, ISerializable
    {
        public APIException() : base("Unkown Exception") { }
        public APIException(string error_text) : base(error_text) { }

        [Serializable]
        /// <summary>
        /// API Key失效
        /// </summary>
        public class APIUnValidException : APIException { public APIUnValidException() :base("APIKey失效") { } }
        /// <summary>
        /// 資料不存在
        /// </summary>
        public class APIDataNotFoundException : APIException { public APIDataNotFoundException() : base("資料不存在") { } }

    }
}