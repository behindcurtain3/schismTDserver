﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace schismTD
{
    public class DamageBoostTower : Tower
    {
        List<Tower> towersEffected = new List<Tower>();
        Cell towerCell;

        public DamageBoostTower(Game g, Player p, Player opponent, Vector2 pos) : base(g, p, opponent, pos)
        {
            FireRate = 0;
            Range = 0;
            Damage = 0;
            SellValue = 210;

            EffectedFireRate = FireRate;
            EffectedDamage = Damage;
            EffectedRange = Range;

            Type = Tower.DAMAGE_BOOST;
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
                t.Effects.RemoveAll(delegate(Effect e)
                {
                    return e.type == "damageboost";
                });
            }
        }

        public override void update(long dt)
        {
            applyEffects(dt);

            if (towerCell == null)
                return;
            lock (towersEffected)
            {

                if (towerCell.Up.Tower != null)
                {
                    if (!towersEffected.Contains(towerCell.Up.Tower))
                    {
                        towerCell.Up.Tower.addEffect(new DamageBoostEffect(towerCell.Up.Tower, towerCell));
                        towersEffected.Add(towerCell.Up.Tower);
                    }
                }
                if (towerCell.Left.Tower != null)
                {
                    if (!towersEffected.Contains(towerCell.Left.Tower))
                    {
                        towerCell.Left.Tower.addEffect(new DamageBoostEffect(towerCell.Left.Tower, towerCell));
                        towersEffected.Add(towerCell.Left.Tower);
                    }
                }
                if (towerCell.Right.Tower != null)
                {
                    if (!towersEffected.Contains(towerCell.Right.Tower))
                    {
                        towerCell.Right.Tower.addEffect(new DamageBoostEffect(towerCell.Right.Tower, towerCell));
                        towersEffected.Add(towerCell.Right.Tower);
                    }
                }
                if (towerCell.Down.Tower != null)
                {
                    if (!towersEffected.Contains(towerCell.Down.Tower))
                    {
                        towerCell.Down.Tower.addEffect(new DamageBoostEffect(towerCell.Down.Tower, towerCell));
                        towersEffected.Add(towerCell.Down.Tower);
                    }
                }

                if (towerCell.Up.Left.Tower != null)
                {
                    if (!towersEffected.Contains(towerCell.Up.Left.Tower))
                    {
                        towerCell.Up.Left.Tower.addEffect(new DamageBoostEffect(towerCell.Up.Left.Tower, towerCell));
                        towersEffected.Add(towerCell.Up.Left.Tower);
                    }
                }
                if (towerCell.Up.Right.Tower != null)
                {
                    if (!towersEffected.Contains(towerCell.Up.Right.Tower))
                    {
                        towerCell.Up.Right.Tower.addEffect(new DamageBoostEffect(towerCell.Up.Right.Tower, towerCell));
                        towersEffected.Add(towerCell.Up.Right.Tower);
                    }
                }
                if (towerCell.Down.Left.Tower != null)
                {
                    if (!towersEffected.Contains(towerCell.Down.Left.Tower))
                    {
                        towerCell.Down.Left.Tower.addEffect(new DamageBoostEffect(towerCell.Down.Left.Tower, towerCell));
                        towersEffected.Add(towerCell.Down.Left.Tower);
                    }
                }
                if (towerCell.Down.Right.Tower != null)
                {
                    if (!towersEffected.Contains(towerCell.Down.Right.Tower))
                    {
                        towerCell.Down.Right.Tower.addEffect(new DamageBoostEffect(towerCell.Down.Right.Tower, towerCell));
                        towersEffected.Add(towerCell.Down.Right.Tower);
                    }
                }
            }
        }

        public override bool fire()
        {
            return true;
        }
    }
}
