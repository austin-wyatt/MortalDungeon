﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;

namespace MortalDungeon.Game.Tiles
{
    public class TileMapController
    {
        public List<TileMap> TileMaps = new List<TileMap>();
        public static StaticBitmap TileBitmap;

        public CombatScene Scene;

        public TileMapController(CombatScene scene = null) 
        {
            //Bitmap tempMap = new Bitmap("Resources/TileSpritesheet.png");

            //TileBitmap = new StaticBitmap(tempMap.Width, tempMap.Height);

            //for (int y = 0; y < tempMap.Height; y++) 
            //{
            //    for (int x = 0; x < tempMap.Width; x++)
            //    {
            //        TileBitmap.SetPixel(x, y, tempMap.GetPixel(x, y));
            //    }
            //}

            Scene = scene;
        }

        public void AddTileMap(TileMapPoint point, TileMap map) 
        {
            map.TileMapCoords = point;
            TileMaps.Add(map);

            PositionTileMaps();
            map.OnAddedToController();
        }

        public void PositionTileMaps() 
        {
            if (TileMaps.Count == 0)
                return;

            Vector3 tileMapDimensions = TileMaps[0].GetTileMapDimensions();

            TileMaps.ForEach(map =>
            {
                Vector3 pos = new Vector3(tileMapDimensions.X * map.TileMapCoords.X, tileMapDimensions.Y * map.TileMapCoords.Y, 0);
                map.SetPosition(pos);
            });
        }

        public void RecreateTileChunks() 
        {
            TileMaps.ForEach(map =>
            {
                map.InitializeTileChunks();
            });
        }

        internal bool IsValidTile(int xIndex, int yIndex, TileMap map)
        {
            int currX;
            int currY;
            for (int i = 0; i < TileMaps.Count; i++) 
            {
                currX = xIndex + TileMaps[i].Width * (map.TileMapCoords.X - TileMaps[i].TileMapCoords.X);
                currY = yIndex + TileMaps[i].Height * (map.TileMapCoords.Y - TileMaps[i].TileMapCoords.Y);

                if (currX >= 0 && currY >= 0 && currX < map.Width && currY < map.Height) 
                {
                    return true;
                }
                    
            }

            return false;
        }

        internal BaseTile GetTile(int xIndex, int yIndex, TileMap map)
        {
            int currX;
            int currY;
            for (int i = 0; i < TileMaps.Count; i++)
            {
                currX = xIndex + TileMaps[i].Width * (map.TileMapCoords.X - TileMaps[i].TileMapCoords.X);
                currY = yIndex + TileMaps[i].Height * (map.TileMapCoords.Y - TileMaps[i].TileMapCoords.Y);

                if (currX >= 0 && currY >= 0 && currX < map.Width && currY < map.Height)
                    return TileMaps[i].GetLocalTile(currX, currY);
            }

            throw new NotImplementedException();
        }

        internal void ClearAllVisitedTiles()
        {
            TileMaps.ForEach(m => m.Tiles.ForEach(tile => tile.TilePoint._visited = false)); //clear visited tiles
        }
    }
}
