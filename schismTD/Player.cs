using System;
using System.Collections.Generic;
using System.Text;
using PlayerIO.GameLibrary;

namespace schismTD
{
    public class Player : BasePlayer
    {
        public string Name;

        // Game data
        public int Mana = Settings.DEFAULT_MANA;
        public int Life = Settings.DEFAULT_LIFE;

        public List<Tower> Towers = new List<Tower>();
        public List<Wall> Walls = new List<Wall>();

        public void reset()
        {
            Mana = Settings.DEFAULT_MANA;
            Life = Settings.DEFAULT_LIFE;

            Towers.Clear();
            Walls.Clear();
        }
    }
}
