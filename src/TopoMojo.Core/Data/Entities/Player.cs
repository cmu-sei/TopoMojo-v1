// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;

namespace TopoMojo.Data
{
    public class Player
    {
        public int Id { get; set; }
        public int GamespaceId { get; set; }
        public virtual Gamespace Gamespace { get; set; }
        public int PersonId { get; set; }
        public virtual User Person { get; set; }
        public Permission Permission { get; set; }
    }
}
