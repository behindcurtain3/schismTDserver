using System;
using System.Collections.Generic;
using System.Text;
using PlayerIO.GameLibrary;

namespace schismTD
{
    [RoomType("$service-room$")]
    public class Lobby : Game<Player>
    {
        public override void  GameStarted()
        {
 	         base.GameStarted();
        }

        public override void GameClosed()
        {
            base.GameClosed();
        }

        public override void UserJoined(Player player)
        {
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
