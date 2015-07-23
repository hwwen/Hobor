using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace Hobor
{
    /// <summary>
    /// 資料庫執行元件
    /// </summary>
    public class DBHelper
    {
        /// <summary>
        /// Transaction是否被開啟
        /// </summary>
        private bool _isTran = false;
        /// <summary>
        /// Transaction是否被開啟
        /// </summary>
        public bool IsTran { get { return this._isTran; } }
        /// <summary>
        /// Command的TimeOut值
        /// </summary>
        private int commTimeOut = 60;

        /// <summary>
        /// 取得或設定CommandTimeOut
        /// </summary>        
        public int CommandTimeOut
        {
            get { return commTimeOut; }
            set { commTimeOut = (value >= 60 ? value : 60); }
        }

        /// <summary>
        /// 資料庫連線字串
        /// </summary>
        private string _connString;

        /// <summary>
        /// Dispose是否被調用
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Transaction物件
        /// </summary>
        private SqlTransaction Transaction;

        /// <summary>
        /// 資料庫連線物件
        /// </summary>
        public SqlConnection Connection { get; set; }

        #region Constuctor
        /// <summary>
        /// 
        /// </summary>
        public DBHelper()
        {
            try
            {
                InitiallConnectionString();
                this._isTran = false;
                this._disposed = false;
                this.commTimeOut = 60;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 解構函式
        /// </summary>
        ~DBHelper()
        {
            Dispose(false);
        }

        #endregion

        #region 回收資源
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    RollBackTransaction();

                    // 釋放托管資源
                    if (Transaction != null)
                    {
                        Transaction.Dispose();
                        Transaction = null;
                    }
                    if (Connection != null)
                    {
                        if (Connection.State != ConnectionState.Closed)
                        {
                            Connection.Close();
                        }

                        Connection.Dispose();
                        Connection = null;
                    }

                    this._isTran = false;
                }
                // 釋放非托管資源，如果disposing為false、
                // 只有托管資源被釋放
            }
            this._disposed = true;
        }

        #endregion
        
        /// <summary>
        /// 將Application或Session中的物件轉成資料庫連線物件
        /// </summary>
        private void InitiallConnectionString()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(GetConfig.GetDBConn()))
                    throw new Exception("資料庫連線設定[\"DbConnection\"]錯誤");

                this._connString = GetConfig.GetDBConn();
            }
            catch (Exception)
            {

            }
            finally
            {
            }
        }

        /// <summary>
        /// 取得資料庫Connection物件
        /// </summary>
        private SqlConnection GetDBConnection()
        {
            return new SqlConnection(this._connString);
        }

        /// <summary>
        /// 取得資料庫 DataAdapter 物件
        /// </summary>
        private SqlDataAdapter GetDbDataAdapter()
        {
            return new SqlDataAdapter();
        }

        /// <summary>
        /// 產生SQL參數
        /// </summary>
        /// <param name="paramName">參數名稱</param>
        /// <param name="paramValue">參數值</param>
        /// <returns>參數物件</returns>
        public DbParameter CreateParameter(string paramName, object paramValue)
        {
            return new SqlParameter(paramName, paramValue);
        }

        /// <summary>
        /// 產生SQL參數
        /// </summary>
        /// <param name="paramName">參數名稱</param>
        /// <param name="paramType"></param>
        /// <returns>參數物件</returns>
        public DbParameter CreateParameter(string paramName, DbType paramType)
        {
            return new SqlParameter(paramName, paramType);
        }

        /// <summary>
        /// 產生SQL參數
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramType"></param>
        /// <param name="paramValue"></param>
        /// <returns></returns>
        public DbParameter CreateParameter(string paramName, DbType paramType, object paramValue)
        {
            SqlParameter param = new SqlParameter(paramName, paramType);
            param.Value = paramValue;
            return param;
        }

        #region 執行命令

        /// <summary>
        /// 執行指定指令並回傳影響結果數
        /// </summary>
        /// <param name="CommandText"></param>
        /// <param name="CommType"></param>
        /// <returns></returns>
        public int ExecuteNoneQuery(string CommandText, CommandType CommType = CommandType.Text)
        {
            if (this._isTran)
            {
                return ExecuteTranNoneQuery(CommandText, new List<Parameter>(), CommType);
            }
            else
            {
                return ExecuteNoTranNoneQuery(CommandText, new List<Parameter>(), CommType);
            }
        }

        /// <summary>
        /// 執行指定指令並回傳影響結果數
        /// </summary>
        /// <param name="CommandText"></param>
        /// <param name="Parameters"></param>
        /// <param name="CommType"></param>
        /// <returns></returns>
        public int ExecuteNoneQuery(string CommandText, List<Parameter> Parameters, CommandType CommType = CommandType.Text)
        {
            if (this._isTran)
            {
                return ExecuteTranNoneQuery(CommandText, Parameters, CommType);
            }
            else
            {
                return ExecuteNoTranNoneQuery(CommandText, Parameters, CommType);
            }
        }

        /// <summary>
        /// 執行交易指定指令並回傳影響結果數
        /// </summary>
        /// <param name="CommandText"></param>
        /// <param name="Parameters"></param>
        /// <param name="CommType"></param>
        /// <returns></returns>
        private int ExecuteTranNoneQuery(string CommandText, List<Parameter> Parameters, CommandType CommType)
        {
            try
            {
                using (SqlCommand Comm = this.Transaction.Connection.CreateCommand())
                {
                    Comm.CommandText = CommandText;
                    Comm.CommandType = CommType;
                    Comm.CommandTimeout = this.commTimeOut;
                    Comm.Transaction = this.Transaction;

                    foreach (Parameter parameter in Parameters)
                    {
                        Comm.Parameters.AddWithValue(parameter.Name, parameter.Value);
                    }

                    int rows = 0;
                    rows = Comm.ExecuteNonQuery();
                    return rows;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }

        /// <summary>
        /// 執行無交易指定指令並回傳影響結果數
        /// </summary>
        /// <param name="CommandText"></param>
        /// <param name="Parameters"></param>
        /// <param name="CommType"></param>
        /// <returns></returns>
        private int ExecuteNoTranNoneQuery(string CommandText, List<Parameter> Parameters, CommandType CommType)
        {
            try
            {
                using (SqlConnection Conn = GetDBConnection())
                {
                    using (SqlCommand Comm = Conn.CreateCommand())
                    {
                        Comm.CommandText = CommandText;
                        Comm.CommandType = CommType;
                        Comm.CommandTimeout = this.commTimeOut;

                        foreach (Parameter parameter in Parameters)
                        {
                            Comm.Parameters.AddWithValue(parameter.Name, parameter.Value);
                        }

                        int rows = 0;

                        Conn.Open();
                        rows = Comm.ExecuteNonQuery();
                        return rows;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }


        #endregion

        #region 取得資料相關
        /// <summary>
        /// 取得依指定頁數區域資料的查詢字串
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="SortColumn">排序依據欄位</param>
        /// <param name="PageNumber">頁數</param>
        /// <param name="RowCountPerPage">每頁資料筆數</param>
        /// <returns></returns>
        public string GetQueryStringByPage(string CommandText, string SortColumn, int PageNumber, int RowCountPerPage)
        {
            int StartRowIndex = ((((PageNumber <= 0) ? 1 : PageNumber) - 1) * RowCountPerPage) + 1;//傳入分頁數若小於等於零,一律以第一頁計,將頁數減一後乘以每頁筆數加1,則為該頁的起始RowIndex
            return GetQueryStringByIndex(CommandText, SortColumn, StartRowIndex, RowCountPerPage);
        }

        /// <summary>
        /// 取得依指定RowIndex區域資料的查詢字串
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="SortColumn">排序依據欄位</param>
        /// <param name="StartRowIndex">開始的Index</param>
        /// <param name="RowCount">需要的資料筆數</param>
        /// <returns></returns>
        public string GetQueryStringByIndex(string CommandText, string SortColumn, int StartRowIndex, int RowCount)
        {
            return String.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY {1} ) AS RowNumber FROM ({0}) AS GIT2) AS GIT1 WHERE GIT1.RowNumber BETWEEN {2} AND {3}", CommandText, SortColumn, StartRowIndex, (StartRowIndex + RowCount));
        }

        #region GetData函式

        /// <summary>
        /// 取得資料,適用於不需要參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <returns></returns>
        public DataTable GetData(string CommandText)
        {
            return GetData(CommandText, CommandType.Text);
        }
        /// <summary>
        /// 取得資料,適用於不需要參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="CommType">命令型態</param>
        /// <returns></returns>
        public DataTable GetData(string CommandText, CommandType CommType = CommandType.Text)
        {
            if (this._isTran)
            {
                return GetTranData(CommandText, new List<Parameter>(), CommType);
            }
            else
            {
                return GetNoTranData(CommandText, new List<Parameter>(), CommType);
            }
        }

        /// <summary>
        /// 取得資料,適用於僅一個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameter">參數</param>
        /// <returns></returns>
        public DataTable GetData(string CommandText, Parameter Parameter)
        {
            return GetData(CommandText, Parameter, CommandType.Text);
        }
        /// <summary>
        /// 取得資料,適用於僅一個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameter">參數</param>
        /// <param name="CommType">命令型態</param>
        /// <returns></returns>
        public DataTable GetData(string CommandText, Parameter Parameter, CommandType CommType = CommandType.Text)
        {
            if (this._isTran)
            {
                return GetTranData(CommandText, new List<Parameter>() { Parameter }, CommType);
            }
            else
            {
                return GetNoTranData(CommandText, new List<Parameter>() { Parameter }, CommType);
            }
        }

        /// <summary>
        /// 取得資料,適用於多個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameters">參數陣列</param>
        /// <returns></returns>
        public DataTable GetData(string CommandText, List<Parameter> Parameters)
        {
            return GetData(CommandText, Parameters, CommandType.Text);
        }

        /// <summary>
        /// 取得資料,適用於多個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameters">參數陣列</param>
        /// <returns></returns>
        public DataSet GetDataToDataSet(string CommandText, List<Parameter> Parameters)
        {
            return GetDataToDataSet(CommandText, Parameters, CommandType.Text);
        }

        /// <summary>
        /// 取得資料,適用於多個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameters">參數陣列</param>
        /// <param name="CommType">命令型態</param>
        /// <returns></returns>
        public DataTable GetData(string CommandText, List<Parameter> Parameters, CommandType CommType = CommandType.Text)
        {
            if (this._isTran)
            {
                return GetTranData(CommandText, Parameters, CommType);
            }
            else
            {
                return GetNoTranData(CommandText, Parameters, CommType);
            }
        }

        /// <summary>
        /// 取得資料,適用於多個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameters">參數陣列</param>
        /// <param name="CommType">命令型態</param>
        /// <returns></returns>
        public DataSet GetDataToDataSet(string CommandText, List<Parameter> Parameters, CommandType CommType = CommandType.Text)
        {
            if (this._isTran)
            {
                return GetTranDataToDataSet(CommandText, Parameters, CommType);
            }
            else
            {
                return GetNoTranDataToDataSet(CommandText, Parameters, CommType);
            }
        }

        /// <summary>
        /// 取得無交易情況下資料,適用於多個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameters">參數陣列</param>
        /// <param name="CommType">命令型態</param>
        /// <returns></returns>
        private DataTable GetNoTranData(string CommandText, List<Parameter> Parameters, CommandType CommType)
        {
            try
            {
                using (SqlConnection Conn = GetDBConnection())
                {
                    using (SqlDataAdapter Adapter = GetDbDataAdapter())
                    {
                        using (SqlCommand Comm = Conn.CreateCommand())
                        {
                            Comm.CommandText = CommandText;
                            Comm.CommandType = CommType;
                            Comm.CommandTimeout = this.commTimeOut;

                            foreach (Parameter parameter in Parameters)
                            {
                                Comm.Parameters.AddWithValue(parameter.Name, parameter.Value);
                            }

                            Adapter.SelectCommand = Comm;
                            DataTable DT = new DataTable();
                            Adapter.Fill(DT);
                            return DT;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        /// <summary>
        /// 取得交易情況下資料,適用於多個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameters">參數陣列</param>
        /// <param name="CommType">命令型態</param>
        /// <returns></returns>
        private DataTable GetTranData(string CommandText, List<Parameter> Parameters, CommandType CommType)
        {
            try
            {
                using (SqlDataAdapter Adapter = GetDbDataAdapter())
                {
                    using (SqlCommand Comm = this.Transaction.Connection.CreateCommand())
                    {
                        Comm.CommandText = CommandText;
                        Comm.CommandType = CommType;
                        Comm.CommandTimeout = this.commTimeOut;
                        Comm.Transaction = this.Transaction;

                        foreach (Parameter parameter in Parameters)
                        {
                            Comm.Parameters.AddWithValue(parameter.Name, parameter.Value);
                        }

                        Adapter.SelectCommand = Comm;
                        DataTable DT = new DataTable();
                        Adapter.Fill(DT);
                        return DT;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        /// <summary>
        /// 取得無交易情況下資料,適用於多個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameters">參數陣列</param>
        /// <param name="CommType">命令型態</param>
        /// <returns>回傳DataSet</returns>
        private DataSet GetNoTranDataToDataSet(string CommandText, List<Parameter> Parameters, CommandType CommType)
        {
            try
            {
                using (SqlConnection Conn = GetDBConnection())
                {
                    using (SqlDataAdapter Adapter = GetDbDataAdapter())
                    {
                        using (SqlCommand Comm = Conn.CreateCommand())
                        {
                            Comm.CommandText = CommandText;
                            Comm.CommandType = CommType;
                            Comm.CommandTimeout = this.commTimeOut;

                            foreach (Parameter parameter in Parameters)
                            {
                                Comm.Parameters.AddWithValue(parameter.Name, parameter.Value);
                            }

                            Adapter.SelectCommand = Comm;
                            DataSet DS = new DataSet();
                            Adapter.Fill(DS);
                            return DS;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        /// <summary>
        /// 取得交易情況下資料,適用於多個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameters">參數陣列</param>
        /// <param name="CommType">命令型態</param>
        /// <returns>回傳DataSet</returns>
        private DataSet GetTranDataToDataSet(string CommandText, List<Parameter> Parameters, CommandType CommType)
        {
            try
            {
                using (SqlDataAdapter Adapter = GetDbDataAdapter())
                {
                    using (SqlCommand Comm = this.Transaction.Connection.CreateCommand())
                    {
                        Comm.CommandText = CommandText;
                        Comm.CommandType = CommType;
                        Comm.CommandTimeout = this.commTimeOut;
                        Comm.Transaction = this.Transaction;

                        foreach (Parameter parameter in Parameters)
                        {
                            Comm.Parameters.AddWithValue(parameter.Name, parameter.Value);
                        }

                        Adapter.SelectCommand = Comm;
                        DataSet DS = new DataSet();
                        Adapter.Fill(DS);
                        return DS;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        #endregion

        #region GetFirstData函式

        /// <summary>
        /// 取得第一筆資料,適用於不需參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="CommType">命令型態</param>
        /// <returns></returns>
        public object GetFirstData(string CommandText, CommandType CommType = CommandType.Text)
        {
            if (this._isTran)
            {
                return GetTranFirstData(CommandText, new List<Parameter>(), CommType);
            }
            else
            {
                return GetNoTranFirstData(CommandText, new List<Parameter>(), CommType);
            }
        }
        /// <summary>
        /// 取得第一筆資料,適用於僅一個參數
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameter">參數</param>
        /// <param name="CommType">命令型態</param>
        /// <returns></returns>
        public object GetFirstData(string CommandText, Parameter Parameter, CommandType CommType = CommandType.Text)
        {
            if (this._isTran)
            {
                return GetTranFirstData(CommandText, new List<Parameter>() { Parameter }, CommType);
            }
            else
            {
                return GetNoTranFirstData(CommandText, new List<Parameter>() { Parameter }, CommType);
            }
        }

        /// <summary>
        /// 取得第一筆資料,適用於多個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameters">參數陣列</param>
        /// <param name="CommType">命令型態</param>
        /// <returns></returns>
        public object GetFirstData(string CommandText, List<Parameter> Parameters, CommandType CommType = CommandType.Text)
        {
            if (this._isTran)
            {
                return GetTranFirstData(CommandText, Parameters, CommType);
            }
            else
            {
                return GetNoTranFirstData(CommandText, Parameters, CommType);
            }
        }

        /// <summary>
        /// 取得交易的第一筆資料,適用於多個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameters">參數陣列</param>
        /// <param name="CommType">命令型態</param>
        /// <returns></returns>
        private object GetTranFirstData(string CommandText, List<Parameter> Parameters, CommandType CommType)
        {
            try
            {
                using (SqlCommand Comm = this.Transaction.Connection.CreateCommand())
                {
                    Comm.CommandText = CommandText;
                    Comm.CommandType = CommType;
                    Comm.CommandTimeout = this.commTimeOut;
                    Comm.Transaction = this.Transaction;

                    foreach (Parameter parameter in Parameters)
                    {
                        Comm.Parameters.AddWithValue(parameter.Name, parameter.Value);
                    }

                    return Comm.ExecuteScalar();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        /// <summary>
        /// 取得無交易的第一筆資料,適用於多個參數的查詢
        /// </summary>
        /// <param name="CommandText">查詢字串</param>
        /// <param name="Parameters">參數陣列</param>
        /// <param name="CommType">命令型態</param>
        /// <returns></returns>
        private object GetNoTranFirstData(string CommandText, List<Parameter> Parameters, CommandType CommType)
        {
            try
            {
                using (SqlConnection Conn = GetDBConnection())
                {
                    using (SqlCommand Comm = Conn.CreateCommand())
                    {
                        Comm.CommandText = CommandText;
                        Comm.CommandType = CommType;
                        Comm.CommandTimeout = this.commTimeOut;

                        foreach (Parameter parameter in Parameters)
                        {
                            Comm.Parameters.AddWithValue(parameter.Name, parameter.Value);
                        }

                        Conn.Open();
                        return Comm.ExecuteScalar();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }
        #endregion

        #endregion

        #region Transaction 相關

        /// <summary>
        /// 開始交易
        /// </summary>
        public void BeginTransaction()
        {
            try
            {
                if (!(this._isTran))
                {
                    Connection = new SqlConnection(this._connString);
                    Connection.Open();
                    Transaction = Connection.BeginTransaction();
                    this._isTran = true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }
        }

        /// <summary>
        /// 同意交易
        /// </summary>
        public void CommitTransaction()
        {
            EndTransaction(true);
        }

        /// <summary>
        /// 取消交易
        /// </summary>
        public void RollBackTransaction()
        {
            EndTransaction(false);
        }

        /// <summary>
        /// 資料交易狀態結束
        /// </summary>
        /// <param name="isCommit">是否同意完成交易</param>
        private void EndTransaction(bool isCommit)
        {
            if (this._isTran)
            {
                try
                {
                    if (isCommit)
                    {
                        Transaction.Commit();
                    }
                    else
                    {
                        Transaction.Rollback();
                    }

                    if (Connection.State != ConnectionState.Closed)
                    {
                        Connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    this._isTran = false;

                    if (Transaction != null)
                    {
                        Transaction.Dispose();
                        Transaction = null;
                    }
                    if (Connection != null)
                    {
                        if (Connection.State != ConnectionState.Closed)
                        {
                            Connection.Close();
                        }
                        Connection.Dispose();
                        Connection = null;
                    }
                }
            }
        }

        #endregion
    }
}