namespace TopoMojo.Core.Models
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