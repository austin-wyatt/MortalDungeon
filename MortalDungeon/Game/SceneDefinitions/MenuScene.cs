﻿using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using MortalDungeon.Game.UI;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Engine_Classes.UIComponents;
using System.Linq;
using MortalDungeon.Game.Tiles.TileMaps;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Map.FeatureEquations;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Objects.PropertyAnimations;
using MortalDungeon.Game.Lighting;

namespace MortalDungeon.Game.SceneDefinitions
{
    class MenuScene : CombatScene
    {
        public MenuScene() : base()
        {
            InitializeFields();
        }

        public override void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null) 
        {
            base.Load(camera, cursorObject, mouseRay);

            PathParams riverParams = new PathParams(new FeaturePoint(-1000, 27), new FeaturePoint(1000, 50), 3);
            riverParams.AddStop(new FeaturePoint(25, 45));
            riverParams.AddStop(new FeaturePoint(40, 30));
            riverParams.AddStop(new FeaturePoint(80, 35));

            River_1 river = new River_1(riverParams);


            PathParams pathParams = new PathParams(new FeaturePoint(-45, -3000), new FeaturePoint(0, 1000), 3);
            pathParams.AddMeanderingPoints(20, 0.25f, 1, 0, 123456);

            Path_1 path = new Path_1(pathParams);


            ForestParams forestParams = new ForestParams(new FeaturePoint(0, 0), 200, 0.1);
            Forest_1 forest = new Forest_1(forestParams);

            GraveyardParams graveyardParams = new GraveyardParams(new FeaturePoint(0, 100), 30, 15, 0.1f);
            Graveyard_1 graveyard = new Graveyard_1(graveyardParams);

            river.GenerateFeature();
            _tileMapController.AddFeature(river);

            path.GenerateFeature();
            _tileMapController.AddFeature(path);

            graveyard.GenerateFeature();
            _tileMapController.AddFeature(graveyard);

            forest.GenerateFeature();
            _tileMapController.AddFeature(forest);



            TestTileMap tileMap = new TestTileMap(default, new TileMapPoint(0, 0), _tileMapController) { Width = 50, Height = 50 };
            tileMap.PopulateTileMap();

            _tileMapController.AddTileMap(new TileMapPoint(0, 0), tileMap);
            _tileMapController.ApplyLoadedFeaturesToMap(tileMap);

            _tileMapController.LoadSurroundingTileMaps(tileMap.TileMapCoords);


            UnitTeam.PlayerUnits.SetRelation(UnitTeam.Skeletons, Relation.Hostile);
            UnitTeam.PlayerUnits.SetRelation(UnitTeam.PlayerUnits, Relation.Friendly);
            UnitTeam.PlayerUnits.SetRelation(UnitTeam.BadGuys, Relation.Neutral);


            Guy guy = new Guy(tileMap[0, 0].Position + new Vector3(0, -tileMap.Tiles[0].GetDimensions().Y / 2, 0.2f), this, tileMap[0, 0]) { Clickable = true };
            guy.SetTeam(UnitTeam.PlayerUnits);
            guy.Info._movementAbility.EnergyCost = 0.3f;
            CurrentUnit = guy;

            guy.Info.PrimaryUnit = true;
            guy.SelectionTile.UnitOffset.Y += tileMap.Tiles[0].GetDimensions().Y / 2;
            guy.SelectionTile.SetPosition(guy.Position);

            _units.Add(guy);


            Guy badGuy = new Guy(tileMap[0, 3].Position + new Vector3(0, -tileMap.Tiles[0].GetDimensions().Y / 2, 0.2f), this, tileMap[0, 3]) { Clickable = true };
            badGuy.SetTeam(UnitTeam.BadGuys);
            badGuy.SetColor(new Vector4(0.76f, 0.14f, 0.26f, 1));

            badGuy.SelectionTile.UnitOffset.Y += tileMap.Tiles[0].GetDimensions().Y / 2;
            badGuy.SelectionTile.SetPosition(badGuy.Position);

            badGuy.AI.ControlType = ControlType.Basic_AI;

            _units.Add(badGuy);

            UIBlock statusBarContainer = new UIBlock(new Vector3());
            statusBarContainer.MultiTextureData.MixTexture = false;
            statusBarContainer.SetAllInline(0);
            statusBarContainer.SetColor(Colors.Transparent);

            //for (int j = 0; j < 20; j++)
            //{
            //    for (int i = 0; i < 10; i++)
            //    {
            //        Skeleton skele = new Skeleton(tileMap[10 + j, i * 2].Position + new Vector3(0, -tileMap.Tiles[0].GetDimensions().Y / 2, 0.2f), this, tileMap[10 + j, i * 2]) { Clickable = true };
            //        //Skeleton skele = new Skeleton(tileMap[10, i * 2].Position + new Vector3(0, -tileMap.Tiles[0].GetDimensions().Y / 2, 0.2f), this, tileMap[10, i * 2]) { Clickable = true };
            //        //skele.SetTeam(UnitTeam.PlayerUnits);
            //        skele.SetTeam((UnitTeam)(new Random().Next() % 3 + 1));
            //        skele.SetColor(new Vector4(0.76f, 0.14f, 0.26f, 1));

            //        skele.SelectionTile.UnitOffset.Y += tileMap.Tiles[0].GetDimensions().Y / 2;
            //        skele.SelectionTile.SetPosition(skele.Position);

            //        skele.AI.ControlType = ControlType.Controlled;


            //        //UnitStatusBar skeleStatusBar = new UnitStatusBar(skele, _camera);

            //        //statusBarContainer.AddChild(skeleStatusBar);

            //        _units.Add(skele);
            //    }
            //}

            for (int i = 0; i < 1; i++)
            {
                Fire fire = new Fire(new Vector3(1150 + i * 250, 950, 0.2f));

                _genericObjects.Add(fire);
            }

            MountainTwo mountainBackground = new MountainTwo(new Vector3(30000, 0, -50));
            mountainBackground.BaseObjects[0].GetDisplay().ScaleAll(10);
            _genericObjects.Add(mountainBackground);
            mountainBackground.SetPosition(new Vector3(guy.Position.X, guy.Position.Y - 200, -50));
            _camera.SetPosition(new Vector3(guy.Position.X / WindowConstants.ScreenUnits.X * 2, guy.Position.Y / WindowConstants.ScreenUnits.Y * -2, _camera.Position.Z));


            float footerHeight = 300;

            Footer = new GameFooter(footerHeight, this); ;
            AddUI(Footer, 100);



            EnergyDisplayBar energyDisplayBar = new EnergyDisplayBar(this, new Vector3(30, WindowConstants.ScreenUnits.Y - Footer.GetDimensions().Y - 30, 0), new UIScale(1, 1), 10);

            EnergyDisplayBar = energyDisplayBar;

            AddUI(energyDisplayBar);

            TurnDisplay = new TurnDisplay();
            TurnDisplay.SetPosition(WindowConstants.CenterScreen - new Vector3(0, WindowConstants.ScreenUnits.Y / 2 - 50, 0));

            AddUI(TurnDisplay);

            UnitStatusBar guyStatusBar = new UnitStatusBar(guy, _camera);

            badGuy.Name = "Other guy";
            UnitStatusBar guyStatusBar2 = new UnitStatusBar(badGuy, _camera);


            badGuy.SetShields(5);
            guy.SetShields(5);

            Skeleton skeleton = new Skeleton(tileMap[1, 5].Position + new Vector3(0, -tileMap.Tiles[0].GetDimensions().Y / 2, 0.2f), this, tileMap[1, 5]) { };
            skeleton.Name = "John";

            skeleton.SetTeam(UnitTeam.Skeletons);
            UnitStatusBar skeletonStatusBar = new UnitStatusBar(skeleton, _camera);

            skeleton.SelectionTile.UnitOffset.Y += tileMap.Tiles[0].GetDimensions().Y / 2;
            skeleton.SelectionTile.SetPosition(skeleton.Position);

            skeleton.AI.ControlType = ControlType.Basic_AI;


            _units.Add(skeleton);


            statusBarContainer.AddChild(skeletonStatusBar);
            statusBarContainer.AddChild(guyStatusBar2);
            statusBarContainer.AddChild(guyStatusBar);

            AddUI(statusBarContainer);


            EndCombat();
            FillInTeamFog();
        }

        public override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
        }
        public override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }

        public override bool OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (!base.OnKeyUp(e)) 
            {
                return false;
            }

            switch (e.Key) 
            {
                case Keys.F11:
                    if (CurrentUnit != null) 
                    {
                        CurrentUnit.Info._movementAbility.CancelMovement();
                    }
                    break;
            }

            return true;
        }

        public override bool OnMouseMove()
        {
            return base.OnMouseMove();
        }

        public override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            float _cameraSpeed = 4.0f;

            if (!GetBit(_interceptKeystrokes, ObjectType.All) && _focusedObj == null)
            {

                if (MouseState.ScrollDelta[1] < 0)
                {
                    Vector3 movement = _camera.Front * _cameraSpeed / 2;
                    if (_camera.Position.Z - movement.Z < 26)
                    {
                        _camera.SetPosition(_camera.Position - movement); // Backwards
                        OnMouseMove();
                        OnCameraMoved();
                    }
                }
                else if (MouseState.ScrollDelta[1] > 0)
                {
                    Vector3 movement = _camera.Front * _cameraSpeed / 2;
                    if (_camera.Position.Z + movement.Z > 0)
                    {
                        _camera.SetPosition(_camera.Position + movement); // Forward
                        OnMouseMove();
                        OnCameraMoved();
                    }
                }

                if (KeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    _cameraSpeed *= 20;
                }


                if (KeyboardState.IsKeyDown(Keys.W))
                {
                    _camera.SetPosition(_camera.Position + Vector3.UnitY * _cameraSpeed * (float)args.Time);
                    OnMouseMove();
                    OnCameraMoved();
                }

                if (KeyboardState.IsKeyDown(Keys.S))
                {
                    //_camera.Position -= _camera.Front * cameraSpeed * (float)args.Time; // Backwards
                    //_camera.Position -= _camera.Up * cameraSpeed * (float)args.Time; // Down
                    _camera.SetPosition(_camera.Position - Vector3.UnitY * _cameraSpeed * (float)args.Time);
                    OnMouseMove();
                    OnCameraMoved();
                }
                if (KeyboardState.IsKeyDown(Keys.A))
                {
                    //_camera.Position -= _camera.Right * _cameraSpeed * (float)args.Time; // Left
                    //_camera.SetPosition(_camera.Position - _camera.Right * _cameraSpeed * (float)args.Time);
                    _camera.SetPosition(_camera.Position - Vector3.UnitX * _cameraSpeed * (float)args.Time);
                    OnMouseMove();
                    OnCameraMoved();
                }
                if (KeyboardState.IsKeyDown(Keys.D))
                {
                    //_camera.Position += _camera.Right * _cameraSpeed * (float)args.Time; // Right
                    //_camera.SetPosition(_camera.Position + _camera.Right * _cameraSpeed * (float)args.Time);
                    _camera.SetPosition(_camera.Position + Vector3.UnitX * _cameraSpeed * (float)args.Time);
                    OnMouseMove();
                    OnCameraMoved();
                }
                if (KeyboardState.IsKeyDown(Keys.Space))
                {
                    //_camera.Position += _camera.Up * cameraSpeed * (float)args.Time; // Up
                }
            }
        }



        public override void OnTileClicked(TileMap map, BaseTile tile, MouseButton button, ContextManager<MouseUpFlags> flags)
        {
            base.OnTileClicked(map, tile, button, flags);

            if (button == MouseButton.Left)
            {
                if (_selectedAbility != null)
                {
                    _selectedAbility.OnTileClicked(map, tile);
                }
                else if (KeyboardState.IsKeyDown(Keys.LeftAlt))
                {

                }
                else if (KeyboardState.IsKeyDown(Keys.RightAlt))
                {
                    _tileMapController.TileMaps.ForEach(m => m.PopulateFeatures());
                }
                else if (KeyboardState.IsKeyDown(Keys.RightShift))
                {
                    Unit unit = _units[0];
                    map.GetLineOfTiles(unit.Info.TileMapPosition, tile.TilePoint).ForEach(t =>
                    {
                        t.SetAnimation(AnimationType.Idle);
                        t.SetColor(new Vector4(0.9f, 0.25f, 0.25f, 1));
                    });
                    //validTiles.Clear();
                    //map.GetRingOfTiles(tile.TileIndex, validTiles, 6);
                    //validTiles.ForEach(t =>
                    //{
                    //    t.SetColor(new Vector4(0.9f, 0.25f, 0.25f, 1));
                    //});
                }
                else if (KeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    map.GenerateCliff(tile);
                }
                else if (KeyboardState.IsKeyDown(Keys.F))
                {
                    List<BaseTile> tiles = new List<BaseTile>();

                    tiles = map.GetVisionInRadius(tile.TilePoint, 6);

                    tiles.ForEach(tile =>
                    {
                        tile.Properties.Height += 2;
                    });
                }
                else if (KeyboardState.IsKeyDown(Keys.V))
                {
                    List<BaseTile> tiles = new List<BaseTile>();

                    tiles = map.GetVisionInRadius(tile.TilePoint, 6);

                    tiles.ForEach(tile =>
                    {
                        tile.Properties.Height -= 2;
                    });
                }
                else if (KeyboardState.IsKeyDown(Keys.G))
                {
                    _tileMapController.TileMaps.ForEach(m => m.GenerateCliffs());
                }
                else if (KeyboardState.IsKeyDown(Keys.H))
                {
                    tile.Properties.Height--;
                }
                else if (KeyboardState.IsKeyDown(Keys.J))
                {
                    tile.Properties.Height++;
                }
                else if (KeyboardState.IsKeyDown(Keys.M))
                {
                    _tileMapController.ToggleHeightmap();
                }
                else if (KeyboardState.IsKeyDown(Keys.KeyPad1))
                {
                    TileMapPoint temp = new TileMapPoint(map.TileMapCoords.X, map.TileMapCoords.Y);
                    if (KeyboardState.IsKeyDown(Keys.KeyPadAdd))
                    {
                        temp.X++;
                        TestTileMap tileMap = new TestTileMap(default, temp, _tileMapController) { Width = 50, Height = 50 };
                        tileMap.PopulateTileMap();

                        _tileMapController.AddTileMap(temp, tileMap);
                    }
                    else if (KeyboardState.IsKeyDown(Keys.KeyPadSubtract))
                    {
                        temp.X--;
                        TestTileMap tileMap = new TestTileMap(default, temp, _tileMapController) { Width = 50, Height = 50 };
                        tileMap.PopulateTileMap();

                        _tileMapController.AddTileMap(temp, tileMap);
                    }
                }
                else if (KeyboardState.IsKeyDown(Keys.KeyPad2))
                {
                    Console.WriteLine(_camera.Position);
                    Console.WriteLine(_units[0].Position);
                }
                else if (KeyboardState.IsKeyDown(Keys.KeyPad3))
                {
                    _tileMapController.RemoveTileMap(map);
                }
                else if (KeyboardState.IsKeyDown(Keys.KeyPadDivide))
                {
                    Window._renderedItems.Clear();

                    Object3D obj = OBJParser.ParseOBJ("Resources/Wall.obj");

                    //SpritesheetObject spritesheetObject = new SpritesheetObject(50, Spritesheets.StructureSheet);
                    SpritesheetObject spritesheetObject = new SpritesheetObject(50, Spritesheets.StructureSheet, 1, 1);

                    RenderableObject testObj = new RenderableObject(spritesheetObject.Create3DObjectDefinition(obj), new Vector4(1, 1, 1, 1), Shaders.DEFAULT_SHADER);

                    //BaseObject baseObject = new BaseObject(testObj, 0, "test", WindowConstants.CenterScreen + new Vector3(0, 0, 4f));
                    BaseObject baseObject = _3DObjects.CreateBaseObject(spritesheetObject, _3DObjects.WallCornerObj, WindowConstants.CenterScreen + new Vector3(0, 0, 4f));
                    baseObject.BaseFrame.CameraPerspective = true;

                    //Engine_Classes.Rendering.Renderer.LoadTextureFromBaseObject(baseObject);

                    //Window._renderedItems.Add(baseObject);

                    GameObject gameObj = new GameObject();
                    gameObj.AddBaseObject(baseObject);

                    _genericObjects.Add(gameObj);
                }
                else if (KeyboardState.IsKeyDown(Keys.GraveAccent))
                {
                    if (_wallTemp == null)
                    {
                        _wallTemp = tile.TilePoint;
                    }
                    else 
                    {
                        TestTileMap temp = map as TestTileMap;
                        List<BaseTile> tiles = new List<BaseTile>();

                        //TileMap.PathToPointParameters param = new TileMap.PathToPointParameters(_wallTemp, tile.TilePoint, 100)
                        //{
                        //    TraversableTypes = new List<TileClassification>() { TileClassification.Ground, TileClassification.Terrain, TileClassification.Water }
                        //};

                        //List<BaseTile> tiles = map.GetPathToPoint(param);

                        //map.GetRingOfTiles(_wallTemp, tiles, 10);

                        //tiles.Insert(tiles.Count, tiles[0]);
                        //tiles.Remove(tiles[0]);
                        //tiles.Insert(tiles.Count, tiles[0]);
                        //tiles.Remove(tiles[0]);

                        //Wall.CreateWalls(map, tiles, Wall.WallMaterial.Iron);

                        Console.WriteLine(VisionMap.TargetInVision(_wallTemp, tile.TilePoint, 10, this));

                        _wallTemp = null;
                    }
                }
                //else if (KeyboardState.IsKeyDown(Keys.LeftControl))
                //{
                //    if (tile.Structure != null) 
                //    {
                //        Wall wall = tile.Structure as Wall;
                //        wall.CreateDoor(tile);
                //    }
                //}
                else if (KeyboardState.IsKeyDown(Keys.F1))
                {
                    List<BaseTile> tiles = new List<BaseTile>();

                    tiles = map.GetVisionInRadius(tile.TilePoint, 6);

                    tiles.ForEach(tile =>
                    {
                        tile.Properties.Type = (TileType)(rand.Next(3) + (int)TileType.Stone_1);

                        if (rand.NextDouble() > 0.9) 
                        {
                            tile.Properties.Type = TileType.Gravel;
                        }

                        tile.Update();
                    });
                }
                else if (KeyboardState.IsKeyDown(Keys.F2))
                {
                    List<BaseTile> tiles = new List<BaseTile>();

                    tiles = map.GetVisionInRadius(tile.TilePoint, 6);

                    tiles.ForEach(tile =>
                    {
                        if (rand.NextDouble() > 0.8) 
                        {
                            tile.Properties.Type = TileType.Dirt;
                        }

                        tile.Update();
                    });
                }
                else if (KeyboardState.IsKeyDown(Keys.F3))
                {
                    List<BaseTile> tiles = new List<BaseTile>();

                    tiles = map.GetVisionInRadius(tile.TilePoint, 2);

                    tiles.ForEach(tile =>
                    {
                        tile.Properties.Type = TileType.WoodPlank;
                        tile.Outline = true;

                        tile.Update();
                    });
                }

            }
            else
            {
                tile.OnRightClick(flags);
            }
        }

        private TilePoint _wallTemp = null;
        private int temp = 0;
    }
}
