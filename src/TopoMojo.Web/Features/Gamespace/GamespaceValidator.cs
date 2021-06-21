using System;
using System.Linq;
using System.Threading.Tasks;
using TopoMojo.Data.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo
{
    public class GamespaceValidator : IModelValidator
    {
        private readonly IGamespaceStore _store;

        public GamespaceValidator(
            IGamespaceStore store
        )
        {
            _store = store;
        }

        public Task Validate(object model)
        {

            if (model is Entity)
                return _validate(model as Entity);

            if (model is WorkspaceEntity)
                return _validate(model as WorkspaceEntity);

            if (model is GamespaceSearch)
                return _validate(model as GamespaceSearch);

            if (model is Player)
                return _validate(model as Player);

            if (model is GamespaceRegistration)
                return _validate(model as GamespaceRegistration);

            throw new NotImplementedException();
        }

        private async Task _validate(Entity model)
        {
            if (! await Exists(model.Id))
                throw new ResourceNotFound();

            await Task.CompletedTask;
        }

        private async Task _validate(WorkspaceEntity model)
        {
            if (await _store.DbContext.Workspaces.FindAsync(model.Id) == null)
                throw new ResourceNotFound();

            await Task.CompletedTask;
        }

        private async Task _validate(GamespaceSearch model)
        {
            await Task.CompletedTask;
        }

        private async Task _validate(GamespaceRegistration model)
        {
            if (! await WorkspaceExists(model.ResourceId))
                throw new ResourceNotFound();

            await Task.CompletedTask;
        }

        private async Task _validate(Player model)
        {
            var entity = await _store.DbContext.Players.FindAsync(model.SubjectId, model.GamespaceId);

            if (entity != null)
                throw new ResourceNotFound();

            await Task.CompletedTask;
        }

        private async Task<bool> Exists(string id)
        {
            return
                id.NotEmpty() &&
                (await _store.Retrieve(id)) is Data.Gamespace
            ;
        }

        private async Task<bool> WorkspaceExists(string id)
        {
            return
                id.NotEmpty() &&
                (await _store.DbContext.Workspaces.FindAsync(id)) is Data.Workspace
            ;
        }

    }
}
