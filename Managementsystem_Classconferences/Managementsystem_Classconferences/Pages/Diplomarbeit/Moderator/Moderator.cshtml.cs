using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Managementsystem_Classconferences.Hubs;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
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
        private string text_Conference_State;
        private string currentroom;




        #region Properties

        //public string Text_Conference_State
        //{
        //    get
        //    {
        //        switch (State_OfConference)
        //        {
        //            case "inactive":
        //                text_Conference_State = "Besprechung starten";
        //                break;
        //            case "running":
        //                text_Conference_State = "Besprechung stoppen";
        //                break;
        //            case "completed":
        //                text_Conference_State = "abgeschlossen";
        //                break;
        //        }
        //        return text_Conference_State;
        //    }
        //}

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
                return "R001";
            }  
            //get
            //{
            //    if (currentroom == null)
            //    {
            //        string url = Request.QueryString.Value;
            //        return Request.Query["handler"];
            //    }
            //    else
            //        return currentroom;
            //}
        }

        public Class CurrentClass
        {
            get
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
                    StreamReader reader = new StreamReader(fileStream, Encoding.GetEncoding("iso-8859-1"));
                    //https://stackoverflow.com/questions/33649756/read-json-file-containing-umlaut-in-c-sharp
                    using (reader)
                    {
                        jsonstring += JsonConvert.DeserializeObject(reader.ReadToEnd());    //Reads every line and appends it to the jsonstring, in order to get the whole
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

        public string State_OfConference
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



        private List<string> GetIntersections()
        {
            List<Teacher> currentClass_teachers = CurrentClass.Teachers;  //teachers of this class
            List<string> intersections = new List<string>();
            List<Teacher> otherClass_teachers = null;

            Class otherClass;
            string otherClass_classname = "";

            //Get the Classname of the currently running conference in the other room
            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command = connection.CreateCommand();

                command.CommandText = $"Select Classname from {TablenameGeneral} WHERE Status='running' AND Room <> '{Currentroom}' order by ID limit 1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        otherClass_classname = reader.GetString(0);
                    }

                }

                //Get all the dedicatet teachers from the class (className_otherClass)

                JObject jobject = JObject.Parse(JsonString);  //creates a new json Object
                JArray jClasses = (JArray)jobject["classes"];   //Puts all the Classes in a new Json Array

                List<Class> classes = jClasses.ToObject<List<Class>>();

                foreach (Class c in classes)    //searches for the specific class "Currentclass". The Classnames are compared and if the 
                {                               //right class is found, the data is written in the Currentlcass
                    bool found = false;
                    List<Teacher> t = new List<Teacher>();

                    if (c.ClassName == otherClass_classname)
                    {
                        foreach (var teacher in c.Teachers)
                        {
                            Teacher temporaryTeacher = Teacherslist.Find(x => x.ID == teacher.ID);
                            teacher.Name = temporaryTeacher.Name;
                            teacher.Name_Short = temporaryTeacher.Name_Short;
                        }
                        otherClass = c;
                        otherClass_teachers = otherClass.Teachers;
                        found = true;
                    }
                    if (found) break;
                }

                
            }

            //loob the Lists to find the intersections / duplicates in the list and put them in a new list
            foreach (Teacher teacher in currentClass_teachers)
            {
                foreach (var otherteacher in otherClass_teachers)
                {
                    if (otherteacher.Name == teacher.Name)
                        intersections.Add(teacher.Name);
                }
            }


                return intersections;
        }

        public JsonResult OnGetIntersections()
        {
          

            List<string> teachers_list = GetIntersections();
            string teachers_string = null;

            foreach(string teacher in teachers_list)
            {
                if (teachers_string != null)
                {
                    teachers_string += ";";
                    teachers_string += teacher;
                }
                else
                    teachers_string += teacher;
            }

            return new JsonResult(teachers_string);
        }

        public JsonResult OnGetTeachers()
        {
          
            string teachers_string = null;
            foreach (Teacher teacher in CurrentClass.Teachers)
            {
                if (teachers_string != null)
                {
                    teachers_string += ";";
                    teachers_string += teacher.Name;
                }
                else
                    teachers_string += teacher.Name;
            }
            return new JsonResult(teachers_string);


        }

        public void OnGetConferenceAction()
        {
            switch (State_OfConference)
            {
                case "inactive":
                    StartConference();
                    break;
                case "running":
                    EndConference();
                    break;
            }
        }

        public JsonResult OnGetClassName()
        {
            return new JsonResult(CurrentClassName);
        }

        public JsonResult OnGetButtonText()
        {
            switch (State_OfConference)
            {
                case "inactive":
                    text_Conference_State = "Besprechung starten";
                    break;
                case "running":
                    text_Conference_State = "Besprechung stoppen";
                    break;
                case "completed":
                    text_Conference_State = "Konferenz abgeschlossen";
                    break;
            }
            return new JsonResult(text_Conference_State);
        }

        public JsonResult OnGetConferenceState()
        {
            return new JsonResult(State_OfConference);
        }

        public void StartConference()
        {
            WriteTime("start");

            State_OfConference = "running";
        }

        private void WriteTime(string time)
        {
            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command = connection.CreateCommand();
                DateTime dt = DateTime.Now;
                string timeonly = dt.ToLongTimeString();
                command.CommandText = $"UPDATE {TablenameGeneral} set {time} = '{timeonly}' WHERE Classname = '{CurrentClassName}'"; 

                connection.Open();
                command.ExecuteNonQuery();      //Execute Command

            }
        }

        private void EndConference()
        {
            WriteTime("end");
            WriteStatus_CurrentClass();
            WriteStatusForNextClass();

            if (Check_If_Conference_Finished() == true)
            {
                State_OfConference = "completed";
            }
            else
            {
                State_OfConference = "inactive";
            }
           
        }

        private bool Check_If_Conference_Finished()
        {
            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))
            {
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM {TablenameGeneral} WHERE Status = 'running' AND Room = '{Currentroom}'";
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

        private void WriteStatus_CurrentClass()
        {

            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))
            {
                var command = connection.CreateCommand();

                command.CommandText = $"UPDATE {TablenameGeneral} set Status='completed' WHERE Classname = '{currentClassName}'";
                //update general set Start = '12' WHERE Classname = '4AHWII'
                connection.Open();
                command.ExecuteNonQuery();

            }
        }

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

    }
}