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

            throw new NotImplementedException();
        }

        private async Task _validate(Entity model)
        {
            if ((await Exists(model.Id)).Equals(false))
                throw new ResourceNotFound();

            await Task.CompletedTask;
        }

        private async Task _validate(GamespaceSearch model)
        {
            await Task.CompletedTask;
        }

        private async Task<bool> Exists(string id)
        {
            return
                id.NotEmpty() &&
                (await _store.Retrieve(id)) is Data.Gamespace
            ;
        }
    }
}
