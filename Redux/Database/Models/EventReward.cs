using System;

namespace Redux.Database.Models
{
    public class EventReward
    {
        public virtual uint Id { get; set; }
        public virtual uint EventEntryId { get; set; }
        public virtual string RewardType { get; set; }
        public virtual uint RewardValue { get; set; }
        public virtual DateTime GrantedAt { get; set; }
        public virtual bool Delivered { get; set; }
    }
}
