using System.Threading.Tasks;
using TopoMojo.Models;

namespace TopoMojo.Abstractions
{
    public interface IPodManager
    {
        Task<Vm> Load(string id);
        Task<Vm> Start(string id);
        Task<Vm> Stop(string id);
        Task<Vm> Save(string id);
        Task<Vm> Revert(string id);
        Task<Vm> Delete(string id);
        Task<Vm> Change(string id, KeyValuePair change);
        Task<Vm> Deploy(Template template);
        Task<Vm> Refresh(Template template);
        Task<Vm[]> Find(string searchText);
        Task<int> CreateDisks(Template template);
        Task<int> VerifyDisks(Template template);
        Task<int> DeleteDisks(Template template);
        Task<DisplayInfo> Display(string id);
        Task<Vm> Answer(string id, string question, string answer);
        Task<TemplateOptions> GetTemplateOptions(string key);
        Task<VmOptions> GetVmIsoOptions(string key);
        Task<VmOptions> GetVmNetOptions(string key);
        string Version { get; }
    }

}
