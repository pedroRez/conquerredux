using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redux.Packets.Game;

namespace Redux.Npcs
{

    public class NPC_31 : INpc
    {

        public NPC_31(Game_Server.Player _client)
            : base(_client)
        {
    		ID = 31;	
			Face = 5;    
    	}
    	
        public override void Run(Game_Server.Player _client, ushort _linkback)
        {
        	Responses = new List<NpcDialogPacket>();
        	AddAvatar();
        	switch(_linkback)
        	{
                case 0:
                    AddText("Bem-vindo ao servidor Redux! Aqui e 100% No Pay to Win e No Pay to Power,");
                    AddText("todo o progresso vem jogando. Aproveite a jornada e divirta-se.");
                    AddOption("Entendi, obrigado!", 255);
                    break;
        		default:
        			break;
        	}
            AddFinish();
            Send();
        }
    }
}
