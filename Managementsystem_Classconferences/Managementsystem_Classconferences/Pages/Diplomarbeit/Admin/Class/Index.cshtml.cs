using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Classes;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Models;
using System.Data.SQLite;

namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Admin.Class
{
    public class IndexModel : PageModel
    {
        private General general = new General();
        private string message;
        

        #region Properties

        public List<MyClasses> Classeslist
        {
            get
            {
                return general.Classeslist;
            }
        }

        public string Message
        {
            get
            {
                if(message == null)
                {
                    message = Request.Query["handler"];
                }
                return message == null ? "" : message.Replace('_', ' ');

            }
        }


        #endregion

        public IActionResult OnPostEdit(string id)
        {
            return new RedirectToPageResult("Classes_Edit", id);
        }

        public IActionResult OnPostDelete(string id)
        {
            //Delete The Class from the classes[]
            JObject obj = JObject.Parse(general.JsonString);
            JArray jclasses = (JArray)obj["classes"];
            bool found = false;

            foreach (JObject obj_class in jclasses)
            {
                if ((string)obj_class["Classname"] == id)
                {
                    obj_class.Remove(); //remove the class object


                    //Find the class element in the order array
                    JArray jorder = (JArray)obj["order"];

                    foreach (JObject obj_order in jorder)   //loop the order to find the class
                    {
                        JArray jorder_classes = (JArray)obj_order["Classes"];
                        foreach (JObject obj_order_class in jorder_classes)
                        {
                            if ((string)obj_order_class["class"] == id)
                            {
                                obj_order_class.Remove(); //remove the element from the order
                                found = true;
                            }
                            if (found)
                                break;
                        }
                        if (found)
                            break;
                    }

                    Delete_Class_From_Database(id);


                    using (StreamWriter writer = new StreamWriter(general.Path_Json))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(writer, obj);
                    }
                    return new RedirectToPageResult("Index", $"Die_Klasse_{id}_wurde_gelöscht");

                }

            }
            return new RedirectToPageResult("Index", "Klasse_konnte_nicht_gelöscht_werden");


        }

        private void Delete_Class_From_Database(string classname)
        {
            using (var connection = new SQLiteConnection($"Data Source={general.Path_DB}"))    //SQLite connection with the path(this is the database not the table)
            {
                var command = connection.CreateCommand();

                //SQL Command to set the new CurrentClassName (get ClassName)
                command.CommandText = $"DELETE FROM {general.Database_General} WHERE Classname = '{classname}'";

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

      
    }
}