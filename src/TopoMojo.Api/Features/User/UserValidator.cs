using System;
using System.Threading.Tasks;
using TopoMojo.Data.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo
{
    public class UserValidator: IModelValidator
    {
        private readonly IUserStore _store;

        public UserValidator(
            IUserStore store
        )
        {
            _store = store;
        }

        public Task Validate(object model)
        {
            if (model is Entity)
                return _validate(model as Entity);

            throw new System.NotImplementedException();
        }

        private async Task _validate(Entity model)
        {
            if ((await Exists(model.Id)).Equals(false))
                throw new ResourceNotFound();

            await Task.CompletedTask;
        }

        private async Task<bool> Exists(string id)
        {
            return
                id.NotEmpty() &&
                (await _store.Retrieve(id)) is Data.User
            ;
        }
    }
}
