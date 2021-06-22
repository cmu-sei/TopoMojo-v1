using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace TopoMojo.Api.Hubs
{
    public class SubjectProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User.FindFirstValue(AppConstants.SubjectClaimName);
        }
    }
}
