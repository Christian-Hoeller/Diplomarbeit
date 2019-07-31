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

    public class IndexModel : PageModel
    {
        private List<Teacher> teacherslist = new List<Teacher>();
        private string path_DB;
        public string path_json;
        private string currentClassName;
        private string nextClassName;
        private string jsonstring;

        //var command_nextclass = connection.CreateCommand();
        ////SQL Command to set the Next Class-Status to running (get ClassName)
        //command_nextclass.CommandText = $"Select * from {tablename_Class_Info} WHERE Status='not edited' order by ID limit 1";

        private string tablename_Class_Info = "Class_Info";

        public Class CurrentClass { get; set; }

        #region Properties

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

        private string CurrentClassName
        {
            get
            {
                using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
                {
                    var command = connection.CreateCommand();

                    //SQL Command to set the new CurrentClassName (get ClassName)
                    command.CommandText = $"Select Classname from {tablename_Class_Info} WHERE Status='running' order by ID limit 1";
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

        private string NextClassName
        {
            get
            {
                using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
                {
                    var command = connection.CreateCommand();

                    //SQL Command to set the new CurrentClassName (get ClassName)
                    command.CommandText = $"Select Classname from {tablename_Class_Info} WHERE Status='not edited' order by ID limit 1";
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
        #endregion

        public List<Teacher> Teacherslist
        {
            get { return teacherslist; }
            set { teacherslist = value; }
        }


        public void OnGet()
        {
            Populate_TeachersList();
            Get_Data_From_CurrentClass();   //Reads the json-File and writes the values for all the teachers in the TeachersList
        }

        public JsonResult OnGetNewClass()
        {
            JsonResult status_currentclass; //only demo
            JsonResult status_nextclass;    //only demo

            status_currentclass = WriteStatusForCurrentclass();        //writes the end 
            status_nextclass = WriteStatusForNextClass();

            return status_currentclass;
          
        }

        private JsonResult WriteStatusForNextClass()
        {
            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command = connection.CreateCommand();
                DateTime dt = DateTime.Now;
                string timeonly = dt.ToLongTimeString();
                command.CommandText = $"UPDATE {tablename_Class_Info} set Status = 'running' WHERE Classname = '{NextClassName}'"; //here you insert into the table from the database
                                                                                                                   //command.CommandText = "Select * FROM Class_Start_Info";

                connection.Open();
                command.ExecuteNonQuery();      //Execute Command


            }
            return new JsonResult("successfully inserted");     //to see whether the Insert was successful or not 
                                                                //Only for testing!
        }


        private JsonResult WriteStatusForCurrentclass()
        {

            using (var connection = new SQLiteConnection($"Data Source={Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command_currentclass = connection.CreateCommand();
               

                //SQL Command to fill in the data for the current completed class (set Status "completed", set End = "DateTime")
                DateTime dt = DateTime.Now;
                string timeonly = dt.ToLongTimeString();
                command_currentclass.CommandText = $"UPDATE {tablename_Class_Info} set Status = 'completed', End = '{timeonly}' WHERE Classname = '{CurrentClassName}'";

                connection.Open();
                command_currentclass.ExecuteNonQuery();      //Execute Command

            }

            return new JsonResult("successfully inserted");     //to see whether the Insert was successful or not 
                                                                //Only for testing
        }

        private void Populate_TeachersList()
        {
            JObject jobject = JObject.Parse(JsonString);  //creates a new json Object
            JArray jTeachers = (JArray)jobject["teachers"];     //puts everey teachers object of the json file in a new JasonArray

            teacherslist = jTeachers.ToObject<List<Teacher>>();     //put the JasonArray in to the teacherslist
            foreach(Teacher teacher in teacherslist)
            {
                teacher.Name_Short = teacher.ID.Split('@')[0].ToUpper();    //get the short name for every teacher by splitting the email
            }
        }
       


        private void Get_Data_From_CurrentClass()
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
                        Teacher temporaryTeacher = teacherslist.Find(x => x.ID == teacher.ID);
                        teacher.Name = temporaryTeacher.Name;
                        teacher.Name_Short = temporaryTeacher.Name_Short;
                    }

                    CurrentClass = c;
                    found = true;
                }
                if (found) break;
            }


        }
    }
}