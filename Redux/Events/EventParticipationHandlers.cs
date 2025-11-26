using System;
using System.Collections.Concurrent;
using System.Net;
using Redux.Database.Models;
using Redux.Game_Server;
using Redux.Managers;

namespace Redux.Events
{
    /// <summary>
    /// Provides helper methods for event signups and ticket distribution.
    /// </summary>
    public static class EventParticipationHandlers
    {
        private static readonly ConcurrentDictionary<uint, ConcurrentDictionary<uint, byte>> _signupAccounts = new ConcurrentDictionary<uint, ConcurrentDictionary<uint, byte>>();
        private static readonly ConcurrentDictionary<uint, ConcurrentDictionary<string, byte>> _signupIps = new ConcurrentDictionary<uint, ConcurrentDictionary<string, byte>>();

        public static EventEntry HandleSignup(Player player, EventConfig config, string entryType = "SOLO", ushort initialTicketCount = 1)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            ValidateAntiFraud(config.Id, player);

            var entry = EventManager.RegisterEntry(config.Id, player.UID, entryType, config.MaxTicketsPerPlayer);

            if (initialTicketCount > 0)
                entry = EventManager.IncrementTickets(entry.Id, initialTicketCount);

            return entry;
        }

        public static EventEntry HandleMiniObjective(Player player, uint entryId, int ticketsToGrant)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));
            if (entryId == 0)
                throw new ArgumentOutOfRangeException(nameof(entryId));

            var entry = EventManager.FindEntry(entryId);
            if (entry == null || entry.CharacterId != player.UID)
                throw new InvalidOperationException("Mini objective tickets can only be granted to the owner of the entry.");

            return EventManager.IncrementTickets(entryId, ticketsToGrant);
        }

        private static void ValidateAntiFraud(uint configId, Player player)
        {
            if (player.Account != null)
            {
                var accounts = _signupAccounts.GetOrAdd(configId, _ => new ConcurrentDictionary<uint, byte>());
                if (!accounts.TryAdd(player.Account.UID, 0))
                    throw new InvalidOperationException("Duplicate event signup detected for this account.");
            }

            var ipAddress = ExtractIp(player.Socket?.RemoteEndPoint);
            if (!string.IsNullOrEmpty(ipAddress))
            {
                var addresses = _signupIps.GetOrAdd(configId, _ => new ConcurrentDictionary<string, byte>());
                if (!addresses.TryAdd(ipAddress, 0))
                    throw new InvalidOperationException("Duplicate event signup detected from the same IP address.");
            }
        }

        private static string ExtractIp(EndPoint endPoint)
        {
            switch (endPoint)
            {
                case IPEndPoint ipEndPoint:
                    return ipEndPoint.Address.ToString();
                default:
                    return null;
            }
        }
    }
}
