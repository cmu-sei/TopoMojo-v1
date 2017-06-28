using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TopoMojo.Core.Entities.Extensions
{
    public static class WorkerExtensions
    {
        public static bool CanManage(this Worker worker)
        {
            return worker.Permission.CanManage();
        }
        public static bool CanEdit(this Worker worker)
        {
            return worker.Permission.CanEdit();
        }
        public static bool IsPending(this Worker worker)
        {
            return worker.Permission == Permission.None;
        }
    }
}