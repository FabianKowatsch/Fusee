
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Effects;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Core.ShaderShards;
using Fusee.Engine.Core.ShaderShards.Fragment;
using Fusee.Engine.Core.ShaderShards.Vertex;
using Fusee.PointCloud.Common;
using Fusee.Engine.GUI;
using Fusee.Math.Core;
using System.Collections.Generic;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;


namespace Fusee.Examples.MuVista.Core
{
    
    public class PanoSphere
    {
        //MuVista
        private static float _angleHorz = M.Pi, _angleVert = 0, _angleVelHorz, _angleVelVert, _zoom;

        private const float RotationSpeed = 2;
        private const float CamTranslationSpeed = -3;
        private const float Damping = 0.8f;

        private SceneRendererForward _sceneRenderer;

        private const float ZNear = 1f;
        private const float ZFar = 1000;
        private const float DistancePlaneCamera = 5;


        private SceneRendererForward _guiRenderer;
        private GUI _gui;
        private SceneInteractionHandler _sih;
        private readonly CanvasRenderMode _canvasRenderMode = CanvasRenderMode.Screen;
        private SceneContainer _animScene;
        private Transform _sphereTransform;
        private Transform _planeTransform;

        private Transform _mainCamTransform;
        private Transform _minimapCamTransform;
        private readonly Camera _mainCam = new Camera(ProjectionMethod.Perspective, 3, 100, M.PiOver4);
        private readonly Camera _minimapCam = new Camera(ProjectionMethod.Perspective, 3, 100, M.PiOver4);
        private readonly Camera _guiCam = new Camera(ProjectionMethod.Orthographic, 1, 1000, M.PiOver4);

        private readonly float4 _minimapViewport = new float4(77, 73, 40, 40);
        private readonly float4 _mainCamViewport = new float4(0, 0, 100, 100);
        private readonly int _minimapLayer = 5;
        private readonly int _mainCamLayer = -1;

        private const float _planeHeight = 4096f / 300f;
        private const float _planeWidth = 8192f / 300f;

        private VertexAnimationSurfaceEffect _animationEffect;

        private bool _sphereIsVisible = true;
        private bool _inverseCams = false;
        private const float AnimDuration = 2f;
        private bool _animActive;
        private float _animTimeStart = 0;


        private Texture sphereTex2;
        private TextureInputOpacity colorInput2;


        //Inactivity Checker
        private float _inActiveTimer = 0f;


        // Init is called on startup.
        public SceneNode initSphereNodes()
        {

            var sphereTex = new Texture(AssetStorage.Get<ImageData>("ladybug_18534664_20210113_GEO2111300_5561.jpg"));

            sphereTex2 = new Texture(AssetStorage.Get<ImageData>("LadyBug_C1P1.jpg"));

            Sphere sphere = new Sphere(10, 20, 50);
            GridPlane plane = new GridPlane(20, 50, _planeHeight, _planeWidth, DistancePlaneCamera);

            TextureInputOpacity colorInput = new TextureInputOpacity()
            {
                Albedo = float4.One,
                TexOpacity = 0.5f,
                //Emission = float4.Zero,
                //Shininess = 1.0f,
                //SpecularStrength = 0.0f,
                AlbedoMix = 1.0f,
                AlbedoTex = sphereTex,
                TexTiles = float2.One,
                Roughness = 0.0f
            };
            colorInput2 = new TextureInputOpacity()
            {
                Albedo = float4.One,
                TexOpacity = 0.0f,
                AlbedoMix = 1.0f,
                AlbedoTex = sphereTex2,
                TexTiles = float2.One,
                Roughness = 0.0f
            };
            var lightingSetup = LightingSetupFlags.Unlit | LightingSetupFlags.AlbedoTex | LightingSetupFlags.AlbedoTexOpacity;

            _animationEffect = new VertexAnimationSurfaceEffect(lightingSetup, colorInput, FragShards.SurfOutBody_Textures(lightingSetup), VertShards.SufOutBody_PosAnimation)
            {
                PercentPerVertex = 1.0f,
                PercentPerVertex1 = 0.0f
            };
            /*
                        _minimapCam.Layer = _minimapLayer;
                        _minimapCam.BackgroundColor = new float4(0.5f, 0.5f, 0.5f, 1);
                        _minimapCam.Viewport = _minimapViewport;

                        _minimapCamTransform = new Transform()
                        {
                            Rotation = new float3(M.PiOver6, 0, 0),//float3.Zero,
                            Translation = new float3(10, 40, -60),
                            Scale = float3.One
                        };

                        _mainCamTransform = new Transform()
                        {
                            Rotation = new float3(_angleVert, _angleHorz, 0),
                            Translation = new float3(0, 0, 0),
                            Scale = new float3(1, 1, 1)
                        };*/

            _sphereTransform = new Transform()
            {
                Rotation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0, 0, 0)
            };

            _planeTransform = new Transform()
            {
                Rotation = new float3(0, M.Pi, 0),
                Scale = new float3(1, 1, 1),
                Translation = new float3(0, 0, 0)
            };


            Mesh sphereAndPlane = new Mesh()
            {
                Vertices = sphere.Vertices,
                Vertices1 = plane.Vertices1,
                Normals = sphere.Normals,
                Normals1 = plane.Normals1,
                UVs = sphere.UVs,
                Triangles = sphere.Triangles
            };

            //_gui = new GUI(Width, Height, _canvasRenderMode, _mainCamTransform, _guiCam);

            // Create the interaction handler

            // Set the clear color for the backbuffer to white (100% intensity in all color channels R, G, B, A).
            //RC.ClearColor = new float4(1, 1, 1, 1);

            //Scene with Main Camera and Mesh

            /*  
            _animScene = new SceneContainer
            {
                Children = new List<SceneNode>
                {
                   new SceneNode
                     {
                        Name = "MainCam",
                        Components = new List<SceneComponent>()
                        {
                            _mainCamTransform,
                            _mainCam
                        }
                     },*/
            var node = new SceneNode()
            { 
                Components = new List<SceneComponent>()
                        {
                             new RenderLayer()
                                {
                                Layer = RenderLayers.Layer02
                                },
                            _sphereTransform,
                            _animationEffect,
                            //CreateSphereAndPlane(10, 20, 50, _planeHeight * 2, _planeWidth * 2, DistancePlaneCamera)
                            sphereAndPlane,
                        }
            };
/*                    new SceneNode
                     {
                        Name = "MiniMapCam",
                        Components = new List<SceneComponent>()
                        {
                            _minimapCamTransform,
                            _minimapCam
                        }
                     
                }
            };
}*/
            /*-----------------------------------------------------------------------
             * Debuggingtools
             -----------------------------------------------------------------------*/

            //RC.SetRenderState(RenderState.CullMode, (uint)Cull.None);
            //RC.SetRenderState(RenderState.FillMode, (uint)FillMode.Wireframe);

            return node;
        }

        // RenderAFrame is called once a frame
        public void UpdateSphere()
        {
            // Clear the backbuffer

            //RC.Viewport(0, 0, Width, Height);

            #region Controls

            MouseWheelZoom();

            HndGuiButtonInput();

            CalculateRotationAngle();

            UpdateCameraTransform();

            if (Mouse.LeftButton || (Keyboard.LeftRightAxis != 0 || Keyboard.UpDownAxis != 0))
            {
                _inActiveTimer = 0f;
            }

            if (Keyboard.IsKeyDown(KeyCodes.Space))
            {
                SwitchBetweenViews();
            }

            if (Keyboard.IsKeyDown(KeyCodes.F5))
            {
                SwitchCamViewport();
            }

            if (Keyboard.IsKeyDown(KeyCodes.W) || Keyboard.IsKeyDown(KeyCodes.S))
            {
                Diagnostics.Debug("surface: " + _animationEffect.SurfaceInput.GetHashCode());
                Diagnostics.Debug("scene: " + _animScene);
                _animationEffect.SurfaceInput = colorInput2;
            }

            #endregion

            #region Animation
            if (_animActive)
            {
                float percentValue = (Time.TimeSinceStart - _animTimeStart) / AnimDuration;

                if (percentValue <= 1 && percentValue >= 0)
                {
                    if (_sphereIsVisible)
                    {
                        _animationEffect.PercentPerVertex = percentValue;
                        _animationEffect.PercentPerVertex1 = 1 - percentValue;
                    }
                    else
                    {
                        _animationEffect.PercentPerVertex = 1 - percentValue;
                        _animationEffect.PercentPerVertex1 = percentValue;
                    }
                }
                else
                {
                    if (_sphereIsVisible)
                    {
                        _animationEffect.PercentPerVertex = 1;
                        _animationEffect.PercentPerVertex1 = 0;
                    }
                    else
                    {
                        _animationEffect.PercentPerVertex = 0;
                        _animationEffect.PercentPerVertex1 = 1;
                    }
                    _animActive = false;
                }
                
            }
            #endregion

            //var perspective = float4x4.CreatePerspectiveFieldOfView(_fovy, (float)Width / Height, ZNear, ZFar);
            //  var orthographic = float4x4.CreateOrthographic(Width, Height, ZNear, ZFar);

            _mainCam.ProjectionMethod = ProjectionMethod.Perspective;

            // Render the scene loaded in Init()
            //RC.View = view;
            //RC.Projection = perspective; //_sceneRenderer.Animate();

        }

        public void MouseWheelZoom()
        {
            _zoom = Mouse.WheelVel * DeltaTime * -0.05f;

            if (_sphereIsVisible)
            {
                if (!(_mainCam.Fov + _zoom >= 1.2) && !(_mainCam.Fov + _zoom <= 0.3))
                {
                    _mainCam.Fov += _zoom;
                }
            }
            else
            {
                if (_zoom != 0)
                {
                    if (!(_mainCam.Fov + _zoom >= M.PiOver2) && !(_mainCam.Fov + _zoom <= 0.3))
                    {
                        _mainCam.Fov += _zoom;
                    }
                }
            }
        }

        public void UpdateCameraTransform()
        {
            if (_sphereIsVisible)
            {
                if (_inActiveTimer < 12f)
                {
                    _inActiveTimer += Time.DeltaTime;

                    _mainCamTransform.Rotation = new float3(_angleVert, _angleHorz, 0);
                }
                if (_inActiveTimer > 12f)
                {
                    RotationAfterInactivity();
                }
            }
            else
            {
                if (_inActiveTimer < 12f)
                {
                    _inActiveTimer += Time.DeltaTime;

                    _mainCamTransform.Translation = new float3(_angleHorz * CamTranslationSpeed, _angleVert * CamTranslationSpeed, 0);
                }
            }
        }

        public void CalculateRotationAngle()
        {
            if (Mouse.LeftButton)
            {
                _angleVelHorz = -RotationSpeed * Mouse.XVel * DeltaTime * 0.0005f;
                _angleVelVert = -RotationSpeed * Mouse.YVel * DeltaTime * 0.0005f;
            }
            else if (Touch.GetTouchActive(TouchPoints.Touchpoint_0))
            {
                var touchVel = Touch.GetVelocity(TouchPoints.Touchpoint_0);
                _angleVelHorz = -RotationSpeed * touchVel.x * DeltaTime * 0.0005f;
                _angleVelVert = -RotationSpeed * touchVel.y * DeltaTime * 0.0005f;
            }
            else
            {
                if (Keyboard.LeftRightAxis != 0 || Keyboard.UpDownAxis != 0)
                {
                    _angleVelHorz = RotationSpeed * Keyboard.LeftRightAxis * DeltaTime;

                    if (_angleVert < 0)
                    {
                        _angleVelVert = -(RotationSpeed / (((_angleVert * -1) + 1) * (1.5f))) * Keyboard.UpDownAxis * DeltaTime;
                    }
                    else
                    {
                        _angleVelVert = -(RotationSpeed / ((_angleVert + 1) * 1.5f)) * Keyboard.UpDownAxis * DeltaTime;
                    }
                }
                else
                {
                    var curDamp = (float)System.Math.Exp(-Damping * DeltaTime);
                    _angleVelHorz *= curDamp;
                    _angleVelVert *= curDamp;
                }
            }
            //Calculations to make sure the Camera has max vertical angle in Sphere View and max hor and vert translation in Plane View
            if (!(_angleVert + _angleVelVert >= 1.5) && !(_angleVert + _angleVelVert <= -1.5))
            {
                _angleVert += _angleVelVert;
            }
            if (_sphereIsVisible)
            {
                _angleHorz += _angleVelHorz;
            }
            else
            {
                if ((_angleHorz + _angleVelHorz) * CamTranslationSpeed <= _planeWidth / 2f && (_angleHorz + _angleVelHorz) * CamTranslationSpeed >= _planeWidth / -2f)
                {
                    _angleHorz += _angleVelHorz;
                }
            }
        }
        public void SwitchBetweenViews()
        {
            _inActiveTimer = 0f;
            _mainCam.Fov = M.PiOver4;

            //Animationsettings
            _animTimeStart = TimeSinceStart;
            _animActive = true;
            _mainCamTransform.Translation = new float3(0, 0, 0);
            _angleVelHorz = 0;
            _angleVert = 0;
            _angleVelVert = 0;
            _mainCamTransform.Rotation = new float3(0, M.Pi, 0);
            _sphereIsVisible = !_sphereIsVisible;

            if (_sphereIsVisible)
                _angleHorz = M.Pi;         
            else       
                _angleHorz = 0;  
        }

        public void HndGuiButtonInput()
        {
            if (_gui._btnZoomOut.IsMouseOver)
            {
                _gui._btnZoomOut.OnMouseDown += BtnZoomOutDown;
            }

            if (_gui._btnZoomIn.IsMouseOver)
            {
                _gui._btnZoomIn.OnMouseDown += BtnZoomInDown;
            }

            if(_gui._btnMiniMap.IsMouseOver)
            {
                _gui._btnMiniMap.OnMouseDown += OnMinimapDown;
            }
        }

        public void RotationAfterInactivity()
        {
            _angleHorz += 0.5f * Time.DeltaTime;
            _mainCamTransform.Rotation = new float3(_angleVert, _angleHorz, 0);
        }


        public void BtnZoomInDown(CodeComponent sender)
        {
            if (!(_mainCam.Fov - 0.001 <= 0.3))
            {
                _mainCam.Fov -= 0.001f;
            }
        }


        public void MoveBetweenPictures()
        {

        }

        public void BtnZoomOutDown(CodeComponent sender)
        {
            if (_sphereIsVisible)
            {
                if (_mainCam.Fov + 0.001 <= 1.2f)
                {
                    _mainCam.Fov += 0.001f;
                }
            }
            else
            {
                if (!(_mainCam.Fov + 0.001f >= M.PiOver3) && !(_mainCam.Fov + 0.001f <= 0.3))
                {
                    _mainCam.Fov += 0.001f;
                }
            }
        }
        
        public void OnMinimapDown(CodeComponent sender)
        {
            SwitchCamViewport();
        }
        public void SwitchCamViewport()
        {
            _inverseCams = !_inverseCams;
            if (_inverseCams)
            {
                _mainCam.Viewport = _minimapViewport;
                _mainCam.Layer = _minimapLayer;
                _minimapCam.Layer = _mainCamLayer;
                _minimapCam.Viewport = _mainCamViewport;
            }
            else
            {
                _mainCam.Layer = _mainCamLayer;
                _mainCam.Viewport = _mainCamViewport;
                _minimapCam.Viewport = _minimapViewport;
                _minimapCam.Layer = _minimapLayer;
            }
        }
    }
}