using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Moderator
{

    public class ModeratorModel : PageModel
    {
        private List<Teacher> teacherslist;
        public Class currentClass { get; set; }

        private string jsonstring;
        private string path_DB;
        private string path_JSON;
        private string name_currentClass;
        private string name_nextClass;

        private string state_of_Conference;

        private string currentroom;

        //private string Roomname
        //private string 


        #region Properties



        //Converts the JSON-File content into a string
        public string Jsonstring
        {
            get
            {
                if (jsonstring == null)
                {


                    //the filetext has to be converted into a string to bind it to the jsonstring variable
                    FileStream fileStream = new FileStream(Path_JSON, FileMode.Open);
                    StreamReader reader = new StreamReader(fileStream);

                    using (reader)
                    {
                        do
                        {
                            jsonstring += reader.ReadLine();        //Reads every line and appends it to the jsonstring, in order to get the whole
                        }                                           //File-Content into a single variable
                        while (!reader.EndOfStream);
                    }
                }
                return jsonstring;

            }
        }

        //Path where the JSON-File is located in
        public string Path_JSON
        {
            get
            {
                string root = "wwwroot";
                string location = "json";
                string fileName = "conference-info.json";
                //lcoate the path that the file is located in
                path_JSON = Path.Combine(
                Directory.GetCurrentDirectory(),
                root,
                location,
                fileName);

                return path_JSON;
            }
        }

        //Path where the JSON-File is located in
        public string Path_DB
        {
            get
            {
                string root = "wwwroot";
                string location = "sqlite";
                string fileName = "database_conference.db";
                //lcoate the path that the file is located in
                path_DB = Path.Combine(
                Directory.GetCurrentDirectory(),
                root,
                location,
                fileName);

                return path_DB;
            }
        }

        //Tablename of general (Info about start, end, class, etc.)
        public string Tablename_general
        {
            get
            {
                return "General";
            }
        }

        public string Tablename_State_Of_Conference
        {
            get
            {
                return "State_of_conference";
            }
        }

        //Name of the currently reviewed class
        public string Name_CurrentClass
        {
            get
            {
                using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"Select Classname from {Tablename_general} WHERE Status = 'running' AND Room = '{Currentroom}' limit 1";
                    //Select Classname from general WHERE Status = 'running' limit 1;
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            name_currentClass = reader.GetString(0);

                        }
                    }
                }
                return name_currentClass;
            }
        }

        //Name of the next class that is reviewed
        public string Name_NextClass
        {
            get
            {
                using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"Select Classname from {Tablename_general} WHERE Status = 'not edited' AND Room = '{Currentroom}' ORDER BY ID limit 1";
                    //select Classname from General WHERE Status = 'not edited' order by ID limit 1
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            name_nextClass = reader.GetString(0);

                        }
                    }
                }
                return name_nextClass;
            }
        }

        public string Currentroom
        {
            get
            {
                return Request.Query["handler"];
            }
        }

        public string State_of_Conference
        {
            get
            {
                using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"Select status from {Tablename_State_Of_Conference} WHERE Room = '{Currentroom}'";
                    //select status from State_of_conference WHERE Room = 'R001'
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            state_of_Conference = reader.GetString(0);

                        }
                    }
                }
                return state_of_Conference;
            }
            set
            {
                state_of_Conference = value;
                using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"UPDATE {Tablename_State_Of_Conference} SET Status = '{state_of_Conference}' WHERE Room = '{Currentroom}'";
                    //UPDATE State_of_conference SET Status = 'running' WHERE Room = ''
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

        }

        public List<Teacher> Teacherslist
        {
            get
            {
                JObject jobject = JObject.Parse(Jsonstring);  //creates a new json Object
                JArray jTeachers = (JArray)jobject["teachers"];

                teacherslist = jTeachers.ToObject<List<Teacher>>();
                foreach (Teacher teacher in teacherslist)
                {
                    teacher.Name_Short = teacher.ID.Split('@')[0].ToUpper();
                }
                return teacherslist;
            }
        }

        public Class CurrentClass
        {
            get
            {
                JObject jobject = JObject.Parse(Jsonstring);
                JArray jClasses = (JArray)jobject["classes"];

                List<Class> classes = jClasses.ToObject<List<Class>>();

                foreach (Class c in classes)
                {
                    bool found = false;
                    List<Teacher> t = new List<Teacher>();

                    if (c.ClassName == Name_CurrentClass)
                    {
                        foreach (var teacher in c.Teachers)
                        {
                            Teacher temporaryTeacher = Teacherslist.Find(x => x.ID == teacher.ID);
                            teacher.Name = temporaryTeacher.Name;
                            teacher.Name_Short = temporaryTeacher.Name_Short;
                        }

                        currentClass = c;
                        found = true;
                    }
                    if (found) break;
                }
                return currentClass;
            }
        }

        #endregion  

        public void OnPost()
        {
            switch (State_of_Conference)
            {
                case "inactive":
                    StartConference();
                    break;
                case "running":
                    StopConference();
                    break;
            }

        }


        private void StartConference()
        {
            SetTime("Start");

            State_of_Conference = "running";
        }

        private void StopConference()
        {
            SetTime("End");
            SetStatus_CurrentClass();
            SetStatus_NextClass();


            if (Check_If_Conference_Finished() == true)
            {
                State_of_Conference = "completed";
            }
            else
            {
                State_of_Conference = "inactive";
            }
           
        }

        private bool Check_If_Conference_Finished()
        {
            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))
            {
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM {Tablename_general} WHERE Status = 'running' AND Room = '{Currentroom}'";
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        return true;
                    }

                }

            }
            return false;
        }

        private void SetStatus_CurrentClass()
        {
            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))
            {
                var command = connection.CreateCommand();

                command.CommandText = $"UPDATE {Tablename_general} set Status='completed' WHERE Classname = '{Name_CurrentClass}'";
                //update general set Start = '12' WHERE Classname = '4AHWII'
                connection.Open();
                command.ExecuteNonQuery();

            }
        }

        private void SetStatus_NextClass()
        {
            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))
            {
                var command = connection.CreateCommand();

                command.CommandText = $"UPDATE {Tablename_general} set Status='running' WHERE Classname = '{Name_NextClass}'";
                //update general set Start = '12' WHERE Classname = '4AHWII'
                connection.Open();
                command.ExecuteNonQuery();

            }
        }

        private void SetTime(string moment)
        {
            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))
            {
                var command = connection.CreateCommand();
                DateTime dt = DateTime.Now;
                string timeonly = dt.ToLongTimeString();

                command.CommandText = $"UPDATE {Tablename_general} set {moment} = '{timeonly}' WHERE Classname = '{Name_CurrentClass}'";
                //update general set Start = '12' WHERE Classname = '4AHWII'
                connection.Open();
                command.ExecuteNonQuery();
            }
        }



    }
}