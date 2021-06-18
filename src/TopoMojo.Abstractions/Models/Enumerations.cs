using System;

namespace TopoMojo
{
    [Flags]
    public enum Permission {
        None =      0,
        Editor =    0x01,
        Manager =   0xff
    }
}
