// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using TopoMojo.Data.Abstractions;

namespace TopoMojo.Data.Entities
{
    public class Message : IEntity
    {
        public int Id { get; set; }
        public string RoomId { get; set; }
        public string Text { get; set; }
        public bool Edited { get; set; }
        public DateTime WhenCreated { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
    }

}
