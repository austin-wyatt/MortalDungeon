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
    internal class GenericSelectGround : Ability
    {
        internal Action<BaseTile> OnGroundSelected = null;

        internal GenericSelectGround(Unit castingUnit, int range = 3)
        {
            Type = AbilityTypes.Empty;
            DamageType = DamageType.NonDamaging;
            Range = range;
            CastingUnit = castingUnit;

            CanTargetGround = true;
            UnitTargetParams.IsHostile = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            Name = "Generic Select Ground";

            Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.QuestionMark, Spritesheets.IconSheet, true);
        }

        internal override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
        {
            base.GetValidTileTargets(tileMap);

            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(CastingUnit.Info.TileMapPosition, Range)
            {
                TraversableTypes = new List<TileClassification>() { TileClassification.Ground },
                Units = units,
                CastingUnit = CastingUnit
            };

            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(param);

            TrimTiles(validTiles, units);

            if (CastingUnit.AI.ControlType == ControlType.Controlled && CanTargetGround)
            {
                validTiles.ForEach(tile =>
                {
                    tile.TilePoint.ParentTileMap.Controller.SelectTile(tile);
                });
            }

            return validTiles;
        }

        internal override void OnTileClicked(TileMap map, BaseTile tile)
        {
            if (AffectedTiles.Exists(t => t == tile))
            {
                SelectedTile = tile;
                EnactEffect();
                Scene._selectedAbility = null;

                map.Controller.DeselectTiles();
            }
        }


        internal override void OnCast()
        {
            ClearSelectedTiles();

            base.OnCast();
        }

        internal override void OnAICast()
        {
            base.OnAICast();
        }

        internal override void EnactEffect()
        {
            base.EnactEffect();

            Casted();
            EffectEnded();

            OnGroundSelected?.Invoke(SelectedTile);
        }

        internal override void OnRightClick()
        {
            base.OnRightClick();
        }

        internal override void OnAbilityDeselect()
        {
            ClearSelectedTiles();

            base.OnAbilityDeselect();

            SelectedTile = null;
        }

        internal void ClearSelectedTiles()
        {
            lock (AffectedTiles)
                AffectedTiles.ForEach(tile =>
                {
                    tile.TilePoint.ParentTileMap.Controller.DeselectTiles();
                });
        }
    }
}
