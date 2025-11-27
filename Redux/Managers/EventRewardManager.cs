using System;
using System.Collections.Generic;
using System.Linq;
using Redux;
using Redux.Database;
using Redux.Database.Models;
using Redux.Enum;
using Redux.Game_Server;
using Redux.Packets.Game;

namespace Redux.Managers
{
    public static class EventRewardManager
    {
        private static readonly object DrawLock = new object();

        public static void RunPendingDraws()
        {
            lock (DrawLock)
            {
                var now = DateTime.UtcNow;
                var configs = EventManager.ListConfigsToDraw(now);

                foreach (var config in configs)
                    ProcessConfig(config);
            }
        }

        private static void ProcessConfig(EventConfig config)
        {
            if (config == null)
                return;

            if (EventManager.HasRewards(config.Id))
                return;

            var entries = EventManager.ListEntries(config.Id) ?? new List<EventEntry>();
            var eligibleEntries = entries
                .Where(entry => entry != null && entry.MiniObjectiveTickets > 0)
                .ToList();

            if (eligibleEntries.Count == 0)
                return;

            var winnersCount = Math.Max(1, config.WinnersCount);
            winnersCount = Math.Min(winnersCount, eligibleEntries.Count);

            var winners = DrawWinners(eligibleEntries, winnersCount);
            var winnerRewards = new List<Tuple<EventEntry, EventReward>>();

            foreach (var winner in winners)
            {
                var reward = new EventReward
                {
                    EventEntryId = winner.Id,
                    RewardType = string.IsNullOrWhiteSpace(config.RewardType) ? "ITEM" : config.RewardType,
                    RewardValue = config.RewardValue,
                    GrantedAt = DateTime.UtcNow,
                    Delivered = false
                };

                var savedRewards = EventManager.SaveRewards(winner.Id, new[] { reward });
                var persistedReward = savedRewards?.FirstOrDefault() ?? reward;

                winnerRewards.Add(new Tuple<EventEntry, EventReward>(winner, persistedReward));
                LogWinner(config, winner, persistedReward);
            }

            AnnounceWinners(config, winnerRewards);
            EventManager.MarkConfigInactive(config.Id);
        }

        private static IList<EventEntry> DrawWinners(IList<EventEntry> entries, int winnersCount)
        {
            var pool = entries.ToList();
            var winners = new List<EventEntry>();

            for (var i = 0; i < winnersCount; i++)
            {
                if (pool.Count == 0)
                    break;

                var winner = DrawSingle(pool);
                if (winner == null)
                    break;

                winners.Add(winner);
                pool.Remove(winner);
            }

            return winners;
        }

        private static EventEntry DrawSingle(IList<EventEntry> pool)
        {
            var totalTickets = pool.Sum(entry => Math.Max(1, entry.MiniObjectiveTickets));
            if (totalTickets <= 0)
                return null;

            var roll = Common.Random.Next(totalTickets);
            foreach (var entry in pool)
            {
                roll -= Math.Max(1, entry.MiniObjectiveTickets);
                if (roll < 0)
                    return entry;
            }

            return pool.LastOrDefault();
        }

        private static void LogWinner(EventConfig config, EventEntry winner, EventReward reward)
        {
            Console.WriteLine(
                $"[EVENT-REWARD] Event '{config.Title}' winner character {winner.CharacterId} for reward {reward.RewardType}:{reward.RewardValue} at {reward.GrantedAt:u}");
        }

        public static int ClaimRewards(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            var pending = EventManager.ListUndeliveredRewards(player.UID) ?? new List<EventReward>();
            if (pending.Count == 0)
                return 0;

            var deliveredIds = new List<uint>();
            foreach (var reward in pending)
            {
                if (TryDeliverReward(player, reward))
                {
                    deliveredIds.Add(reward.Id);
                    LogDelivery(player, reward);
                }
            }

            if (deliveredIds.Count > 0)
                EventManager.MarkDelivered(deliveredIds, DateTime.UtcNow);

            return deliveredIds.Count;
        }

        private static bool TryDeliverReward(Player player, EventReward reward)
        {
            if (reward == null)
                return false;

            switch (reward.RewardType?.ToUpperInvariant())
            {
                case "CURRENCY":
                    player.CP += reward.RewardValue;
                    player.SendMessage($"Você resgatou {reward.RewardValue} CP do evento.");
                    return true;
                case "EXPERIENCE":
                    player.GainExperience(reward.RewardValue);
                    player.SendMessage($"Você ganhou {reward.RewardValue} EXP do evento.");
                    return true;
                default:
                    var item = player.CreateItem(reward.RewardValue);
                    if (item != null && player.Inventory.ContainsKey(item.UniqueID))
                    {
                        player.SendMessage($"Item {reward.RewardValue} adicionado do evento.");
                        return true;
                    }

                    player.SendMessage("Inventário cheio. Libere espaço e tente novamente.");
                    return false;
            }
        }

        private static void LogDelivery(Player player, EventReward reward)
        {
            Console.WriteLine(
                $"[EVENT-DELIVERY] Character {player.UID} received reward {reward.RewardType}:{reward.RewardValue} at {DateTime.UtcNow:u}");
        }

        public static string GetRewardSummary(EventConfig config)
        {
            if (config == null)
                return "recompensa desconhecida";

            var rewardType = (config.RewardType ?? "ITEM").ToUpperInvariant();
            switch (rewardType)
            {
                case "CURRENCY":
                    return $"{config.RewardValue} CP";
                case "EXPERIENCE":
                    return $"{config.RewardValue} EXP";
                default:
                    return $"item #{config.RewardValue}";
            }
        }

        public static string GetRewardSummary(EventReward reward)
        {
            if (reward == null)
                return "recompensa";

            var rewardType = (reward.RewardType ?? "ITEM").ToUpperInvariant();
            switch (rewardType)
            {
                case "CURRENCY":
                    return $"{reward.RewardValue} CP";
                case "EXPERIENCE":
                    return $"{reward.RewardValue} EXP";
                default:
                    return $"item #{reward.RewardValue}";
            }
        }

        private static void AnnounceWinners(EventConfig config, IList<Tuple<EventEntry, EventReward>> winners)
        {
            if (config == null || winners == null || winners.Count == 0)
                return;

            var summaries = new List<string>();
            foreach (var tuple in winners)
            {
                var entry = tuple?.Item1;
                var reward = tuple?.Item2;
                if (entry == null)
                    continue;

                var character = ServerDatabase.Context.Characters.GetByUID(entry.CharacterId);
                var name = character?.Name ?? $"#{entry.CharacterId}";
                var rewardText = GetRewardSummary(reward) ?? GetRewardSummary(config);
                summaries.Add($"{name} ({rewardText})");
            }

            if (summaries.Count == 0)
                return;

            var message = $"[Evento] '{config.Title}' finalizado! Vencedores: {string.Join(", ", summaries)}.";
            PlayerManager.SendToServer(new TalkPacket(ChatType.Broadcast, message));
        }
    }
}
