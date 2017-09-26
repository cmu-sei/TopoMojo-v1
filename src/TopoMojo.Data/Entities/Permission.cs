using System;

namespace TopoMojo.Data.Entities
{
    [Flags]
    public enum Permission {
        None =      0,
        Editor =    0x01,
        Manager =   0xff
    }

}