using System.Threading.Tasks;

namespace TopoMojo
{
    public interface IModelValidator
    {
        Task Validate(object model);
    }
}
