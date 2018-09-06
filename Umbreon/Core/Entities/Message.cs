﻿using System;
using Umbreon.Interfaces;

namespace Umbreon.Core.Entities
{
    public class Message : IRemoveable
    {
        public ulong UserId { get; set; }
        public ulong ExecutingId { get; set; }
        public ulong ResponseId { get; set; }
        public ulong ChannelId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool AttachedFile { get; set; }

        public int Identifier { get; set; }
        public DateTime When => DateTime.UtcNow + TimeSpan.FromMinutes(5);
        public IRemoveableService Service { get; set; }
    }
}
