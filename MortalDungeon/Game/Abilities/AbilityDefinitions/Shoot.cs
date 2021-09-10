﻿using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using MortalDungeon.Game.Map;
using System.Diagnostics;

namespace MortalDungeon.Game.Abilities
{
    public class Shoot : Ability
    {
        public Shoot(Unit castingUnit, int range = 6, int minRange = 2, float damage = 10)
        {
            Type = AbilityTypes.RangedAttack;
            Range = range;
            MinRange = minRange;
            CastingUnit = castingUnit;
            Damage = damage;
            EnergyCost = 7;

            Name = "Shoot";

            Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.BowAndArrow, Spritesheets.IconSheet, true);
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
        {
            TilePoint point = position == null ? CastingUnit.Info.TileMapPosition.TilePoint : position.TilePoint;

            //List <BaseTile> validTiles = tileMap.GetTargetsInRadius(point, (int)Range, new List<TileClassification>(), units);
            List<Unit> validUnits = VisionMap.GetUnitsInRadius(CastingUnit, units, (int)Range, Scene);
            
            TrimUnits(validUnits, false, MinRange);

            TargetAffectedUnits();

            return new List<BaseTile>();
        }

        public override bool UnitInRange(Unit unit, BaseTile position = null)
        {
            TilePoint point = position == null ? CastingUnit.Info.TileMapPosition.TilePoint : position.TilePoint;
            
            bool inRange = false;

            CastingUnit.Info.TemporaryPosition = point;

            List<Vector2i> teamVision = VisionMap.GetTeamVision(CastingUnit.AI.Team, Scene);

            Vector2i clusterPos = Scene._tileMapController.PointToClusterPosition(unit.Info.TileMapPosition);
            

            if (teamVision.Exists(p => p == clusterPos)
                && VisionMap.TargetInVision(point, unit.Info.TileMapPosition, (int)Range, Scene)) 
            {
                return true;
            }

            return inRange;
        }

        public override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (unit.AI.Team != CastingUnit.AI.Team && AffectedUnits.FindIndex(u => u.ObjectID == unit.ObjectID) != -1)
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }

        public override void OnCast()
        {
            TileMap.DeselectTiles();

            base.OnCast();
        }

        public override void OnAICast()
        {
            base.OnAICast();
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            SelectedUnit.ApplyDamage(GetDamage(), DamageType);

            Casted();
            EffectEnded();
        }
    }
}
