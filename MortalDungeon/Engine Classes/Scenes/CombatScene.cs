﻿using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Objects.PropertyAnimations;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.UI;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MortalDungeon.Engine_Classes.Scenes
{
    public enum GeneralContextFlags
    {
        UITooltipOpen,
        TileTooltipOpen,
        ContextMenuOpen,
        AbilitySelected,
        TabMenuOpen,
        EnableTileMapUpdate,

        DisableVisionMapUpdate,
        TileMapLoadInProgress,

        UpdateLighting,
        UpdateLightObstructionMap,

        UnitCollationRequired,

        CameraPanning
    }
    public class CombatScene : Scene
    {
        public int Round = 0;
        public QueuedList<Unit> InitiativeOrder = new QueuedList<Unit>();
        public int UnitTakingTurn = 0; //the unit in the initiative order that is going
        public EnergyDisplayBar EnergyDisplayBar;
        public EnergyDisplayBar ActionEnergyBar;

        public TurnDisplay TurnDisplay;
        public GameFooter Footer;

        public QueuedList<TemporaryVision> TemporaryVision = new QueuedList<TemporaryVision>();

        public static Color EnvironmentColor = new Color(0.25f, 0.25f, 0.25f, 0.25f);
        public static int Time = 0;
        public static DayNightCycle DayNightCycle;

        public static TabMenu TabMenu = new TabMenu();

        public Ability _selectedAbility = null;
        public List<Unit> _selectedUnits = new List<Unit>();


        public Unit CurrentUnit;
        public UnitTeam CurrentTeam = UnitTeam.PlayerUnits;

        public bool InCombat = false;

        public bool AbilityInProgress = false;

        public bool DisplayUnitStatuses = true;


        protected const AbilityTypes DefaultAbilityType = AbilityTypes.Move;

        Texture _normalFogTexture;
        public UIBlock _tooltipBlock;
        public Action _closeContextMenu;

        public BaseTile _debugSelectedTile;

        public CombatScene() 
        {
            Texture fogTex = Texture.LoadFromFile("Resources/FogTexture.png", default, TextureName.FogTexture);

            fogTex.Use(TextureUnit.Texture1);

            _normalFogTexture = fogTex;
        }

        protected override void InitializeFields()
        {
            base.InitializeFields();

            _tileMapController = new TileMapController(this);

            _tooltipBlock = new UIBlock(new Vector3());
            _tooltipBlock.MultiTextureData.MixTexture = false;
            _tooltipBlock.SetColor(Colors.Transparent);
            _tooltipBlock.SetAllInline(0);

            AddUI(_tooltipBlock, 10000);

            TabMenu.AddToScene(this);
            TabMenu.Display(false);
        }

        public override void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null)
        {
            base.Load(camera, cursorObject, mouseRay);


            DayNightCycle = new DayNightCycle(90, 450);
            TickableObjects.Add(DayNightCycle);

            Lighting.InitializeFramebuffers();

            LightObject = Lighting.CreateTexturedQuad(Lighting.LightTexture, TextureName.Lighting);
            LightObject.SetPosition(new Vector3(0, 0, 1f));
            LightObject.BaseObject.BaseFrame.ScaleX(0.8975f);
            LightObject.BaseObject.BaseFrame.ScaleY(1.047f);
            LightObject.ScaleAll(147.1f);
            LightObject.BaseObject.RenderData.AlphaThreshold = 0;
            LightObject.TextureLoaded = true;

            EnvironmentColor.OnChangeEvent += (_, __) => 
            {
                ContextManager.SetFlag(GeneralContextFlags.UpdateLighting, true);
            };

            #region Light modals
            UIBlock leftModal = new UIBlock(new Vector3(-66025, 10697f, -0f), new UIScale(300, 154f));
            leftModal.BaseObject.BaseFrame.CameraPerspective = true;
            leftModal.BaseObject.RenderData.AlphaThreshold = 0;
            leftModal.BaseObject.BaseFrame.SetBaseColor(EnvironmentColor.ToVector());
            leftModal.MultiTextureData.MixTexture = false;
            leftModal.BaseObject.OutlineParameters.SetAllInline(0);

            EnvironmentColor.OnChangeEvent += (_, __) =>
            {
                leftModal.BaseObject.BaseFrame.SetBaseColor(EnvironmentColor.ToVector());
            };

            _lowPriorityObjects.Add(leftModal);

            UIBlock rightModal = new UIBlock(new Vector3(84360.5f, 10697f, -0f), new UIScale(300, 154f));
            rightModal.BaseObject.BaseFrame.CameraPerspective = true;
            rightModal.BaseObject.RenderData.AlphaThreshold = 0;
            rightModal.BaseObject.BaseFrame.SetBaseColor(EnvironmentColor.ToVector());
            rightModal.MultiTextureData.MixTexture = false;
            rightModal.BaseObject.OutlineParameters.SetAllInline(0);

            EnvironmentColor.OnChangeEvent += (_, __) =>
            {
                rightModal.BaseObject.BaseFrame.SetBaseColor(EnvironmentColor.ToVector());
            };

            _lowPriorityObjects.Add(rightModal);

            UIBlock topModal = new UIBlock(new Vector3(0, -102802.5f, -0f), new UIScale(750, 300f));
            topModal.BaseObject.BaseFrame.CameraPerspective = true;
            topModal.BaseObject.RenderData.AlphaThreshold = 0;
            topModal.BaseObject.BaseFrame.SetBaseColor(EnvironmentColor.ToVector());
            topModal.MultiTextureData.MixTexture = false;
            topModal.BaseObject.OutlineParameters.SetAllInline(0);

            EnvironmentColor.OnChangeEvent += (_, __) =>
            {
                topModal.BaseObject.BaseFrame.SetBaseColor(EnvironmentColor.ToVector());
            };

            _lowPriorityObjects.Add(topModal);

            UIBlock botModal = new UIBlock(new Vector3(0, 124203.5f, -0f), new UIScale(750, 300f));
            botModal.BaseObject.BaseFrame.CameraPerspective = true;
            botModal.BaseObject.RenderData.AlphaThreshold = 0;
            botModal.BaseObject.BaseFrame.SetBaseColor(EnvironmentColor.ToVector());
            botModal.MultiTextureData.MixTexture = false;
            botModal.BaseObject.OutlineParameters.SetAllInline(0);

            EnvironmentColor.OnChangeEvent += (_, __) =>
            {
                botModal.BaseObject.BaseFrame.SetBaseColor(EnvironmentColor.ToVector());
            };

            _lowPriorityObjects.Add(botModal);

            #endregion
        }

        /// <summary>
        /// Start the next round
        /// </summary>
        public virtual void AdvanceRound()
        {
            Round++;

            StartRound();
        }

        /// <summary>
        /// End the current round and calculate anything that needs to be calculated at that point
        /// </summary>
        public virtual void CompleteRound()
        {
            //do stuff that needs to be done when a round is completed
            InitiativeOrder = QueuedList<Unit>.FromEnumerable(InitiativeOrder.OrderBy(i => i.Info.Speed));

            AdvanceRound();

            _units.ForEach(unit =>
            {
                for (int i = 0; i < unit.Info.Buffs.Count; i++)
                {
                    unit.Info.Buffs[i].OnRoundEnd();
                }
            });
        }

        /// <summary>
        /// Makes any calculations that need to be made at the start of the round
        /// </summary>
        public virtual void StartRound()
        {
            UnitTakingTurn = 0;

            InitiativeOrder = QueuedList<Unit>.FromEnumerable(InitiativeOrder.OrderBy(i => i.Info.Speed)); //sort the list by speed

            //do calculations here (advance an event, show a cutscene, etc)

            _units.ForEach(unit =>
            {
                for (int i = 0; i < unit.Info.Buffs.Count; i++)
                {
                    unit.Info.Buffs[i].OnRoundStart();
                }
            });

            TemporaryVision.ForEach(t =>
            {
                if (t.TickTarget == TickDurationTarget.OnRoundStart)
                {
                    TickTemporaryVision(t);
                }
            });
            UpdateTemporaryVision();

            StartTurn();
        }

        /// <summary>
        /// Start the turn for the unit that is currently up in the initiative order
        /// </summary>
        public virtual void StartTurn()
        {
            UnitTeam prevTeam = CurrentUnit.AI.Team;

            CurrentUnit = InitiativeOrder[UnitTakingTurn];

            
            TemporaryVision.ForEach(t =>
            {
                if (t.TickTarget == TickDurationTarget.OnUnitTurnStart && t.TargetUnit.ObjectID == CurrentUnit.ObjectID) 
                {
                    TickTemporaryVision(t);
                }
            });
            UpdateTemporaryVision();

            //max energy displayed is the larger between current energy with buffs and default max energy.
            //If buffs are reducing energy the max will still be the default max for the unit.
            if (CurrentUnit.AI.ControlType == ControlType.Controlled)
            {
                SetCurrentUnitEnergy();

                DeselectAllUnits();

                Footer.UpdateFooterInfo(CurrentUnit);
                CurrentUnit.Select();

                Footer.EndTurnButton.SetRender(true);

                FillInTeamFog(true);

                Vector4 pos = CurrentUnit.BaseObject.BaseFrame.Position;

                SmoothPanCamera(new Vector3(pos.X, pos.Y - _camera.Position.Z / 5, _camera.Position.Z), 1);
            }
            else 
            {
                Footer.EndTurnButton.SetRender(false);
            }

            TurnDisplay.SetCurrentUnit(UnitTakingTurn);

            lock (CurrentUnit.Info.Buffs)
            for (int i = 0; i < CurrentUnit.Info.Buffs.Count; i++)
            {
                CurrentUnit.Info.Buffs[i].OnTurnStart();
            }

            List<Ability> abilitiesToDecay = new List<Ability>();

            lock (CurrentUnit.Info.Abilities)
            foreach (var ability in CurrentUnit.Info.Abilities)
            {
                if (ability.DecayCombo()) 
                {
                    abilitiesToDecay.Add(ability);
                }
            }

            foreach (var ability in abilitiesToDecay) 
            {
                ability.CompleteDecay();
            }

            if (CurrentUnit.AI.ControlType == ControlType.Controlled)
            {
                Footer.UpdateFooterInfo(CurrentUnit);
            }

            CurrentUnit.OnTurnStart();
        }

        public void SetCurrentUnitEnergy() 
        {
            float maxEnergy = CurrentUnit.Info.MaxEnergy > CurrentUnit.Info.CurrentEnergy ? CurrentUnit.Info.MaxEnergy : CurrentUnit.Info.CurrentEnergy;
            EnergyDisplayBar.SetMaxEnergy(maxEnergy);
            EnergyDisplayBar.SetActiveEnergy(CurrentUnit.Info.CurrentEnergy);

            float maxActionEnergy = CurrentUnit.Info.MaxActionEnergy > CurrentUnit.Info.CurrentActionEnergy ? CurrentUnit.Info.MaxActionEnergy : CurrentUnit.Info.CurrentActionEnergy;
            ActionEnergyBar.SetMaxEnergy(maxActionEnergy);
            ActionEnergyBar.SetActiveEnergy(CurrentUnit.Info.CurrentActionEnergy);
        }

        /// <summary>
        /// Complete the current unit's turn and start the next unit's turn
        /// </summary>
        public virtual void CompleteTurn()
        {
            if (CurrentUnit != null) 
            {
                CurrentUnit.OnTurnEnd();
            }

            TemporaryVision.ForEach(t =>
            {
                if (t.TickTarget == TickDurationTarget.OnUnitTurnEnd && t.TargetUnit.ObjectID == CurrentUnit.ObjectID)
                {
                    TickTemporaryVision(t);
                }
            });
            UpdateTemporaryVision();

            UnitTakingTurn++;


            if (UnitTakingTurn == InitiativeOrder.Count)
            {
                CompleteRound();
                return;
            }


            for (int i = 0; i < CurrentUnit.Info.Buffs.Count; i++)
            {
                CurrentUnit.Info.Buffs[i].OnTurnEnd();
            }

            if (InCombat) 
            {
                StartTurn(); //Advance to the next unit's turn
            }
        }

        public virtual void StartCombat() 
        {
            InitiativeOrder.RemoveAll(u => u.Info.NonCombatant || u.Info.Dead);

            if (InitiativeOrder.Count == 0)
                return;

            EnergyDisplayBar.SetRender(true);
            ActionEnergyBar.SetRender(true);

            //Footer.EndTurnButton.SetDisabled(false);
            Footer.EndTurnButton.SetRender(true);

            TurnDisplay.SetRender(true);
            TurnDisplay.SetUnits(InitiativeOrder);


            InCombat = true;

            Round = 0;


            EvaluateVentureButton();

            FillInTeamFog(true);

            StartRound();
        }

        public virtual void EndCombat() 
        {
            //InitiativeOrder.ForEach(unit =>
            //{
            //    unit.RefillAbilityCharges();
            //});

            InitiativeOrder.Clear();
            InCombat = false;

            EnergyDisplayBar.SetRender(false);
            ActionEnergyBar.SetRender(false);
            Footer.EndTurnButton.SetRender(false);

            EvaluateVentureButton();

            TurnDisplay.SetRender(false);
            TurnDisplay.SetUnits(InitiativeOrder);

            SetCurrentUnitEnergy();

            Footer.UpdateFooterInfo();

            FillInTeamFog(true);
        }

        public virtual void SelectAbility(Ability ability, Unit unit)
        {
            if (unit.AI.ControlType != ControlType.Controlled || AbilityInProgress)
                return;

            if(_selectedAbility != null) 
            {
                DeselectAbility();
            }

            ContextManager.SetFlag(GeneralContextFlags.AbilitySelected, true);

            Message msg = new Message(MessageType.Request, MessageBody.InterceptKeyStrokes, MessageTarget.All);
            MessageCenter.SendMessage(msg);

            _selectedAbility = ability;
            ability.OnSelect(this, ability.CastingUnit.GetTileMap());

            _onSelectAbilityActions.ForEach(a => a?.Invoke(ability));
        }

        public virtual void DeselectAbility()
        {
            if (_selectedAbility != null)
            {
                _selectedAbility.TileMap.DeselectTiles();

                _selectedAbility?.OnAbilityDeselect();
                _selectedAbility = null;
            }

            ContextManager.SetFlag(GeneralContextFlags.AbilitySelected, false);

            Message msg = new Message(MessageType.Request, MessageBody.EndKeyStrokeInterception, MessageTarget.All);
            MessageCenter.SendMessage(msg);

            _onDeselectAbilityActions.ForEach(a => a?.Invoke());
        }

        public void SelectUnit(Unit unit) 
        {
            if (_selectedUnits.Count > 0) 
            {
                DeselectUnits();
            }

            _selectedUnits.Add(unit);
            unit.Select();
        }

        public void DeselectUnit(Unit unit) 
        {
            _selectedUnits.Remove(unit);
            unit.Deselect();
        }

        public void DeselectUnits() 
        {
            _selectedUnits.ForEach(u => u.Deselect());
            _selectedUnits.Clear();
        }

        public void DeselectAllUnits()
        {
            _units.ForEach(u => u.Deselect());
        }

        public virtual void FillInTeamFog(bool updateAll = false, bool waitForVisionMap = true) 
        {
            if (ContextManager.GetFlag(GeneralContextFlags.TileMapLoadInProgress))
            {
                return;
            }

            Task temp = new Task(() =>
            {
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();

                //don't allow the tilemap to be re-rendered until all of the changes are applied
                ContextManager.SetFlag(GeneralContextFlags.EnableTileMapUpdate, false);
                ContextManager.SetFlag(GeneralContextFlags.DisableVisionMapUpdate, true);


                if (waitForVisionMap && VisionMapTask != null)
                {
                    VisionMapTask.Wait();
                    VisionMapTask = null;
                }

                List<Task> visionTasks = new List<Task>();
                foreach (UnitTeam team in ActiveTeams)
                {
                    visionTasks.Add(Task.Run(() =>
                    {
                        bool isPlayerTeam = team == UnitTeam.PlayerUnits;

                        for (int i = 0; i < VisionMap.DIMENSIONS; i++)
                        {
                            for (int j = 0; j < VisionMap.DIMENSIONS; j++)
                            {
                        
                                BaseTile tile = _tileMapController.GetTile(i, j);

                                if (VisionMap.InVision(i, j, team))
                                {
                                    if (tile == null) continue;

                                    tile.SetExplored(true, team);
                                    tile.SetFog(false, team);
                                }
                                else
                                {
                                    if (tile == null) continue;

                                    if (isPlayerTeam && !InCombat && !tile.Properties.MustExplore) 
                                    {
                                        tile.SetFog(false, team);
                                    }
                                    else 
                                    {
                                        tile.SetFog(true, team);
                                    }

                                    if (updateAll)
                                        tile.Update();
                                }
                            }
                        }
                    }));
                }

                foreach (var task in visionTasks)
                {
                    task.Wait();
                }

                TemporaryVision.ForEach(vision =>
                {
                    vision.TilesToReveal.ForEach(tile =>
                    {
                        tile.SetExplored(true, vision.Team);
                        tile.SetFog(false, vision.Team);
                    });
                });

                //if (!InCombat)
                //{
                //    _tileMapController.TileMaps.ForEach(map =>
                //    {
                //        map.Tiles.ForEach(tile =>
                //        {
                //            if (!tile.Properties.MustExplore)
                //            {
                //                //tile.SetExplored(true, UnitTeam.PlayerUnits);
                //                tile.SetFog(false, UnitTeam.PlayerUnits);
                //            }
                //        });
                //    });
                //}


                CalculateRevealedUnits();

                HideNonVisibleObjects();

                //Console.WriteLine($"Fog filled in in {stopwatch.ElapsedMilliseconds}ms");

                ContextManager.SetFlag(GeneralContextFlags.EnableTileMapUpdate, true);
                ContextManager.SetFlag(GeneralContextFlags.DisableVisionMapUpdate, false);
                //});
            });

            temp.Start();
        }

        public List<BaseTile> GetTeamVision(UnitTeam team)
        {
            List<BaseTile> returnList = new List<BaseTile>();

            for (int i = 0; i < VisionMap.DIMENSIONS; i++)
            {
                for (int j = 0; j < VisionMap.DIMENSIONS; j++)
                {
                    if (VisionMap.InVision(i, j, team))
                    {
                        BaseTile tile = _tileMapController.GetTile(i, j);

                        if (tile == null)
                            continue;

                        returnList.Add(tile);
                    }
                }
            }

            return returnList;
        }


        public void FillInAllFog(bool reveal = false, bool updateAll = false)
        {
            //foreach (UnitTeam team in Enum.GetValues(typeof(UnitTeam)))
            List<Task> fogTasks = new List<Task>();

            foreach (UnitTeam team in ActiveTeams)
            {
                fogTasks.Add(new Task(() =>
                {
                    _tileMapController.TileMaps.ForEach(m =>
                    {
                        if (!m.Render)
                            return;
                        m.Tiles.ForEach(tile =>
                        {
                            if (reveal)
                            {
                                tile.SetExplored(true, team);
                                tile.SetFog(false, team);
                            }
                            else
                            {
                                //tile.SetExplored(tile.Explored[team], team);
                                tile.SetFog(true, team);
                            }

                            if (updateAll)
                                tile.Update();
                        });
                    });
                }));
            }

            foreach (var task in fogTasks) 
            {
                task.Start();
            }

            foreach (var task in fogTasks)
            {
                task.Wait();
            }
        }

        public void CalculateRevealedUnits() 
        {
            for (int i = 0; i < _units.Count; i++) 
            {
                //if the unit isn't attempting to hide then it is revealed
                if (!_units[i].Info.Stealth.Hiding) 
                {
                    _units[i].Info.Stealth.SetAllRevealed();
                    continue;
                }

                foreach (UnitTeam team in ActiveTeams) 
                {
                    //we don't need to check the unit's team
                    if (team == _units[i].AI.Team)
                        continue;

                    //if the unit's position is in fog for a team then we can assume that it's hidden
                    if (_units[i].Info.Stealth.PositionInFog(team))
                    {
                        _units[i].Info.Stealth.SetRevealed(team, false);
                        continue;
                    }

                    //if we get to this point then we need to compare and contrast Stealth and Scout skill levels for the units
                    bool couldSee = false;
                    _units.ForEach(u =>
                    {
                        if (u.AI.Team == team) 
                        {
                            if (u.Info.Scouting.CouldSeeUnit(_units[i], _units[i].Info.Map.GetDistanceBetweenPoints(u.Info.Point, _units[i].Info.Point))) 
                            {
                                couldSee = true;
                            }
                        }
                    });

                    //if a unit is being revealed from stealth then we want to stop the current movement ability
                    if (team == CurrentTeam && !_units[i].Info.Stealth.Revealed[team] && couldSee) 
                    {
                        if (CurrentUnit.AI.ControlType == ControlType.Controlled && CurrentUnit.Info._movementAbility.Moving) 
                        {
                            CurrentUnit.Info._movementAbility.CancelMovement();
                        }
                    }

                    _units[i].Info.Stealth.SetRevealed(team, couldSee);

                }
            }
        }

        public void HideNonVisibleObjects() 
        {
            try
            {
                _units.ForEach(unit =>
                {
                    if (unit.Info.Visible(CurrentTeam))
                    {
                        unit.SetRender(true);
                    }
                    else
                    {
                        unit.SetRender(false);
                    }
                });

                for (int i = 0; i < _tileMapController.TileMaps.Count; i++)
                {
                    for (int j = 0; j < _tileMapController.TileMaps[i].TileChunks.Count; j++)
                    {
                        for (int k = 0; k < _tileMapController.TileMaps[i].TileChunks[j].Structures.Count; k++)
                        {
                            Game.Structures.Structure structure = _tileMapController.TileMaps[i].TileChunks[j].Structures[k];

                            if (structure.Info.Visible(CurrentTeam))
                            {
                                structure.SetRender(true);
                            }
                            else
                            {
                                structure.SetRender(false);
                            }
                        }
                    }
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine($"Error in HideNonVisibleObjects: {e.Message}");
            }
        }

        public void EvaluateVentureButton() 
        {
            if (Footer == null)
                return;

            if(CurrentUnit == null || CurrentUnit.AI.Team != UnitTeam.PlayerUnits || CurrentUnit.AI.ControlType != ControlType.Controlled) 
            {
                Footer.VentureForthButton.SetRender(false);
                return;
            }

            if (!InCombat && _tileMapController.PointAtEdge(CurrentUnit.Info.Point))
            {
                Footer.VentureForthButton.SetRender(true);
            }
            else 
            {
                Footer.VentureForthButton.SetRender(false);
            }
        }

        /// <summary>
        /// Determine which units should be present in the initiative order
        /// </summary>
        public void EvaluateCombat() 
        {
            foreach (var unit in _units) 
            {
                if (unit.AI.Team != UnitTeam.PlayerUnits)
                    continue;

                foreach (UnitTeam team in Enum.GetValues(typeof(UnitTeam))) 
                {
                    if (unit.AI.Team.GetRelation(team) == Relation.Hostile) 
                    {
                        if (unit.Info.Visible(team))
                        {
                            EvaluateUnitsInCombat(unit, 20);

                            if (!InCombat) 
                            {
                                foreach (var initiativeUnit in InitiativeOrder)
                                {
                                    if (InitiativeOrder.Exists(u => u.AI.Team.GetRelation(initiativeUnit.AI.Team) == Relation.Hostile)) 
                                    {
                                        //stop unit movement
                                        if (CurrentUnit != null && CurrentUnit.Info._movementAbility != null) 
                                        {
                                            CurrentUnit.Info._movementAbility.CancelMovement();
                                        }

                                        //if we aren't already in combat and we can verify that at least one enemy is present, start combat
                                        StartCombat();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void EvaluateUnitsInCombat(Unit seedUnit, int radius) 
        {
            List<Unit> unitsToCheck = new List<Unit>();
            HashSet<Unit> checkedUnits = new HashSet<Unit>();

            //Stopwatch timer = new Stopwatch();
            //timer.Start();

            unitsToCheck.Add(seedUnit);
            checkedUnits.Add(seedUnit);

            for (int i = 0; i < unitsToCheck.Count; i++)
            {
                foreach (var u in _units)
                {
                    if (!u.Info.Dead && u.Info.TileMapPosition.TileMap.GetDistanceBetweenPoints(u.Info.TileMapPosition, unitsToCheck[i].Info.TileMapPosition) <= radius)
                    {
                        Relation unitRelation = u.AI.Team.GetRelation(unitsToCheck[i].AI.Team);
                        if (unitRelation == Relation.Friendly || unitRelation == Relation.Hostile)
                        {
                            if (!checkedUnits.Contains(u))
                            {
                                unitsToCheck.Add(u);
                                checkedUnits.Add(u);
                            }

                            if (!InitiativeOrder.Contains(u))
                            {
                                InitiativeOrder.AddImmediate(u);
                                TurnDisplay.SetUnits(InitiativeOrder);
                            }
                        }
                    }
                }
            }

            //Console.WriteLine($"Combat unit evaluation completed in {timer.ElapsedMilliseconds}ms");
        }

        private bool _endTurnButtonShouldDisplayAfterAbility = false;
        public void SetAbilityInProgress(bool abilityInProgress) 
        {
            AbilityInProgress = abilityInProgress;

            if (AbilityInProgress && Footer.EndTurnButton.Render)
            {
                Footer.EndTurnButton.SetRender(false);
                _endTurnButtonShouldDisplayAfterAbility = true;
            }
            else if (!InCombat) 
            {
                _endTurnButtonShouldDisplayAfterAbility = false;
            }
            else if (_endTurnButtonShouldDisplayAfterAbility)
            {
                Footer.EndTurnButton.SetRender(true);
                _endTurnButtonShouldDisplayAfterAbility = false;
            }
        }

        public virtual void RemoveUnit(Unit unit)
        {
            _units.Remove(unit);
            InitiativeOrder.Remove(unit);

            FillInTeamFog();
            Footer.UpdateFooterInfo();
        }


        #region Event handlers

        public override void OnVisionMapUpdated()
        {
            base.OnVisionMapUpdated();

            FillInTeamFog(false, false);
        }

        public void OnUnitMoved(Unit unit, BaseTile prevTile) 
        {
            if (CurrentUnit == unit && CurrentUnit.AI.Team == UnitTeam.PlayerUnits && CurrentUnit.AI.ControlType == ControlType.Controlled) 
            {
                EvaluateVentureButton();
            }

            UnitVisionGenerators.ManuallyIncrementChangeToken();

            UpdateVisionMap(() => 
            {
                EvaluateCombat();
            }, unit.AI.Team);
        }

        public void OnStructureMoved() 
        {
            LightObstructions.ManuallyIncrementChangeToken(); //indicate that something about the light obstructions has changed

            UpdateVisionMap();
        }

        public override void OnRender()
        {
            base.OnRender();

            UpdateTemporaryVision();

            if (InitiativeOrder.HasQueuedItems()) 
            {
                InitiativeOrder.HandleQueuedItems();
            }
        }

        public override void EvaluateObjectHover(Vector3 mouseRayNear, Vector3 mouseRayFar)
        {
            _tileMapController.TileMaps.ForEach(map =>
            {
                if (!map.Render)
                    return;

                map.EndHover();

                map.TileChunks.ForEach(chunk =>
                {
                    if (!chunk.Cull) 
                    {
                        ObjectCursorBoundsCheck(chunk.Tiles, mouseRayNear, mouseRayFar, (tile) =>
                        {
                            if (tile.Hoverable)
                            {
                                map.HoverTile(tile);
                                if (_selectedAbility != null && _selectedAbility.HasHoverEffect)
                                {
                                    _selectedAbility.OnHover(tile, map);
                                }

                                if (Game.Settings.EnableTileTooltips) 
                                {
                                    UIHelpers.StringTooltipParameters param = new UIHelpers.StringTooltipParameters(this, BaseTile.GetTooltipString(tile, this), tile, _tooltipBlock)
                                    {
                                        TooltipFlag = GeneralContextFlags.TileTooltipOpen,
                                        Position = new Vector3(WindowConstants.ScreenUnits.X, 0, 0),
                                        Anchor = UIAnchorPosition.TopRight,
                                        BackgroundColor = new Vector4(0.85f, 0.85f, 0.85f, 0.9f),
                                        TextScale = 0.04f
                                    };

                                    UIHelpers.CreateToolTip(param);
                                }
                            }

                            if (tile.HasTimedHoverEffect)
                            {
                                _hoverTimer.Restart();
                                _hoveredObject = tile;
                            }
                        });
                    }
                });
            });

            ObjectCursorBoundsCheck(_units, mouseRayNear, mouseRayFar, (unit) =>
            {
                if (unit.Hoverable)
                    unit.OnHover();

                if (unit.HasTimedHoverEffect) 
                {
                    _hoverTimer.Restart();
                    _hoveredObject = unit;
                }

            }, notFound => notFound.OnHoverEnd());
        }

        public override void OnMouseUp(MouseButtonEventArgs e)
        {
            SetMouseStateFlags();

            if ((e.Button == MouseButton.Right) && e.Action == InputAction.Release && !GetBit(_interceptClicks, ObjectType.All))
            {
                if (MouseUpStateFlags.GetFlag(MouseUpFlags.ContextMenuOpen))
                {
                    CloseContextMenu();
                }

                if (_selectedAbility != null)
                {
                    _selectedAbility.OnRightClick();
                    return;
                }
            }

            CheckMouseUp(e);
        }

        protected override void SetMouseStateFlags()
        {
            base.SetMouseStateFlags();

            MouseUpStateFlags.SetFlag(MouseUpFlags.ContextMenuOpen, ContextManager.GetFlag(GeneralContextFlags.ContextMenuOpen));
            MouseUpStateFlags.SetFlag(MouseUpFlags.AbilitySelected, ContextManager.GetFlag(GeneralContextFlags.ContextMenuOpen));
            MouseUpStateFlags.SetFlag(MouseUpFlags.TabMenuOpen, ContextManager.GetFlag(GeneralContextFlags.TabMenuOpen));
        }

        protected override void ActOnMouseStateFlag(MouseUpFlags flag)
        {
            base.ActOnMouseStateFlag(flag);

            switch (flag) 
            {
                case MouseUpFlags.ContextMenuOpen:
                    MouseUpStateFlags.SetFlag(MouseUpFlags.ClickProcessed, true);
                    CloseContextMenu();
                    break;
                case MouseUpFlags.TabMenuOpen:
                    MouseUpStateFlags.SetFlag(MouseUpFlags.ClickProcessed, true);
                    //TabMenu.Display(false); //We probably don't need to close the tab menu when a click happens
                    break;
            }
        }

        public override bool OnKeyDown(KeyboardKeyEventArgs e)
        {
            bool processKeyStrokes = base.OnKeyDown(e);

            if (processKeyStrokes) 
            {
                switch (e.Key) 
                {
                    case Keys.LeftAlt:
                    case Keys.RightAlt:
                        if (!e.IsRepeat) 
                        {
                            DisplayUnitStatuses = !DisplayUnitStatuses;
                            _units.ForEach(u =>
                            {
                                if (u.StatusBarComp != null && !u.Info.Dead && u.Render) 
                                {
                                    u.StatusBarComp.SetWillDisplay(DisplayUnitStatuses);
                                }
                                    
                            });
                        }
                        break;
                    case Keys.Escape:
                        Sound sound = new Sound(Sounds.Select) { Gain = 0.15f, Pitch = GlobalRandom.NextFloat(0.5f, 0.5f) };
                        sound.Play();

                        if (ContextManager.GetFlag(GeneralContextFlags.TabMenuOpen))
                        {
                            TabMenu.Display(false);
                        }
                        else if (ContextManager.GetFlag(GeneralContextFlags.ContextMenuOpen))
                        {
                            CloseContextMenu();
                        }
                        else if (ContextManager.GetFlag(GeneralContextFlags.AbilitySelected))
                        {
                            DeselectAbility();
                        }
                        else if (ContextManager.GetFlag(GeneralContextFlags.UITooltipOpen))
                        {
                            UIForceClose(new SceneEventArgs(this, EventAction.CloseTooltip));
                        }
                        else if (Footer._currentUnit.AI.ControlType != ControlType.Controlled)
                        {
                            DeselectUnits();

                            Unit firstControlledUnit = _units.Find(u => u.AI.ControlType == ControlType.Controlled);
                            if (firstControlledUnit != null) 
                            {
                                firstControlledUnit.Select();
                            }
                        }
                        else if (_selectedUnits.Count > 0) 
                        {
                            DeselectUnits();
                        }
                        else
                        {
                            MessageCenter.SendMessage(new Message(MessageType.Request, MessageBody.Flag, MessageTarget.All) { Flag = MessageFlag.OpenEscapeMenu });
                        }
                        break;
                    case Keys.Tab:
                        if (!e.IsRepeat) 
                        {
                            TabMenu.Display(!TabMenu.Render);
                        }
                        break;
                    case Keys.F12:
                        if (!e.IsRepeat)
                        {
                            _tileMapController.LoadSurroundingTileMaps(CurrentUnit.GetTileMap().TileMapCoords);
                        }
                        break;
                }
            }

            return processKeyStrokes;
        }

        public virtual void OnUnitKilled(Unit unit) 
        {
            if (unit.StatusBarComp != null) 
            {
                unit.StatusBarComp.SetWillDisplay(false);
            }

            int index = InitiativeOrder.FindIndex(u => u.ObjectID == unit.ObjectID);
            if (index != -1) 
            {
                if (index <= UnitTakingTurn) 
                {
                    UnitTakingTurn--;
                }
            }

            InitiativeOrder.RemoveImmediate(unit);
            TurnDisplay.SetUnits(InitiativeOrder);

            EvaluateCombat();

            if (InitiativeOrder.All(unit => unit.AI.Team.GetRelation(UnitTeam.PlayerUnits) == Relation.Friendly || !unit.AI.Fighting))
            {
                EndCombat();
            }
            else if (InitiativeOrder.All(unit => unit.AI.Team.GetRelation(UnitTeam.PlayerUnits) != Relation.Friendly))
            {
                EndCombat();
            }
        }

        public override void OnUnitClicked(Unit unit, MouseButton button)
        {
            base.OnUnitClicked(unit, button);

            if (button == MouseButton.Left)
            {
                if (_selectedAbility == null)
                {
                    if (unit.Selectable && unit.Info.Visible(CurrentTeam))
                        SelectUnit(unit);
                }
                else
                {
                    _selectedAbility.OnUnitClicked(unit);
                }
            }
            else 
            {
                unit.OnRightClick();
            }
        }

        public virtual void OnAbilityCast(Ability ability) 
        {
            _onAbilityCastActions.ForEach(a => a?.Invoke(ability));

            Footer.UpdateFooterInfo(Footer._currentUnit);
        }

        public List<Action<Ability>> _onSelectAbilityActions = new List<Action<Ability>>();
        public List<Action> _onDeselectAbilityActions = new List<Action>();
        public List<Action<Ability>> _onAbilityCastActions = new List<Action<Ability>>();

        #endregion

        public void OpenContextMenu(Tooltip menu) 
        {
            Task.Run(() =>
            {
                Thread.Sleep(10);
                UIHelpers.CreateContextMenu(this, menu, _tooltipBlock);

                Message msg = new Message(MessageType.Request, MessageBody.InterceptKeyStrokes, MessageTarget.All);
                MessageCenter.SendMessage(msg);
            });
        }

        public void CloseContextMenu() 
        {
            _closeContextMenu?.Invoke();

            Message msg = new Message(MessageType.Request, MessageBody.EndKeyStrokeInterception, MessageTarget.All);
            MessageCenter.SendMessage(msg);
        }

        public void TickTemporaryVision(TemporaryVision t) 
        {
            t.TickDuration();

            if (t.Duration <= 0) 
            {
                TemporaryVision.Remove(t);
                t.TilesToReveal.Clear();
            }
        }

        /// <summary>
        /// Add or remove any new TemporaryVision objects and apply those changes to the vision.
        /// </summary>
        public void UpdateTemporaryVision() 
        {
            if (TemporaryVision.HasQueuedItems()) 
            {
                TemporaryVision.HandleQueuedItems();
                FillInTeamFog();
            }
        }

        public override void UpdateLightObstructionMap()
        {
            base.UpdateLightObstructionMap();

            //BaseTile centerTile = _tileMapController.GetCenterTile();

            //LightObject.SetPosition(centerTile.Position + new Vector3(0, 220, 0f));
            LightObject.SetPosition(new Vector3(9168.026f, 220 + 10480.5537f, 0f));
        }
    }
}
