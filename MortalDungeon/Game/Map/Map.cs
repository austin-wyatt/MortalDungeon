﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.MapGeneration
{
    public class Cell
    {
        protected int CellSeed = 0;
        public int[] CellLocation = new int[2]; //0, 0 is in the top left

        public Cell(int[] location) 
        {
            if (location.Length == 2) 
            {
                location.CopyTo(CellLocation, 0);

                GenerateCellSeed();
            }
        }
        protected void GenerateCellSeed()
        {
            int xVal = new Random(int.MaxValue - CellLocation[0]).Next();
            int yVal = new Random(CellLocation[1]).Next();

            CellSeed = Math.Abs((xVal + yVal) / 2); //should be sufficient to generate a seed based on the X and Y position
        }
    }
}
