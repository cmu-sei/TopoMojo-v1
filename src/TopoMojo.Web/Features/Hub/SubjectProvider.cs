using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace TopoMojo.Web.Controllers
{
    public class SubjectProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User.FindFirstValue(AppConstants.SubjectClaimName);
        }
    }
}
