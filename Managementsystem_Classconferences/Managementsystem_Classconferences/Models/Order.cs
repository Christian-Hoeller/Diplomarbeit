using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Managementsystem_Classconferences.Models
{
    public class Order
    {
        [Key]
        public string Room { get; set; }
        public string Room_only { get; set; }
        public List<string> Classes { get; set; }
    }
}
