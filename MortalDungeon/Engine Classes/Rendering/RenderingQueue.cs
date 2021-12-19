﻿using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MortalDungeon.Engine_Classes.Rendering
{
    internal enum RenderingStates 
    {
        GuassianBlur
    }
    internal static class RenderingQueue
    {
        private static readonly List<Letter> _LettersToRender = new List<Letter>();
        private static readonly List<GameObject> _UIToRender = new List<GameObject>();
        private static readonly List<List<GameObject>> _ObjectsToRender = new List<List<GameObject>>();

        private static readonly List<List<Unit>> _UnitsToRender = new List<List<Unit>>();
        private static readonly List<List<Structure>> _StructuresToRender = new List<List<Structure>>();


        private static readonly List<List<BaseTile>> _TilesToRender = new List<List<BaseTile>>();
        private static readonly List<ParticleGenerator> _ParticleGeneratorsToRender = new List<ParticleGenerator>();
        private static readonly List<GameObject> _TileQuadsToRender = new List<GameObject>();

        private static readonly List<List<GameObject>> _LowPriorityQueue = new List<List<GameObject>>();

        private static readonly List<GameObject> _LightQueue = new List<GameObject>();

        internal static ContextManager<RenderingStates> RenderStateManager = new ContextManager<RenderingStates>();

        internal static Action RenderSkybox = null;

        /// <summary>
        /// Render all queued objects
        /// </summary>
        internal static void RenderQueue()
        {
            RenderSkybox?.Invoke();


            if (RenderStateManager.GetFlag(RenderingStates.GuassianBlur)) 
            {
                Renderer.DrawToFrameBuffer(Renderer.MainFBO); //Framebuffer should only be used when we want to do post processing
                Renderer.MainFBO.ClearBuffers();
            }

            GL.Enable(EnableCap.FramebufferSrgb);

            //RenderFrameBuffer(MainFBO);

            //MainFBO.UnbindFrameBuffer();
            //MainFBO.ClearColorBuffer(false);

            //DrawToFrameBuffer(MainFBO); 

            RenderQueuedLetters();

            RenderTileQueue();

            RenderTileQuadQueue();

            RenderQueuedParticles();

            //RenderQueuedStructures();
            RenderQueuedObjects();
            RenderQueuedUnits();

            RenderLowPriorityQueue();

            //RenderSkybox?.Invoke();

            GL.Clear(ClearBufferMask.DepthBufferBit);
            //RenderLightQueue();

            if (RenderStateManager.GetFlag(RenderingStates.GuassianBlur))
            {
                Renderer.RenderFrameBuffer(Renderer.MainFBO);
                Renderer.MainFBO.UnbindFrameBuffer();
            }

            GL.Disable(EnableCap.FramebufferSrgb);


            GL.Clear(ClearBufferMask.DepthBufferBit);
            //GL.Disable(EnableCap.DepthTest);
            //GL.Disable(EnableCap.Blend);
            //GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
            //GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);

            RenderQueuedUI();

            //GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.Blend);
            //GL.BlendEquation(BlendEquationMode.FuncAdd);
            //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            //RenderFrameBuffer(MainFBO);
        }


        #region Particle queue
        internal static void QueueParticlesForRender(ParticleGenerator generator)
        {
            _ParticleGeneratorsToRender.Add(generator);
        }
        internal static void RenderQueuedParticles()
        {
            for (int i = 0; i < _ParticleGeneratorsToRender.Count; i++)
            {
                Renderer.RenderParticlesInstanced(_ParticleGeneratorsToRender[i]);
            }

            _ParticleGeneratorsToRender.Clear();
        }
        #endregion

        #region Text queue
        internal static void QueueLettersForRender(List<Letter> letters)
        {
            for (int i = 0; i < letters.Count; i++)
            {
                _LettersToRender.Add(letters[i]);
            }
        }
        internal static void QueueTextForRender(List<Text> text)
        {
            for (int i = 0;i < text.Count; i++)
            {
                if (text[i].Render)
                    QueueLettersForRender(text[i].Letters);
            }
        }
        #endregion

        #region UI queue
        internal static void QueueUITextForRender(List<Text> text, bool scissorFlag = false)
        {
            for(int i = 0; i < text.Count; i++)
            {
                if (text[i].Render)
                    QueueUIForRender(text[i].Letters, scissorFlag);
            }
        }
        internal static void RenderQueuedLetters()
        {
            Renderer.RenderObjectsInstancedGeneric(_LettersToRender, ref Renderer._instancedRenderArray);
            _LettersToRender.Clear();
        }

        internal static void QueueNestedUI<T>(List<T> uiObjects, int depth = 0, ScissorData scissorData = null, Action<List<UIObject>> renderAfterParent = null, bool overrideRender = false) where T : UIObject
        {
            List<UIObject> renderAfterParentList = new List<UIObject>();

            try //This is a lazy solution for a random crash. If I can figure out why it's happening then I'll come back to this
            {
                if (uiObjects.Count > 0)
                {
                    for (int i = 0; i < uiObjects.Count; i++)
                    {
                        if (uiObjects[i].Render && !uiObjects[i].Cull)
                        {
                            if (uiObjects[i].RenderAfterParent && renderAfterParent != null && !overrideRender)
                            {
                                renderAfterParentList.Add(uiObjects[i]);
                                continue;
                            }

                            if (uiObjects[i].ScissorData.Scissor == true)
                            {
                                scissorData = uiObjects[i].ScissorData;
                                scissorData._startingDepth = depth;
                            }

                            bool scissorFlag = false;
                            if (scissorData != null && depth - scissorData._startingDepth <= scissorData.Depth && depth != scissorData._startingDepth)
                            {
                                scissorFlag = true;
                            }
                            else
                            {
                                scissorData = null;
                            }

                            QueueUITextForRender(uiObjects[i].TextObjects, scissorFlag || uiObjects[i].ScissorData.Scissor);

                            List<UIObject> objsToRenderAfterParent = new List<UIObject>();

                            if (uiObjects[i].Children.Count > 0)
                            {
                                QueueNestedUI(uiObjects[i].Children, depth + 1, uiObjects[i].ScissorData.Scissor ? uiObjects[i].ScissorData : scissorData, (list) => 
                                {
                                    objsToRenderAfterParent = list;
                                });
                            }

                            QueueUIForRender(uiObjects[i], scissorFlag || uiObjects[i].ScissorData.Scissor);

                            if (objsToRenderAfterParent.Count > 0) 
                            {
                                QueueNestedUI(objsToRenderAfterParent, depth + 1, uiObjects[i].ScissorData.Scissor ? uiObjects[i].ScissorData : scissorData, null, true);
                            }
                        }
                    }

                    //RenderableObject display = uiObjects[0].GetDisplay();

                    //RenderObjectsInstancedGeneric(uiObjects, display);
                    //QueueUIForRender(uiObjects);
                }

                if (renderAfterParentList.Count > 0 && renderAfterParent != null)
                {
                    renderAfterParent(renderAfterParentList);
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine("Exception in QueueNestedUI: " + e.Message);
            }
        }
        internal static void QueueUIForRender<T>(List<T> objList, bool scissorFlag = false) where T : GameObject
        {
            for (int i = 0; i < objList.Count; i++)
            {
                objList[i].ScissorData._scissorFlag = scissorFlag;

                _UIToRender.Add(objList[i]);
            }
        }
        internal static void QueueUIForRender<T>(T obj, bool scissorFlag = false) where T : GameObject
        {
            obj.ScissorData._scissorFlag = scissorFlag;

            _UIToRender.Add(obj);
        }
        internal static void RenderQueuedUI()
        {
            Renderer.RenderObjectsInstancedGeneric(_UIToRender, ref Renderer._instancedRenderArray, null, true, false);
            _UIToRender.Clear();
        }

        #endregion

        #region Object queue
        internal static void QueueObjectsForRender(List<GameObject> objList)
        {
            _ObjectsToRender.Add(objList);
        }
        internal static void RenderQueuedObjects()
        {
            for(int i = 0; i < _ObjectsToRender.Count; i++)
            {
                Renderer.RenderObjectsInstancedGeneric(_ObjectsToRender[i], ref Renderer._instancedRenderArray);
            }
            _ObjectsToRender.Clear();
        }
        #endregion

        #region Unit queue
        internal static void QueueUnitsForRender(List<Unit> objList)
        {
            _UnitsToRender.Add(objList);
        }

        internal static void RenderQueuedUnits()
        {
            for(int i = 0; i < _UnitsToRender.Count; i ++)
            {
                Renderer.RenderObjectsInstancedGeneric(_UnitsToRender[i], ref Renderer._instancedRenderArray);
            }

            _UnitsToRender.Clear();
        }
        #endregion

        #region Tile queue
        internal static void QueueTileObjectsForRender(List<BaseTile> objList)
        {
            if (objList.Count == 0)
                return;

            _TilesToRender.Add(objList);
        }

        internal static void RenderTileQueue()
        {
            for(int i = 0; i < _TilesToRender.Count; i++)
            {
                Renderer.RenderObjectsInstancedGeneric(_TilesToRender[i], ref Renderer._instancedRenderArray);
            }

            _TilesToRender.Clear();
        }
        #endregion

        #region Tile quad queue
        internal static void QueueTileQuadForRender(GameObject obj)
        {
            if (obj == null)
                return;

            _TileQuadsToRender.Add(obj);
        }

        private static readonly List<GameObject> _tempGameObjList = new List<GameObject>();
        internal static void RenderTileQuadQueue()
        {
            for (int i = 0; i < _TileQuadsToRender.Count; i++)
            {
                _tempGameObjList.Add(_TileQuadsToRender[i]);
                Renderer.RenderObjectsInstancedGeneric(_tempGameObjList, ref Renderer._instancedRenderArray);
                _tempGameObjList.Clear();
            }

            _TileQuadsToRender.Clear();
        }
        #endregion

        #region Low priority object queue
        internal static void QueueLowPriorityObjectsForRender(List<GameObject> objList)
        {
            if (objList.Count == 0)
                return;

            _LowPriorityQueue.Add(objList);
        }
        internal static void RenderLowPriorityQueue()
        {
            for (int i = 0; i < _LowPriorityQueue.Count; i++)
            {
                Renderer.RenderObjectsInstancedGeneric(_LowPriorityQueue[i], ref Renderer._instancedRenderArray);
            }

            _LowPriorityQueue.Clear();
        }
        #endregion
    }
}
