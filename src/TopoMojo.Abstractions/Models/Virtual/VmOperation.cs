namespace TopoMojo.Abstractions
{
    public class VmOperation
    {
        public string Id { get; set; }
        public VmOperationType Type { get; set; }
        public int WorkspaceId { get; set; }
    }

    public enum VmOperationType
    {
        Start,
        Stop,
        Save,
        Revert,
        Delete
    }
}
