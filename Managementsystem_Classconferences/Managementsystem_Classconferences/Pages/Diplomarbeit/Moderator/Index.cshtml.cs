using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Moderator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Moderator
{
    public class IndexModel : PageModel
    {
        private List<Order> orderlist;
        private string jsonstring;
        private string path_json;



        #region Properties

        public List<Order> OrderList
        {
            get
            {
                if (orderlist == null)
                {
                    JObject jobject = JObject.Parse(JsonString);  //creates a new json Object
                    JArray jOrder = (JArray)jobject["order"];     //puts everey teachers object of the json file in a new JasonArray

                    orderlist = jOrder.ToObject<List<Order>>();     //put the JasonArray in to the teacherslist
                    foreach (Order order in orderlist)
                    {
                        order.Room_only = order.Room.Split(' ')[0];
                    }
                }
                return orderlist;
            }
        }

        private string Path_Json
        {
            get
            {
                if (path_json == null)
                {
                    string root = "wwwroot";
                    string location = "json";
                    string fileName = "conference-info.json";

                    path_json = Path.Combine(        //locate the path where the file is in 
                    Directory.GetCurrentDirectory(),
                    root,
                    location,
                    fileName);
                }
                return path_json;
            }
        }

        private string JsonString
        {
            get
            {
                if (jsonstring == null)
                {
                    //the filetext has to be converted into a string to bind it to the jsonstring variable
                    FileStream fileStream = new FileStream(Path_Json, FileMode.Open);
                    StreamReader reader = new StreamReader(fileStream);

                    using (reader)
                    {
                        do
                        {
                            jsonstring += reader.ReadLine();        //Reads every line and appends it to the jsonstring, in order to get the whole
                        }                                           //File-Content into a single variable
                        while (!reader.EndOfStream);
                    }
                }
                return jsonstring;
            }
        }



        #endregion

        public IActionResult OnPost(string room)
        {
            return new RedirectToPageResult("Moderator", room);
        }
    }

}