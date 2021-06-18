using System;
using System.Linq;
using System.Threading.Tasks;
using TopoMojo.Data.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo
{
    public class WorkspaceValidator : IModelValidator
    {
        private readonly IWorkspaceStore _store;

        public WorkspaceValidator(
            IWorkspaceStore store
        )
        {
            _store = store;
        }

        public Task Validate(object model)
        {

            if (model is Entity)
                return _validate(model as Entity);

            if (model is NewWorkspace)
                return _validate(model as NewWorkspace);

            if (model is ChangedWorkspace)
                return _validate(model as ChangedWorkspace);

            if (model is ClientAudience)
                return _validate(model as ClientAudience);

            if (model is Models.v2.ChallengeSpec)
                return _validate(model as Models.v2.ChallengeSpec);

            if (model is WorkspaceSearch)
                return _validate(model as WorkspaceSearch);

            throw new NotImplementedException();
        }

        private async Task _validate(WorkspaceSearch model)
        {
            await Task.CompletedTask;
        }

        private async Task _validate(NewWorkspace model)
        {
            await Task.CompletedTask;
        }

        private async Task _validate(ChangedWorkspace model)
        {
            if (model.Name.IsEmpty())
                throw new ArgumentException("ChangedWorkspace.Name");

            if ((await Exists(model.GlobalId)).Equals(false))
                throw new ResourceNotFound();

            await Task.CompletedTask;
        }

        private async Task _validate(Models.v2.ChallengeSpec model)
        {
            await Task.CompletedTask;
        }

        private async Task _validate(Entity model)
        {
            if ((await Exists(model.Id)).Equals(false))
                throw new ResourceNotFound();

            await Task.CompletedTask;
        }

        private async Task _validate(ClientAudience model)
        {
            if (
                model.Audience.IsEmpty() ||
                model.Scope.IsEmpty() ||
                model.Scope.Split(' ').Contains(model.Audience).Equals(false)
            )
            {
                throw new InvalidClientAudience();
            }

            await Task.CompletedTask;
        }

        private async Task<bool> Exists(string id)
        {
            return
                id.NotEmpty() &&
                (await _store.Retrieve(id)) is Data.Workspace
            ;
        }
    }
}
