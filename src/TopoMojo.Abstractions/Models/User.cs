// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Models
{
    public class User
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public bool IsAdmin { get; set; }
        public int WorkspaceLimit { get; set; }
        public string WhenCreated { get; set; }
    }

    public class ChangedUser
    {
        public string GlobalId { get; set; }
        public string Name { get; set; }
    }

}
