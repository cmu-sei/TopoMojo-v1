namespace TopoMojo.Data.Abstractions
{
    public interface IEntity
    {
        int Id { get; set; }
        string GlobalId { get; set; }
        string Name { get; set; }
        System.DateTime WhenCreated { get; set; }
    }
}