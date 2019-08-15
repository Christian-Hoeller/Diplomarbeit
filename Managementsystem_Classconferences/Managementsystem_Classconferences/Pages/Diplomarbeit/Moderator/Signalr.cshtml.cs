using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Moderator
{
    public class SignalrModel : PageModel
    {
        public void OnGet()
        {
        }

        public JsonResult OnGetData()
        {
            List<string> teachers = new List<string>();
            teachers.Add("SOEK");
            teachers.Add("ABLD");
            string message = null;

            foreach (var item in teachers)
            {
                if(message != null)
                {
                    message += ';';
                    message += item;
                }
                else
                {
                    message += item;
                }
            }
            return new JsonResult(message);
        }
    }
}