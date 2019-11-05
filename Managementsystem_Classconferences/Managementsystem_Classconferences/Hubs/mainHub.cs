using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Classes;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Models;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Moderator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Managementsystem_Classconferences.Hubs
{
    public class MainHub : Hub
    {

        #region variables


        public General general = new General();
        private MyClasses currentClass;

        private List<Teacher> teacherslist;

        private string currentClassName;
        private string nextClassName;
        private string state_endOrStart;
        private string text_Conference_State;
        private string currentroom;

        #endregion

        #region Properties

        public string Currentroom
        {
            get
            {
                return currentroom;
            }
            set
            {
                currentroom = value;
            }
        }


        public MyClasses CurrentClass
        {
            get
            {

                //Gets the data from the current class:
                //First it puts the class in a new JsonArray. After that the currentClass is located and the data is being read.
                JObject jobject = JObject.Parse(general.JsonString);  //creates a new json Object
                JArray jClasses = (JArray)jobject["classes"];   //Puts all the Classes in a new Json Array

                List<MyClasses> classes = jClasses.ToObject<List<MyClasses>>();

                foreach (MyClasses c in classes)    //searches for the specific class "Currentclass". The Classnames are compared and if the 
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
                    JObject jobject = JObject.Parse(general.JsonString);  //creates a new json Object
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


        public string CurrentClassName
        {
            get
            {
                using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
                {
                    var command = connection.CreateCommand();

                    //SQL Command to set the new CurrentClassName (get ClassName)
                    command.CommandText = $"Select ID from {general.Table_General} WHERE Status='not edited' AND Room = '{Currentroom}' order by ClassOrder limit 1";
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
                using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
                {
                    var command = connection.CreateCommand();

                    //SQL Command to set the new CurrentClassName (get ClassName)
                    command.CommandText = $"Select ID from {general.Table_General} WHERE Status='not edited' AND Room ='{Currentroom}' order by ClassOrder limit 1";
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
                using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
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
                using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"Update {Tablename_State_of_conference} set Status = '{value}' where Room = '{Currentroom}'"; 
                                                                                                                                       
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

        }

        public string Buttontext
        {
            get
            {
                switch (State_OfConference)
                {
                    case "inactive":
                        text_Conference_State = "Konferenz starten";
                        break;
                    case "running":
                        text_Conference_State = "Nächste Klasse";
                        break;
                    case "completed":
                        text_Conference_State = "Konferenz abgeschlossen";
                        break;
                }
                return text_Conference_State;
            }
        }


        #endregion

        #region Tasks

        public async Task ConferenceAction(string _currentroom)
        {
            Currentroom = _currentroom;

            switch (State_OfConference)
            {
                case "inactive":
                    StartConference();
                    break;
                case "running":
                    NextClass();
                    break;
            }

            await LoadInformation(_currentroom);

        }

        public async Task LoadInformation(string _currentroom)
        {
            

            Currentroom = _currentroom;
            string sqlcommand_GetClasses_completed = $"Select ID FROM {general.Table_General} WHERE status = 'completed' AND Room = '{Currentroom}' order by ID";
            string sqlcommand_getClasses_notedited = $"Select ID FROM {general.Table_General} WHERE status <> 'completed' AND Room = '{Currentroom}' order by ID";


            if (State_OfConference != "completed")
            {
                await Clients.Caller.SendAsync("ReveiveLoadInformation",
                    CurrentClassName, Buttontext, GetClasses(sqlcommand_GetClasses_completed), GetClasses(sqlcommand_getClasses_notedited));
            }
            else
            {
                await Clients.Caller.SendAsync("ReveiveLoadInformation", 
                    "Alle Klassen abgeschlossen", "Konferenz abgeschlossen", GetClasses(sqlcommand_GetClasses_completed), "abgeschlossen");
            }

            await SendIntersections();
            await SendTeachers();

        }

        public async Task SendTeachers()
        {
            await Clients.Caller.SendAsync("ReceiveTeachers", GetTeachers());
        }

        public async Task SendIntersections()
        {
            await Clients.All.SendAsync("ReceiveIntersections", GetIntersections());
        }
        #endregion

        #region Methods

        private string GetIntersections()
        {
            if (State_OfConference == "completed")  //when the conference is completed, we dont have to load all the intersections again
                return "Keine Überschneidungen";

            List<Teacher> currentClass_teachers = CurrentClass.Teachers;  //teachers of this class
            List<string> intersections = new List<string>();
            List<Teacher> otherClass_teachers = null;

            MyClasses otherClass;
            string otherClass_classname = "";

            //Get the Classname of the currently running conference in the other room
            using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {

                var command = connection.CreateCommand();

                command.CommandText = $"Select ID from {general.Table_General} WHERE Status='not edited' AND Room <> '{Currentroom}' order by ClassOrder limit 1";
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                        return "Keine Überschneidungen";
                    while (reader.Read())
                    {
                        otherClass_classname = reader.GetString(0);
                    }

                }

                //Get all the dedicatet teachers from the class (className_otherClass)

                JObject jobject = JObject.Parse(general.JsonString);  //creates a new json Object
                JArray jClasses = (JArray)jobject["classes"];   //Puts all the Classes in a new Json Array

                List<MyClasses> classes = jClasses.ToObject<List<MyClasses>>();

                foreach (MyClasses c in classes)    //searches for the specific class "Currentclass". The Classnames are compared and if the 
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



                //loop the Lists to find the intersections / duplicates in the list and put them in a new list
                foreach (Teacher teacher in currentClass_teachers)
                {
                    foreach (var otherteacher in otherClass_teachers)
                    {
                        if (otherteacher.Name == teacher.Name)
                            intersections.Add(teacher.Name);
                    }
                }

                string teachers_string = null;

                foreach (string teacher in intersections)
                {
                    if (teachers_string != null)
                    {
                        teachers_string += ";";
                        teachers_string += teacher;
                    }
                    else
                        teachers_string += teacher;
                }
                if (teachers_string == null)
                    return "Keine Überschneidungen";
                return teachers_string;


            }


        }

        public string GetTeachers()
        {
            if (State_OfConference == "completed")  //when the conference is completed, we dont have to load all the teachers again
                return "Keine Lehrer";

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
            return teachers_string;
        }

        public string GetClasses(string sqlcommand_text)
        {
            string classes = null;
            using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command = connection.CreateCommand();
                command.CommandText = sqlcommand_text;

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                        return "";
                    while (reader.Read())
                    {

                        classes += $";{reader.GetString(0)}";
                    }

                }
            }
            return classes;
        }

        public void StartConference()
        {
            WriteTime("start");

            State_OfConference = "running";
        }

        private void WriteTime(string time) //time can be "start or end" (names in the database)
        {
            using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command = connection.CreateCommand();
                DateTime dt = DateTime.Now;
                string timeonly = dt.ToLongTimeString();
                command.CommandText = $"UPDATE {general.Table_General} set {time} = '{timeonly}' WHERE ID = '{CurrentClassName}'";

                connection.Open();
                command.ExecuteNonQuery();      //Execute Command

            }
        }

        private void NextClass()
        {
            //current class
            WriteTime("end");   //Write the time when the class is completed
            WriteStatus_CurrentClass(); //write status "completed" for the current class

            //next class
            WriteTime("start");     //Write the time when the class is started


            if (Check_If_Conference_Finished() == true)
            {
                State_OfConference = "completed";
            }
            else
            {
                State_OfConference = "running";
            }

        }

        private bool Check_If_Conference_Finished()
        {
            using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))
            {
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT * FROM {general.Table_General} WHERE Status = 'not edited' AND Room = '{Currentroom}'";
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

            using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))
            {
                var command = connection.CreateCommand();

                command.CommandText = $"UPDATE {general.Table_General} set Status='completed' WHERE ID = '{currentClassName}'";
                //update general set Start = '12' WHERE Classname = '4AHWII'
                connection.Open();
                command.ExecuteNonQuery();

            }
        }

        private void WriteStatusForNextClass()
        {
            using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command = connection.CreateCommand();
                command.CommandText = $"UPDATE {general.Table_General} set Status = 'running' WHERE ID = '{NextClassName}'"; //here you insert into the table from the database
                                                                                                                               //command.CommandText = "Select * FROM Class_Start_Info";

                connection.Open();
                command.ExecuteNonQuery();      //Execute Command

            }
        }


        #endregion





    }

}


