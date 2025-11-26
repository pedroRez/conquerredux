using System;

namespace Redux.Database.Models
{
    public class EventConfig
    {
        public virtual uint Id { get; set; }
        public virtual string EventCode { get; set; }
        public virtual string Title { get; set; }
        public virtual byte MaxSignups { get; set; }
        public virtual ushort MaxTicketsPerPlayer { get; set; }
        public virtual byte WinnersCount { get; set; }
        public virtual string RewardType { get; set; }
        public virtual uint RewardValue { get; set; }
        public virtual DateTime StartsAt { get; set; }
        public virtual DateTime EndsAt { get; set; }
        public virtual DateTime CreatedAt { get; set; }
        public virtual string Status { get; set; }
    }
}
