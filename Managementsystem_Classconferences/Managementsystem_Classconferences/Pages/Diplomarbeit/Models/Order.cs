using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Managementsystem_Classconferences.Pages.Diplomarbeit.Models
{
    public class Order
    {
        [Key]
        public string Room { get; set; }
        public string Room_short { get; set; }
    }
}
