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
            if (User.Identity.IsAuthenticated)      //redirect User to moderatorSelection after the login
            {
                var identity = User.Identity as ClaimsIdentity;
                string teacherId = identity.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
                DataTable TeacherID = DB.Reader("SELECT TeacherID from Moderators WHERE TeacherID LIKE ? LIMIT 1", teacherId);

                if (TeacherID.Rows.Count != 0)
                {
                    return new RedirectToPageResult("moderatorselection");
                }
                else return new RedirectToPageResult("conference");

            }
            else
            {
                return null;
            }
        }
    }
}
