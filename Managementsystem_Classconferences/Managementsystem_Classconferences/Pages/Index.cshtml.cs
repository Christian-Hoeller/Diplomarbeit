using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Managementsystem_Classconferences.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated)      //redirect User to moderatorSelection after the login
            {
            return new RedirectToPageResult("moderatorselection");
            }
            else
            {
                return null;
            }
        }
    }
}
