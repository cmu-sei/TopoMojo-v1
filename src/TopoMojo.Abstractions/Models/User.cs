// Copyright 2020 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Models
{
    public class User
    {
        public int Id { get; set; }
        public string GlobalId { get; set; }
        public string Name { get; set; }
        public UserRole Role { get; set; }
        public int WorkspaceLimit { get; set; }
        public string WhenCreated { get; set; }
        public bool IsAdmin => Role == UserRole.Administrator;
        public bool IsCreator =>
            Role == UserRole.Creator ||
            Role == UserRole.Administrator
        ;
        public bool IsBuilder =>
            Role == UserRole.Builder ||
            Role == UserRole.Creator ||
            Role == UserRole.Administrator
        ;
        public bool IsAgent =>
            string.IsNullOrEmpty(GlobalId) &&
            !string.IsNullOrEmpty(Client.Id)
        ;
        public Client Client { get; set; }
    }

    public class ChangedUser
    {
        public string GlobalId { get; set; }
        public string Name { get; set; }
    }

    public enum UserRole
    {
        User,
        Builder,
        Creator,
        Administrator
    }
}
