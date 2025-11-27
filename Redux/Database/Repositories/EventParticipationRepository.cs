using System;
using System.Collections.Generic;
using System.Linq;
using Redux.Database.Models;
using NHibernate.Criterion;

namespace Redux.Database.Repositories
{
    public class EventParticipationRepository
    {
        public EventConfig SaveConfig(EventConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (config.CreatedAt == DateTime.MinValue)
                config.CreatedAt = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(config.RewardType))
                config.RewardType = "ITEM";

            if (config.WinnersCount == 0)
                config.WinnersCount = 1;

            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(config);
                transaction.Commit();
                return config;
            }
        }

        public EventConfig GetConfig(uint configId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.Get<EventConfig>(configId);
            }
        }

        public IList<EventConfig> ListConfigsToDraw(DateTime referenceTime)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session
                    .QueryOver<EventConfig>()
                    .Where(config => config.Status == "ACTIVE" && config.EndsAt <= referenceTime)
                    .List();
            }
        }

        public IList<EventConfig> ListActiveConfigs(DateTime referenceTime)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session
                    .QueryOver<EventConfig>()
                    .Where(config => config.Status == "ACTIVE" && config.StartsAt <= referenceTime && config.EndsAt >= referenceTime)
                    .OrderBy(config => config.StartsAt).Asc
                    .List();
            }
        }

        public EventEntry RegisterEntry(uint configId, uint characterId, string entryType, ushort maxTicketsPerPlayer)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var config = session.Get<EventConfig>(configId) ?? throw new InvalidOperationException("Event configuration not found.");

                if (maxTicketsPerPlayer > 0 && config.MaxTicketsPerPlayer != maxTicketsPerPlayer)
                {
                    config.MaxTicketsPerPlayer = maxTicketsPerPlayer;
                    session.SaveOrUpdate(config);
                }

                var existingEntry = session
                    .QueryOver<EventEntry>()
                    .Where(entry => entry.EventConfigId == configId && entry.CharacterId == characterId)
                    .SingleOrDefault();

                if (existingEntry != null)
                    return existingEntry;

                if (config.MaxSignups > 0)
                {
                    var currentSignups = session
                        .QueryOver<EventEntry>()
                        .Where(entry => entry.EventConfigId == configId)
                        .RowCount();

                    if (currentSignups >= config.MaxSignups)
                        throw new InvalidOperationException("The event has reached its maximum number of signups.");
                }

                var newEntry = new EventEntry
                {
                    EventConfigId = configId,
                    CharacterId = characterId,
                    EntryType = entryType,
                    State = "PENDING",
                    SignedAt = DateTime.UtcNow,
                    MiniObjectiveTickets = 0
                };

                session.Save(newEntry);
                transaction.Commit();
                return newEntry;
            }
        }

        public bool HasRewards(uint configId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var entryIds = session
                    .QueryOver<EventEntry>()
                    .Where(entry => entry.EventConfigId == configId)
                    .Select(entry => entry.Id)
                    .List<uint>();

                if (entryIds.Count == 0)
                    return false;

                return session
                    .QueryOver<EventReward>()
                    .WhereRestrictionOn(reward => reward.EventEntryId)
                    .IsIn(entryIds.ToArray())
                    .RowCount() > 0;
            }
        }

        public EventEntry IncrementMiniObjectiveTickets(uint entryId, int amount)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var entry = session.Get<EventEntry>(entryId);
                if (entry == null)
                    return null;

                var config = session.Get<EventConfig>(entry.EventConfigId);
                var maxTickets = (int)(config?.MaxTicketsPerPlayer ?? 0);

                var target = entry.MiniObjectiveTickets + amount;
                if (maxTickets > 0)
                    target = Math.Min(target, maxTickets);

                entry.MiniObjectiveTickets = (ushort)Math.Max(0, target);

                session.Update(entry);
                transaction.Commit();
                return entry;
            }
        }

        public void MarkConfigInactive(uint configId)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var config = session.Get<EventConfig>(configId);
                if (config != null)
                {
                    config.Status = "INACTIVE";
                    session.SaveOrUpdate(config);
                }

                transaction.Commit();
            }
        }

        public EventEntry GetEntry(uint entryId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.Get<EventEntry>(entryId);
            }
        }

        public EventEntry GetEntryForPlayer(uint configId, uint characterId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session
                    .QueryOver<EventEntry>()
                    .Where(entry => entry.EventConfigId == configId && entry.CharacterId == characterId)
                    .Take(1)
                    .SingleOrDefault();
            }
        }

        public IList<EventEntry> ListEntries(uint configId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session
                    .QueryOver<EventEntry>()
                    .Where(entry => entry.EventConfigId == configId)
                    .List();
            }
        }

        public IList<EventReward> SaveRewards(uint entryId, IEnumerable<EventReward> rewards)
        {
            if (rewards == null)
                return new List<EventReward>();

            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var persistedRewards = new List<EventReward>();

                foreach (var reward in rewards.Where(r => r != null))
                {
                    reward.EventEntryId = entryId;
                    if (reward.GrantedAt == DateTime.MinValue)
                        reward.GrantedAt = DateTime.UtcNow;
                    session.SaveOrUpdate(reward);
                    persistedRewards.Add(reward);
                }

                transaction.Commit();
                return persistedRewards;
            }
        }

        public IList<EventReward> ListUndeliveredRewards(uint characterId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var entryIds = session
                    .QueryOver<EventEntry>()
                    .Where(entry => entry.CharacterId == characterId)
                    .Select(entry => entry.Id)
                    .List<uint>();

                if (entryIds.Count == 0)
                    return new List<EventReward>();

                return session
                    .QueryOver<EventReward>()
                    .Where(reward => reward.Delivered == false)
                    .WhereRestrictionOn(reward => reward.EventEntryId)
                    .IsIn(entryIds.ToArray())
                    .List();
            }
        }

        public IList<EventReward> ListRewardsByCharacter(uint characterId, int limit = 20)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                var entryIds = session
                    .QueryOver<EventEntry>()
                    .Where(entry => entry.CharacterId == characterId)
                    .Select(entry => entry.Id)
                    .List<uint>();

                if (entryIds.Count == 0)
                    return new List<EventReward>();

                var query = session
                    .QueryOver<EventReward>()
                    .WhereRestrictionOn(reward => reward.EventEntryId)
                    .IsIn(entryIds.ToArray())
                    .OrderBy(reward => reward.GrantedAt).Desc();

                if (limit > 0)
                    return query.Take(limit).List();

                return query.List();
            }
        }

        public int MarkDelivered(IEnumerable<uint> rewardIds, DateTime deliveredAt)
        {
            if (rewardIds == null)
                return 0;

            var ids = rewardIds.ToArray();
            if (ids.Length == 0)
                return 0;

            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var affected = session
                    .CreateQuery("update Redux.Database.Models.EventReward set Delivered = :delivered, DeliveredAt = :deliveredAt where Id in (:rewardIds)")
                    .SetParameter("delivered", true)
                    .SetParameter("deliveredAt", deliveredAt)
                    .SetParameterList("rewardIds", ids)
                    .ExecuteUpdate();

                transaction.Commit();
                return affected;
            }
        }
    }
}
