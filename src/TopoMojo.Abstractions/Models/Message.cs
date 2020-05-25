// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

namespace TopoMojo.Models
{
    public class NewMessage
    {
        public string RoomId { get; set; }
        public string Text { get; set; }
    }

    public class ChangedMessage
    {
        public int Id { get; set; }
        public string Text { get; set; }

    }

    public class Message
    {
        public int Id { get; set; }
        public string RoomId { get; set; }
        public string AuthorName { get; set; }
        public string Text { get; set; }
        public string WhenCreated { get; set; }
        public bool Edited { get; set; }

    }
}
