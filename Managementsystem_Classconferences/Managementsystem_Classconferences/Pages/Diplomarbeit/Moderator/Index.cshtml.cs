using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Managementsystem_Classconferences.Pages.Diplomarbeit.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Moderator
{
    public class IndexModel : PageModel
    {
        private string jsonstring;
        private string path_JSON;
        private List<Order> orderlist = new List<Order>();

        #region Properties

        public string Jsonstring
        {
            get
            {
                if (jsonstring == null)
                {


                    //the filetext has to be converted into a string to bind it to the jsonstring variable
                    FileStream fileStream = new FileStream(Path_JSON, FileMode.Open);
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

        public string Path_JSON
        {
            get
            {
                string root = "wwwroot";
                string location = "json";
                string fileName = "conference-info.json";
                //lcoate the path that the file is located in
                path_JSON = Path.Combine(
                Directory.GetCurrentDirectory(),
                root,
                location,
                fileName);

                return path_JSON;
            }
        }

        public List<Order> Orderlist
        {
            get
            {
                JObject jobject = JObject.Parse(Jsonstring);  //creates a new json Object
                JArray jOrder = (JArray)jobject["order"];

                orderlist = jOrder.ToObject<List<Order>>();
                foreach (Order order in orderlist)
                {
                    order.Room_short = order.Room.Split(' ')[0];
                }
                return orderlist;
            }
        }


        #endregion

        public RedirectToPageResult OnPost(string room)
        {
            return new RedirectToPageResult("Moderator", room);
        }
    }
}