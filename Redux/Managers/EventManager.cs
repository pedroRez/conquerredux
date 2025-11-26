using System.Collections.Generic;
using Redux.Database.Models;
using Redux.Database.Repositories;
using System;

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

        public static EventConfig FindConfig(uint configId)
        {
            return Repository.GetConfig(configId);
        }

        public static IList<EventConfig> ListConfigsToDraw(DateTime referenceTime)
        {
            return Repository.ListConfigsToDraw(referenceTime);
        }

        public static EventEntry RegisterEntry(uint configId, uint characterId, string entryType, ushort maxTicketsPerPlayer)
        {
            return Repository.RegisterEntry(configId, characterId, entryType, maxTicketsPerPlayer);
        }

        public static bool HasRewards(uint configId)
        {
            return Repository.HasRewards(configId);
        }

        public static EventEntry IncrementTickets(uint entryId, int amount)
        {
            return Repository.IncrementMiniObjectiveTickets(entryId, amount);
        }

        public static void MarkConfigInactive(uint configId)
        {
            Repository.MarkConfigInactive(configId);
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

        public static IList<EventReward> ListUndeliveredRewards(uint characterId)
        {
            return Repository.ListUndeliveredRewards(characterId);
        }

        public static int MarkDelivered(IEnumerable<uint> rewardIds, DateTime deliveredAt)
        {
            return Repository.MarkDelivered(rewardIds, deliveredAt);
        }
    }
}
