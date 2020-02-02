using System;
using System.Collections.Generic;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Core
{
    public static class TemplateExtensions
    {
        public static VmTemplate ToVirtualTemplate(this ConvergedTemplate template, string isolationTag = "")
        {
            TemplateUtility tu = new TemplateUtility(template.Detail);
            tu.Name = template.Name;
            tu.Networks = template.Networks ?? "lan";
            tu.Iso = template.Iso;
            tu.IsolationTag = isolationTag.HasValue() ? isolationTag : template.TopologyGlobalId ?? Guid.Empty.ToString();
            tu.Id = template.Id.ToString();
            tu.UseUplinkSwitch = template.TopologyUseUplinkSwitch;
            return tu.AsTemplate();
        }

        public static void AddSettings(this ConvergedTemplate template, KeyValuePair<string,string>[] settings)
        {
            TemplateUtility tu = new TemplateUtility(template.Detail);
            tu.GuestSettings = settings;
            template.Detail = tu.ToString();
        }
    }
}
