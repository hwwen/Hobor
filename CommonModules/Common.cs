using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;

namespace Hobor
{
    public class Common
    {
        /// <summary>
        /// SHA512加密
        /// </summary>
        /// <param name="value">要加密字串</param>
        /// <returns></returns>
        public static string SHA512Encoder(string value)
        {

            byte[] Key = { 123, 45, 67, 8, 90, 12, 34, 5 };//演算法金鑰
            try
            {
                return Convert.ToBase64String(new HMACSHA512(Key).ComputeHash(Encoding.UTF8.GetBytes(value)));
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 將物件轉換為INT16(Short)型態,若發生錯誤或null時,則回傳0
        /// </summary>
        /// <param name="obj">轉換物件</param>
        /// <returns></returns>
        public static Int16 ConvertToShort(object obj)
        {
            Int16 result = 0;
            if (obj == null) return result;
            try
            {
                result = Convert.ToInt16(obj);
            }
            catch (Exception)
            {
            }
            return result;
        }
        /// <summary>
        /// 將物件轉換為INT32(Int)型態,若發生錯誤或null時,則回傳0
        /// </summary>
        /// <param name="obj">轉換物件</param>
        /// <returns></returns>
        public static Int32 ConvertToInt(object obj)
        {
            Int32 result = 0;
            if (obj == null) return result;
            try
            {
                result = Convert.ToInt32(obj);
            }
            catch (Exception)
            {
            }
            return result;
        }

        public static double ConvertToDouble(object obj)
        {
            double result = 0;
            if (obj == null) return result;
            try
            {
                result = Convert.ToDouble(obj);
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
}