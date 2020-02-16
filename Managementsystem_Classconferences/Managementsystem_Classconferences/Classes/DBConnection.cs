using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Managementsystem_Classconferences.Classes
{
    public class DBConnection
    {

        private string pathDB;


        public string PathDB
        {
            get
            {
                if (pathDB == null)
                {
                    pathDB = Path.Combine(Directory
                        .GetCurrentDirectory(), "wwwroot", "sqlite", "database_conference.db");
                }
                return pathDB;
            }

        }

        public void Query(string sqlstring)
        {
            using (var connection = new SQLiteConnection($"Data Source={PathDB}"))
            {
                var command = connection.CreateCommand();
                command.CommandText = sqlstring;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public int TestQuery(string sqlstring, SQLiteParameter[] parametervalues)
        {
            using (var connection = new SQLiteConnection($"Data Source={PathDB}"))
            {
                using (var command = new SQLiteCommand(sqlstring, connection))
                {
                    command.CommandText = sqlstring;

                    if (parametervalues != null)
                    {
                        command.Parameters.AddRange(parametervalues);
                    }

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public DataTable TestReader(string sqlstring, params object[] parametervalues)
        {
            DataTable dt = new DataTable();
            using (var connection = new SQLiteConnection($"Data Source={PathDB}"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = sqlstring;
                    if (parametervalues != null)
                    {
                        foreach (var param in parametervalues)
                        {
                            command.Parameters.Add(new SQLiteParameter() { Value=param});
                        }

                    }

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public DataTable Reader(string sqlstring)
        {
            DataTable dt = new DataTable();
            using (var connection = new SQLiteConnection($"Data Source={PathDB}"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sqlstring, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }
    }
}
