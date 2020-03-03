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

                List<Teacher> GetTeachersList()
                {
                    JArray jTeachers = (JArray)jobject["teachers"];     //puts everey teachers object of the json file in a new JasonArray

                    List<Teacher> teachers = jTeachers.ToObject<List<Teacher>>();     //put the JasonArray in to the teacherslist
                    teachers.ForEach(teacher => teacher.Name_Short = teacher.ID.Split('@')[0].ToUpper());

                    return teachers;
                }

                var teacherslist = GetTeachersList();

                for (int i = 0; i < myclass.Teachers.Count; i++)
                {
                    myclass.Teachers[i] = teacherslist.Find(x => x.ID == myclass.Teachers[i].ID);
                }
                return myclass;
            }
            else
                return null;
        }

        private List<Order> GetOrderList()
        {
            JObject jobject = JObject.Parse(general.JsonString);
            JArray jOrder = (JArray)jobject["order"];

            var order = jOrder.ToObject<List<Order>>();
            foreach (Order item in order)
            {
                item.Room_only = item.Room.Split(' ')[0];
            }

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

        private string GetIntersections()
        {
            JArray jArrayIntersections = new JArray();
            string sqlstring = $"Select ID from {general.Table_General} WHERE Status='not edited' AND Room <> ? order by ClassOrder limit 1";
            DataTable dt = dB.Reader(sqlstring, Currentroom);


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

        #endregion

        #region User

        public async Task LoadRooms()
        {
            var order = GetOrderList();
            JArray jArrayRooms = new JArray(order.Select(room => room.Room_only).ToList());
            await Clients.All.SendAsync("ReceiveRooms", jArrayRooms.ToString());
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
                    DataTable dt = dB.Reader($"SELECT room, start FROM {general.Table_General} WHERE ID = ? limit 1", GetCurrentClassName());

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

        private string GetClassesCompleted()
        {
            var classes = GetOrderList().Find(order => order.Room.Split(' ')[0] == Currentroom).Classes;

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
            var classes = GetOrderList().Find(order => order.Room.Split(' ')[0] == Currentroom).Classes;

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
            DateTime date = DateTime.Now;
            string timeonly = date.ToLongTimeString();
            dB.Query($"UPDATE {general.Table_General} set {time} = ? WHERE ID = ?", timeonly, GetCurrentClassName()) ;
        }

        public async Task SendTeacherCall(int indexOfTeacher, string _currenroom)
        {
            Currentroom = _currenroom;

            var currentClass = GetClass(GetCurrentClassName());
            var teacherToCall = currentClass.Teachers[indexOfTeacher].ID;

            //here comes the Clients.(CallSomeone) command to call out a teacher

            await Clients.All.SendAsync("ReceiveTeacherCall", teacherToCall.ToString());
        }
    }

}


