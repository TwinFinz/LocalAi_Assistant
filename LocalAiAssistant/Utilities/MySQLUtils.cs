using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace LocalAiAssistant.Utilities
{
    public class SqlDataHandler
    {
        private readonly string connectionString;

        public SqlDataHandler(string connectString)
        {
            this.connectionString = connectString;
            _ = connectionString; // Just to Ignore Error.
        }

#if WINDOWS

        public void CreateDatabase(string databaseName)
        {
            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();
                string query = $"CREATE DATABASE {databaseName}";
                using (SqlCommand command = new(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        public void CreateTable(string tableName, string[] columnNames, string[] dataTypes)
        {
            if (columnNames.Length != dataTypes.Length)
            {
                throw new ArgumentException("Number of column names must match the number of data types.");
            }

            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();
                string query = $"CREATE TABLE {tableName} (";
                for (int i = 0; i < columnNames.Length; i++)
                {
                    query += $"{columnNames[i]} {dataTypes[i]}, ";
                }
                query = query.TrimEnd(',', ' ') + ")";
                using (SqlCommand command = new(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateData(string tableName, string columnName, string data, string conditionColumn, string conditionValue)
        {
            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();
                string query = $"UPDATE {tableName} SET {columnName} = @Data WHERE {conditionColumn} = @ConditionValue";
                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@Data", data);
                    command.Parameters.AddWithValue("@ConditionValue", conditionValue);
                    command.ExecuteNonQuery();
                }
            }
        }
        public DataTable ReadData(string tableName)
        {
            DataTable dataTable = new();
            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM {tableName}";
                using (SqlCommand command = new(query, connection))
                {
                    using (SqlDataAdapter adapter = new(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            return dataTable;
        }
#elif ANDROID
        // Placeholder code for Android implementation
        public void CreateDatabase(string databaseName)
        {
            throw new NotImplementedException("Android implementation not provided.");
        }

        public void CreateTable(string tableName, string[] columnNames, string[] dataTypes)
        {
            throw new NotImplementedException("Android implementation not provided.");
        }

        public void UpdateData(string tableName, string columnName, string data, string conditionColumn, string conditionValue)
        {
            throw new NotImplementedException("Android implementation not provided.");
        }

        public DataTable ReadData(string tableName)
        {
            throw new NotImplementedException("Android implementation not provided.");
        }
#endif
    }
}