using System.Collections.Generic;
using Redux.Database.Models;
using Redux.Database.Repositories;

namespace Redux.Managers
{
    public static class EventManager
    {
        private static EventParticipationRepository Repository
        {
            get { return Database.ServerDatabase.Context.EventParticipation; }
        }

        public static EventConfig Save(EventConfig config)
        {
            return Repository.SaveConfig(config);
        }

        public static EventEntry RegisterEntry(uint configId, uint characterId, string entryType, ushort maxTicketsPerPlayer)
        {
            return Repository.RegisterEntry(configId, characterId, entryType, maxTicketsPerPlayer);
        }

        public static EventEntry IncrementTickets(uint entryId, int amount)
        {
            return Repository.IncrementMiniObjectiveTickets(entryId, amount);
        }

        public static EventEntry FindEntry(uint entryId)
        {
            return Repository.GetEntry(entryId);
        }

        public static IList<EventEntry> ListEntries(uint configId)
        {
            return Repository.ListEntries(configId);
        }

        public static IList<EventReward> SaveRewards(uint entryId, IEnumerable<EventReward> rewards)
        {
            return Repository.SaveRewards(entryId, rewards);
        }

        public static int MarkDelivered(uint entryId)
        {
            return Repository.MarkDelivered(entryId);
        }
    }
}
