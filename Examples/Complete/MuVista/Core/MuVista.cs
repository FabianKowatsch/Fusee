using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Effects;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Core.ShaderShards;
using Fusee.Engine.GUI;
using Fusee.Math.Core;
using Fusee.PointCloud.Common;
using Fusee.PointCloud.OoCReaderWriter;
using Fusee.PointCloud.PointAccessorCollections;
using Fusee.Xene;
using System;
using System.Collections.Generic;
using System.Linq;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;

namespace Fusee.Examples.MuVista.Core
{
    [FuseeApplication(Name = "FUSEE MuVista Viewer", Description = "Viewer for Pointclouds and 360 degree pictures.")]
    public class MuVista<TPoint> : RenderCanvas, IPcRendering where TPoint : new()
    {
        public AppSetupHelper.AppSetupDelegate AppSetup;

        public PtOctantLoader<TPoint> OocLoader { get; set; }

        public PtOctreeFileReader<TPoint> OocFileReader { get; set; }

        public bool UseWPF { get; set; }
        public bool DoShowOctants { get; set; }
        public bool IsSceneLoaded { get; private set; }
        public bool ReadyToLoadNewFile { get; private set; }
        public bool IsInitialized { get; private set; } = false;
        public bool IsAlive { get; private set; }


        private const float CamTranslationSpeed = -3;
        private const float RotationSpeed = 7;
        private const float Damping = 0.8f;

        private const float _planeHeight = 4096f / 300f;
        private const float _planeWidth = 8192f / 300f;
        // angle variables
        private static float _angleHorz, _angleVert, _angleVelHorz, _angleVelVert, _angleRoll, _angleRollInit, _zoom;
        private static float2 _offset;
        private static float2 _offsetInit;



        private SceneContainer _scene;
        private SceneRendererForward _sceneRenderer;

        private bool _keys;
        private bool _pointCloudActive = true;
        private bool _isSpaceUsed = false;
        private AxisDescription _spaceAxis;

        private const float ZNear = 1f;
        private const float ZFar = 1000;

        private readonly float Fov = M.PiOver3;

        private SceneRendererForward _guiRenderer;
        private GUI _gui;
        private SceneInteractionHandler _sih;
        private readonly CanvasRenderMode _canvasRenderMode = CanvasRenderMode.Screen;

        private float3 _initCamPos;
        public float3 InitCameraPos { get => _initCamPos; private set { _initCamPos = value; OocLoader.InitCamPos = _initCamPos; } }


        private float3 _camPosBeforeSwitching = float3.Zero;

        private bool _isTexInitialized = false;
        private bool _sphereIsVisible = true;
        private bool _isSpaceMouseMoving;

        private Texture _octreeTex;
        private double3 _octreeRootCenter;
        private double _octreeRootLength;

        private WritableTexture _depthTex;

        private Transform _mainCamTransform;
        private Camera _mainCam;

        private readonly Camera _minimapCam = new Camera(ProjectionMethod.Perspective, 3, 100, M.PiOver4);
        private Transform _minimapCamTransform;

        private readonly Camera _guiCam = new Camera(ProjectionMethod.Orthographic, 1, 1000, M.PiOver4);

        private SixDOFDevice _spaceMouse;

        private PanoSphere _panoSphere;

        private bool _inverseCams = false;

        private readonly float4 _minimapViewport = new float4(77, 73, 40, 40);
        private readonly float4 _mainCamViewport = new float4(0, 0, 100, 100);
        private readonly int _minimapLayer = 5;
        private readonly int _mainCamLayer = 1;

        private ScenePicker _scenePicker;
        private PickResult _currentPick;
        private float4 _oldColor;

        // Init is called on startup. 
        public override void Init()
        {

            _panoSphere = PanoSphereFactory.createPanoSpheres().ElementAt(0);
            _panoSphere.Name = "PanoSphere";
            _spaceMouse = GetDevice<SixDOFDevice>();

            _depthTex = WritableTexture.CreateDepthTex(Width, Height);

            IsAlive = true;
            AppSetup();

            _scene = new SceneContainer
            {
                Children = new List<SceneNode>()
            };

            _mainCamTransform = new Transform()
            {
                Name = "MainCamTransform",
                Scale = float3.One,
                Translation = InitCameraPos,
                Rotation = float3.Zero
            };

            _mainCam = new Camera(ProjectionMethod.Perspective, ZNear, ZFar, Fov, RenderLayers.Layer01)
            {
                BackgroundColor = float4.One
            };


            var mainCam = new SceneNode()
            {
                Name = "MainCam",
                Components = new List<SceneComponent>()
                {
                    _mainCamTransform,
                    _mainCam
                }
            };

            _scene.Children.Insert(0, mainCam);

            _minimapCam.Layer = _minimapLayer;
            _minimapCam.BackgroundColor = new float4(0, 0, 0, 1);
            _minimapCam.Viewport = _minimapViewport;
            _minimapCam.RenderLayer = RenderLayers.Layer02;

            _minimapCamTransform = new Transform()
            {
                Rotation = new float3(M.PiOver2, 0, 0),
                Translation = new float3(0, 100, 0),
                Scale = float3.One
            };


            var miniMapCam = new SceneNode()
            {
                Name = "MiniMapCam",
                Components = new List<SceneComponent>()
                {
                    _minimapCamTransform,
                    _minimapCam
                }
            };

            _guiCam.ClearColor = false;
            _guiCam.ClearDepth = false;
            _guiCam.FrustumCullingOn = false;
            _guiCam.Layer = 99;

            _angleRoll = 0;
            _angleRollInit = 0;
            _offset = float2.Zero;
            _offsetInit = float2.Zero;

            // Set the clear color for the back buffer to white (100% intensity in all color channels R, G, B, A).            
            if (!UseWPF)
                LoadPointCloudFromFile();

            _scene.Children.Add(CreateWaypoint(new float3(40, 40, 0)));
            _scene.Children.Add(CreateWaypoint(new float3(50, 40, 0)));


            _scene.Children.Add(_panoSphere);

            //_scene.Children.Add(miniMapCam);

            _gui = new GUI(Width, Height, _canvasRenderMode, _mainCamTransform, _guiCam);
            //Create the interaction handler
            _sih = new SceneInteractionHandler(_gui);

            // Wrap a SceneRenderer around the model.
            _sceneRenderer = new SceneRendererForward(_scene);
            _guiRenderer = new SceneRendererForward(_gui);

            _scenePicker = new ScenePicker(_scene);
            /*-----------------------------------------------------------------------
            * Debuggingtools
            -----------------------------------------------------------------------*/

            //RC.SetRenderState(RenderState.CullMode, (uint)Cull.None);
            //RC.SetRenderState(RenderState.FillMode, (uint)FillMode.Wireframe);


            _spaceAxis = Keyboard.RegisterSingleButtonAxis(32);


            IsInitialized = true;

        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            ReadyToLoadNewFile = false;

            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            if (IsSceneLoaded)
            {


                // ------------ Enable to update the Scene only when the user isn't moving ------------------
                /*if (Keyboard.WSAxis != 0 || Keyboard.ADAxis != 0 || (Touch.GetTouchActive(TouchPoints.Touchpoint_0) && !Touch.TwoPoint) || isSpaceMouseMoving)
                    OocLoader.IsUserMoving = true;
                else
                    OocLoader.IsUserMoving = false;*/
                //--------------------------------------------------------------------------------------------
                #region Controls
                HndGuiButtonInput();
                MouseWheelZoom();

                if (Keyboard.IsKeyDown(KeyCodes.Enter))
                {
                    switchModes();
                }

                if (Keyboard.IsKeyDown(KeyCodes.F5))
                {
                    SwitchCamViewport();
                }

                if (_inverseCams)
                {
                    CheckWaypointPicking();
                }

                if (_pointCloudActive)
                {
                    pcUserInput();

                }
                else
                {
                    sphereUserInput();
                }


                #endregion

                //----------------------------  

                if (PtRenderingParams.CalcSSAO || PtRenderingParams.Lighting != Lighting.Unlit)
                {
                    //Render Depth-only pass
                    _scene.Children[1].RemoveComponent<ShaderEffect>();
                    _scene.Children[1].Components.Insert(1, PtRenderingParams.DepthPassEf);

                    _mainCam.RenderTexture = _depthTex;
                    _sceneRenderer.Render(RC);
                    _mainCam.RenderTexture = null;
                }

                //Render color pass
                //Change shader effect in complete scene
                _scene.Children[1].RemoveComponent<ShaderEffect>();
                _scene.Children[1].Components.Insert(1, PtRenderingParams.ColorPassEf);
                _sceneRenderer.Render(RC);

                //UpdateScene after Render / Traverse because there we calculate the view matrix (when using a camera) we need for the update.
                OocLoader.RC = RC;
                OocLoader.UpdateScene(PtRenderingParams.PtMode, PtRenderingParams.DepthPassEf, PtRenderingParams.ColorPassEf);

                if (UseWPF)
                {
                    if (PtRenderingParams.ShaderParamsToUpdate.Count != 0)
                    {
                        UpdateShaderParams();
                        PtRenderingParams.ShaderParamsToUpdate.Clear();
                    }
                }

                if (DoShowOctants)
                    OocLoader.ShowOctants(_scene);
            }

            //Render GUI
            RC.Projection = float4x4.CreateOrthographic(Width, Height, ZNear, ZFar);

            // Constantly check for interactive objects.
            _sih.CheckForInteractiveObjects(RC, Mouse.Position, Width, Height);

            _guiRenderer.Render(RC);

            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();

            ReadyToLoadNewFile = true;


        }
        private bool SpaceMouseMoving(out float3 velPos, out float3 velRot)
        {
            if (_spaceMouse != null && _spaceMouse.IsConnected)
            {
                bool spaceMouseMovement = false;
                velPos = 0.001f * _spaceMouse.Translation;
                if (velPos.LengthSquared < 0.01f)
                    velPos = float3.Zero;
                else
                    spaceMouseMovement = true;
                velRot = 0.0001f * _spaceMouse.Rotation;
                velRot.z = 0;
                if (velRot.LengthSquared < 0.000005f)
                    velRot = float3.Zero;
                else
                    spaceMouseMovement = true;

                return spaceMouseMovement;
            }
            velPos = float3.Zero;
            velRot = float3.Zero;
            return false;
        }
        private void pcUserInput()
        {
            _isSpaceMouseMoving = SpaceMouseMoving(out float3 velPos, out float3 velRot);
            Diagnostics.Debug(Keyboard.GetAxis(_spaceAxis.Id));
            Diagnostics.Debug(Keyboard.GetAxisRaw(_spaceAxis.Id));

            if (Keyboard.GetAxis(_spaceAxis.Id) > 0f && _isSpaceUsed == false)
            {
                _isSpaceUsed = true;
            }

            if (_isSpaceUsed)
            {
                _mainCamTransform.Translate(new float3(0, -0.2f * (Keyboard.GetAxis(_spaceAxis.Id) - 1f), 0));
            }



            // Mouse and keyboard movement
            if (Keyboard.LeftRightAxis != 0 || Keyboard.UpDownAxis != 0)
                _keys = true;

            // UpDown / LeftRight rotation


            if (Mouse.LeftButton)
            {
                _keys = false;

                _angleVelHorz = RotationSpeed * Mouse.XVel * DeltaTime * 0.0005f;
                _angleVelVert = RotationSpeed * Mouse.YVel * DeltaTime * 0.0005f;
            }

            else
            {
                if (_keys)
                {
                    _angleVelHorz = RotationSpeed * Keyboard.LeftRightAxis * DeltaTime;
                    _angleVelVert = RotationSpeed * Keyboard.UpDownAxis * DeltaTime;
                }
            }

            if (_isSpaceMouseMoving)
            {
                _angleHorz -= velRot.y;
                _angleVert -= velRot.x;

                float speed = DeltaTime * 12;

                _mainCamTransform.FpsView(_angleHorz, _angleVert, velPos.z, velPos.x, speed);
                _mainCamTransform.Translation.y += velPos.y * speed;
            }
            else
            {
                _angleHorz += _angleVelHorz;
                _angleVert += _angleVelVert;
                _angleVelHorz = 0;
                _angleVelVert = 0;

                if (HasUserMoved() || _mainCamTransform.Translation == InitCameraPos)
                {
                    _mainCamTransform.FpsView(_angleHorz, _angleVert, Keyboard.WSAxis, Keyboard.ADAxis, DeltaTime * 20);
                }
            }

            _minimapCamTransform.Translation.z = _mainCamTransform.Translation.z;
            _minimapCamTransform.Translation.x = _mainCamTransform.Translation.x;
        }

        private void switchModes()
        {
            _pointCloudActive = !_pointCloudActive;

            if (!_pointCloudActive)
            {
                _mainCamTransform.Translation = _panoSphere.sphereTransform.Translation;
                _scene.Children.Find(children => children.Name == "Pointcloud").GetComponent<RenderLayer>().Layer = RenderLayers.Layer01;
                _scene.Children.Find(children => children.Name == "PanoSphere").GetComponent<RenderLayer>().Layer = RenderLayers.Layer01;
            }
            else
            {
                _mainCam.Fov = M.PiOver3;
                _scene.Children.Find(children => children.Name == "Pointcloud").GetComponent<RenderLayer>().Layer = RenderLayers.All;
                _scene.Children.Find(children => children.Name == "PanoSphere").GetComponent<RenderLayer>().Layer = RenderLayers.None;
            }
        }

        private void sphereUserInput()
        {
            CalculateRotationAngle();

            UpdateCameraTransform();

            /*if (Mouse.LeftButton)
            {
                float2 pickPosClip = Mouse.Position * new float2(2.0f / Width, -2.0f / Height) + new float2(-1, 1);

                
                PickResult newPick = _scenePicker.Pick(RC, pickPosClip).OrderBy(pr => pr.ClipPos.z).FirstOrDefault();
                Diagnostics.Debug(newPick.Node.Name);
                
            }*/

        }

        public void CalculateRotationAngle()
        {
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

        }
        public void UpdateCameraTransform()
        {
            if (_sphereIsVisible)
            {
                _mainCamTransform.Rotation = new float3(_angleVert, _angleHorz, 0);

            }
            else
            {
                _mainCamTransform.Translation = new float3(_angleHorz * CamTranslationSpeed, _angleVert * CamTranslationSpeed, 0);
            }
        }
        /*        public void SwitchBetweenViews()
                {
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
                }*/
        // Is called when the window was resized
        public override void Resize(ResizeEventArgs e)
        {
            if (!PtRenderingParams.CalcSSAO && PtRenderingParams.Lighting == Lighting.Unlit) return;

            //(re)create depth tex and fbo
            if (_isTexInitialized)
            {
                _depthTex = WritableTexture.CreateDepthTex(Width, Height);

                PtRenderingParams.DepthPassEf.SetFxParam("ScreenParams", new float2(Width, Height));
                PtRenderingParams.ColorPassEf.SetFxParam("ScreenParams", new float2(Width, Height));
                PtRenderingParams.ColorPassEf.SetFxParam("DepthTex", _depthTex);
            }

            _isTexInitialized = true;
        }

        public override void DeInit()
        {
            IsAlive = false;
            base.DeInit();

        }

        private bool HasUserMoved()
        {
            return RC.View == float4x4.Identity
                || Mouse.LeftButton
                || Keyboard.WSAxis != 0 || Keyboard.ADAxis != 0
                || (Touch.GetTouchActive(TouchPoints.Touchpoint_0) && !Touch.TwoPoint);
        }

        public RenderContext GetRc()
        {
            return RC;
        }

        public SceneNode GetOocLoaderRootNode()
        {
            return OocLoader.RootNode;
        }

        public bool GetOocLoaderWasSceneUpdated()
        {
            return OocLoader.WasSceneUpdated;
        }

        public int GetOocLoaderPointThreshold()
        {
            return OocLoader.PointThreshold;
        }

        public void SetOocLoaderPointThreshold(int value)
        {
            OocLoader.PointThreshold = value;
        }

        public void SetOocLoaderMinProjSizeMod(float value)
        {
            OocLoader.MinProjSizeModifier = value;
        }

        public float GetOocLoaderMinProjSizeMod()
        {
            return OocLoader.MinProjSizeModifier;
        }

        public void LoadPointCloudFromFile()
        {
            //create Scene from octree structure
            var root = OocFileReader.GetScene();
            var ptOctantComp = root.GetComponent<OctantD>();

            InitCameraPos = _mainCamTransform.Translation = new float3((float)ptOctantComp.Center.x, 0, (float)(ptOctantComp.Center.z - (ptOctantComp.Size * 2f)));
            root.AddComponent(new RenderLayer()
            {
                Layer = RenderLayers.All
            });
            _scene.Children.Add(root);
            Diagnostics.Debug(root.GetComponent<Transform>().Translation);
            OocLoader.RootNode = root;
            OocLoader.FileFolderPath = PtRenderingParams.PathToOocFile;

            var octreeTexImgData = new ImageData(ColorFormat.uiRgb8, OocFileReader.NumberOfOctants, 1);
            _octreeTex = new Texture(octreeTexImgData);
            OocLoader.VisibleOctreeHierarchyTex = _octreeTex;

            var byteSize = OocFileReader.NumberOfOctants * octreeTexImgData.PixelFormat.BytesPerPixel;
            octreeTexImgData.PixelData = new byte[byteSize];

            var ptRootComponent = root.GetComponent<OctantD>();
            _octreeRootCenter = ptRootComponent.Center;
            _octreeRootLength = ptRootComponent.Size;

            PtRenderingParams.DepthPassEf = PtRenderingParams.CreateDepthPassEffect(new float2(Width, Height), InitCameraPos.z, _octreeTex, _octreeRootCenter, _octreeRootLength);
            PtRenderingParams.ColorPassEf = PtRenderingParams.CreateColorPassEffect(new float2(Width, Height), InitCameraPos.z, new float2(ZNear, ZFar), _depthTex, _octreeTex, _octreeRootCenter, _octreeRootLength);

            var pointcloud = _scene.Children.Find(children => children.Name == "Pointcloud");
            pointcloud.RemoveComponent<ShaderEffect>();
            if (PtRenderingParams.CalcSSAO || PtRenderingParams.Lighting != Lighting.Unlit)
                pointcloud.AddComponent(PtRenderingParams.DepthPassEf);
            else
                pointcloud.AddComponent(PtRenderingParams.ColorPassEf);

            IsSceneLoaded = true;
        }

        public void DeletePointCloud()
        {
            IsSceneLoaded = false;

            while (!OocLoader.WasSceneUpdated || !ReadyToLoadNewFile)
            {
                continue;
            }

            if (OocLoader.RootNode != null)
                _scene.Children.Remove(OocLoader.RootNode);

        }

        public void ResetCamera()
        {
            _mainCamTransform.Translation = InitCameraPos;
            _angleHorz = _angleVert = 0;
        }

        public void DeleteOctants()
        {
            IsSceneLoaded = false;

            while (!OocLoader.WasSceneUpdated || !ReadyToLoadNewFile)
            {
                continue;
            }

            DoShowOctants = false;
            OocLoader.DeleteOctants(_scene);
            IsSceneLoaded = true;
        }

        private void UpdateShaderParams()
        {
            foreach (var param in PtRenderingParams.ShaderParamsToUpdate)
            {
                if (PtRenderingParams.DepthPassEf.ParamDecl.ContainsKey(param.Key))
                    PtRenderingParams.DepthPassEf.SetFxParam(param.Key, param.Value);
                if (PtRenderingParams.ColorPassEf.ParamDecl.ContainsKey(param.Key))
                    PtRenderingParams.ColorPassEf.SetFxParam(param.Key, param.Value);
            }

            PtRenderingParams.ShaderParamsToUpdate.Clear();
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

            if (_gui._btnMiniMap.IsMouseOver)
            {
                _gui._btnMiniMap.OnMouseDown += OnMinimapDown;
            }
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

        public void BtnZoomOutDown(CodeComponent sender)
        {
            if (_sphereIsVisible)
            {
                if (_inverseCams)
                {
                    if (_minimapCam.Fov + 0.001 <= 1.2f)
                    {
                        _minimapCam.Fov += 0.001f;
                    }
                }
                else
                {
                    if (_mainCam.Fov + 0.001 <= 1.2f)
                    {
                        _mainCam.Fov += 0.001f;
                    }
                }
            }
            else
            {
                if (_inverseCams)
                {
                    if (!(_minimapCam.Fov + 0.001f >= M.PiOver3) && !(_minimapCam.Fov + 0.001f <= 0.3))
                    {
                        _minimapCam.Fov += 0.001f;
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
        }

        public void BtnZoomInDown(CodeComponent sender)
        {
            if (_inverseCams)
            {
                if (!(_minimapCam.Fov - 0.001 <= 0.3))
                {
                    _minimapCam.Fov -= 0.001f;
                }
            }
            else
            {
                if (!(_mainCam.Fov - 0.001 <= 0.3))
                {
                    _mainCam.Fov -= 0.001f;
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

        public SceneNode CreateWaypoint(float3 translation)
        {
            return new SceneNode()
            {
                Name = "Waypoint",
                Components = new List<SceneComponent>
                {
                    new RenderLayer {Layer = RenderLayers.Layer02 },
                    new Transform {Translation=translation,  Scale = float3.One },
                    MakeEffect.FromDiffuseSpecular((float4)ColorUint.Red, float4.Zero, 4.0f, 1f),
                    CreateCuboid(new float3(3, 10, 3))
                }
            };
        }

        public static Mesh CreateCuboid(float3 size)
        {
            return new Mesh
            {
                Vertices = new[]
                {
                    new float3 {x = +0.5f * size.x, y = -0.5f * size.y, z = +0.5f * size.z},
                    new float3 {x = +0.5f * size.x, y = +0.5f * size.y, z = +0.5f * size.z},
                    new float3 {x = -0.5f * size.x, y = +0.5f * size.y, z = +0.5f * size.z},
                    new float3 {x = -0.5f * size.x, y = -0.5f * size.y, z = +0.5f * size.z},
                    new float3 {x = +0.5f * size.x, y = -0.5f * size.y, z = -0.5f * size.z},
                    new float3 {x = +0.5f * size.x, y = +0.5f * size.y, z = -0.5f * size.z},
                    new float3 {x = +0.5f * size.x, y = +0.5f * size.y, z = +0.5f * size.z},
                    new float3 {x = +0.5f * size.x, y = -0.5f * size.y, z = +0.5f * size.z},
                    new float3 {x = -0.5f * size.x, y = -0.5f * size.y, z = -0.5f * size.z},
                    new float3 {x = -0.5f * size.x, y = +0.5f * size.y, z = -0.5f * size.z},
                    new float3 {x = +0.5f * size.x, y = +0.5f * size.y, z = -0.5f * size.z},
                    new float3 {x = +0.5f * size.x, y = -0.5f * size.y, z = -0.5f * size.z},
                    new float3 {x = -0.5f * size.x, y = -0.5f * size.y, z = +0.5f * size.z},
                    new float3 {x = -0.5f * size.x, y = +0.5f * size.y, z = +0.5f * size.z},
                    new float3 {x = -0.5f * size.x, y = +0.5f * size.y, z = -0.5f * size.z},
                    new float3 {x = -0.5f * size.x, y = -0.5f * size.y, z = -0.5f * size.z},
                    new float3 {x = +0.5f * size.x, y = +0.5f * size.y, z = +0.5f * size.z},
                    new float3 {x = +0.5f * size.x, y = +0.5f * size.y, z = -0.5f * size.z},
                    new float3 {x = -0.5f * size.x, y = +0.5f * size.y, z = -0.5f * size.z},
                    new float3 {x = -0.5f * size.x, y = +0.5f * size.y, z = +0.5f * size.z},
                    new float3 {x = +0.5f * size.x, y = -0.5f * size.y, z = -0.5f * size.z},
                    new float3 {x = +0.5f * size.x, y = -0.5f * size.y, z = +0.5f * size.z},
                    new float3 {x = -0.5f * size.x, y = -0.5f * size.y, z = +0.5f * size.z},
                    new float3 {x = -0.5f * size.x, y = -0.5f * size.y, z = -0.5f * size.z}
                },

                Triangles = new ushort[]
                {
                    // front face
                    0, 2, 1, 0, 3, 2,

                    // right face
                    4, 6, 5, 4, 7, 6,

                    // back face
                    8, 10, 9, 8, 11, 10,

                    // left face
                    12, 14, 13, 12, 15, 14,

                    // top face
                    16, 18, 17, 16, 19, 18,

                    // bottom face
                    20, 22, 21, 20, 23, 22
                },

                Normals = new[]
                {
                    new float3(0, 0, 1),
                    new float3(0, 0, 1),
                    new float3(0, 0, 1),
                    new float3(0, 0, 1),
                    new float3(1, 0, 0),
                    new float3(1, 0, 0),
                    new float3(1, 0, 0),
                    new float3(1, 0, 0),
                    new float3(0, 0, -1),
                    new float3(0, 0, -1),
                    new float3(0, 0, -1),
                    new float3(0, 0, -1),
                    new float3(-1, 0, 0),
                    new float3(-1, 0, 0),
                    new float3(-1, 0, 0),
                    new float3(-1, 0, 0),
                    new float3(0, 1, 0),
                    new float3(0, 1, 0),
                    new float3(0, 1, 0),
                    new float3(0, 1, 0),
                    new float3(0, -1, 0),
                    new float3(0, -1, 0),
                    new float3(0, -1, 0),
                    new float3(0, -1, 0)
                },

                UVs = new[]
                {
                    new float2(1, 0),
                    new float2(1, 1),
                    new float2(0, 1),
                    new float2(0, 0),
                    new float2(1, 0),
                    new float2(1, 1),
                    new float2(0, 1),
                    new float2(0, 0),
                    new float2(1, 0),
                    new float2(1, 1),
                    new float2(0, 1),
                    new float2(0, 0),
                    new float2(1, 0),
                    new float2(1, 1),
                    new float2(0, 1),
                    new float2(0, 0),
                    new float2(1, 0),
                    new float2(1, 1),
                    new float2(0, 1),
                    new float2(0, 0),
                    new float2(1, 0),
                    new float2(1, 1),
                    new float2(0, 1),
                    new float2(0, 0)
                },
                BoundingBox = new AABBf(-0.5f * size, 0.5f * size)
            };
        }

        public void CheckWaypointPicking()
        {
            if (Mouse.LeftButton)
            {
                //float2 pickPosClip = Mouse.Position * new float2(2.0f / Width, -2.0f / Height) + new float2(-1, 1);

                //PickResult newPick = _scenePicker.Pick(RC, pickPosClip).OrderBy(pr => pr.ClipPos.z).FirstOrDefault();

                //Diagnostics.Debug(newPick.Node.Name);
                //if (newPick?.Node != _currentPick?.Node)
                //{
                //    if (_currentPick != null)
                //    {
                //        var ef = _currentPick.Node.GetComponent<DefaultSurfaceEffect>();
                //        ef.SurfaceInput.Albedo = _oldColor;
                //    }
                //    if (newPick != null)
                //    {
                //        var ef = newPick.Node.GetComponent<SurfaceEffect>();
                //        _oldColor = ef.SurfaceInput.Albedo;
                //        ef.SurfaceInput.Albedo = (float4)ColorUint.OrangeRed;
                //    }
                //    _currentPick = newPick;
                //}
            }
        }
    }
}