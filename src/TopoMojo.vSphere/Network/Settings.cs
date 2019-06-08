// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using NetVimClient;

namespace TopoMojo.vSphere.Network
{
    public class Settings
    {
        public VimPortTypeClient vim { get; set; }
        public ManagedObjectReference cluster { get; set; }
        public ManagedObjectReference props { get; set; }
        public ManagedObjectReference pool { get; set; }
        public ManagedObjectReference vmFolder { get; set; }
        public ManagedObjectReference dvs { get; set; }
        public ManagedObjectReference net { get; set; }
        public string UplinkSwitch { get; set; }
        public string DvsUuid { get; set; }
    }
}
