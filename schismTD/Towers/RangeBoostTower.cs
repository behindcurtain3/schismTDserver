﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class RangeBoostTower : Tower
    {
        List<Tower> towersEffected = new List<Tower>();
        Cell towerCell;

        public RangeBoostTower(Game g, Player p, Player opponent, Vector2 pos) : base(g, p, opponent, pos)
        {
            FireRate = 0;
            Range = 0;
            Damage = 0;
            SellValue = 217;

            EffectedFireRate = FireRate;
            EffectedDamage = Damage;
            EffectedRange = Range;

            Type = Tower.RANGE_BOOST;
        }

        public override void onPlaced(Cell c)
        {
            towerCell = c;

            base.onPlaced(c);
        }

        public override void onRemoved(Cell c)
        {
            foreach (Tower t in towersEffected)
            {
                if (t.hasEffect("rangeboost"))
                {
                    lock (t.Effects)
                    {
                        Effect e = t.getEffect("rangeboost");
                        t.Effects.Remove(e);
                    }
                }
            }
        }

        public override void update(long dt)
        {
            applyEffects(dt);

            if (towerCell == null)
                return;

            if (towerCell.Up.Tower != null)
            {
                if (!towersEffected.Contains(towerCell.Up.Tower))
                {
                    towerCell.Up.Tower.addEffect(new RangeBoostEffect(towerCell.Up.Tower, towerCell));
                    towersEffected.Add(towerCell.Up.Tower);
                }
            }
            if (towerCell.Left.Tower != null)
            {
                if (!towersEffected.Contains(towerCell.Left.Tower))
                {
                    towerCell.Left.Tower.addEffect(new RangeBoostEffect(towerCell.Left.Tower, towerCell));
                    towersEffected.Add(towerCell.Left.Tower);
                }
            }
            if (towerCell.Right.Tower != null)
            {
                if (!towersEffected.Contains(towerCell.Right.Tower))
                {
                    towerCell.Right.Tower.addEffect(new RangeBoostEffect(towerCell.Right.Tower, towerCell));
                    towersEffected.Add(towerCell.Right.Tower);
                }
            }
            if (towerCell.Down.Tower != null)
            {
                if (!towersEffected.Contains(towerCell.Down.Tower))
                {
                    towerCell.Down.Tower.addEffect(new RangeBoostEffect(towerCell.Down.Tower, towerCell));
                    towersEffected.Add(towerCell.Down.Tower);
                }
            }

            if (towerCell.Up.Left.Tower != null)
            {
                if (!towersEffected.Contains(towerCell.Up.Left.Tower))
                {
                    towerCell.Up.Left.Tower.addEffect(new RangeBoostEffect(towerCell.Up.Left.Tower, towerCell));
                    towersEffected.Add(towerCell.Up.Left.Tower);
                }
            }
            if (towerCell.Up.Right.Tower != null)
            {
                if (!towersEffected.Contains(towerCell.Up.Right.Tower))
                {
                    towerCell.Up.Right.Tower.addEffect(new RangeBoostEffect(towerCell.Up.Right.Tower, towerCell));
                    towersEffected.Add(towerCell.Up.Right.Tower);
                }
            }
            if (towerCell.Down.Left.Tower != null)
            {
                if (!towersEffected.Contains(towerCell.Down.Left.Tower))
                {
                    towerCell.Down.Left.Tower.addEffect(new RangeBoostEffect(towerCell.Down.Left.Tower, towerCell));
                    towersEffected.Add(towerCell.Down.Left.Tower);
                }
            }
            if (towerCell.Down.Right.Tower != null)
            {
                if (!towersEffected.Contains(towerCell.Down.Right.Tower))
                {
                    towerCell.Down.Right.Tower.addEffect(new RangeBoostEffect(towerCell.Down.Right.Tower, towerCell));
                    towersEffected.Add(towerCell.Down.Right.Tower);
                }
            }
        }

        public override bool fire()
        {
            return true;
        }
    }
}
