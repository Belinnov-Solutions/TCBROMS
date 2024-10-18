using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace TCBROMS_Android_Webservice.Models
{
    public class HushiDataManager
    {
        private string connectionString = string.Empty;
        List<SqlParameter> parameters;
        int commandTimeout = 0;

        public HushiDataManager()
        {
            connectionString = ConfigurationManager.ConnectionStrings["HushiTillConnection"].ConnectionString;
            parameters = new List<SqlParameter>();
        }

        public HushiDataManager(string databaseName)
        {
            connectionString = ConfigurationManager.ConnectionStrings["HushiTillConnection"].ConnectionString; ;
            // ConfigUtility.GetConfigValue<string>(databaseName);
            parameters = new List<SqlParameter>();
        }
        public SqlConnection GetConnection()
        {
            SqlConnection connectionInstance = new SqlConnection(connectionString);
            return connectionInstance;
        }

        public void AddParameter(string parameterName, DbType parameterType, object parameterValue)
        {
            SqlParameter parameter = new SqlParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = parameterType;
            parameter.Value = parameterValue;
            parameters.Add(parameter);
        }

        public void AddParameter(string name, string value)
        {
            AddParameter(name, DbType.String, value);
        }

        public void AddParameter(string name, Guid value)
        {
            AddParameter(name, DbType.Guid, value);
        }

        public void AddParameter(string name, short value)
        {
            AddParameter(name, DbType.Int16, value);
        }

        public void AddParameter(string name, int value)
        {
            AddParameter(name, DbType.Int32, value);
        }

        public void AddParameter(string name, long value)
        {
            AddParameter(name, DbType.Int64, value);
        }

        public void AddParameter(string name, double value)
        {
            AddParameter(name, DbType.Double, value);
        }

        public void AddParameter(string name, decimal value)
        {
            AddParameter(name, DbType.Decimal, value);
        }

        public void AddParameter(string name, bool value)
        {
            AddParameter(name, DbType.Boolean, value);
        }



        public object GetParameterValue(string parameterName)
        {
            foreach (SqlParameter parameter in parameters)
            {
                if (parameter.ParameterName == parameterName)
                {
                    return parameter.Value;
                }
            }
            return null;
        }

        public void AddOutputParameter(string parameterName, DbType parameterType, object parameterValue)
        {
            SqlParameter parameter = new SqlParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = parameterType;
            parameter.Value = parameterValue;
            parameter.Direction = ParameterDirection.Output;
            parameters.Add(parameter);
        }

        public DataTable ExecuteDataTable(string storedProcedureName)
        {
            return ExecuteDataTable(storedProcedureName, parameters);
        }

        public DataTable ExecuteDataTable(string storedProcedureName, List<SqlParameter> parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter parameter in parameters)
                {
                    if (parameter.Value == null)
                        parameter.Value = DBNull.Value;
                    command.Parameters.Add(parameter);
                }
                if (commandTimeout > 0)
                    command.CommandTimeout = commandTimeout;

                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();

                connection.Open();
                dataAdapter.Fill(dataTable);
                connection.Close();

                return dataTable;
            }
        }

        public int ExecuteNonQuery(string storedProcedureName, List<SqlParameter> parameters)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter parameter in parameters)
                {
                    if (parameter.Value == null)
                        parameter.Value = DBNull.Value;
                    command.Parameters.Add(parameter);
                }
                if (commandTimeout > 0)
                    command.CommandTimeout = commandTimeout;

                connection.Open();
                int result = command.ExecuteNonQuery();
                connection.Close();
                return result;
            }
        }

        public int ExecuteNonQuery(string storedProcedureName)
        {
            return ExecuteNonQuery(storedProcedureName, parameters);
        }

        public string ExecuteScalar(string storedProcedureName, List<SqlParameter> parameters)
        {
            string result = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(storedProcedureName, connection);
                command.CommandType = CommandType.StoredProcedure;
                foreach (SqlParameter parameter in parameters)
                {
                    if (parameter.Value == null)
                        parameter.Value = DBNull.Value;
                    command.Parameters.Add(parameter);
                }
                if (commandTimeout > 0)
                    command.CommandTimeout = commandTimeout;

                connection.Open();
                object val = command.ExecuteScalar();
                if (val != null)
                {
                    result = val.ToString();
                }

                connection.Close();
                return result;
            }
        }

        public string ExecuteScalar(string storedProcedureName)
        {
            return ExecuteScalar(storedProcedureName, parameters);
        }

        public int CommandTimeout
        {
            set { commandTimeout = value; }
            get { return commandTimeout; }
        }

        internal void AddParameter(string v, int? adCnt)
        {
            throw new NotImplementedException();
        }
    }
}
