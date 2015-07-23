using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;

namespace Hobor.Base
{
    public class BaseController : System.Web.Mvc.Controller
    {
        /// <summary>
        /// 資料庫執行元件
        /// </summary>
        internal Hobor.DBHelper MyHelper { get; set; }
        public BaseController()
        {
            MyHelper = new DBHelper();
        }
    }
}
