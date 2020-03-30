using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Managementsystem_Classconferences.Classes;
using Managementsystem_Classconferences.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace Managementsystem_Classconferences.Hubs
{
    public class MainHub : Hub
    {
        private General general = new General();
        private DBConnection dB = new DBConnection();

        private string Currentroom { get; set; }

        private string GetCurrentClassName()
        {
            DataTable dt = dB.Reader($"SELECT ID FROM {general.Table_General} WHERE Status='not edited' AND Room = ? order by ClassOrder limit 1", Currentroom);
            if (dt.Rows.Count == 0)
                return null;
            return dt.Rows[0]["id"].ToString();
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

                return myclass;
            }
            else
                return null;
        }

        private Teacher GetTeacher(string teacherId)
        {
            JObject jobject = JObject.Parse(general.JsonString);
            JArray jTeachers = (JArray)jobject["teachers"];

            var teachers = jTeachers.ToObject<List<Teacher>>();


            return teachers.Find(teacher => teacher.ID == teacherId);
        }

        

        private List<Order> GetOrderList()
        {
            JObject jobject = JObject.Parse(general.JsonString);
            JArray jOrder = (JArray)jobject["order"];

            var order = jOrder.ToObject<List<Order>>();

            return order;
        }

        private string GetCurrentStateOfConference()
        {
            DataTable dt = dB.Reader($"Select Status from {general.TableStateOfConference} where Room = ? limit 1", Currentroom);
            return dt.Rows[0]["status"].ToString();
        }

        #region Moderator

        public async Task LoadModeratorPage(string _currentroom)
        {
            Currentroom = _currentroom;
            await LoadModeratorContent();
            await LoadGeneralContent();
            await LoadIntersections();
        }

        public async Task LoadModeratorContent()
        {
            JObject content = new JObject();
            var currentClassTeachers = new List<Teacher>();
            var teachersString = string.Empty;
            if (GetCurrentStateOfConference() != "completed")
            {
                var currentClassTeacherIds = GetClass(GetCurrentClassName()).Teachers;
                foreach(var teacher in currentClassTeacherIds)
                {
                    currentClassTeachers.Add(GetTeacher(teacher));
                    teachersString = JsonConvert.SerializeObject(currentClassTeachers);
                }
            }

            content.Add("teachers", teachersString);
            content.Add("buttonText", GetButtonText());

            await Clients.Caller.SendAsync("ReceiveModeratorContent", content.ToString());
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

        public async Task LoadIntersections()
        {
            JArray jArrayIntersections = new JArray();
            DataTable dt = dB.Reader($"Select ID from {general.Table_General} WHERE Status='not edited' AND Room <> ? order by ClassOrder limit 1", Currentroom);
            bool isOtherConferenceFinished = dt.Rows.Count == 0 ? true : false;

            if (GetCurrentStateOfConference() == "completed" || isOtherConferenceFinished)  //when the conference is completed, we dont have to load all the intersections again
            {
                jArrayIntersections.Add("");
            }
            else
            {
                string otherclassname = dt.Rows[0]["ID"].ToString();
                MyClasses currentClass = GetClass(GetCurrentClassName());
                MyClasses otherclass = GetClass(otherclassname);


                List<string> otherClassTeachers = otherclass.Teachers.Select(teacher => GetTeacher(teacher).ID).ToList();
                List<string> currentClassTeachers = currentClass.Teachers.Select(teacher => GetTeacher(teacher).ID).ToList();

                List<string> intersections = otherClassTeachers.Intersect(currentClassTeachers).ToList();

                jArrayIntersections = new JArray(intersections);
            }

            await Clients.All.SendAsync("ReveiveIntersections",  jArrayIntersections.ToString());
        }

        #endregion

        #region User

        public async Task LoadRooms()
        {
            JArray jOrder = new JArray(GetOrderList().Select(order => order.Room));

            await Clients.All.SendAsync("ReceiveRooms", jOrder.ToString());
        }

        public async Task LoadUserPageContent(string _currentroom)
        {
            Currentroom = _currentroom;
            await LoadGeneralContent();
        }

        #endregion

        public async Task LoadGeneralContent()
        {
            JObject information = new JObject();
            var currentClass = GetClass(GetCurrentClassName());
            string headOfDepartment = string.Empty;
            string formTeacher = string.Empty;

            switch (GetCurrentStateOfConference())
            {
                case "inactive":

                    formTeacher = JsonConvert.SerializeObject(GetTeacher(currentClass.FormTeacher));
                    headOfDepartment = JsonConvert.SerializeObject(GetTeacher(currentClass.HeadOfDepartment));

                    information.Add("room", Currentroom);
                    information.Add("classname", GetCurrentClassName());
                    information.Add("formTeacher", formTeacher);
                    information.Add("headOfDepartment", headOfDepartment);
                    information.Add("time", "Besprechung noch nicht gestartet");
                    break;

                case "running":
                    DataTable dt = dB.Reader($"SELECT room, start FROM {general.Table_General} WHERE ID = ? limit 1", GetCurrentClassName());
                    formTeacher = JsonConvert.SerializeObject(GetTeacher(currentClass.FormTeacher));
                    headOfDepartment = JsonConvert.SerializeObject(GetTeacher(currentClass.HeadOfDepartment));

                    information.Add("room", dt.Rows[0]["room"].ToString());
                    information.Add("time", dt.Rows[0]["start"].ToString());
                    information.Add("classname", GetCurrentClassName());
                    information.Add("formTeacher", formTeacher);
                    information.Add("headOfDepartment", headOfDepartment);
                    break;

                case "completed":

                    information.Add("room", Currentroom);
                    information.Add("classname", "Alle Klassen abgeschlossen");
                    information.Add("formTeacher", JsonConvert.SerializeObject(new Teacher() { ID="-", Name="-"}));
                    information.Add("headOfDepartment", JsonConvert.SerializeObject(new Teacher() { ID = "-", Name = "-" }));
                    information.Add("time", "-");
                    break;
            }

            information.Add(new JProperty("classesCompleted", GetClassesCompleted()));
            information.Add(new JProperty("classesNotEdited", GetClassesNotEdited()));
            await Clients.All.SendAsync("ReceiveGeneralContent", information.ToString());
        }

        private string GetClassesCompleted()
        {
            var classes = GetOrderList().Find(order => order.Room == Currentroom).Classes;

            if (GetCurrentStateOfConference() == "completed")
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
            var classes = GetOrderList().Find(order => order.Room == Currentroom).Classes;

            if (GetCurrentStateOfConference() == "completed")
            {
                classesNotedited.Add("Keine weiteren Klassen");
            }
            else
            {
                int index = classes.IndexOf(GetCurrentClassName()) + 1; //the current class shouldn't be displayed
                if (index == classes.Count)
                {
                    classesNotedited.Add("Keine weiteren Klassen");
                }
                else
                {
                    classesNotedited = new JArray(classes.GetRange(index, classes.Count - index));
                }
            }
            return classesNotedited.ToString();
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

        public void StartConference()
        {
            WriteTimeInDatabase("start");
            SetStateOfConference("running");
        }

        private void NextClass()
        {
            //current class
            WriteTimeInDatabase("end");   //Write the time when the class is completed
            dB.Query($"UPDATE {general.Table_General} set Status='completed' WHERE ID = ?", GetCurrentClassName());     //Write Status for current class

            if (GetCurrentClassName() == null)
            {
                SetStateOfConference("completed");
            }
            else
            {
                WriteTimeInDatabase("start");     //Write the time when the class is started
            }
        }

        private void SetStateOfConference(string status)
        {
            dB.Query($"Update {general.TableStateOfConference} set Status = ? where Room = ?", status, Currentroom);
        }

        private void WriteTimeInDatabase(string time) //time can be "start or end" (names in the database)
        {
            dB.Query($"UPDATE {general.Table_General} set {time} = ? WHERE ID = ?",
                DateTime.Now.ToLongTimeString(), GetCurrentClassName()) ;
        }

        public async Task SendTeacherCall(int indexOfCalledTeacher, string moderatorID, string _currenroom)
        {
            Currentroom = _currenroom;

            var currentClass = GetClass(GetCurrentClassName());
            var teacherToCall = currentClass.Teachers[indexOfCalledTeacher];

            dB.Query($"INSERT INTO {general.TableTeacherCall}(Moderator, Teacher, Time, Class) VALUES(?,?,?,?)",
                moderatorID, teacherToCall, DateTime.Now.ToLongTimeString(), currentClass.ClassName);

            var message = $"Sie werden in Raum {Currentroom} erwartet";

            await Clients.All.SendAsync("ReceiveTeacherCall", teacherToCall, message);
        }
    }

}


