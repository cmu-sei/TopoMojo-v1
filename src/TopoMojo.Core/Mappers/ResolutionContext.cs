using System;
using TopoMojo.Core.Models;

namespace TopoMojo.Core.Mappers
 {
    public static class ResolutionContextExtensions
    {
        public static int GetActorId(this AutoMapper.ResolutionContext res)
        {
            int id = 0;
            if (res.Items.ContainsKey("ActorId"))
                int.TryParse(Convert.ToString(res.Items["ActorId"]), out id);

            return id;
        }
    }
 }