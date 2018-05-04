using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;

namespace Utils.Database
{
  public class DBHelper
  {

    #region Connection
    public string ConnectionString
    {
      get { return ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString; }
    }
    public OracleConnection CreateConnection()
    {
      return new OracleConnection(ConnectionString);
    }
    #endregion

    #region Insert
    public void Insert(string tableName, params object[] fieldValues)
    {
      Insert(tableName, FieldValueArrayToDictionary(fieldValues));
    }
    public void Insert(string tableName, Dictionary<string, object> parameters)
    {
      Insert(CurrentTransaction, tableName, parameters);
    }
    public void Insert(OracleTransaction trans, string tableName, Dictionary<string, object> parameters)
    {
      string fields = JoinParameters(parameters, delegate(KeyValuePair<string, object> item)
      {
        return item.Key;
      });
      string values = JoinParameters(parameters, delegate(KeyValuePair<string, object> item)
      {
        return ":" + item.Key;
      });
      string sql = string.Format("insert into {0} ({1}) values ({2})", tableName, fields, values); ;
      Executor(trans, sql, parameters).Execute();
    }
    #endregion

    #region Update
    public void SimpleUpdate(string tableName, params object[] fieldValues)
    {
      Update(tableName, FieldValueArrayToDictionary(fieldValues));
    }
    public void SimpleUpdateIF(string tableName, string idField, params object[] fieldValues)
    {
      Update(tableName, idField, FieldValueArrayToDictionary(fieldValues));
    }
    public void SimpleUpdateIV(string tableName, object idValue, params object[] fieldValues)
    {
      SimpleUpdateIFIV(tableName, "id", idValue, fieldValues);
    }
    public void SimpleUpdateIFIV(string tableName, string idField, object idValue, params object[] fieldValues)
    {
      Dictionary<string, object> parameters = FieldValueArrayToDictionary(fieldValues);
      parameters.Add(idField, idValue);
      Update(tableName, idField, parameters);
    }
    public void Update(string tableName, Dictionary<string, object> parameters)
    {
      Update(tableName, "ID", parameters);
    }
    public void Update(string tableName, string idField, Dictionary<string, object> parameters)
    {
      string strSet = JoinParameters(parameters, delegate(KeyValuePair<string, object> item)
      {
        if (item.Key == idField) return null;
        return item.Key + " = :" + item.Key;
      });
      string sql = string.Format("update {0} set {1} where {2}=:{2}", tableName, strSet, idField);
      Executor(CurrentTransaction, sql, parameters).Execute();
    }
    #endregion

    #region Delete
    public void Delete(string tableName, object id)
    {
      Delete(CurrentTransaction, tableName, id);
    }
    public void DeleteWhere(string tableName, string whereCondition)
    {
      Executor(string.Format("delete from {0} where {1}", tableName, whereCondition)).Execute();
    }
    public void Delete(OracleTransaction trans, string tableName, object id)
    {
      Executor(trans, string.Format("delete from {0} where id = :0", tableName), id).Execute();
    }
    #endregion

    #region Transaction
    OracleTransaction _transaction;
    public OracleTransaction CurrentTransaction
    {
      get { return _transaction; }
    }
    public void BeginTransaction()
    {
      OracleConnection conn = CreateConnection();
      conn.Open();
      _transaction = conn.BeginTransaction();
    }
    public void EndTransaction()
    {
      OracleConnection conn = _transaction.Connection;
      _transaction.Commit();
      _transaction = null;
      conn.Close();
    }
    public void Transaction(Action<OracleTransaction> execute)
    {
      BeginTransaction();
      execute(_transaction);
      EndTransaction();
    }
    #endregion

    #region Query Functions
    public void ReaderEach(string sql, Action<OracleDataReader> read)
    {
      Executor(sql).ReaderEach(read);
    }
    public DataTable GetTable(string sql)
    {
      return Executor(sql).GetTable();
    }
    public DataRow GetRow(string sql)
    {
      DataTable table = GetTable(sql);
      if (table.Rows.Count > 0) return table.Rows[0];
      return null;
    }
    public DataTable GetPageWithPageIndex(string sql, int index, int pageSize, out int itemCount)
    {
      return GetPage(sql, (index - 1) * pageSize, pageSize, out itemCount);
    }
    public DataTable GetPage(string sql, int index, int pageSize, out int itemCount)
    {
      itemCount = Count(sql);
      return GetPage(sql, index, pageSize);
    }
    public DataTable GetPageWithPageIndex(string sql, int index, int pageSize)
    {
      return GetPage(sql, (index - 1) * pageSize + 1, pageSize);
    }
    public DataTable GetPage(string sql, int index, int pageSize)
    {
      string newSql = string.Format(
        "select * from (select t.*, rownum as rn from ({0}) t where rownum < {2}) where rn >= {1}",
        sql, index, index + pageSize);
      return GetTable(newSql);
    }
    public int Count(string sql)
    {
      string newSql = string.Format("select count(*) from ({0})", sql);
      return int.Parse(Executor(newSql).GetScalar().ToString());
    }

    public DataRow GetRowById(string tableName, object idValue)
    {
      return GetRowById(tableName, "id", idValue);
    }
    public DataRow GetRowById(string tableName, string idFieldName, object idValue)
    {
      return Executor(string.Format("select * from ({0}) where {1} = :0", tableName, idFieldName), idValue).GetRow();
    }
    public object GetRowFieldValue(string selectSql, string fieldName)
    {
      DataRow row = Executor(selectSql).GetRow();
      if (row != null) return Executor(selectSql).GetRow()[fieldName];
      return null;
    }
    public object GetRowFieldValue(string tableName, object idValue, string fieldName)
    {
      return GetRowById(tableName, idValue)[fieldName];
    }
    public object GetRowFieldValue(string tableName, string idFieldName, object idValue, string fieldName)
    {
      return GetRowById(tableName, idFieldName, idValue)[fieldName];
    }
    #endregion

    #region Common Functions
    public string WrapSqlWithWhere(string sql, string condition)
    {
      if (string.IsNullOrWhiteSpace(condition)) return sql;
      return string.Format("select * from ({0}) where {1}", sql, condition);
    }
    static Dictionary<string, object> FieldValueArrayToDictionary(params object[] fieldValues)
    {
      Dictionary<string, object> parameters = new Dictionary<string, object>();
      for (int i = 0; i < fieldValues.Length; i += 2)
      {
        parameters.Add(fieldValues[i] + "", fieldValues[i + 1]);
      }
      return parameters;
    }
    static string JoinParameters(
      Dictionary<string, object> parameters, Func<KeyValuePair<string, object>, string> generateValue)
    {
      string result = "";
      foreach (KeyValuePair<string, object> item in parameters)
      {
        string val = generateValue(item);
        if (val == null) continue;
        result += val + ", ";
      }
      if (!string.IsNullOrWhiteSpace(result)) 
        result = result.Substring(0, result.Length - 2);
      return result;
    }
    #endregion

    #region Execute
    public void Execute(string sql, params object[] parameters)
    {
      Execute(CurrentTransaction, sql, parameters);
    }
    public void Execute(OracleTransaction trans, string sql, params object[] parameters)
    {
      Executor(trans, sql, parameters).Execute();
    }
    public void Execute(string sql, Dictionary<string, object> parameters)
    {
      Execute(CurrentTransaction, sql, parameters);
    }
    public void Execute(OracleTransaction trans, string sql, Dictionary<string, object> parameters)
    {
      Executor(trans, sql, parameters).Execute();
    }

    public SqlExecutor Executor(string sql, Dictionary<string, object> parameters)
    {
      return Executor(CurrentTransaction, sql, parameters);
    }
    public SqlExecutor Executor(OracleTransaction trans, string sql, Dictionary<string, object> parameters)
    {
      if (trans == null)
        return new SqlExecutor(CreateConnection(), sql, parameters);
      return new SqlExecutor(trans, sql, parameters);
    }
    public SqlExecutor Executor(string sql, params object[] parameters)
    {
      return Executor(CurrentTransaction, sql, parameters);
    }
    public SqlExecutor Executor(OracleTransaction trans, string sql, params object[] parameters)
    {
      if (trans == null)
        return new SqlExecutor(CreateConnection(), sql, parameters);
      return new SqlExecutor(trans, sql, parameters);
    }
    #endregion
  }

  #region class SqlExecutor
  public class SqlExecutor
  {
    OracleCommand command;

    public SqlExecutor(OracleConnection connection, string sql, params object[] parameters)
      : this(connection, sql, ArrayToDictionary(parameters)) { }
    public SqlExecutor(OracleConnection connection, string sql, Dictionary<string, object> parameters)
    {
      command = new OracleCommand();
      command.Connection = connection;
      command.CommandText = sql;
      AddParameter(parameters);
    }

    public SqlExecutor(OracleTransaction trans, string sql, params object[] parameters)
      : this(trans, sql, ArrayToDictionary(parameters)) { }
    public SqlExecutor(OracleTransaction trans, string sql, Dictionary<string, object> parameters)
      : this(trans.Connection, sql, parameters)
    {
      command.Transaction = trans;
    }

    static Dictionary<string, object> ArrayToDictionary(object[] parameters)
    {
      Dictionary<string, object> dicParam = new Dictionary<string, object>();
      for (int i = 0; i < parameters.Length; i++)
      {
        object parameter = parameters[i];
        if (parameter == null) parameter = DBNull.Value;
        dicParam.Add(i.ToString(), parameter);
      }
      return dicParam;
    }
    void AddParameter(Dictionary<string, object> parameters)
    {
      foreach (KeyValuePair<string, object> param in parameters)
      {
        object parameter = param.Value;
        if (parameter == null) parameter = DBNull.Value;
        command.Parameters.AddWithValue(param.Key, parameter);
      }
    }
    public SqlExecutor WrapperWithWhere(string condition, Dictionary<string, object> parameters)
    {
      string sql = command.CommandText;
      if (!string.IsNullOrWhiteSpace(condition))
        sql = string.Format("select * from ({0}) where {1}", sql, condition);
      SqlExecutor newExecutor = new SqlExecutor(command.Connection, sql, parameters);
      return newExecutor;
    }
    public void ReaderEach(Action<OracleDataReader> read)
    {
      EnsureConnected(delegate()
      {
        OracleDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
          read(reader);
        }
        reader.Close();
      });
    }
    public DataTable GetTable()
    {
      DataTable table = new DataTable();
      OracleDataAdapter adapter = new OracleDataAdapter();
      adapter.SelectCommand = command;
      adapter.Fill(table);
      return table;
    }
    public DataRow GetRow()
    {
      DataTable table = GetTable();
      if (table.Rows.Count == 0) return null;
      return table.Rows[0];
    }
    public object GetCell(string columnName)
    {
      DataRow row = GetRow();
      if (row == null) return null;
      return row[columnName];
    }
    public object GetScalar()
    {
      Object result = null;
      EnsureConnected(delegate()
      {
        result = command.ExecuteScalar();
      });
      return result;
    }
    public void Execute()
    {
      EnsureConnected(delegate()
      {
        command.ExecuteNonQuery();
      });
    }
    void EnsureConnected(Action execute)
    {
      bool controlConnect = command.Connection.State != ConnectionState.Open;
      if (controlConnect) command.Connection.Open();
      execute();
      if (controlConnect) command.Connection.Close();
    }
  }
  #endregion
}
