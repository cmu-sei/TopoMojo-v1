// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TopoMojo.Abstractions;
using TopoMojo.Core.Models;
using TopoMojo.Data.EntityFrameworkCore;

namespace TopoMojo.Core
{
    public class ChatService : EntityService<Data.Message>
    {
        public ChatService (
            TopoMojoDbContext dbContext,
            ILoggerFactory mill,
            IMapper mapper,
            CoreOptions options,
            IIdentityResolver identityResolver
        ) : base (mill, mapper, options, identityResolver)
        {
            _dbContext = dbContext;
        }

        private readonly TopoMojoDbContext _dbContext;

        public async Task<Models.Message[]> List(string roomId, int take, int marker = 0)
        {
            if (! await IsAllowed(roomId))
                throw new InvalidOperationException();

            var q = _dbContext.Messages.Where(m => m.RoomId == roomId);

            if (marker > 0)
                q = q.Where(m => m.Id < marker);

            var msgs = await q
                .OrderByDescending(m => m.Id)
                .Take(take)
                .ToArrayAsync();

            return Mapper.Map<Models.Message[]>(msgs);
        }

        public async Task<Models.Message> Find(int msgId)
        {
            var entity = await _dbContext.Messages.FindAsync(msgId);
            if (! await IsAllowed(entity.RoomId))
                throw new InvalidOperationException();

            return Mapper.Map<Models.Message>(entity);
        }

        public async Task<Models.Message> Add(NewMessage message)
        {
            if (! await IsAllowed(message.RoomId))
                throw new InvalidOperationException();

            var entity = Mapper.Map<Data.Message>(message);
            entity.AuthorId = User.Id;
            entity.AuthorName = User.Name;
            entity.WhenCreated = DateTime.UtcNow;
            _dbContext.Messages.Add(entity);
            await _dbContext.SaveChangesAsync();
            return Mapper.Map<Models.Message>(entity);
        }

        public async Task<Models.Message> Update(ChangedMessage message)
        {
            var entity = await _dbContext.Messages.FindAsync(message.Id);
            if (!(entity.AuthorId == User.Id))
                throw new InvalidOperationException();

            Mapper.Map(message, entity);
            _dbContext.Messages.Update(entity);
            await _dbContext.SaveChangesAsync();
            return Mapper.Map<Models.Message>(entity);

        }

        public async Task<Models.Message> Delete(int id)
        {
            var entity = await _dbContext.Messages.FindAsync(id);
            if (!User.IsAdmin && entity.AuthorId != User.Id)
                throw new InvalidOperationException();

            _dbContext.Messages.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return Mapper.Map<Models.Message>(entity);

        }

        private async Task<bool> IsAllowed(string roomId)
        {
            if (User.IsAdmin)
                return true;

            if (await _dbContext.Workers.AnyAsync(w => w.Topology.GlobalId == roomId && w.Person.GlobalId == User.GlobalId))
                return true;

            if (await _dbContext.Players.AnyAsync(w => w.Gamespace.GlobalId == roomId && w.Person.GlobalId == User.GlobalId))
                return true;

            return false;
        }
    }
}
