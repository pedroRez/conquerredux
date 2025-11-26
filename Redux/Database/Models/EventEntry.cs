using System;

namespace Redux.Database.Models
{
    public class EventEntry
    {
        public virtual uint Id { get; set; }
        public virtual uint EventConfigId { get; set; }
        public virtual uint CharacterId { get; set; }
        public virtual string EntryType { get; set; }
        public virtual string State { get; set; }
        public virtual DateTime SignedAt { get; set; }
        public virtual ushort MiniObjectiveTickets { get; set; }
    }
}
