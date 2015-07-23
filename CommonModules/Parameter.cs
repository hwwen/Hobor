using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hobor
{
    /// <summary>
    /// 參數
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// 參數
        /// </summary>
        public Parameter() { }
        /// <summary>
        /// 參數
        /// </summary>
        /// <param name="Name">參數名稱</param>
        /// <param name="Value">參數值</param>
        public Parameter(string Name, object Value) { this.Name = Name; this.Value = Value; }
        /// <summary>
        /// 參數名稱
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 參數值
        /// </summary>
        public object Value { get; set; }
    }
}