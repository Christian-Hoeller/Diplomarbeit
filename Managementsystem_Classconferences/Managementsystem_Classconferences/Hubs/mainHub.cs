using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
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
        private DBConnection db = new DBConnection();

        private List<Order> order;

        private MyClasses currentclass;
        private string currentClassName;
        private string nextClassName;
        private string lastClassName;
        private string state_endOrStart;
        private string text_Conference_State;
        private string currentroom;

        #endregion

        #region Properties

        public MyClasses Currentclass
        {
            get
            {
                if(currentclass == null)
                {
                    currentclass = general.GetClass(CurrentClassName);
                }
                return currentclass;
            }
        }

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
            set
            {
                currentClassName = value;
            }
        }

        public string LastClassName
        {
            get
            {
                using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
                {
                    var command = connection.CreateCommand();

                    //SQL Command to set the new CurrentClassName (get ClassName)
                    command.CommandText = $"Select ID from {general.Table_General} WHERE Status='completed' AND Room = '{Currentroom}' ORDER BY ClassOrder DESC limit 1";
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                lastClassName = reader.GetString(0);
                            }
                        }
                        else
                        {
                            lastClassName = CurrentClassName;
                        }
                    }
                }
                return lastClassName;
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

       

        public string State_OfConference
        {
            get
            {
                using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = $"Select Status from {general.Tablename_State_of_conference} where Room = '{Currentroom}'";


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
                    command.CommandText = $"Update {general.Tablename_State_of_conference} set Status = '{value}' where Room = '{Currentroom}'"; 
                                                                                                                                       
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

        public List<Order> Order
        {
            get
            {
                JObject jobject = JObject.Parse(general.JsonString); 
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
            await LoadUserViewInfo(_currentroom);
        }
        
        public async Task LoadInformation(string _currentroom)
        {
            Currentroom = _currentroom;

            if (State_OfConference != "completed")
            {
                await Clients.Caller.SendAsync("ReveiveLoadInformation",
                    CurrentClassName, Buttontext, Get_classes_from_JSON("previous"), Get_classes_from_JSON("next"));
            }
            else
            {
                CurrentClassName = null;
                await Clients.Caller.SendAsync("ReveiveLoadInformation", 
                    "Alle Klassen abgeschlossen", "Konferenz abgeschlossen", Get_classes_from_JSON("previous"), "abgeschlossen");
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

        public async Task LoadUserViewInfo(string _currentroom)
        {
            Currentroom = _currentroom;


            JObject myobject = new JObject();

            DataTable dt = db.Reader($"SELECT room, start FROM {general.Table_General} WHERE ID='{Currentclass.ClassName}'");
            if(dt.Rows.Count == 0)
            {
                myobject.Add("room", "-");
                myobject.Add("classname", "-");
                myobject.Add("formteacher", "-");
                myobject.Add("head_of_department", "-");
                myobject.Add("time", "-");
            }
            else
            {
                myobject.Add("room", dt.Rows[0]["room"].ToString());
                myobject.Add("time", dt.Rows[0]["start"].ToString());
                myobject.Add("classname", Currentclass.ClassName);
                myobject.Add("formteacher", Currentclass.FormTeacher);
                myobject.Add("head_of_department", Currentclass.HeadOfDepartment);


            }
           
            myobject.Add("classes_completed", Get_classes_from_JSON("previous"));
            myobject.Add("classes_not_edited", Get_classes_from_JSON("next"));



            await Clients.All.SendAsync("ReceiveUserViewInfo", myobject.ToString());

        }
        
        public async Task LoadRooms()
        {
            string orderlist = null;
            foreach(Order orderitem in Order)
            {
                if(orderlist != null)
                    orderlist += $";{orderitem.Room_only};";
                else
                    orderlist += orderitem.Room_only;
            }
        
            await Clients.All.SendAsync("ReceiveRooms", orderlist);
        }

        #endregion

        #region Methods

        private string GetIntersections()
        {
            if (State_OfConference == "completed")  //when the conference is completed, we dont have to load all the intersections again
                return "Keine Überschneidungen";


            string otherclassname = string.Empty;
            string sqlstring = $"Select ID from {general.Table_General} WHERE Status='not edited' AND Room <> '{Currentroom}' order by ClassOrder limit 1";
            DataTable dt = db.Reader(sqlstring);

            if (dt.Rows.Count == 0)
            {
                return "Keine Überschneidungen";
            }
            else
                otherclassname = dt.Rows[0]["ID"].ToString();


            MyClasses otherclass = general.GetClass(otherclassname);

            List<Teacher> intersections = new List<Teacher>();
            //loop the Lists to find the intersections / duplicates in the list and put them in a new list
            foreach (Teacher teacher in Currentclass.Teachers)
            {
                Teacher intersection = otherclass.Teachers.Find(x => x.ID == teacher.ID);
                if (intersection != null)
                    intersections.Add(intersection);
            }

            return JoinTeachersListWithChar(';', intersections);
        }

        private string JoinTeachersListWithChar(char separator, List<Teacher> intersections)
        {

            List<string> intersections_string = new List<string>();
            foreach (var teacher in intersections)
            {
                intersections_string.Add(teacher.Name);
            }
            return string.Join(separator, intersections_string);
        }

        public string GetTeachers()
        {
            if (State_OfConference == "completed")  //when the conference is completed, we dont have to load all the teachers again
                return "Keine Lehrer";


            return JoinTeachersListWithChar(';', Currentclass.Teachers);
        }


        public string Get_classes_from_JSON(string type)
        {
            JObject jobject = JObject.Parse(general.JsonString);
            JArray jOrder = (JArray)jobject["order"];

            List<Order> orderlist = jOrder.ToObject<List<Order>>();

            List<string> classes_in_order = orderlist.Find(x => x.Room.Split(' ')[0] == Currentroom).Classes;    //find the Classes in the order where the room is equal to the CurrentRoom
            List<string> classes = new List<string>();

            int index = classes_in_order.IndexOf(CurrentClassName);     //get the index of the class in the list

            if (type == "next" || index == -1)     //if the type is next we return all the classes that haven't been reviewed yet
            {
                //if index is -1 then the Currentclass isn't in the list which means that all classes are done
                for (int i = index + 1; i < classes_in_order.Count; i++)    //we want to exclude the currentclass from the loop so we start at index + 1
                {
                    classes.Add(classes_in_order[i]);
                }
            }
            else
            {
                for (int i = 0; i < index; i++)
                {
                    classes.Add(classes_in_order[i]);
                }
            }
            return string.Join(';', classes);  //return the list joined with ';'
        }
       

        public void StartConference()
        {
            WriteTime("start");
            State_OfConference = "running";
        }

        private void WriteTime(string time) //time can be "start or end" (names in the database)
        {
            DateTime date = DateTime.Now;
            string timeonly = date.ToLongTimeString();
            db.Query($"UPDATE {general.Table_General} set {time} = '{timeonly}' WHERE ID = '{CurrentClassName}'");
        }

        private void NextClass()
        {
            //current class
            WriteTime("end");   //Write the time when the class is completed
            db.Query($"UPDATE {general.Table_General} set Status='completed' WHERE ID = '{currentClassName}'");     //Write Status for current class

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
            DataTable dt = db.Reader($"SELECT * FROM {general.Table_General} WHERE Status = 'not edited' AND Room = '{Currentroom}'");
            if (dt.Rows.Count == 0)
                return true;
            
            return false;
        }
        #endregion
    }

}


