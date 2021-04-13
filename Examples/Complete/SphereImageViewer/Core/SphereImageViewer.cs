using Fusee.Base.Common;
using Fusee.Base.Core;
using Fusee.Engine.Common;
using Fusee.Engine.Core;
using Fusee.Engine.Core.Effects;
using Fusee.Engine.Core.Scene;
using Fusee.Engine.Core.ShaderShards;
using Fusee.Engine.Core.ShaderShards.Fragment;
using Fusee.Engine.Core.ShaderShards.Vertex;
using Fusee.Engine.GUI;
using Fusee.Math.Core;
using Fusee.Xene;
using System.Collections.Generic;
using System.Linq;
using static Fusee.Engine.Core.Input;
using static Fusee.Engine.Core.Time;


namespace Fusee.Examples.SphereImageViewer.Core
{
    [FuseeApplication(Name = "FUSEE Spherical Image Viewer", Description = "Viewer for 360 degree pictures.")]
    public class SphereImageViewer : RenderCanvas
    {

        private static float _angleHorz = M.Pi, _angleVert = 0, _angleVelHorz, _angleVelVert, _zoom;
        private static float _planePositionX = 0, _planePositionY = 0, _planePosX, _planePosY;

        private const float RotationSpeed = 2;
        private const float Damping = 0.8f;

        private SceneRendererForward _sceneRenderer;

        private const float ZNear = 1f;
        private const float ZFar = 1000;
        private const float DistancePlaneCamera = 5;


        private SceneRendererForward _guiRenderer;
        private SceneContainer _gui;
        private SceneInteractionHandler _sih;
        private readonly CanvasRenderMode _canvasRenderMode = CanvasRenderMode.Screen;

        private GUIButton _btnZoomIn;
        private GUIButton _btnZoomOut;

        private bool _keys;

        private SceneContainer _animScene;
        private Transform _sphereTransform;
        private Transform _planeTransform;

        private Transform _mainCamTransform;
        private readonly Camera _mainCam = new Camera(ProjectionMethod.Perspective, 5, 100, M.PiOver4);
        private readonly Camera _guiCam = new Camera(ProjectionMethod.Orthographic, 1, 1000, M.PiOver4);

        private float2 _zoomInBtnPosition;
        private float2 _zoomOutBtnPosition;

        private const float _planeHeight = 4096f / 300f;
        private const float _planeWidth = 8192f / 300f;

        private VertexAnimationSurfaceEffect _animationEffect;

        private bool _sphereIsVisible = true;

        private const float AnimDuration = 2f;
        private bool _animActive;
        private float _animTimeStart = 0;


        // Init is called on startup.
        public override void Init()
        {

            var sphereTex = new Texture(AssetStorage.Get<ImageData>("LadyBug_C2P2.jpg"));

            Mesh sphere = CreateSphere(10, 20, 50);
            Mesh plane = CreateGridPlane(20, 50, _planeHeight, _planeWidth, DistancePlaneCamera);
            
            TextureInputSpecular colorInput = new TextureInputSpecular()
            {
                Albedo = float4.One,
                //Emission = float4.Zero,
                //Shininess = 1.0f,
                //SpecularStrength = 0.0f,
                AlbedoMix = 1.0f,
                AlbedoTex = sphereTex,
                TexTiles = float2.One,
                Roughness = 0.0f
            };
            var lightingSetup = LightingSetupFlags.Unlit | LightingSetupFlags.AlbedoTex;

            _animationEffect = new VertexAnimationSurfaceEffect(lightingSetup, colorInput, FragShards.SurfOutBody_Textures(lightingSetup), VertShards.SufOutBody_PosAnimation);

            _animationEffect.PercentPerVertex = 1.0f;
            _animationEffect.PercentPerVertex1 = 0.0f;
            
            //Creating CameraComponent and TransformComponent
            _mainCam.Viewport = new float4(0, 0, 100, 100);
            _mainCam.BackgroundColor = new float4(0f, 0f, 0f, 1);
            _mainCam.Layer = -1;
            _mainCam.Active = true;

            _guiCam.ClearColor = false;
            _guiCam.ClearDepth = false;
            _guiCam.FrustumCullingOn = false;
            _guiCam.Layer = 99;

            _mainCamTransform = new Transform()
            {
                Rotation = new float3(_angleVert, _angleHorz, 0),
                Translation = new float3(0, 0, 0),
                Scale = new float3(1, 1, 1)
            };

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

            SceneNode planeNode = new SceneNode()
            {
                Components = new List<SceneComponent>()
                {
                    _planeTransform,
                    plane,
                }
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

            _gui = CreateGui();

            // Create the interaction handler
            _sih = new SceneInteractionHandler(_gui);

            // Set the clear color for the backbuffer to white (100% intensity in all color channels R, G, B, A).
            //RC.ClearColor = new float4(1, 1, 1, 1);


            //Scene with Main Camera and Mesh
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
                     },
                    new SceneNode
                    {
                        Components = new List<SceneComponent>
                        {
                            _sphereTransform,
                            _animationEffect,
                            //CreateSphereAndPlane(10, 20, 50, _planeHeight * 2, _planeWidth * 2, DistancePlaneCamera)
                            sphereAndPlane
                        }
                    }
                }
            };

            
            //Debuggingtools
            //RC.SetRenderState(RenderState.CullMode, (uint)Cull.None);
            //RC.SetRenderState(RenderState.FillMode, (uint)FillMode.Wireframe);

            // Wrap a SceneRenderer around the model.
            _sceneRenderer = new SceneRendererForward(_animScene);

            
             _guiRenderer = new SceneRendererForward(_gui);
        }

        // RenderAFrame is called once a frame
        public override void RenderAFrame()
        {
            // Clear the backbuffer
            RC.Clear(ClearFlags.Color | ClearFlags.Depth);

            //RC.Viewport(0, 0, Width, Height);


            #region Controls
            // Mouse and keyboard movement
            _zoom = Mouse.WheelVel * DeltaTime * -0.05f;

            if (_sphereIsVisible)
            {
                if (Keyboard.LeftRightAxis != 0 || Keyboard.UpDownAxis != 0)
                {
                    _keys = true;
                }
                if (Mouse.LeftButton)
                {
                    _keys = false;
                    _angleVelHorz = -RotationSpeed * Mouse.XVel * DeltaTime * 0.0005f;
                    _angleVelVert = -RotationSpeed * Mouse.YVel * DeltaTime * 0.0005f;
                }
                else if (Touch.GetTouchActive(TouchPoints.Touchpoint_0))
                {
                    _keys = false;
                    var touchVel = Touch.GetVelocity(TouchPoints.Touchpoint_0);
                    _angleVelHorz = -RotationSpeed * touchVel.x * DeltaTime * 0.0005f;
                    _angleVelVert = -RotationSpeed * touchVel.y * DeltaTime * 0.0005f;

                    //_touchPosition = Touch.GetPosition(TouchPoints.Touchpoint_0);

                }
                else
                {
                    if (_keys)
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

                if (!(_mainCam.Fov + _zoom >= 1.2) && !(_mainCam.Fov + _zoom <= 0.3))
                {
                    _mainCam.Fov += _zoom;
                }
            }
            
            else
            {
                if(Keyboard.LeftRightAxis != 0 || Keyboard.UpDownAxis != 0)
                {
                    _keys = true;
                    
                }

                //Zoom in/out
                if(_zoom != 0)
                {
                    if (!(_mainCam.Fov + _zoom >= M.PiOver3) && !(_mainCam.Fov + _zoom <= 0.3))
                    {
                        _mainCam.Fov += _zoom;
                        if(_mainCam.Fov > 0.75f)  //um bei weitem herauszoomen das flackern zwischen den Rändern zu verhindern
                        {
                            _planePositionY = 0;
                            _planePositionX = 0;
                        }
                    }
                }
                

                if (Mouse.LeftButton)
                {
                    _keys = false;
                    _planePosX = RotationSpeed * Mouse.XVel * DeltaTime * 0.0005f;
                    _planePosY = -RotationSpeed * Mouse.YVel * DeltaTime * 0.0005f;
                }
                else if (Touch.GetTouchActive(TouchPoints.Touchpoint_0))
                {
                    _keys = false;
                    var touchVel = Touch.GetVelocity(TouchPoints.Touchpoint_0);
                    _planePosX = RotationSpeed * touchVel.x * DeltaTime * 0.0005f;
                    _planePosY = -RotationSpeed * touchVel.y * DeltaTime * 0.0005f;

                    //_touchPosition = Touch.GetPosition(TouchPoints.Touchpoint_0);

                }
                else
                {
                    if (_keys)
                    {
                        _planePosX = -RotationSpeed * Keyboard.LeftRightAxis * DeltaTime;
                        _planePosY = -RotationSpeed * Keyboard.UpDownAxis * DeltaTime;
                    }
                    else
                    {
                        var curDamp = (float)System.Math.Exp(-Damping * DeltaTime);
                        _planePosX *= curDamp;
                        _planePosY *= curDamp;
                    }
                }

                float aspect = (float)Width / (float)Height;

                float yMax = DistancePlaneCamera * (float)System.Math.Tan(0.5f * _mainCam.Fov);
                float yMin = -yMax;
                float xMin = yMin * aspect;
                float xMax = yMax * aspect;

                if (yMax - _planePositionY - _planePosY > (_planeHeight / 2) || yMin - _planePositionY - _planePosY < -(_planeHeight / 2)) 
                {
                    if (_planePositionY > 0)
                    {
                        _planePositionY = (_planeHeight / 2) - yMax;
                    }
                    else if(_planePositionY < 0)
                    {
                        _planePositionY = -(_planeHeight / 2) + yMax;
                    }
                }
                else
                {
                    _planePositionY += _planePosY;
                }
                Diagnostics.Debug("xMax: " + xMax);
                Diagnostics.Debug("planePositionX: " + _planePositionX);
                Diagnostics.Debug("planePosX: " + _planePosX);
                Diagnostics.Debug("planeWidth/2: " + _planeWidth / 2f);
                if (xMax - _planePositionX - _planePosX > (_planeWidth / 2f) || xMin - _planePositionX - _planePosX < -(_planeWidth / 2f)) 
                {

                    if (_planePositionX > 0)
                    {
                        Diagnostics.Debug("Bumper hit");
                        _planePositionX = (_planeWidth / 2) - xMax - 0.000001f;
                    }
                    else if(_planePositionX < 0)
                    {
                        Diagnostics.Debug("Bumper hit");
                        _planePositionX = -(_planeWidth / 2) + xMax - 0.000001f;

                    }
                }
                else
                {
                    _planePositionX -= _planePosX;
                }
            }


            Diagnostics.Debug("----------------------------");

            _angleHorz += _angleVelHorz;
            if(!(_angleVert + _angleVelVert >= 1.5) && !(_angleVert + _angleVelVert <= -1.5))
            {
                _angleVert += _angleVelVert;
            }


            
            if (_sphereIsVisible)
            {
                _mainCamTransform.Rotation = new float3(_angleVert, _angleHorz, 0);
            }
            //_imagePlaneTransform.Rotation = new float3(_angleVert, _angleHorz, 0);
            _sphereTransform.Translation = new float3(_planePositionX, _planePositionY, 0);
            //_mainCamTransform.Translation = new float3(-_planePositionX, -_planePositionY, 0);


            //GUI Button Input
            if (_btnZoomOut.IsMouseOver)
            {
                _btnZoomOut.OnMouseDown += BtnZoomOutDown;
            }

            if (_btnZoomIn.IsMouseOver)
            {
                _btnZoomIn.OnMouseDown += BtnZoomInDown;
            }


            //Input Space Bar to switch between spherical and plane view
            if (Keyboard.IsKeyDown(KeyCodes.Space))
            {
                _mainCam.Fov = M.PiOver4;

                //Animationsettings
                _animTimeStart = Time.TimeSinceStart;
                _animActive = true;

                _sphereTransform.Translation = new float3(0, 0, 0);
                _sphereTransform.Rotation = new float3(0, 0, 0);
                _mainCamTransform.Rotation = new float3(0, M.Pi, 0);

                if (_sphereIsVisible)
                {
                    _sphereIsVisible = false;
                }
                else
                {
                    _sphereIsVisible = true;
                }
            }
            //Zoom In Button Check
            //if ((_touchPosition.x <= _zoomInBtnPosition.x + 0.25 && _touchPosition.x >= _zoomInBtnPosition.x - 0.25) && (_touchPosition.y <= _zoomInBtnPosition.y + 0.25 && _touchPosition.y >= _zoomInBtnPosition.y - 0.25))
            //{
            //    _btnZoomIn.OnMouseDown += BtnZoomInDown;
            //}

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
                #endregion
            }





            // Create the camera matrix and set it as the current ModelView transformation
            //var mtxRot = float4x4.CreateRotationX(_angleVert) * float4x4.CreateRotationY(_angleHorz);
            //var mtxCam = float4x4.LookAt(0, 0, -10, 0, 0, 0, 0, 1, 0);

            //var view = mtxCam * mtxRot;
            //var perspective = float4x4.CreatePerspectiveFieldOfView(_fovy, (float)Width / Height, ZNear, ZFar);
            var orthographic = float4x4.CreateOrthographic(Width, Height, ZNear, ZFar);

            //_mainCamTransform.FpsView(_angleHorz, _angleVert, Keyboard.WSAxis, Keyboard.ADAxis, Time.DeltaTime * 1000);
            _mainCam.ProjectionMethod = ProjectionMethod.Perspective;
            //_mainCam.Viewport = new float4(0, 0, 100f, 100f);

            // Render the scene loaded in Init()
            //RC.View = view;
            //RC.Projection = perspective; //_sceneRenderer.Animate();
            _sceneRenderer.Render(RC);
            //_guiRenderer.Render(RC);

            //Constantly check for interactive objects.
            //RC.Projection = orthographic;


            if (!Mouse.Desc.Contains("Android"))
                _sih.CheckForInteractiveObjects(RC, Mouse.Position, Width, Height);
            if (Touch.GetTouchActive(TouchPoints.Touchpoint_0) && !Touch.TwoPoint)
            {
                _sih.CheckForInteractiveObjects(RC, Touch.GetPosition(TouchPoints.Touchpoint_0), Width, Height);
            }


            // Swap buffers: Show the contents of the backbuffer (containing the currently rendered frame) on the front buffer.
            Present();
        }

        private SceneContainer CreateGui()
        {
            var vsTex = AssetStorage.Get<string>("texture.vert");
            var psTex = AssetStorage.Get<string>("texture.frag");
            var psText = AssetStorage.Get<string>("text.frag");
            var vsNineSlice = AssetStorage.Get<string>("nineSlice.vert");
            var psNineSlice = AssetStorage.Get<string>("nineSliceTile.frag");


            var canvasWidth = Width / 100f;
            var canvasHeight = Height / 100f;

            var btnFuseeLogo = new GUIButton
            {
                Name = "Canvas_Button"
            };
            btnFuseeLogo.OnMouseEnter += BtnLogoEnter;
            btnFuseeLogo.OnMouseExit += BtnLogoExit;
            //btnFuseeLogo.OnMouseDown += BtnLogoDown;

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
                "FUSEE Spherical Image Viewer",
                "ButtonText",
                vsTex,
                psText,
                UIElementPosition.GetAnchors(AnchorPos.StretchHorizontal),
                UIElementPosition.CalcOffsets(AnchorPos.StretchHorizontal, new float2(canvasWidth / 2 - 4, 0), canvasHeight, canvasWidth, new float2(8, 1)),
                guiLatoBlack,
                ColorUint.Tofloat4(ColorUint.Greenery),
                HorizontalTextAlignment.Center,
                VerticalTextAlignment.Center);

            _btnZoomOut = new GUIButton
            {
                Name = "Zoom_Out_Button"
            };

            _btnZoomIn = new GUIButton
            {
                Name = "Zoom_In_Button"
            };

            _zoomInBtnPosition = new float2(canvasWidth - 1f, 1f);
            var zoomInNode = new TextureNode(
                "ZoomInLogo",
                vsTex,
                psTex,
                new Texture(AssetStorage.Get<ImageData>("FuseePlusIcon.png")),
                UIElementPosition.GetAnchors(AnchorPos.DownDownRight),
                UIElementPosition.CalcOffsets(AnchorPos.DownDownRight, _zoomInBtnPosition, canvasHeight, canvasWidth, new float2(0.5f, 0.5f)),
                float2.One
                );
            zoomInNode.Components.Add(_btnZoomIn);

            _zoomOutBtnPosition = new float2(canvasWidth - 1f, 0.4f);
            var zoomOutNode = new TextureNode(
                "ZoomOutLogo",
                vsTex,
                psTex,
                new Texture(AssetStorage.Get<ImageData>("FuseeMinusIcon.png")),
                UIElementPosition.GetAnchors(AnchorPos.DownDownRight),
                UIElementPosition.CalcOffsets(AnchorPos.DownDownRight, _zoomOutBtnPosition, canvasHeight, canvasWidth, new float2(0.5f, 0.5f)),
                float2.One
                );
            zoomOutNode.Components.Add(_btnZoomOut);

            


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
                    text,
                    zoomInNode,
                    zoomOutNode
                }
            };


            return new SceneContainer
            {
                Children = new List<SceneNode>
                {
                    new SceneNode
                    {
                        Name = "GuiCam",
                        Components = new List<SceneComponent>()
                        {
                            _mainCamTransform,
                            _guiCam

                        }
                     },
                    
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

        //public void BtnLogoDown(CodeComponent sender)
        //{
        //    OpenLink("http://fusee3d.org");
        //}
        
        public void BtnZoomInDown(CodeComponent sender)
        {
            if (!(_mainCam.Fov - 0.001 <= 0.3))
            {
                _mainCam.Fov -= 0.001f;
            }
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
                    if (_mainCam.Fov > 0.75f)  //um bei weitem herauszoomen das flackern zwischen den Rändern zu verhindern
                    {
                        _planePositionY = 0;
                        _planePositionX = 0;
                    }
                }
            }
            
        }

        public static Mesh CreateSphere(float radius, int longSegments, int latSegments)
        {
            
            //Deklaration der Arrays
            float3[] verts = new float3[(longSegments + 1) * latSegments];
            float3[] norms = new float3[(longSegments + 1) * latSegments];
            float2[] uvs = new float2[(longSegments + 1) * latSegments];
            ushort[] tris = new ushort[longSegments * (latSegments - 1) * 6];

            //Initialisierung der verts, norms und uvs
            for (int i = 0; i < latSegments; i++)
            {
                float theta = (float)M.Pi / (latSegments - 1) * i;

                for (int j = 0; j <= longSegments; j++)
                {
                    float phi = 2 * (float)M.Pi / (longSegments) * j;

                    if (i == 0)  //Nordpol
                    {
                        verts[j] = new float3(0, radius, 0);
                        norms[j] = -float3.UnitY;
                        uvs[j] = new float2((float)(j) / (float)(longSegments), 1);
                    }
                    else if (i == latSegments - 1)   //Südpol
                    {
                        verts[(latSegments - 1) * (longSegments + 1) + j] = new float3(0, -radius, 0);
                        norms[(latSegments - 1) * (longSegments + 1) + j] = float3.UnitY;
                        uvs[(latSegments - 1) * (longSegments + 1) + j] = new float2((float)(j) / (float)(longSegments), 0);
                    }
                    else
                    {
                        if (j == longSegments)
                        {
                            phi = 2 * (float)M.Pi / (longSegments - 1) * 0;
                         }
                        verts[i * longSegments + j + i * 1] = new float3(
                            (float)(radius * M.Sin(phi) * M.Sin(theta)),
                            (float)(radius * M.Cos(theta)),
                            (float)(radius * M.Cos(phi) * M.Sin(theta)));

                        norms[i * longSegments + j + i * 1] = new float3(
                            -(float)(radius * M.Sin(phi) * M.Sin(theta)),
                            -(float)(radius * M.Cos(theta)),
                            -(float)(radius * M.Cos(phi) * M.Sin(theta)));

                        uvs[i * longSegments + j + i * 1] = new float2(
                            (float)(j) / (float)(longSegments),
                            1 - ((float)(i) / (float)(latSegments - 1)));
                    }
                }
            }

            for (int j = 0, k = 0; j < (latSegments - 1) * longSegments; j++, k++)   //j für die Array-Indices und k ist ein Punkt in den aktuellen Dreiecken
            {
                if ((k + 1) % (longSegments + 1) == 0)
                {
                    k++;
                }

                tris[j * 6] = (ushort)(k);
                tris[j * 6 + 1] = (ushort)(k + (longSegments + 1) + 1);
                tris[j * 6 + 2] = (ushort)(k + 1);

                tris[j * 6 + 3] = (ushort)(k);
                tris[j * 6 + 4] = (ushort)(k + (longSegments + 1));
                tris[j * 6 + 5] = (ushort)(k + (longSegments + 1) + 1);

            }

            return new Mesh
            {
                Vertices = verts,
                Normals = norms,
                UVs = uvs,
                Triangles = tris,

            };
        }

        public static Mesh CreateSphereAndPlane(float radius, int longSegments, int latSegments, float height, float width, float distancePlane)
        {
            //Deklaration der Arrays
            float3[] verts = new float3[(longSegments + 1) * latSegments];
            float3[] verts1 = new float3[(longSegments + 1) * latSegments];
            float3[] norms = new float3[(longSegments + 1) * latSegments];
            float3[] norms1 = new float3[(longSegments + 1) * latSegments];
            float2[] uvs = new float2[(longSegments + 1) * latSegments];
            ushort[] tris = new ushort[longSegments * (latSegments - 1) * 6];


            //Sphere
            //Initialisierung der verts, norms und uvs
            for (int i = 0; i < latSegments; i++)
            {
                float theta = (float)M.Pi / (latSegments - 1) * i;

                for (int j = 0; j <= longSegments; j++)
                {
                    float phi = 2 * (float)M.Pi / (longSegments) * j;

                    if (i == 0)  //Nordpol
                    {
                        verts[j] = new float3(0, radius, 0);
                        norms[j] = -float3.UnitY;
                        uvs[j] = new float2((float)(j) / (float)(longSegments), 1);
                    }
                    else if (i == latSegments - 1)   //Südpol
                    {
                        verts[(latSegments - 1) * (longSegments + 1) + j] = new float3(0, -radius, 0);
                        norms[(latSegments - 1) * (longSegments + 1) + j] = float3.UnitY;
                        uvs[(latSegments - 1) * (longSegments + 1) + j] = new float2((float)(j) / (float)(longSegments), 0);
                    }
                    else
                    {
                        if (j == longSegments)
                        {
                            phi = 2 * (float)M.Pi / (longSegments - 1) * 0;
                        }
                        verts[i * longSegments + j + i * 1] = new float3(
                            (float)(radius * M.Sin(phi) * M.Sin(theta)),
                            (float)(radius * M.Cos(theta)),
                            (float)(radius * M.Cos(phi) * M.Sin(theta)));

                        norms[i * longSegments + j + i * 1] = new float3(
                            -(float)(radius * M.Sin(phi) * M.Sin(theta)),
                            -(float)(radius * M.Cos(theta)),
                            -(float)(radius * M.Cos(phi) * M.Sin(theta)));

                        uvs[i * longSegments + j + i * 1] = new float2(
                            (float)(j) / (float)(longSegments),
                            1 - ((float)(i) / (float)(latSegments - 1)));
                    }
                }
            }

            //Initialisierung der tris
            for (int j = 0, k = 0; j < (latSegments - 1) * longSegments; j++, k++)   //j für die Array-Indices und k ist ein Punkt in den aktuellen Dreiecken
            {
                if ((k + 1) % (longSegments + 1) == 0)
                {
                    k++;
                }

                tris[j * 6] = (ushort)(k);
                tris[j * 6 + 1] = (ushort)(k + (longSegments + 1) + 1);
                tris[j * 6 + 2] = (ushort)(k + 1);

                tris[j * 6 + 3] = (ushort)(k);
                tris[j * 6 + 4] = (ushort)(k + (longSegments + 1));
                tris[j * 6 + 5] = (ushort)(k + (longSegments + 1) + 1);

            }




            //Plane
            float rowHeight = height / (latSegments - 1);
            float columnWidth = width / longSegments;
            float startHeight = height / 2;
            float startWidth = -width / 2;

            //Initialisierung der verts und norms
            for (int i = 0; i < latSegments; i++)
            {
                for (int j = 0; j <= longSegments; j++)
                {
                    verts1[i * (longSegments + 1) + j] = new float3(-(startWidth + (j * columnWidth)), startHeight - (i * rowHeight), -distancePlane); //Add - to x and z value to mirror the image
                    norms1[i * (longSegments + 1) + j] = float3.UnitZ;
                }
            }

            return new Mesh
            {
                Vertices = verts,
                Vertices1 = verts1,
                Normals = norms,
                Normals1 = norms1,
                UVs = uvs,
                Triangles = tris,

            };
        }

        public static Mesh CreateGridPlane(int longSegments, int latSegments, float height, float width, float distancePlane)
        {
            float3[] verts1 = new float3[(longSegments + 1) * latSegments];
            float3[] norms1 = new float3[(longSegments + 1) * latSegments];

            //Plane
            float rowHeight = height / (latSegments - 1);
            float columnWidth = width / longSegments;
            float startHeight = height / 2f;
            float startWidth = -width / 2f;

            //Initialisierung der verts und norms
            for (int i = 0; i < latSegments; i++)
            {
                for (int j = 0; j <= longSegments; j++)
                {
                    //Vektor wird um 45° um den Koordinatenursprung gedreht
                    float x = M.Cos(M.Pi) * (startWidth + (j * columnWidth)) + M.Sin(M.Pi) * distancePlane;
                    float y = startHeight - (i * rowHeight);
                    float z = -M.Sin(M.Pi) * (startWidth + (j * columnWidth)) + M.Cos(M.Pi) * distancePlane;
                    verts1[i * (longSegments + 1) + j] = new float3(x, y, z); //Add - to x and z value to mirror the image
                    norms1[i * (longSegments + 1) + j] = float3.UnitZ;
                }
            }

            return new Mesh
            {
                Vertices1 = verts1,
                Normals1 = norms1,

            };
        }


    }
}