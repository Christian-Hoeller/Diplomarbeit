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
        public Class CurrentClass { get; set; }
        private string Jsonstring;

        public string CurrentClassName { get; set; } = "4AHWII";

        public List<Teacher> Teacherslist
        {
            get { return teacherslist; }
            set { teacherslist = value; }
        }

        public void OnGet()
        {
            ConvertJsonFileText_TO_String();
            Populate_TeachersList();
            Get_Data_From_CurrentClass();   //Reads the json-File and writes the values for all the teachers in the TeachersList
        }

        public JsonResult OnGetTime()
        {
            string root = "wwwroot";
            string location = "sqlite";
            string fileName = "database_conference.db";
            string tableName = "Classes_Time_Info";
            
            var path = Path.Combine(
            Directory.GetCurrentDirectory(),
            root,
            location,
            fileName);


            try
            {
                using (var connection = new SQLiteConnection($"Data Source={path}"))
                {
                    var command = connection.CreateCommand();
                    DateTime dt = DateTime.Now;
                    string timeonly = dt.ToLongTimeString();

                    command.CommandText = $"INSERT INTO {tableName} (Class, Started_time) VALUES ('{CurrentClassName}', '{timeonly}')";
                    //command.CommandText = "Select * FROM Class_Start_Info";

                    connection.Open();
                    command.ExecuteNonQuery();

                    return new JsonResult("successfully inserted");     //to see whether the Insert was successful or not 
                                                                        //Only for testing!
                }
            }
            catch(Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }



        private void Populate_TeachersList()
        {
            JObject jobject = JObject.Parse(Jsonstring);  //creates a new json Object
            JArray jTeachers = (JArray)jobject["teachers"];

            teacherslist = jTeachers.ToObject<List<Teacher>>();
            foreach(Teacher teacher in teacherslist)
            {
                teacher.Name_Short = teacher.ID.Split('@')[0].ToUpper();
            }
        }
       

        private void ConvertJsonFileText_TO_String()
        {
            string root = "wwwroot";
            string location = "json";
            string fileName = "conference-info.json";
            //lcoate the path that the file is located in
            var path = Path.Combine(
            Directory.GetCurrentDirectory(),
            root,
            location,
            fileName);

            //the filetext has to be converted into a string to bind it to the jsonstring variable
            FileStream fileStream = new FileStream(path.ToString(), FileMode.Open);
            StreamReader reader = new StreamReader(fileStream);
            string jsonstring = null;

            using (reader)
            {
                do
                {
                    jsonstring += reader.ReadLine();        //Reads every line and appends it to the jsonstring, in order to get the whole
                }                                           //File-Content into a single variable
                while (!reader.EndOfStream);
            }

            Jsonstring = jsonstring;
        }

        private void Get_Data_From_CurrentClass()
        {
            JObject jobject = JObject.Parse(Jsonstring);  //creates a new json Object
            JArray jClasses = (JArray)jobject["classes"];

            List<Class> classes = jClasses.ToObject<List<Class>>();

            foreach (Class c in classes)
            {
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