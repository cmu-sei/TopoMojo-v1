namespace TopoMojo.Data.Abstractions
{
    public interface IRepositoryContext
    {
        int UserId { get; set; }
        bool UserIsAdmin { get; set; }
    }
}