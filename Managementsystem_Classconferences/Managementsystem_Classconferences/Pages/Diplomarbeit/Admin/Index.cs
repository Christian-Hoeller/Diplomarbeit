using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Classes;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Admin
{
    public class IndexModel : PageModel
    {

        #region Variables

        private General general = new General();

        #endregion

        [HttpPost]
        public void OnPostReset()
        {
            using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command_general = connection.CreateCommand();
                command_general.CommandText = $"UPDATE {general.Table_General} set Status = 'not edited', start=null, end=null";

                var command_state = connection.CreateCommand();
                command_state.CommandText = $"Update {general.Table_State} set Status = 'inactive'";


                connection.Open();

                command_general.ExecuteNonQuery();
                command_state.ExecuteNonQuery();

                connection.Close();
            }

        }

        public void OnPostSetJsonData()
        {



            JObject jobject = JObject.Parse(general.JsonString);  //creates a new json Object
            JArray jOrder = (JArray)jobject["order"];   //Puts all the Classes in a new Json Array

            List<Order> classes = jOrder.ToObject<List<Order>>();

            DeleteEverythingFromDatabase();

            using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                foreach (var orderitem in classes)
                {

                    string roomonly = orderitem.Room.Split(' ')[0];
                    int ordercounter = 1;

                    SetStateSettings(roomonly);


                    foreach (string myclass in orderitem.Classes)
                    {

                        var command = connection.CreateCommand();
                        command.CommandText = $"INSERT INTO {general.Table_General} (ID, Room, ClassOrder, Status) VALUES('{myclass}', '{roomonly}', {ordercounter}, 'not edited')";

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();

                        ordercounter++;
                    }
                    ordercounter = 1;
                }
            }

        }

        private void SetStateSettings(string room)
        {
            using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command = connection.CreateCommand();
                command.CommandText = $"INSERT INTO {general.Table_State} (Room, Status) VALUES ('{room}', 'inactive')";

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void DeleteEverythingFromDatabase()
        {
            using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command_deletefrom_General = connection.CreateCommand();
                command_deletefrom_General.CommandText = $"DELETE FROM {general.Table_General}";

                var command_deletefrom_State = connection.CreateCommand();
                command_deletefrom_State.CommandText = $"DELETE FROM {general.Table_State}";

                connection.Open();
                command_deletefrom_General.ExecuteNonQuery();      //Delete from Table General
                command_deletefrom_State.ExecuteNonQuery();         //Delete from Table State


            }
        }
    }
}