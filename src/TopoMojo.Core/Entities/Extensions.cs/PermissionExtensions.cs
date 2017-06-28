using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TopoMojo.Core.Entities.Extensions
{
    public static class PermissionExtensions
    {
        public static bool CanManage(this Permission flag)
        {
            return flag.HasFlag(Permission.Manager);
        }
        public static bool CanEdit(this Permission flag)
        {
            return flag.HasFlag(Permission.Editor);
        }
        public static bool IsPending(this Permission flag)
        {
            return flag == Permission.None;
        }
    }
}