// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Core.Models
{
    public class Player
    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string PersonGlobalId { get; set; }
        public bool CanManage { get; set; }
        public bool CanEdit { get; set; }
        //public bool Online { get; set; }
    }

}
