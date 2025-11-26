using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redux.Packets.Game;

namespace Redux.Npcs
{
    /// <summary>
    /// Handles NPC usage for [1337] Welcomer
    /// </summary>
    public class NPC_1337 : INpc
    {
        public NPC_1337(Game_Server.Player _client)
            : base(_client)
        {
            ID = 1337;
            Face = 1;
        }

        public override void Run(Game_Server.Player _client, ushort _linkback)
        {
            Responses = new List<NpcDialogPacket>();
            AddAvatar();
            switch (_linkback)
            {
                case 0:
                    AddText("Welcome to Triumph CO! We want everyone to have fun without pay-to-win pressure.");
                    AddText("Donations help keep the servers running but never buy power, gear, or unfair advantages.");
                    AddText("Play, explore, and grow your character—your progress comes from skill and teamwork, not spending.");
                    AddOption("Sounds fair!", 255);
                    break;
            }
            AddFinish();
            Send();
        }
    }
}
