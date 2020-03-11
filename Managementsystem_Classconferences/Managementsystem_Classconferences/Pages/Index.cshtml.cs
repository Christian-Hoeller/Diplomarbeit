using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Managementsystem_Classconferences.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace Managementsystem_Classconferences.Pages
{
    public class IndexModel : PageModel
    {
        private DBConnection dB = new DBConnection();
        private General general = new General();

        public string SchoolYear
        {
            get
            {
                if(DateTime.Now.Month > 8)  //If the current schoolyear is over
                {
                    return $"Schuljahr {DateTime.Now.Year}/{DateTime.Now.Year + 1}";
                }
                else
                {
                    return $"Schuljahr {DateTime.Now.Year - 1}/{DateTime.Now.Year}";
                }
            }
        }

        private DBConnection DB
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


        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated)      
            {
                var identity = User.Identity as ClaimsIdentity;
                string teacherId = identity.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
                DataTable TeacherRight = DB.Reader($"SELECT UserGroup from {general.TableUserRights} WHERE TeacherID LIKE ? LIMIT 1", teacherId);

                if (TeacherRight.Rows.Count != 0)
                {
                    if (Convert.ToInt64(TeacherRight.Rows[0][0]) == 0)  // 0 = Moderator and 1 = Admin
                    {
                        return new RedirectToPageResult("roomselection");   //redirect to roomSelection if
                    }
                    else return new RedirectToPageResult("admin_settings");
                }
                else return new RedirectToPageResult("conference");

            }
            else return null;
        }
    }
}
