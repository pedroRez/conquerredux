using System;
using System.Collections.Generic;
using System.Linq;
using Redux.Managers;
using Redux.Packets.Game;

namespace Redux.Npcs
{

    /// <summary>
    /// Handles NPC usage for [923] LadyLuck
    /// </summary>
    public class NPC_923 : INpc
    {

        public NPC_923(Game_Server.Player _client)
            : base(_client)
        {
            ID = 923;
            Face = 3;
        }

        public override void Run(Game_Server.Player _client, ushort _linkback)
        {
            Responses = new List<NpcDialogPacket>();
            AddAvatar();
            switch (_linkback)
            {
                case 0:
                    AddText("Olá! Posso mostrar os eventos ativos, seus tickets e recompensas recentes.");
                    AddOption("Ver eventos ativos", 1);
                    AddOption("Histórico de recompensas", 2);
                    AddOption("Encerrar.", 255);
                    break;
                case 1:
                    ShowActiveEvents(_client);
                    AddOption("Voltar", 0);
                    AddOption("Fechar", 255);
                    break;
                case 2:
                    ShowRewardHistory(_client);
                    AddOption("Voltar", 0);
                    AddOption("Fechar", 255);
                    break;
            }
            AddFinish();
            Send();

        }

        private void ShowActiveEvents(Game_Server.Player client)
        {
            var now = DateTime.UtcNow;
            var configs = EventManager.ListActiveConfigs(now);

            if (configs == null || configs.Count == 0)
            {
                AddText("Nenhum evento ativo no momento.");
                return;
            }

            foreach (var config in configs.Take(4))
            {
                var entry = EventManager.FindEntryForPlayer(config.Id, client.UID);
                var tickets = entry?.MiniObjectiveTickets ?? 0;
                var maxTickets = config.MaxTicketsPerPlayer > 0 ? config.MaxTicketsPerPlayer.ToString() : "∞";
                var rewardSummary = EventRewardManager.GetRewardSummary(config);

                AddText($"Evento: {config.Title}");
                AddText($"Início: {config.StartsAt.ToLocalTime():g} | Fim: {config.EndsAt.ToLocalTime():g}");
                AddText($"Recompensa: {rewardSummary} (até {config.WinnersCount} vencedor(es))");
                AddText($"Tickets: {tickets}/{maxTickets}");
            }
        }

        private void ShowRewardHistory(Game_Server.Player client)
        {
            var rewards = EventManager.ListRewardsByCharacter(client.UID, 5);
            if (rewards == null || rewards.Count == 0)
            {
                AddText("Você ainda não possui recompensas de eventos.");
                return;
            }

            foreach (var reward in rewards)
            {
                var entry = EventManager.FindEntry(reward.EventEntryId);
                var config = entry != null ? EventManager.FindConfig(entry.EventConfigId) : null;
                var rewardSummary = EventRewardManager.GetRewardSummary(reward);
                var delivered = reward.Delivered ? "entregue" : "pendente";
                var title = config?.Title ?? "Evento";

                AddText($"{title}: {rewardSummary} ({delivered}) em {reward.GrantedAt.ToLocalTime():g}");
            }
        }
    }
}
