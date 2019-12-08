using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Managementsystem_Classconferences.Models
{
    public class Teacher
    {
        [Key]
        public string ID { get; set; }
        public string Name { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Name_Short { get; set; }
        public string Subject { get; set; }

    }
}
