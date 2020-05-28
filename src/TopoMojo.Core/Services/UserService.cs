using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Data.Abstractions;
using TopoMojo.Extensions;
using TopoMojo.Models;

namespace TopoMojo.Services
{
    public class UserService: _Service
    {

        public UserService(
            IUserStore userStore,
            IMemoryCache userCache,
            ILogger<WorkspaceService> logger,
            IMapper mapper,
            CoreOptions options,
            IIdentityResolver identityResolver
        ) : base (logger, mapper, options, identityResolver)
        {
            _userStore = userStore;
            _userCache = userCache;
        }

        private readonly IUserStore _userStore;
        private readonly IMemoryCache _userCache;

        public async Task<User[]> List(Search search, CancellationToken ct = default(CancellationToken))
        {
            var q = _userStore.List(search.Term);

            if (search.Term.HasValue())
                q = q.Where(p => p.Name.ToLower().Contains(search.Term.ToLower()));

            if (search.HasFilter("admins"))
                q = q.Where(p => p.Role == Data.UserRole.Administrator);

            q = q.OrderBy(p => p.Name);

            if (search.Skip > 0)
                q = q.Skip(search.Skip);

            if (search.Take > 0)
                q = q.Take(search.Take);

            return await Mapper.ProjectTo<User>(q).ToArrayAsync(ct);
        }

        public async Task<User> Load(string id)
        {
            if (string.IsNullOrEmpty(id))
                return User;

            var user = await _userStore.Load(id);

            return (user != null)
                ? Mapper.Map<User>(user)
                : null;
        }

        public async Task<User> Add(User profile)
        {
            var entity = await _userStore.Add(
                Mapper.Map<Data.User>(profile)
            );

            return Mapper.Map<User>(entity);
        }

        public async Task Update(User model)
        {
            var entity = await _userStore.Load(model.Id);

            entity.Name = model.Name;

            if (User.IsAdmin)
            {
                // entity.IsAdmin = model.IsAdmin;
                entity.Role = Enum.Parse<Data.UserRole>(model.Role);
                entity.WorkspaceLimit = model.WorkspaceLimit;
            }

            await _userStore.Update(entity);
        }

        public async Task Delete(int id)
        {
            var entity = await _userStore.Load(id);

            if (entity == null || (!User.IsAdmin && User.GlobalId != entity.GlobalId))
                throw new InvalidOperationException();

            await _userStore.Delete(id);
        }

        public async Task<bool> MemberOf(string globalId)
        {
            return await _userStore.MemberOf(globalId, User);
        }
    }
}
