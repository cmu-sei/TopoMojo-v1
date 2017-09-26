using System.Threading.Tasks;
using TopoMojo.Data.Entities;

namespace TopoMojo.Data.Abstractions
{
    public interface ITemplateRepository : IRepository<Template>
    {
        Task<bool> IsParentTemplate(int id);
        Task<bool> AtTemplateLimit(int topoId);
        Task<Template[]> ListLinkedTemplates(int parentId);
    }
}