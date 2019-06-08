// Copyright 2019 Carnegie Mellon University. All Rights Reserved.
// Released under a 3 Clause BSD-style license. See LICENSE.md in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TopoMojo.Core.Abstractions;
using TopoMojo.Core.Models;
using TopoMojo.Data.Abstractions;
using TopoMojo.Data.Entities;
using TopoMojo.Data.EntityFrameworkCore;

namespace TopoMojo.Core
{
    public class ChatService : EntityManager<Data.Entities.Message>
    {
        public ChatService (
            ILoggerFactory mill,
            CoreOptions options,
            IProfileResolver profileResolver,
            TopoMojoDbContext db
        ) : base (mill, options, profileResolver)
        {
            _db = db;
        }

        private readonly TopoMojoDbContext _db;

        public async Task<Models.Message[]> List(string roomId, int take, int marker = 0)
        {
            if (! await IsAllowed(roomId))
                throw new InvalidOperationException();

            var q = _db.Messages.Where(m => m.RoomId == roomId);

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
            var entity = await _db.Messages.FindAsync(msgId);
            if (! await IsAllowed(entity.RoomId))
                throw new InvalidOperationException();

            return Mapper.Map<Models.Message>(entity);
        }

        public async Task<Models.Message> Add(NewMessage message)
        {
            if (! await IsAllowed(message.RoomId))
                throw new InvalidOperationException();

            var entity = Mapper.Map<Data.Entities.Message>(message);
            entity.AuthorId = Profile.Id;
            entity.AuthorName = Profile.Name;
            entity.WhenCreated = DateTime.UtcNow;
            _db.Messages.Add(entity);
            await _db.SaveChangesAsync();
            return Mapper.Map<Models.Message>(entity);
        }

        public async Task<Models.Message> Update(ChangedMessage message)
        {
            var entity = await _db.Messages.FindAsync(message.Id);
            if (!(entity.AuthorId == Profile.Id))
                throw new InvalidOperationException();

            Mapper.Map(message, entity);
            _db.Messages.Update(entity);
            await _db.SaveChangesAsync();
            return Mapper.Map<Models.Message>(entity);

        }

        public async Task<Models.Message> Delete(int id)
        {
            var entity = await _db.Messages.FindAsync(id);
            if (!Profile.IsAdmin && entity.AuthorId != Profile.Id)
                throw new InvalidOperationException();

            _db.Messages.Remove(entity);
            await _db.SaveChangesAsync();
            return Mapper.Map<Models.Message>(entity);

        }

        private async Task<bool> IsAllowed(string roomId)
        {
            if (Profile.IsAdmin)
                return true;

            if (await _db.Workers.AnyAsync(w => w.Topology.GlobalId == roomId && w.Person.GlobalId == Profile.GlobalId))
                return true;

            if (await _db.Players.AnyAsync(w => w.Gamespace.GlobalId == roomId && w.Person.GlobalId == Profile.GlobalId))
                return true;

            return false;
        }
    }
}
