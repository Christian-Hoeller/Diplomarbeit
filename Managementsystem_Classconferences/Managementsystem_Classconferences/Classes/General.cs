using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Managementsystem_Classconferences
{
    public class General
    {
        private string path_json;
        private string pathDB;
        private string jsonstring;

        private static string tableNameGeneral = "General";
        private static string tablenNameStateOfConference = "State";

        public string PathDB
        {
            get
            {
                if (pathDB == null)
                {
                    string root = "wwwroot";
                    string location = "sqlite";
                    string fileName = "database_conference.db";

                    pathDB = Path.Combine(        //locate the path where the file is in 
                    Directory.GetCurrentDirectory(),
                    root,
                    location,
                    fileName);
                }
                return pathDB;
            }
        }

        private string Path_Json
        {
            get
            {
                if (path_json == null)
                {
                    string root = "wwwroot";
                    string location = "json";
                    string fileName = "conference-info.json";

                    path_json = Path.Combine(        //locate the path where the file is in 
                    Directory.GetCurrentDirectory(),
                    root,
                    location,
                    fileName);
                }
                return path_json;
            }
        }

        public string JsonString
        {
            get
            {
                if (jsonstring == null)
                {
                    FileStream fileStream = new FileStream(Path_Json, FileMode.Open);
                    StreamReader reader = new StreamReader(fileStream, Encoding.UTF8);
                    using (reader)
                    {
                        jsonstring += JsonConvert.DeserializeObject(reader.ReadToEnd());
                    }
                }
                return jsonstring;
            }
        }


        public string Table_General
        {
            get
            {
                return tableNameGeneral;
            }
        }

        public string TableStateOfConference
        {
            get
            {
                return tablenNameStateOfConference;
            }
        }
    }
}
