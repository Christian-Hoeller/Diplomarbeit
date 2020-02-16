﻿using System;
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
        private MyClasses currentclass;

        private string text_Conference_State;
        private List<Order> order;


        #endregion

        #region Properties

        public General General
        {
            get
            {
                if(general == null)
                {
                    general = new General();
                }
                return general;
            }
        }

        public DBConnection DB
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

        public MyClasses Currentclass
        {
            get
            {
                if(currentclass == null)
                {
                    currentclass = General.GetClass(CurrentClassName);
                }
                return currentclass;
            }
        }

        public string Currentroom { get; set; }

        public string CurrentClassName
        {
            get
            {
                DataTable dt = DB.Reader($"Select ID from {General.Table_General} WHERE Status='not edited' AND Room = '{Currentroom}' order by ClassOrder limit 1");
                if (dt.Rows.Count == 0)
                    return null;
                return dt.Rows[0]["id"].ToString();
            }
        }

        public string NextClassName
        {
            get
            {
                    DataTable dt = DB.Reader($"Select ID from {General.Table_General} WHERE Status='not edited' AND Room ='{Currentroom}' order by ClassOrder limit 1");
                    return dt.Rows[0]["id"].ToString();
            }
        }

        public string State_OfConference
        {
            get
            {
                DataTable dt = DB.Reader($"Select Status from {General.Tablename_State_of_conference} where Room = '{Currentroom}' limit 1");
                return dt.Rows[0]["status"].ToString();
            }
            set
            {
                DB.Query($"Update {General.Tablename_State_of_conference} set Status = '{value}' where Room = '{Currentroom}'");
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

        public List<Order> Order
        {
            get
            {
                JObject jobject = JObject.Parse(General.JsonString); 
                JArray jOrder = (JArray)jobject["order"];     

                order = jOrder.ToObject<List<Order>>();
                foreach(Order item in order)
                {
                    item.Room_only = item.Room.Split(' ')[0];
                }

                return order;
            }
        }


        #endregion

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
            await LoadModeratorViewInfo(_currentroom);
            await LoadUserViewInfo(_currentroom);
        }

        public void StartConference()
        {
            WriteTime("start");
            State_OfConference = "running";
        }

        private void NextClass()
        {
            //current class
            WriteTime("end");   //Write the time when the class is completed
            DB.Query($"UPDATE {General.Table_General} set Status='completed' WHERE ID = '{CurrentClassName}'");     //Write Status for current class

            if(CurrentClassName == null)    
            {
                State_OfConference = "completed";
            }
            else
            {
                WriteTime("start");     //Write the time when the class is started
            }
        }

        private void WriteTime(string time) //time can be "start or end" (names in the database)
        {
            DateTime date = DateTime.Now;
            string timeonly = date.ToLongTimeString();
            DB.Query($"UPDATE {General.Table_General} set {time} = '{timeonly}' WHERE ID = '{CurrentClassName}'");
        }

        public async Task LoadModeratorViewInfo(string _currentroom)
        {
            Currentroom = _currentroom;

            JObject obj = new JObject();

            if (State_OfConference != "completed")
            {
                obj.Add("classname", CurrentClassName);
                obj.Add("buttontext", Buttontext);
                obj.Add(new JProperty("classes_completed", Get_classes_from_JSON("previous")));
                obj.Add(new JProperty("classes_not_edited", Get_classes_from_JSON("next")));
            }
            else
            {
                obj.Add("classname", "Alle Klassen abgeschlossen");
                obj.Add("buttontext", "Konferenz abgeschlossen");
                obj.Add(new JProperty("classes_completed", Get_classes_from_JSON("previous")));
                obj.Add(new JProperty("classes_not_edited", "abgeschlossen"));
            }

            await Clients.Caller.SendAsync("ReveiveLoadInformation", obj.ToString());

            await SendIntersections();
            await SendTeachers();

        }

        public async Task SendTeachers()
        {
            await Clients.Caller.SendAsync("ReceiveTeachers", GetTeachers());
        }

        public string GetTeachers()
        {
            JArray jArrayTeachers = new JArray();
            if (State_OfConference == "completed")  //when the conference is completed, we dont have to load all the teachers again
            {
                jArrayTeachers.Add("Keine Leherer");
            }
            else
            {
                jArrayTeachers = new JArray(Currentclass.Teachers.Select(teacher => teacher.Name));
            }

            return jArrayTeachers.ToString();
        }

        public async Task SendIntersections()
        {
            await Clients.All.SendAsync("ReceiveIntersections", GetIntersections());
        }

        private string GetIntersections()
        {
            JArray jArrayIntersections = new JArray();
            string sqlstring = $"Select ID from {General.Table_General} WHERE Status='not edited' AND Room <> '{Currentroom}' order by ClassOrder limit 1";
            DataTable dt = DB.Reader(sqlstring);


            if (State_OfConference == "completed" || dt.Rows.Count == 0)  //when the conference is completed, we dont have to load all the intersections again
            {
                jArrayIntersections.Add("Keine Überschneidungen");
            }
            else
            {
                string otherclassname = dt.Rows[0]["ID"].ToString();
                MyClasses otherclass = General.GetClass(otherclassname);

                List<Teacher> otherClassTeacherss = otherclass.Teachers.ToList();
                List<Teacher> currentClassTeachers = Currentclass.Teachers.ToList();

                List<Teacher> intersections = otherClassTeacherss.Intersect(currentClassTeachers).ToList();

                jArrayIntersections = new JArray(intersections.Select(teacher => teacher.Name));
            }

            return jArrayIntersections.ToString();
        }

        public async Task LoadUserViewInfo(string _currentroom)
        {
            Currentroom = _currentroom;

            JObject information = new JObject();

            switch (State_OfConference)
            {
                case "inactive":
                    information.Add("room", Currentroom);
                    information.Add("classname", CurrentClassName);
                    information.Add("formteacher", Currentclass.FormTeacher);
                    information.Add("head_of_department", Currentclass.HeadOfDepartment);
                    information.Add("time", "Besprechung noch nicht gestartet");
                    information.Add(new JProperty("classes_not_edited", Get_classes_from_JSON("next")));
                    break;
                case "running":
                    DataTable dt = DB.Reader($"SELECT room, start FROM {General.Table_General} WHERE ID='{CurrentClassName}' limit 1");

                    information.Add("room", dt.Rows[0]["room"].ToString());
                    information.Add("time", dt.Rows[0]["start"].ToString());
                    information.Add("classname", CurrentClassName);
                    information.Add("formteacher", Currentclass.FormTeacher);
                    information.Add("head_of_department", Currentclass.FormTeacher);
                    information.Add(new JProperty("classes_not_edited", Get_classes_from_JSON("next")));
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

            information.Add("classes_completed", Get_classes_from_JSON("previous"));

            await Clients.All.SendAsync("ReceiveUserViewInfo", information.ToString());

        }

        public async Task LoadRooms()
        {
            JArray jArrayRooms = new JArray(Order.Select(room => room.Room_only).ToList());
            await Clients.All.SendAsync("ReceiveRooms", jArrayRooms.ToString());
        }


        public string Get_classes_from_JSON(string type)
        {
            List<string> classesInOrder = Order.Find(order => order.Room.Split(' ')[0] == Currentroom).Classes;
            List<string> classes = new List<string>();

            if (State_OfConference == "completed")
            {
                classes = classesInOrder;
            }
            else
            {
                int index = classesInOrder.IndexOf(CurrentClassName);
                if (type == "next")
                {
                    classes = classesInOrder.Skip(index + 1).Take(classesInOrder.Count - index).ToList();
                }
                else
                {
                    classes = classesInOrder.Take(index).ToList();
                }
            }

            return new JArray(classes).ToString();  //return classes as a JasonArray
        }
      
    }

}


