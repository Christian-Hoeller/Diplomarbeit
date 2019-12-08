using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Managementsystem_Classconferences.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;

namespace Managementsystem_Classconferences.Pages
{
    public class ModeratorSelectionModel : PageModel
    {
        private List<Order> orderlist;
        private General general = new General();



        #region Properties

        public List<Order> OrderList
        {
            get
            {
                if (orderlist == null)
                {
                    JObject jobject = JObject.Parse(general.JsonString);  //creates a new json Object
                    JArray jOrder = (JArray)jobject["order"];     //puts everey teachers object of the json file in a new JasonArray

                    orderlist = jOrder.ToObject<List<Order>>();     //put the JasonArray in to the teacherslist

                    foreach (var item in orderlist)
                    {
                        item.Room_only = item.Room.Split(' ')[0];
                    }
                }
                return orderlist;
            }
        }

 


        #endregion

        public IActionResult OnPost(string room)
        {
            HttpContext.Items["classroom"] = room;

            return new RedirectToPageResult("Moderator", room);
        }
    }

}