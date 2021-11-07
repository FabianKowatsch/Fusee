using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Effects;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Core.ShaderShards;
using Fusee.PointCloud.Common;
using Fusee.PointCloud.OoCReaderWriter;
using Fusee.PointCloud.PointAccessorCollections;
using Fusee.Engine.GUI;
using Fusee.Math.Core;
using Fusee.Xene;
using System;
using System.Linq;
using System.Collections.Generic;
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
        private AxisDescription _spaceAxis;

        private const float ZNear = 1f;
        private const float ZFar = 1000;

        private readonly float Fov = M.PiOver3;

        private SceneRendererForward _guiRenderer;
        private SceneContainer _gui;
        private SceneInteractionHandler _sih;
        private readonly CanvasRenderMode _canvasRenderMode = CanvasRenderMode.Screen;

        private float3 _initCamPos;
        private float3 _spherePos;
        private float3 _sphereRot;
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

        private SixDOFDevice _spaceMouse;

        private PanoSphere _panoSphere;

        // Init is called on startup. 
        public override void Init()
        {

            _panoSphere = PanoSphereFactory.createPanoSpheres().ElementAt(0);
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

            _angleRoll = 0;
            _angleRollInit = 0;
            _offset = float2.Zero;
            _offsetInit = float2.Zero;

            // Set the clear color for the back buffer to white (100% intensity in all color channels R, G, B, A).            
            if (!UseWPF)
                LoadPointCloudFromFile();

            _scene.Children.Add(_panoSphere);



            _gui = CreateGui();
            //Create the interaction handler
            _sih = new SceneInteractionHandler(_gui);

            // Wrap a SceneRenderer around the model.
            _sceneRenderer = new SceneRendererForward(_scene);
            _guiRenderer = new SceneRendererForward(_gui);

            /*-----------------------------------------------------------------------
            * Debuggingtools
            -----------------------------------------------------------------------*/

            //RC.SetRenderState(RenderState.CullMode, (uint)Cull.None);
            //RC.SetRenderState(RenderState.FillMode, (uint)FillMode.Wireframe);

            IsInitialized = true;
            _spaceAxis = Keyboard.RegisterSingleButtonAxis(32);


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

                if (Keyboard.IsKeyDown(KeyCodes.Enter))
                {
                    switchModes();
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

            //_guiRenderer.Render(RC);

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
            _mainCamTransform.Translate(new float3(0, 0.1f * (Keyboard.GetAxis(_spaceAxis.Id) - 1f) * -1f, 0));

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
        }

        private void switchModes()
        {
            _pointCloudActive = !_pointCloudActive;

            if (!_pointCloudActive)
            {
                _mainCamTransform.Translation = _spherePos;
                _scene.Children[2].GetComponent<RenderLayer>().Layer = RenderLayers.Layer01;
            }
            else
            {
                _mainCam.Fov = M.PiOver3;
                _scene.Children[2].GetComponent<RenderLayer>().Layer = RenderLayers.Layer02;
            }
        }

        private void sphereUserInput()
        {
            MouseWheelZoom();

            //HndGuiButtonInput();

            CalculateRotationAngle();

            UpdateCameraTransform();

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
            var cameraZBasedOnCenter = (float)(ptOctantComp.Center.z - ptOctantComp.Size * 2f);
            InitCameraPos = _mainCamTransform.Translation = new float3((float)ptOctantComp.Center.x, (float)ptOctantComp.Center.y - 10, 0);
            Diagnostics.Debug(ptOctantComp.Center.x + "/" + ptOctantComp.Center.y + "/" + ptOctantComp.Center.z);
            //InitCameraPos = _mainCamTransform.Translation = new float3(0, 0, 0);
            _spherePos = _spherePos = new float3(47, 310, -2f);
            _sphereRot = new float3(0, 5.95f, 0);
            root.AddComponent(new RenderLayer()
            {
                Layer = RenderLayers.Layer01
            });
            _scene.Children.Add(root);

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

            PtRenderingParams.DepthPassEf = PtRenderingParams.CreateDepthPassEffect(new float2(Width, Height), cameraZBasedOnCenter, _octreeTex, _octreeRootCenter, _octreeRootLength);
            PtRenderingParams.ColorPassEf = PtRenderingParams.CreateColorPassEffect(new float2(Width, Height), cameraZBasedOnCenter, new float2(ZNear, ZFar), _depthTex, _octreeTex, _octreeRootCenter, _octreeRootLength);

            _scene.Children[1].RemoveComponent<ShaderEffect>();
            if (PtRenderingParams.CalcSSAO || PtRenderingParams.Lighting != Lighting.Unlit)
                _scene.Children[1].AddComponent(PtRenderingParams.DepthPassEf);
            else
                _scene.Children[1].AddComponent(PtRenderingParams.ColorPassEf);

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

        #region UI

        private SceneContainer CreateGui()
        {
            var vsTex = AssetStorage.Get<string>("texture.vert");
            var psTex = AssetStorage.Get<string>("texture.frag");
            var psText = AssetStorage.Get<string>("text.frag");

            var canvasWidth = Width / 100f;
            var canvasHeight = Height / 100f;

            var btnFuseeLogo = new GUIButton
            {
                Name = "Canvas_Button"
            };
            btnFuseeLogo.OnMouseEnter += BtnLogoEnter;
            btnFuseeLogo.OnMouseExit += BtnLogoExit;
            btnFuseeLogo.OnMouseDown += BtnLogoDown;

            var guiFuseeLogo = new Texture(AssetStorage.Get<ImageData>("FuseeText.png"));
            var fuseeLogo = new TextureNode(
                "fuseeLogo",
                vsTex,
                psTex,
                //Set the albedo texture you want to use.
                guiFuseeLogo,
                //Define anchor points. They are given in percent, seen from the lower left corner, respectively to the width/height of the parent.
                //In this setup the element will stretch horizontally but stay the same vertically if the parent element is scaled.
                UIElementPosition.GetAnchors(AnchorPos.TopTopLeft),
                //Define Offset and therefor the size of the element.
                UIElementPosition.CalcOffsets(AnchorPos.TopTopLeft, new float2(0, canvasHeight - 0.5f), canvasHeight, canvasWidth, new float2(1.75f, 0.5f)),
                float2.One
                );
            fuseeLogo.AddComponent(btnFuseeLogo);

            var fontLato = AssetStorage.Get<Font>("Lato-Black.ttf");
            var guiLatoBlack = new FontMap(fontLato, 24);

            var text = new TextNode(
                "FUSEE Simple Example",
                "ButtonText",
                vsTex,
                psText,
                UIElementPosition.GetAnchors(AnchorPos.StretchHorizontal),
                UIElementPosition.CalcOffsets(AnchorPos.StretchHorizontal, new float2(canvasWidth / 2 - 4, 0), canvasHeight, canvasWidth, new float2(8, 1)),
                guiLatoBlack,
                ColorUint.Tofloat4(ColorUint.Greenery),
                HorizontalTextAlignment.Center,
                VerticalTextAlignment.Center);

            var canvas = new CanvasNode(
                "Canvas",
                _canvasRenderMode,
                new MinMaxRect
                {
                    Min = new float2(-canvasWidth / 2, -canvasHeight / 2f),
                    Max = new float2(canvasWidth / 2, canvasHeight / 2f)
                })
            {
                Children = new ChildList()
                {
                    //Simple Texture Node, contains the fusee logo.
                    fuseeLogo,
                    text
                }
            };

            return new SceneContainer
            {
                Children = new List<SceneNode>
                {
                    //Add canvas.
                    canvas
                }
            };
        }

        public void BtnLogoEnter(CodeComponent sender)
        {
            var effect = _gui.Children.FindNodes(node => node.Name == "fuseeLogo").First().GetComponent<Effect>();
            effect.SetFxParam(UniformNameDeclarations.Albedo, new float4(0.0f, 0.0f, 0.0f, 1f));
            effect.SetFxParam(UniformNameDeclarations.AlbedoMix, 0.8f);
        }

        public void BtnLogoExit(CodeComponent sender)
        {
            var effect = _gui.Children.FindNodes(node => node.Name == "fuseeLogo").First().GetComponent<Effect>();
            effect.SetFxParam(UniformNameDeclarations.Albedo, float4.One);
            effect.SetFxParam(UniformNameDeclarations.AlbedoMix, 1f);
        }

        public void BtnLogoDown(CodeComponent sender)
        {
            OpenLink("http://fusee3d.org");
        }

        #endregion       
    }
}
