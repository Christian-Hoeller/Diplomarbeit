using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Managementsystem_Classconferences.Hubs
{
    public class MainHub : Hub
    {
        public async Task SendMessage(string user, string message_intersections, string message_teachers)
        {
            
            await SendInfo(user, message_intersections);
            await Clients.All.SendAsync("ReceiveTeachers", user, message_teachers);
        }

        public async Task SendInfo(string user, string message_intersections)
        {
            await Clients.All.SendAsync("ReceiveIntersections", user, message_intersections);
        }
    }




}
