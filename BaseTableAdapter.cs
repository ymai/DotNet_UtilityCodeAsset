using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using iAnywhere.Data.SQLAnywhere;
using Logging;
using System.Reflection;
using System.Configuration;
using System.Data.Common;
using System.Threading;

namespace UtilityCodeAsset.Database
{
    public abstract class BaseTableAdapter
    {
        protected static SAConnection conn = null;
        protected LogManager _log = LogManager.GetInstance();
        //protected PropertyInfo[] _props;
        protected SACommand _querycmd;
        protected SACommand _updatecmd;
        protected static SATransaction _trans;
        protected static object _translock = new object();
        protected static readonly string MissDBParam = "Missing database parameter \"SM_Remote_DB_Conn\" in configuration.";
        protected static readonly string WrongDBParam = "Fail connecting to database.";

        public static void DBConnect()
        {
            if (BaseTableAdapter.conn == null)
            {
                Logger filelogger = new FileLogger("DB");
                LogManager.GetInstance().AddLogger(filelogger, "DB", false);
                string connstring = ConfigurationManager.ConnectionStrings["SM_Remote_DB_Conn"].ConnectionString.Trim();
                if (connstring == null)
                {
                    System.Windows.Forms.MessageBox.Show(MissDBParam);
                    throw(new Exception(MissDBParam));
                }

                try
                {
                    BaseTableAdapter.conn = new SAConnection(ConfigurationManager.ConnectionStrings["SM_Remote_DB_Conn"].ConnectionString.Trim());
                    BaseTableAdapter.conn.Open();
                }
                catch (Exception e)
                {
                    string errstr = String.Format("{0} ConnectionString={1}", WrongDBParam, connstring);
                    System.Windows.Forms.MessageBox.Show(errstr);
                    throw (e);
                }
            }
        }

        public static void DBDisconnect()
        {
            if (BaseTableAdapter.conn != null)
            {
                BaseTableAdapter.conn.Close();
            }
        }

        protected BaseTableAdapter()
        {
           // _props = GetClassProperties();
        }
        protected abstract void UpdateParams(AbstractBusinessObject ado);

        public virtual int Update(AbstractBusinessObject ado, Type impltype)
        {
            if (!(ado.GetType() == impltype))
                return 0;
            _updatecmd.Parameters.Clear();
            UpdateParams(ado);
            return ExecuteNonQuery(_updatecmd);
        }
        //internal abstract PropertyInfo[] GetClassProperties();

        public virtual IList<AbstractBusinessObject> Query(string tablename, Type boType) //string sql, SAParameterCollection criteria, 
        {
            return Query(tablename, boType, _querycmd);
        }


        public virtual IList<AbstractBusinessObject> Query(string tablename, Type boType, SACommand cmd) //string sql, SAParameterCollection criteria, 
        {


            //SACommand objSql;
            //SADataReader dataReader;
            SADataAdapter adp = new SADataAdapter();

            DataSet ds;
            ArrayList abos =  new ArrayList();
            // Getting rowcount
            //sql = "SELECT count(*) as Count FROM "+tablename+" where UserID = @userid";
            //objSql = new SACommand(sql, conn);
            //objSql.Parameters.AddWithValue("@userid", UserID);
            //dataReader = ExecuteReader(objSql);
            //dataReader.Read();
            //int rowcount = dataReader.GetInt32(0);
            //dataReader.Close();

            //if (rowcount <= 0)
            //    return null;
            if (BaseTableAdapter._trans != null)
                cmd.Transaction = _trans;

            try
            {
                // filling the DataSet with the DataAdapter
                adp.TableMappings.Add("Table", tablename);
                //add Parameters

                cmd.CommandType = CommandType.Text;
                adp.SelectCommand = cmd;
                ds = new DataSet(tablename);
                lock(BaseTableAdapter.conn)
                    adp.Fill(ds);

                // Retrieve all the rows
                DataRowCollection rows = ds.Tables[0].Rows;
                if (rows.Count < 1)
                    return null;

                // Loop through the Columns in the DataSet and map that to the properties of the class
                IEnumerator columns = ds.Tables[0].Columns.GetEnumerator();
                DataColumn datacolumn;
                DataRow datarow;
                string cname;
                object cvalue;

                ConstructorInfo cons = boType.GetConstructor(Type.EmptyTypes);
                PropertyInfo[] props = boType.GetProperties();
                //( rows.Count
                _log.Log(String.Format("Querying cmd={0}", cmd.CommandText), "DB", 5 );
                for (int r = 0; r < rows.Count; r++)
                {
                    datarow = rows[r];
                    
                    AbstractBusinessObject curr = (AbstractBusinessObject)cons.Invoke(null);
                    columns.Reset();
                    while (columns.MoveNext())
                    {
                        try
                        {
                            datacolumn = (DataColumn)columns.Current;
                            cname = datacolumn.ColumnName;
                            for (int i = 0; i < props.Length; i++)
                            {
                                if (props[i].Name.ToLower() == cname.ToLower())
                                {
                                    cvalue = Convert.ChangeType(datarow[datacolumn], props[i].PropertyType);
                                    props[i].SetValue(curr, cvalue, null);
                                    _log.Log(String.Format("\tName={0} Value={1}", cname, cvalue), "DB", 5);
                                    break; // break for loop
                                }
                            }
                        }
                        catch (InvalidCastException ivce)
                        {
                            // go to next column
                        }
                    }
                    abos.Add(curr);
                }
            }
            catch (Exception ex)
            {
                string logoutput = String.Format("----Error in BaseTableAdapter, Query----\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
                _log.Log(logoutput, "DB", 3);
                _log.Log(logoutput);
            }
            finally
            {
                adp.Dispose();
            }

            AbstractBusinessObject[] rc = (AbstractBusinessObject[])abos.ToArray(boType);
            return rc;
        }


        public virtual IList<AbstractBusinessObject> Delete(string tablename, Type type, string criteria,
            Hashtable parameters)
        {
            SACommand selectcmd = new SACommand("SELECT * from " + tablename + " where " + criteria);
            SACommand deletecmd = new SACommand ("DELETE from " + tablename + " where " + criteria);
            selectcmd.Connection = conn;
            deletecmd.Connection = conn;
            
            //string name, value;
            if (parameters != null)
            {
                IDictionaryEnumerator ps = parameters.GetEnumerator();
                while (ps.MoveNext())
                {
                    if (ps.Key is string)
                    {
                        selectcmd.Parameters.Add((string)ps.Key, ps.Value);
                        deletecmd.Parameters.Add((string)ps.Key, ps.Value);
                    }
                }
            }
            //for (int i=0; i < parameters.Count; i++)
            //{
            //    name = parameters.GetKey(i);
            //    value = parameters[name];
            //    selectcmd.Parameters.Add(name, value);
            //    deletecmd.Parameters.Add(name, value);
            //}
            IList<AbstractBusinessObject> rc = Query(tablename, type, selectcmd);
            if (selectcmd != null)
                selectcmd.Dispose();
            
            ExecuteNonQuery(deletecmd);
            if (deletecmd != null) 
                deletecmd.Dispose();

            return rc;
        }

        protected int ExecuteNonQuery(SACommand cmd)
        {
            int rc = -1;
            int retries = -1;
            bool retryFlag = true;

            while (retryFlag && retries < 1200)
            {
                try
                {
                    lock (BaseTableAdapter.conn)
                    {
                        if (cmd.Connection.State == ConnectionState.Open)
                        {
                            _log.Log(String.Format("ExecuteNonQuery - {0}", cmd.CommandText), "DB", 5);
                            cmd.Transaction = _trans;
                            rc = cmd.ExecuteNonQuery();
                        }
                        else
                        {
                            _log.Log(String.Format("ExecuteNonQuery Error connection not open - {0}", cmd.CommandText), "DB", 5);
                        }
                    }
                    return rc;
                }
                catch (SAException ex)
                {
                    retryFlag = ShouldRetry(ex, cmd);
                    if (!retryFlag)
                    {
                        rc = -1;
                        throw(ex);
                    }
                    string logoutput = String.Format("----ExecuteNonQuery----\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
                    _log.Log(logoutput, "DB", 3);
                    _log.Log(logoutput);
                }
            }
            return rc;
        }

        protected SADataReader ExecuteReader(SACommand cmd)
        {
            SADataReader rc = null;
            int retries = -1;
            bool retryFlag = true;

            while (retryFlag && retries < 1200)
            {
                try
                {
                    lock (BaseTableAdapter.conn)
                    {
                        if (cmd.Connection.State == ConnectionState.Open)
                        {
                            _log.Log(String.Format("ExecuteReader - {0}", cmd.CommandText), "DB",  5);
                            cmd.Transaction = _trans;
                            rc = cmd.ExecuteReader();
                        }
                        else
                        {
                            _log.Log(String.Format("ExecuteReader Error connection not open - {0}", cmd.CommandText), "DB", 5);
                        }
                    }
                    return rc;
                }
                catch (SAException ex)
                {
                    retryFlag = ShouldRetry(ex, cmd);

                   string logoutput = String.Format("---- ExecuteReader ----\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
                    _log.Log(logoutput, "DB", 3);
                    _log.Log(logoutput);
                    //LogCommandText(cmd)
                    retries = retries + 1;
                }
            }
            return rc;
        }



        protected Object ExecuteScalar(SACommand cmd)
        {
            Object rc = null;
            int retries = -1;
            bool retryFlag = true;

            while (retryFlag && retries < 1200)
            {
                try
                {
                    lock (BaseTableAdapter.conn)
                    {
                        if (cmd.Connection.State == ConnectionState.Open)
                        {
                            _log.Log(String.Format("ExecuteScalar - {0}", cmd.CommandText), "DB", 5);
                            cmd.Transaction = _trans;
                            rc = cmd.ExecuteScalar();
                        }
                        else
                        {
                            _log.Log(String.Format("ExecuteScalar Error connection not open - {0}", cmd.CommandText), "DB", 5);
                        }
                    }
                    return rc;
                }
                catch (SAException ex)
                {
                    retryFlag = ShouldRetry(ex, cmd);

                    string logoutput = String.Format("---- ExecuteScalar ----\r\n{0}\r\n{1}", ex.Message, ex.StackTrace);
                    _log.Log(logoutput, "DB", 3);
                    _log.Log(logoutput);
                    retries = retries + 1;
                }

            }
            return rc;
        }


        /// <summary>
        /// TODO: fill in whether it should retry.
        /// 
        /// </summary>
        /// <param name="ulex"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected bool ShouldRetry(SAException ulex, SACommand cmd)
        {
            return false;
        }

        /// <summary>
        /// Start a transaction
        /// </summary>
        public static bool BeginTransaction()
        {
            Monitor.Enter(_translock);
            if (_trans == null && conn.State == ConnectionState.Open)
            {
                _trans = conn.BeginTransaction();
                return true;
            }
            Monitor.Exit(_translock);
            return false;
        }

        /// <summary>
        /// Start a transaction
        /// </summary>
        public static bool CommitTransaction()
        {
            bool rc = false;
            try
            {
                if (_trans != null)
                {
                    _trans.Commit();
                    _trans.Dispose();
                    _trans = null;
                    rc = true;
                }
                else
                {
                    throw (new Exception("Commit transaction before begin transaction."));
                }

                return rc;
            }
            finally
            {
                Monitor.Exit(_translock);
            }
        }


        ~BaseTableAdapter()
        {
            if (_querycmd != null)
                _querycmd.Dispose();
            if (_updatecmd != null)
                _updatecmd.Dispose();
        }
    }
}
