using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Managementsystem_Classconferences.Classes;
using Managementsystem_Classconferences.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Managementsystem_Classconferences.Hubs
{
    [Authorize]
    public class MainHub : Hub
    {
        private General general;
        private DBConnection dB;
        private List<Order> order;

        private General General
        {
            get
            {
                if (general == null)
                {
                    general = new General();
                }
                return general;
            }
        }

        private DBConnection DB
        {
            get
            {
                if (dB == null)
                {
                    dB = new DBConnection();
                }
                return dB;
            }
        }

        private string Currentroom { get; set; }


        public async Task LoadModeratorPage(string _currentroom)
        {
            Currentroom = _currentroom;
            await LoadGeneralContent();
            await LoadModeratorContent();
        }

        public async Task LoadUserPageContent(string _currentroom)
        {
            Currentroom = _currentroom;
            await LoadGeneralContent();
        }

        public async Task LoadModeratorContent()
        {
            JObject content = new JObject();

            string teachersString = string.Empty;

            if (GetCurrentStateOfConference() != "completed")
            {
                var teachers = GetClass(GetCurrentClassName()).Teachers;
                teachersString = JsonConvert.SerializeObject(teachers);
            }

            content.Add("teachers", teachersString);
            content.Add("intersections", GetIntersections());

            content.Add("buttontext", GetButtonText());
            await Clients.All.SendAsync("ReceiveModeratorContent", content.ToString());
        }

        public async Task ConferenceAction(string _currentroom)
        {
            Currentroom = _currentroom;

            switch (GetCurrentStateOfConference())
            {
                case "inactive":
                    StartConference();
                    break;
                case "running":
                    NextClass();
                    break;
            }
            await LoadModeratorPage(_currentroom);
        }

        public async Task LoadGeneralContent()
        {
            JObject information = new JObject();
            var currentClass = GetClass(GetCurrentClassName());

            switch (GetCurrentStateOfConference())
            {
                case "inactive":
                    information.Add("room", Currentroom);
                    information.Add("classname", GetCurrentClassName());
                    information.Add("formTeacher", currentClass.FormTeacher);
                    information.Add("headOfDepartment", currentClass.HeadOfDepartment);
                    information.Add("time", "Besprechung noch nicht gestartet");
                    break;

                case "running":
                    DataTable dt = DB.Reader($"SELECT room, start FROM {General.Table_General} WHERE ID = ? limit 1", GetCurrentClassName());

                    information.Add("room", dt.Rows[0]["room"].ToString());
                    information.Add("time", dt.Rows[0]["start"].ToString());
                    information.Add("classname", GetCurrentClassName());
                    information.Add("formTeacher", currentClass.FormTeacher);
                    information.Add("headOfDepartment", currentClass.HeadOfDepartment);
                    break;

                case "completed":
                    information.Add("room", Currentroom);
                    information.Add("classname", "Alle Klassen abgeschlossen");
                    information.Add("formTeacher", "-");
                    information.Add("headOfDepartment", "-");
                    information.Add("time", "-");
                    break;
            }

            information.Add(new JProperty("classesCompleted", GetClassesCompleted()));
            information.Add(new JProperty("classesNotEdited", GetClassesNotEdited()));
            await Clients.All.SendAsync("ReceiveGeneralContent", information.ToString());
        }

        public async Task LoadRooms()
        {
            var order = GetOrderList();
            JArray jArrayRooms = new JArray(order.Select(room => room.Room_only).ToList());
            await Clients.All.SendAsync("ReceiveRooms", jArrayRooms.ToString());
        }

        public async Task SendTeacherCall(int indexOfTeacher, string _currenroom)
        {
            Currentroom = _currenroom;

            var currentClass = GetClass(GetCurrentClassName());
            var teacherToCall = currentClass.Teachers[indexOfTeacher].ID;

            //here comes the Clients.(CallSomeone) command to call out a teacher

            await Clients.All.SendAsync("ReceiveTeacherCall", teacherToCall.ToString());
        }

        private MyClasses GetClass(string classname)
        {
            if (classname != null)
            {
                MyClasses myclass;

                JObject jobject = JObject.Parse(general.JsonString);  //creates a new json Object
                JArray jClasses = (JArray)jobject["classes"];   //Puts all the Classes in a new Json Array

                List<MyClasses> classes = jClasses.ToObject<List<MyClasses>>();

                myclass = classes.Find(x => x.ClassName == classname);

                var teacherslist = GetTeachersList();

                for (int i = 0; i < myclass.Teachers.Count; i++)
                {
                    myclass.Teachers[i] = teacherslist.Find(x => x.ID == myclass.Teachers[i].ID);
                    myclass.Teachers[i].Name_Short = myclass.Teachers[i].ID.Split("@")[0];
                }
                return myclass;
            }
            else
                return null;

        }

        private string GetCurrentClassName()
        {
            DataTable dt = DB.Reader($"SELECT ID FROM {General.Table_General} WHERE Status='not edited' AND Room = ? order by ClassOrder limit 1", Currentroom);
            if (dt.Rows.Count == 0)
                return null;
            return dt.Rows[0]["id"].ToString();
        }

        private string GetCurrentStateOfConference()
        {
            DataTable dt = DB.Reader($"Select Status from {General.TableStateOfConference} where Room = ? limit 1", Currentroom);
            return dt.Rows[0]["status"].ToString();
        }

        private void SetStateOfConference(string status)
        {
            DB.Query($"Update {General.TableStateOfConference} set Status = ? where Room = ?", status, Currentroom);
        }

        private List<Teacher> GetTeachersList()
        {
            JObject jobject = JObject.Parse(general.JsonString);  //creates a new json Object
            JArray jTeachers = (JArray)jobject["teachers"];     //puts everey teachers object of the json file in a new JasonArray

            List<Teacher> teacherslist = jTeachers.ToObject<List<Teacher>>();     //put the JasonArray in to the teacherslist
            foreach (Teacher teacher in teacherslist)
            {
                teacher.Name_Short = teacher.ID.Split('@')[0].ToUpper();    //get the short name for every teacher by splitting the email
            }
            return teacherslist;
        }

        private List<Order> GetOrderList()
        {
            JObject jobject = JObject.Parse(General.JsonString);
            JArray jOrder = (JArray)jobject["order"];

            order = jOrder.ToObject<List<Order>>();
            foreach (Order item in order)
            {
                item.Room_only = item.Room.Split(' ')[0];
            }

            return order;
        }

        private string GetButtonText()
        {
            string textConferenceState = string.Empty;
            switch (GetCurrentStateOfConference())
            {
                case "inactive":
                    textConferenceState = "Konferenz starten";
                    break;
                case "running":
                    textConferenceState = "Nächste Klasse";
                    break;
                case "completed":
                    textConferenceState = "Konferenz abgeschlossen";
                    break;
            }
            return textConferenceState;
        }

        public void StartConference()
        {
            WriteTimeInDatabase("start");
            SetStateOfConference("running");
        }

        private void NextClass()
        {
            //current class
            WriteTimeInDatabase("end");   //Write the time when the class is completed
            DB.Query($"UPDATE {General.Table_General} set Status='completed' WHERE ID = ?", GetCurrentClassName());     //Write Status for current class

            if (GetCurrentClassName() == null)
            {
                SetStateOfConference("completed");
            }
            else
            {
                WriteTimeInDatabase("start");     //Write the time when the class is started
            }
        }

        private void WriteTimeInDatabase(string time) //time can be "start or end" (names in the database)
        {
            DateTime date = DateTime.Now;
            string timeonly = date.ToLongTimeString();
            DB.Query($"UPDATE {General.Table_General} set {time} = ? WHERE ID = ?", timeonly, GetCurrentClassName()) ;
        }

        private string GetIntersections()
        {
            JArray jArrayIntersections = new JArray();
            string sqlstring = $"Select ID from {General.Table_General} WHERE Status='not edited' AND Room <> ? order by ClassOrder limit 1";
            DataTable dt = DB.Reader(sqlstring, Currentroom);


            if (GetCurrentStateOfConference() == "completed" || dt.Rows.Count == 0)  //when the conference is completed, we dont have to load all the intersections again
            {
                jArrayIntersections.Add("Keine Überschneidungen");
            }
            else
            {
                string otherclassname = dt.Rows[0]["ID"].ToString();
                MyClasses currentClass = GetClass(GetCurrentClassName());
                MyClasses otherclass = GetClass(otherclassname);

                List<string> otherClassTeachers = otherclass.Teachers.Select(teacher => teacher.Name).ToList();
                List<string> currentClassTeachers = currentClass.Teachers.Select(teacher => teacher.Name).ToList();

                List<string> intersections = otherClassTeachers.Intersect(currentClassTeachers).ToList();

                jArrayIntersections = new JArray(intersections);
            }

            return jArrayIntersections.ToString();
        }

        private string GetClassesCompleted()
        {
            var classes = GetClassesFromJSON();

            if(GetCurrentStateOfConference() == "completed")
            {
                return new JArray(classes).ToString();
            }
            else
            {
                int index = classes.IndexOf(GetCurrentClassName());
                return new JArray(classes.Take(index)).ToString();
            }
        }

        private string GetClassesNotEdited()
        {
            JArray classesNotedited = new JArray();
            var classes = GetClassesFromJSON();
            if (GetCurrentStateOfConference() == "completed")
            {
                classesNotedited.Add("Keine weiteren Klassen");
            }
            else
            {
                int index = classes.IndexOf(GetCurrentClassName()) + 1; //the current class shouldn't be displayed
                if(index == classes.Count)
                {
                    classesNotedited.Add("Keine weiteren Klassen");
                }
                else
                {
                    classesNotedited =  new JArray(classes.GetRange(index, classes.Count - index));
                }
            }
            return classesNotedited.ToString();
        }

        private List<string> GetClassesFromJSON()
        {
            var orderlist = GetOrderList();
            return orderlist.Find(order => order.Room.Split(' ')[0] == Currentroom).Classes;
        }
    }

}


