using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Models
{
    public class Class
    {
        public string ClassName { get; set; }
        public string HeadOfDepartment { get; set;}
        public string FormTeacher { get; set; }
        public List<Teacher> Teachers { get; set; }
    }
}
