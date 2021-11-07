using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Gui;
using Fusee.Math.Core;
using Fusee.Xene;
using System;
using System.Collections.Generic;
using System.Linq;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;

namespace Fusee.Examples.Starkiller.Core
{
    [FuseeApplication(Name = "Starkiller", Description = "Yet another FUSEE App.")]
    public class Starkiller : RenderCanvas
    {
        private SceneRendererForward _sceneRenderer;

        private SceneNode _meteors;
        private SceneNode _projectiles;
        private SceneNode _schiff;

        private readonly float MeteorSpeedFactor = 2;

        private bool[] abgefeuert;

        private SceneContainer _scene;

        float Leben = 0;
        bool gamestart = false;

        private SceneRendererForward _guiRenderer;
        private SceneContainer _gui;

        private SceneContainer CreateScene()
        {
            SceneContainer sc = new();
            SceneContainer _starkillerScene = AssetStorage.Get<SceneContainer>("StarkillerAssets.fus");

            if (_starkillerScene != null)
            {
                _meteors = AddHierarchy(_starkillerScene, "Meteorit", "Meteors");
                sc.Children.Add(_meteors);

                _projectiles = AddHierarchy(_starkillerScene, "AP", "Projectiles");
                sc.Children.Add(_projectiles);

                abgefeuert = new bool[_projectiles.Children.Count];

                _schiff = _starkillerScene.Children.FindNodes(n => n.Name == "Schiff").First();
                sc.Children.Add(_schiff);
            }

            return sc;
        }


        private SceneNode AddHierarchy(SceneContainer searchTarget, string searchName, string hierarchyName)
        {
            List<SceneNode> projectiles = searchTarget.Children.FindNodes(n => n.Name.Contains(searchName)).ToList();

            var sn = new SceneNode() { Name = hierarchyName };

            foreach (var p in projectiles)
            {
                sn.Children.Add(p);
            }

            return sn;
        }


        public override void Init()
        {
            //Set the clear color for the backbuffer to white (100% intensity in all color channels R, G, B, A).
            RC.ClearColor = new float4(0, 0, 0, 0);

            _scene = CreateScene();
            _gui = FuseeGuiHelper.CreateDefaultGui(this, CanvasRenderMode.Screen, "Starkiller");
            _sceneRenderer = new SceneRendererForward(_scene);
            _guiRenderer = new SceneRendererForward(_gui);
        }

        //RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            //Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            RC.Viewport(0, 0, Width, Height);

            var perspective = float4x4.CreatePerspectiveFieldOfView(M.PiOver4, (float)Width / Height, 0.01f, 1000f);
            RC.Projection = perspective;
            RC.View = float4x4.CreateTranslation(0, -20, 50) * float4x4.CreateRotationX(-5 * M.Pi / 180);

            var schiffTranslation = _schiff.GetTransform().Translation;

            if (Keyboard.IsKeyDown(KeyCodes.Enter))
            {
                if (!gamestart)
                    gamestart = true;
                Leben = 3;
            }
            if (Leben > 0 && gamestart)
            {
                ////Bewegung des Schiffs
                float bewegungHorizontal = schiffTranslation.x;
                bewegungHorizontal += 0.7f * Keyboard.ADAxis;
                float bewegungVertikal = schiffTranslation.y;
                bewegungVertikal += 0.7f * Keyboard.WSAxis;

                schiffTranslation.x = bewegungHorizontal;
                schiffTranslation.y = bewegungVertikal;
                schiffTranslation.z = 15;

                //Bewegung der Meteoriten
                foreach (var m in _meteors.Children)
                {
                    var mTransform = m.GetTransform();
                    var mTranslation = mTransform.Translation;
                    var mRotation = mTransform.Rotation;

                    if (mTranslation.z < -200)
                    {
                        mTranslation.z = 2000;
                    }

                    mTranslation.z -= (430 - (m.GetMesh().BoundingBox.Size.Length * MeteorSpeedFactor)) * DeltaTime;
                    mRotation.y += 1 * DeltaTime;
                    mRotation.z += 1 * DeltaTime;

                    mTransform.Translation = mTranslation;
                    mTransform.Rotation = mRotation;
                }

                //abfeuern des einzelnen Projektils + Platzierung vor dem Raumschiff
                if (Keyboard.IsKeyDown(KeyCodes.Space))
                {
                    for (var i = 0; i < _projectiles.Children.Count; i++)
                    {
                        if (!abgefeuert[i])
                        {
                            abgefeuert[i] = true;

                            var projectileTranslation = _projectiles.Children[i].GetTransform().Translation;
                            projectileTranslation.x = schiffTranslation.x;
                            projectileTranslation.y = schiffTranslation.y;
                            projectileTranslation.z = schiffTranslation.z + 5;
                            _projectiles.Children[i].GetTransform().Translation = projectileTranslation;

                            break;
                        }
                    }
                }

                // Bewegung des Projektils
                for (var i = 0; i < _projectiles.Children.Count; i++)
                {
                    if (abgefeuert[i])
                    {
                        var projectileTranslation = _projectiles.Children[i].GetTransform().Translation;

                        projectileTranslation.z += DeltaTime * 300;

                        if (projectileTranslation.z > 500)
                        {
                            projectileTranslation.z = -50;
                            abgefeuert[i] = false;
                        }

                        _projectiles.Children[i].GetTransform().Translation = projectileTranslation;
                    }

                }

                //Kollisions abfrage
                for (var i = 0; i < _projectiles.Children.Count; i++)
                {
                    if (abgefeuert[i])
                    {
                        var projectileTranslation = _projectiles.Children[i].GetTransform().Translation;

                        var centerX = _projectiles.Children[i].GetMesh().BoundingBox.Center.x + projectileTranslation.x;
                        var centerY = _projectiles.Children[i].GetMesh().BoundingBox.Center.y + projectileTranslation.y;
                        var centerZ = _projectiles.Children[i].GetMesh().BoundingBox.Center.z + projectileTranslation.z;

                        for (var j = 0; j < _meteors.Children.Count; j++)
                        {
                            var meteorTranslation = _meteors.Children[j].GetTransform().Translation;

                            var minX = _meteors.Children[j].GetMesh().BoundingBox.min.x + meteorTranslation.x;
                            var maxX = _meteors.Children[j].GetMesh().BoundingBox.max.x + meteorTranslation.x;
                            var minY = _meteors.Children[j].GetMesh().BoundingBox.min.y + meteorTranslation.y;
                            var maxY = _meteors.Children[j].GetMesh().BoundingBox.max.y + meteorTranslation.y;
                            var minZ = _meteors.Children[j].GetMesh().BoundingBox.min.z + meteorTranslation.z;
                            var maxZ = _meteors.Children[j].GetMesh().BoundingBox.max.z + meteorTranslation.z;

                            if (minX <= centerX && centerX <= maxX && minY <= centerY && centerY <= maxY && minZ <= centerZ && centerZ <= maxZ)
                            {
                                abgefeuert[i] = false;
                                projectileTranslation.z = -50;
                                meteorTranslation.z = -100;
                            }

                            _meteors.Children[j].GetTransform().Translation = meteorTranslation;
                        }

                        _projectiles.Children[i].GetTransform().Translation = projectileTranslation;
                    }

                    //Schiff Kollision 
                    for (var k = 0; k < _meteors.Children.Count; k++)
                    {
                        var SchiffAABBf = _schiff.GetTransform().Matrix * _schiff.GetMesh().BoundingBox;

                        var MeteorsAABBf = _meteors.Children[k].GetTransform().Matrix * _meteors.Children[k].GetMesh().BoundingBox;

                        if (MeteorsAABBf.Intersects(SchiffAABBf))
                        {
                            schiffTranslation.x = 0;
                            schiffTranslation.y = 0;
                            schiffTranslation.z = -100;
                            Leben -= 1;
                            if (Leben == 0)
                            {
                                gamestart = false;
                            }

                        }

                    }

                }
            }

            _schiff.GetTransform().Translation = schiffTranslation;

            //Tick any animations and Render the scene loaded in Init()
            _sceneRenderer.Render(RC);

            var orthographic = float4x4.CreateOrthographic(Width, Height, 0.01f, 1000);
            RC.Projection = orthographic;
            _guiRenderer.Render(RC);

            //Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }

        public void SetProjectionAndViewport()
        {
            //Set the rendering area to the entire window size

            RC.Viewport(0, 0, Width, Height);

            //Create a new projection matrix generating undistorted images on the new aspect ratio.
            var aspectRatio = Width / (float)Height;

            //0.25*PI Rad -> 45 Deg opening angle along the vertical direction. Horizontal opening angle is calculated based on the aspect ratio
            //Front clipping happens at 1 (Objects nearer than 1 world unit get clipped)
            //Back clipping happens at 2000 (Anything further away from the camera than 2000 world units gets clipped, polygons will be cut)
            var projection = float4x4.CreatePerspectiveFieldOfView(0, aspectRatio, 1, 20000);

            RC.Projection = projection;

        }
    }
}