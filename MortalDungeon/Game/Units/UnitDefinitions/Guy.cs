﻿using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units
{
    public class Guy : Unit
    {
        public Guy(Vector3 position, CombatScene scene, int tileMapPosition, string name = "Guy") : base(scene)
        {
            Name = name;
            TileMapPosition = tileMapPosition;
            Clickable = true;
            Selectable = true;

            BaseObject Guy = new BaseObject(BAD_GUY_ANIMATION.List, ObjectID, "BadGuy", position, EnvironmentObjects.BASE_TILE.Bounds);
            Guy.BaseFrame.CameraPerspective = true;
            Guy.BaseFrame.RotateX(25);

            BaseObjects.Add(Guy);

            SetPosition(position);

            VisionRadius = 6;

            Abilities.Strike melee = new Abilities.Strike(this, 1, 45);
            Abilities.Add(melee.AbilityID, melee);
        }

        public override void OnKill()
        {
            base.OnKill();

            BaseObjects[0].SetAnimation(AnimationType.Die);
        }
    }
}
