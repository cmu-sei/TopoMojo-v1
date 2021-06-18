using System;
using System.Threading.Tasks;
using TopoMojo.Abstractions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo
{
        public class VmValidator : IModelValidator
    {
        private readonly IWorkspaceStore _store;
        private readonly IHypervisorService _pod;

        public VmValidator(
            IHypervisorService pod,
            IWorkspaceStore store
        )
        {
            _pod = pod;
            _store = store;
        }

        public Task Validate(object model)
        {

            if (model is VmOperation)
                return _validate(model as VmOperation);

            throw new NotImplementedException();
        }

        private async Task _validate(VmOperation model)
        {
            if (model.Type != VmOperationType.Save)
                await Task.CompletedTask;

            string isolationId = model.Id.Contains("#")
                ? model.Id.Tag()
                : (await _pod.Load(model.Id))?.Name.Tag();

            if (await _store.HasGames(isolationId))
                throw new WorkspaceNotIsolated();

            await Task.CompletedTask;
        }
    }
}
