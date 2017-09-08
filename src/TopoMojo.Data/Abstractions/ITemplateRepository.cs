using System.Threading.Tasks;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.Abstractions
{
    public interface ITemplateRepository : IRepository<Template>
    {
        Task<bool> HasLinkedTemplates(int parentId);
        Task<Template[]> ListLinkedTemplates(int parentId);
    }
}