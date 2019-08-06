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
        private string path_DB;
        public string path_json;

        private string currentClassName;
        private string nextClassName;
        private string jsonstring;
        private string state_endOrStart;
        private Class currentClass;
        


        #region Properties

        public string TablenameGeneral
        {
            get
            {
                return "General";
            }
        }

        public string Currentroom
        {
            get
            {
                return Request.Query["handler"];
            }
        }

        public Class CurrentClass
        {
            get
            {
                if(currentClass == null)
                {
                    //Gets the data from the current class:
                    //First it puts the class in a new JsonArray. After that the currentClass is located and the data is being read.
                    JObject jobject = JObject.Parse(JsonString);  //creates a new json Object
                    JArray jClasses = (JArray)jobject["classes"];   //Puts all the Classes in a new Json Array

                    List<Class> classes = jClasses.ToObject<List<Class>>();

                    foreach (Class c in classes)    //searches for the specific class "Currentclass". The Classnames are compared and if the 
                    {                               //right class is found, the data is written in the Currentlcass
                        bool found = false;
                        List<Teacher> t = new List<Teacher>();

                        if (c.ClassName == CurrentClassName)
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
                }
                return currentClass;
            }
        }

        public List<Teacher> Teacherslist
        {
            get
            {
                if (teacherslist == null)
                {
                    JObject jobject = JObject.Parse(JsonString);  //creates a new json Object
                    JArray jTeachers = (JArray)jobject["teachers"];     //puts everey teachers object of the json file in a new JasonArray

                    teacherslist = jTeachers.ToObject<List<Teacher>>();     //put the JasonArray in to the teacherslist
                    foreach (Teacher teacher in teacherslist)
                    {
                        teacher.Name_Short = teacher.ID.Split('@')[0].ToUpper();    //get the short name for every teacher by splitting the email
                    }
                }
                return teacherslist;
            }
        }

        private string Path_DB
        {
            get
            {
                if (path_DB == null)
                {
                    string root = "wwwroot";
                    string location = "sqlite";
                    string fileName = "database_conference.db";

                    path_DB = Path.Combine(        //locate the path where the file is in 
                    Directory.GetCurrentDirectory(),
                    root,
                    location,
                    fileName);
                }
                return path_DB;
            }
        }

        private string Path_Json
        {
            get
            {
                if(path_json == null)
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

        private string JsonString
        {
            get
            {
                if (jsonstring == null)
                {
                    //the filetext has to be converted into a string to bind it to the jsonstring variable
                    FileStream fileStream = new FileStream(Path_Json, FileMode.Open);
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

        public string CurrentClassName
        {
            get
            {
                using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
                {
                    var command = connection.CreateCommand();

                    //SQL Command to set the new CurrentClassName (get ClassName)
                    command.CommandText = $"Select Classname from {TablenameGeneral} WHERE Status='running' AND Room = '{Currentroom}' order by ID limit 1";
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            currentClassName = reader.GetString(0);
                        }

                    }
                }
                return currentClassName;
            }
        }

        public string NextClassName
        {
            get
            {
                using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
                {
                    var command = connection.CreateCommand();

                    //SQL Command to set the new CurrentClassName (get ClassName)
                    command.CommandText = $"Select Classname from {TablenameGeneral} WHERE Status='not edited' AND Room='{Currentroom}' order by ID limit 1";
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            nextClassName = reader.GetString(0);
                        }

                    }
                }
                return nextClassName;
            }
        }

        public string Tablename_State_of_conference
        {
            get
            {
                return "State_of_conference";
            }
        }

        public string State_endOrStart
        {
            get
            {
                using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"Select Status from {Tablename_State_of_conference} where Room = '{Currentroom}'"; 
                                                                                                                              

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            state_endOrStart = reader.GetString(0);
                        }

                    }
                }

                return state_endOrStart;
            }
            set
            {
                using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"Update {Tablename_State_of_conference} set Status = '{value}' where Room = '{Currentroom}'"; //here you insert into the table from the database
                                                                                                                                  //command.CommandText = "Select * FROM Class_Start_Info";
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

        }

        #endregion

        public void OnGet()
        {
        }

        public void OnPost()
        {
            switch (State_endOrStart)
            {
                case "inactive":
                    StartConference();
                    break;
                case "running":
                    EndConference();
                    break;
            }

        }


        private void StartConference()
        {
            WriteStatus_START_ForCurrentclass();

            State_endOrStart = "running";
        }

        private void WriteStatus_START_ForCurrentclass()
        {
            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command = connection.CreateCommand();
                DateTime dt = DateTime.Now;
                string timeonly = dt.ToLongTimeString();
                command.CommandText = $"UPDATE {TablenameGeneral} set Start = '{timeonly}' WHERE Classname = '{CurrentClassName}'"; 

                connection.Open();
                command.ExecuteNonQuery();      //Execute Command

            }
        }

        private void EndConference()
        {
            WriteStatus_END_ForCurrentclass();
            WriteStatusForNextClass();

            //Get_Teachers_for_nextClass();
            State_endOrStart = "inactive";
        }

        //private void Get_Teachers_for_nextClass()
        //{
        //    Get_Teachers(NextClassName);
        //}

        private void WriteStatusForNextClass()
        {
            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE {TablenameGeneral} set Status = 'running' WHERE Classname = '{NextClassName}'"; //here you insert into the table from the database
                                                                                                                   //command.CommandText = "Select * FROM Class_Start_Info";

                connection.Open();
                command.ExecuteNonQuery();      //Execute Command

            }
        }


        private void WriteStatus_END_ForCurrentclass()
        {

            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command_currentclass = connection.CreateCommand();
               

                //SQL Command to fill in the data for the current completed class (set Status "completed", set End = "DateTime")
                DateTime dt = DateTime.Now;
                string timeonly = dt.ToLongTimeString();
                command_currentclass.CommandText = $"UPDATE {TablenameGeneral} set Status = 'completed', End = '{timeonly}' WHERE Classname = '{CurrentClassName}'";

                connection.Open();
                command_currentclass.ExecuteNonQuery();      //Execute Command

            }
        }
    }
}