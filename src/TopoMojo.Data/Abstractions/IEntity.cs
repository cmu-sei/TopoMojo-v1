namespace TopoMojo.Data.Abstractions
{
    public interface IEntity
    {
        int Id { get; set; }
        System.DateTime WhenCreated { get; set; }
    }

    public interface IEntityPrimary : IEntity
    {
        string GlobalId { get; set; }
        string Name { get; set; }

    }
}