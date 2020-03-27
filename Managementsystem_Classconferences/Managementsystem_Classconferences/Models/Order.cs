using System.Collections.Generic;

namespace Managementsystem_Classconferences.Models
{
    public class Order
    {
        public string Room { get; set; }
        public List<string> Classes { get; set; }
    }
}
