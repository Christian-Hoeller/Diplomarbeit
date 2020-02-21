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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Managementsystem_Classconferences.Hubs
{
    public class MainHub : Hub
    {

        #region variables


        private General general = new General();
        private DBConnection dB = new DBConnection();

        private string text_Conference_State;
        private List<Order> order;

        #endregion

        #region properties

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

        #endregion

        private string Currentroom { get; set; }

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
                item.RoomOnly = item.Room.Split(' ')[0];
            }

            return order;
        }

        private string GetButtonText()
        {
            switch (GetCurrentStateOfConference())
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
            await LoadModeratorViewInfo(_currentroom);
            await LoadUserViewInfo(_currentroom);
        }

        public async Task LoadModeratorViewInfo(string _currentroom)
        {
            Currentroom = _currentroom;

            JObject obj = new JObject();

            if (GetCurrentStateOfConference() != "completed")
            {
                obj.Add("classname", GetCurrentClassName());
                obj.Add("buttontext", GetButtonText());
                obj.Add(new JProperty("classes_completed", GetClassesFromJSON("previous")));
                obj.Add(new JProperty("classes_not_edited", GetClassesFromJSON("next")));
            }
            else
            {
                obj.Add("classname", "Alle Klassen abgeschlossen");
                obj.Add("buttontext", "Konferenz abgeschlossen");
                obj.Add(new JProperty("classes_completed", GetClassesFromJSON("previous")));
                obj.Add(new JProperty("classes_not_edited", "abgeschlossen"));
            }

            await Clients.Caller.SendAsync("ReveiveLoadInformation", obj.ToString());

            await SendIntersections();
            await SendTeachers();

        }

        public async Task SendTeachers()
        {
            await Clients.Caller.SendAsync("ReceiveTeachers", GetTeachersOfCurrentClass());
        }

        public async Task SendIntersections()
        {
            await Clients.All.SendAsync("ReceiveIntersections", GetIntersections());
        }

        public async Task LoadUserViewInfo(string _currentroom)
        {
            Currentroom = _currentroom;
            JObject information = new JObject();
            var currentClass = GetClass(GetCurrentClassName());

            switch (GetCurrentStateOfConference())
            {
                case "inactive":


                    information.Add("room", Currentroom);
                    information.Add("classname", GetCurrentClassName());
                    information.Add("formteacher", currentClass.FormTeacher);
                    information.Add("head_of_department", currentClass.HeadOfDepartment);
                    information.Add("time", "Besprechung noch nicht gestartet");
                    information.Add(new JProperty("classes_not_edited", GetClassesFromJSON("next")));
                    break;
                case "running":
                    DataTable dt = DB.Reader($"SELECT room, start FROM {General.Table_General} WHERE ID = ? limit 1", GetCurrentClassName());

                    information.Add("room", dt.Rows[0]["room"].ToString());
                    information.Add("time", dt.Rows[0]["start"].ToString());
                    information.Add("classname", GetCurrentClassName());
                    information.Add("formteacher", currentClass.FormTeacher);
                    information.Add("head_of_department", currentClass.FormTeacher);
                    information.Add(new JProperty("classes_not_edited", GetClassesFromJSON("next")));
                    break;

                case "completed":
                    information.Add("room", Currentroom);
                    information.Add("classname", "Alle Klassen abgeschlossen");
                    information.Add("formteacher", "-");
                    information.Add("head_of_department", "-");
                    information.Add("time", "-");
                    information.Add("classes_not_edited", "-");
                    break;
            }

            information.Add("classes_completed", GetClassesFromJSON("previous"));

            await Clients.All.SendAsync("ReceiveUserViewInfo", information.ToString());

        }

        public async Task LoadRooms()
        {
            var order = GetOrderList();
            JArray jArrayRooms = new JArray(order.Select(room => room.RoomOnly).ToList());
            await Clients.All.SendAsync("ReceiveRooms", jArrayRooms.ToString());
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

        public string GetTeachersOfCurrentClass()
        {
            JArray jArrayTeachers = new JArray();
            if (GetCurrentStateOfConference() is "completed")  //when the conference is completed, we dont have to load all the teachers again
            {
                jArrayTeachers.Add("Keine Leherer");
            }
            else
            {
                var currentClass = GetClass(GetCurrentClassName());
                jArrayTeachers = new JArray(currentClass.Teachers.Select(teacher => teacher.Name));
            }

            return jArrayTeachers.ToString();
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

        public string GetClassesFromJSON(string type)
        {
            var orderlist = GetOrderList();
            List<string> classesInOrder = orderlist.Find(order => order.Room.Split(' ')[0] == Currentroom).Classes;
            List<string> returnClasses = new List<string>();

            if (GetCurrentStateOfConference() == "completed")
            {
                returnClasses = classesInOrder;
            }
            else
            {
                int index = classesInOrder.IndexOf(GetCurrentClassName());
                if (type == "next")
                {
                    returnClasses = classesInOrder.Skip(index + 1).Take(classesInOrder.Count - index).ToList();
                }
                else
                {
                    returnClasses = classesInOrder.Take(index).ToList();
                }
            }

            return new JArray(returnClasses).ToString();
        }

    }

}


