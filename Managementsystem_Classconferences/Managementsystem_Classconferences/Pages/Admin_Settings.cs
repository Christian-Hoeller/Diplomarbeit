using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Managementsystem_Classconferences.Classes;
using Managementsystem_Classconferences.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Managementsystem_Classconferences.Pages
{
    public class Admin_SettingsModel : PageModel
    {
        private General general = new General();
        private DBConnection db = new DBConnection();

        [HttpPost]
        public void OnPostReset()
        {
            db.Query($"UPDATE {general.Table_General} set Status = 'not edited', start=null, end=null");
            db.Query($"Update {general.TableStateOfConference} set Status = 'inactive'");
        }

        public void OnPostSetJsonData()
        {
            JObject jobject = JObject.Parse(general.JsonString);  //creates a new json Object
            JArray jOrder = (JArray)jobject["order"];   //Puts all the Classes in a new Json Array

            List<Order> orderlist = jOrder.ToObject<List<Order>>();

            DeleteEverythingFromDatabase();

            foreach (var orderitem in orderlist)
            {
                int ordercounter = 1;

                SetStateSettings(orderitem.Room);

                foreach (string classitem in orderitem.Classes)
                {
                    db.Query($"INSERT INTO {general.Table_General} (ID, Room, ClassOrder, Status) VALUES(?,?,?, 'not edited')", classitem, orderitem.Room, ordercounter);

                    ordercounter++;
                }
            }
        }

        private void SetStateSettings(string room)
        {
            db.Query($"INSERT INTO {general.TableStateOfConference} (Room, Status) VALUES (?, 'inactive')", room);
        }

        private void DeleteEverythingFromDatabase()
        {
            db.Query($"DELETE FROM {general.Table_General}");
            db.Query($"DELETE FROM {general.TableStateOfConference}");
        }
    }
}