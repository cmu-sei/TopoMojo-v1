using System.Threading.Tasks;

namespace TopoMojo.Api.Validators
{
    public interface IModelValidator
    {
        Task Validate(object model);
    }
}
