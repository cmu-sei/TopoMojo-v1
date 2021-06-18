// Copyright 2020 Carnegie Mellon University.
// Released under a MIT (SEI) license. See LICENSE.md in the project root.

using System;
using System.Collections.Generic;
using System.Text.Json;
using TopoMojo.Extensions;
using TopoMojo.Models;
using TopoMojo.Services;

namespace TopoMojo
{
    public static class TemplateExtensions
    {
        public static VmTemplate ToVirtualTemplate(this ConvergedTemplate template, string isolationTag = "")
        {
            TemplateUtility tu = new TemplateUtility(template.Detail);
            tu.Name = template.Name;
            tu.Networks = template.Networks ?? "lan";
            tu.Iso = template.Iso;
            tu.IsolationTag = isolationTag.NotEmpty() ? isolationTag : template.WorkspaceGlobalId ?? Guid.Empty.ToString();
            tu.Id = template.Id.ToString();
            tu.UseUplinkSwitch = template.WorkspaceUseUplinkSwitch;
            tu.AddGuestSettings(template.Guestinfo ?? "");
            return tu.AsTemplate();
        }

        // public static void AddSettings(this ConvergedTemplate template, KeyValuePair<string,string>[] settings)
        // {
        //     TemplateUtility tu = new TemplateUtility(template.Detail);
        //     tu.GuestSettings = settings;
        //     template.Detail = tu.ToString();
        // }

        public static T Clone<T>(this T obj)
        {
            return JsonSerializer.Deserialize<T>(
                JsonSerializer.Serialize(obj, null)
            );
        }
    }
}
