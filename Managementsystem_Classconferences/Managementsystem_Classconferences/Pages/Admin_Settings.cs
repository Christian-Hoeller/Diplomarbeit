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

            List<Order> classes = jOrder.ToObject<List<Order>>();

            DeleteEverythingFromDatabase();

            foreach (var orderitem in classes)
            {
                string roomonly = orderitem.Room.Split(' ')[0];
                int ordercounter = 1;

                SetStateSettings(roomonly);

                foreach (string myclass in orderitem.Classes)
                {
                    db.Query($"INSERT INTO {general.Table_General} (ID, Room, ClassOrder, Status) VALUES('{myclass}', '{roomonly}', {ordercounter}, 'not edited')");

                    ordercounter++;
                }
            }
        }

        private void SetStateSettings(string room)
        {
            db.Query($"INSERT INTO {general.TableStateOfConference} (Room, Status) VALUES ('{room}', 'inactive')");
        }

        private void DeleteEverythingFromDatabase()
        {
            db.Query($"DELETE FROM {general.Table_General}");
            db.Query($"DELETE FROM {general.TableStateOfConference}");
        }
    }
}