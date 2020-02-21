using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Managementsystem_Classconferences.Classes
{
    public class DBConnection
    {
        General general = new General();

        public int Query(string sqlstring, params object[] parametervalues)
        {
            using (var connection = new SQLiteConnection($"Data Source={general.PathDB}"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(sqlstring, connection))
                {
                    command.CommandText = sqlstring;

                    foreach (var param in parametervalues)
                    {
                        command.Parameters.Add(new SQLiteParameter() { Value = param });
                    }
                    return command.ExecuteNonQuery();
                }
            }
        }

        public DataTable Reader(string sqlstring, params object[] parametervalues)
        {
            DataTable dt = new DataTable();
            using (var connection = new SQLiteConnection($"Data Source={general.PathDB}"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = sqlstring;
                    if (parametervalues != null)
                    {
                        foreach (var param in parametervalues)
                        {
                            command.Parameters.Add(new SQLiteParameter() { Value = param });
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
    }
}
