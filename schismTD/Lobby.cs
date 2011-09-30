using System;
using System.Collections.Generic;
using System.Text;
using PlayerIO.GameLibrary;

namespace schismTD
{
    [RoomType("$service-room$")]
    public class Lobby : Game<Player>
    {
        private List<Player> idsSentTo = new List<Player>();

        public override void  GameStarted()
        {
            AddTimer(delegate
            {
                if (PlayerCount < 2)
                    return;

                Guid gameId = Guid.NewGuid();
                int playersAdded = 0;

                foreach (Player p in Players)
                {
                    if (idsSentTo.Contains(p))
                        continue;

                    playersAdded++;
                    if (playersAdded == 1)
                        p.Send(Messages.MATCH_CREATE, gameId.ToString());
                    else
                        p.Send(Messages.MATCH_ID, gameId.ToString());

                    lock (idsSentTo)
                        idsSentTo.Add(p);

                    if(playersAdded == 2)
                        break;
                }
            }, 500);


 	         base.GameStarted();
        }

        public override bool AllowUserJoin(Player player)
        {
            // TODO authentication
            return base.AllowUserJoin(player);
        }

        public override void GameClosed()
        {
            base.GameClosed();
        }

        public override void UserJoined(Player player)
        {
            if (idsSentTo.Contains(player))
                lock (idsSentTo)
                    idsSentTo.Remove(player);

            //player.Name = player.JoinData["name"];

            base.UserJoined(player);
        }

        public override void UserLeft(Player player)
        {
            base.UserLeft(player);
        }

        public override void GotMessage(Player player, Message message)
        {
            switch (message.Type)
            {
                case Messages.CHAT:
                    Broadcast(Messages.CHAT, message.GetString(0));
                    break;
            }
        }
    }
}
