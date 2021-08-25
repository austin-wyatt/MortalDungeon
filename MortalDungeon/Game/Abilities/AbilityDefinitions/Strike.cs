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
    public class Strike : Ability
    {
        public Strike(Unit castingUnit, int range = 1, float damage = 10)
        {
            Type = AbilityTypes.MeleeAttack;
            Range = range;
            CastingUnit = castingUnit;
            Damage = damage;
            EnergyCost = 5;

            Name = "Strike";

            Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.CrossedSwords, Spritesheets.IconSheet, true);
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default)
        {
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

        public override bool UnitInRange(Unit unit)
        {
            GetValidTileTargets(unit.GetTileMap(), new List<Unit> { unit });

            return AffectedUnits.Exists(u => u.ObjectID == unit.ObjectID);
        }

        public override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;
            
            if (unit.AI.Team != CastingUnit.AI.Team && AffectedTiles.FindIndex(t => t.TilePoint == unit.Info.TileMapPosition) != -1) 
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }

        public override void OnCast()
        {
            Scene.EnergyDisplayBar.HoverAmount(0);

            float energyCost = GetEnergyCost();

             //special cases for energy reduction go here

            Scene.EnergyDisplayBar.AddEnergy(-energyCost);

            TileMap.DeselectTiles();

            base.OnCast();
        }

        public override void OnAICast()
        {
            float energyCost = GetEnergyCost();

            CastingUnit.Info.Energy -= energyCost;

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
