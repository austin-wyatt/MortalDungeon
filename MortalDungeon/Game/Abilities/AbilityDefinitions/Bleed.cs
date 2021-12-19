﻿using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;

namespace MortalDungeon.Game.Abilities
{
    internal class Bleed : Ability
    {
        internal Bleed(Unit castingUnit, int range = 1, float bleedDamage = 15f, int duration = 3)
        {
            Type = AbilityTypes.Debuff;
            Range = range;
            CastingUnit = castingUnit;

            Duration = duration;
            Damage = bleedDamage;

            Name = "Bleed";

            CanTargetGround = false;

            Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.BleedingDagger, Spritesheets.IconSheet, true, Icon.BackgroundType.DebuffBackground);
        }

        internal override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
        {
            base.GetValidTileTargets(tileMap);

            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(CastingUnit.Info.TileMapPosition, Range)
            {
                TraversableTypes = TileMapConstants.AllTileClassifications,
                Units = units,
                CastingUnit = CastingUnit
            };


            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(param);

            TrimTiles(validTiles, units);

            TargetAffectedUnits();

            return validTiles;
        }

        internal override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (AffectedTiles.FindIndex(t => t.TilePoint == unit.Info.TileMapPosition) != -1)
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }

        internal override void OnCast()
        {
            TileMap.Controller.DeselectTiles();

            base.OnCast();
        }

        internal override void EnactEffect()
        {
            base.EnactEffect();

            BleedDebuff bleedDebuff = new BleedDebuff(SelectedUnit, Duration, Damage);

            SelectedUnit.Info.AddBuff(bleedDebuff); 

            Casted();
            EffectEnded();
        }
    }
}
