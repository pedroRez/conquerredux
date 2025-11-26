using System;
using System.Collections.Generic;
using System.Linq;
using Redux.Database.Models;

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

            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(config);
                transaction.Commit();
                return config;
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

        public EventEntry IncrementMiniObjectiveTickets(uint entryId, int amount)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var entry = session.Get<EventEntry>(entryId);
                if (entry == null)
                    return null;

                var config = session.Get<EventConfig>(entry.EventConfigId);
                var maxTickets = config?.MaxTicketsPerPlayer ?? 0;

                var target = entry.MiniObjectiveTickets + amount;
                if (maxTickets > 0)
                    target = Math.Min(target, maxTickets);

                entry.MiniObjectiveTickets = (ushort)Math.Max(0, target);

                session.Update(entry);
                transaction.Commit();
                return entry;
            }
        }

        public EventEntry GetEntry(uint entryId)
        {
            using (var session = NHibernateHelper.OpenSession())
            {
                return session.Get<EventEntry>(entryId);
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
                    session.SaveOrUpdate(reward);
                    persistedRewards.Add(reward);
                }

                transaction.Commit();
                return persistedRewards;
            }
        }

        public int MarkDelivered(uint entryId)
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var affected = session
                    .CreateQuery("update Redux.Database.Models.EventReward set Delivered = :delivered where EventEntryId = :entryId")
                    .SetParameter("delivered", true)
                    .SetParameter("entryId", entryId)
                    .ExecuteUpdate();

                transaction.Commit();
                return affected;
            }
        }
    }
}
