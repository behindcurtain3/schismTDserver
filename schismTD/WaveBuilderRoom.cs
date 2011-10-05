using System;
using System.Collections.Generic;
using System.Text;
using PlayerIO.GameLibrary;
using schismTD.Config;

namespace schismTD
{
    [RoomType("WaveBuilder")]
    public class WaveBuilderRoom : Game<Player>
    {
        public override void GameStarted()
        {
            PreloadPlayerObjects = true;
            AddMessageHandler(WaveBuilderMessages.SAVE_WAVE, SaveWave);
            AddMessageHandler(WaveBuilderMessages.SAVE_ALL_WAVES, SaveAllWaves);
            base.GameStarted();
        }

        public override void UserJoined(Player player)
        {
            base.UserJoined(player);
        }

        public void SaveWave(Player player, Message message)
        {
            player.Send(WaveBuilderMessages.INVALID_WAVE, "Not yet implemented");
        }

        public void SaveAllWaves(Player player, Message message)
        {
            //Console.WriteLine(message.ToString());
            DatabaseArray waves = new DatabaseArray();            

            for (uint i = 0; i < message.Count; i++)
            {
                DatabaseArray wave = new DatabaseArray();
                while(message.GetString(i) != "--ENDOFWAVE--")
                {
                    wave.Add(message.GetString(i));
                    i++;
                }
                waves.Add(wave);
            }

            player.PlayerObject.Set("Waves", waves);
            player.PlayerObject.Save(true, true, delegate()
            {
                player.Send(WaveBuilderMessages.WAVES_SAVED);
                Console.WriteLine("Player waves saved.");
            }, delegate(PlayerIOError e)
            {
                player.Send(WaveBuilderMessages.INVALID_WAVE, e.Message);
                PlayerIO.ErrorLog.WriteError(e.Message);
            });
            
        }
    }
}
