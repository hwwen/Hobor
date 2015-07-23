using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hobor
{
    /// <summary>
    /// 取得Web.Config內容
    /// </summary>
    public static class GetConfig
    {
        /// <summary>
        /// web.config["DBConn"]資料庫連線設定,錯誤時回傳空白
        /// </summary>
        /// <returns></returns>
        public static string GetDBConn()
        {
            try
            {
                return System.Configuration.ConfigurationManager.AppSettings["DBConn"];
            }
            catch (Exception)
            {
                return "";
            }
        }
        /// <summary>
        ///  web.config["PageSize"]預設分頁筆數,錯誤時回傳5
        /// </summary>
        /// <returns></returns>
        public static int GetPageSize()
        {
            try
            {
                return Convert.ToInt32( System.Configuration.ConfigurationManager.AppSettings["PageSize"]);
            }
            catch (Exception)
            {
                return 5;
            }
        }
        /// <summary>
        ///  web.config["AllowRange"]預設座標誤差範圍,錯誤時回傳0.005
        /// </summary>
        /// <returns></returns>
        public static double GetAllowRange()
        {
            try
            {
                return Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["AllowRange"]);
            }
            catch (Exception)
            {
                return 0.005;
            }
        }

    }
}