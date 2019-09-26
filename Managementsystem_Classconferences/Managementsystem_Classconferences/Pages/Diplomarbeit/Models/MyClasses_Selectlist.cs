using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Models
{
    public class MyClasses_Selectlist
    {
        public List<SelectListItem> Selectllist { get; set; }
        public Teacher teacher { get; set; }
    }
}
