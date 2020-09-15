using System;
using System.Diagnostics;
using System.Drawing;
using SDPixelFormat = System.Drawing.Imaging.PixelFormat;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using Fusee.Engine.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Fusee.Engine.Imp.Graphics.Desktop
{
    //RenderCanvasWindowImp isn't working with the current version of OpenTk 4.0 because we cannot bind a GraphicsContext to an already existing window in GLWF right now.
    //See: https://github.com/glfw/glfw/issues/25
    /*
    /// <summary>
    /// Use this class as a base class for implementing connectivity to whatever windows system you intend to support.
    /// Inherit from this class, make sure to call the constructor with the window handle to render on, implement the
    /// Run method and call the DoInit, DoUnload, DoRender and DoResize methods at appropriate incidences. Make sure
    /// that _width and _height are set to the new window size before calling DoResize.
    /// </summary>
    public abstract class RenderCanvasWindowImp : RenderCanvasImpBase, IRenderCanvasImp, IDisposable
    {
        #region Internal Fields

        //internal IWindowInfo _wi;
        internal IGraphicsContext _context;
        //internal GraphicsMode _mode;
        internal int _major, _minor;
        internal ContextFlags _flags;

        #endregion

        #region Fields
        /// <summary>
        /// Window handle for the window the engine renders to.
        /// </summary>
        public IWindowHandle WindowHandle { get; }

        /// <summary>
        /// Gets and sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public virtual int Width
        {
            get { return BaseWidth; }
            set { BaseWidth = value; }
        }

        /// <summary>
        /// Gets and sets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public virtual int Height
        {
            get { return BaseHeight; }
            set { BaseHeight = value; }
        }

        /// <summary>
        /// Gets and sets the left position.
        /// </summary>
        /// <value>
        /// The left position.
        /// </value>
        public virtual int Left
        {
            get { return BaseLeft; }
            set { BaseLeft = value; }
        }

        /// <summary>
        /// Gets and sets the top position.
        /// </summary>
        /// <value>
        /// The top position.
        /// </value>
        public virtual int Top
        {
            get { return BaseTop; }
            set { BaseTop = value; }
        }

        /// <summary>
        /// Gets and sets the caption(title of the window).
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        public string Caption { get; set; }

        private float _lastTimeTick;
        private float _deltaFrameTime;
        private static Stopwatch _watch;

        /// <summary>
        /// Gets the delta time.
        /// The delta time is the time that was required to render the last frame in milliseconds.
        /// This value can be used to determine the frames per second of the application.
        /// </summary>
        /// <value>
        /// The delta time in milliseconds.
        /// </value>
        public float DeltaTime
        {
            get
            {
                return _deltaFrameTime;
            }
        }

        /// <summary>
        /// Gets and sets a value indicating whether [vertical synchronize].
        /// This option is used to reduce "Glitches" during rendering.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [vertical synchronize]; otherwise, <c>false</c>.
        /// </value>
        public bool VerticalSync
        {
            get => _vSync;
            set
            {

                if (!value)
                    GLFW.SwapInterval(0);
                else
                    GLFW.SwapInterval(1);

                _vSync = value;
            }
        }
        private bool _vSync;

        /// <summary>
        /// Gets and sets a value indicating whether [enable blending].
        /// Blending is used to display transparent graphics.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable blending]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableBlending { get; set; }
        /// <summary>
        /// Gets and sets a value indicating whether [fullscreen] is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [fullscreen]; otherwise, <c>false</c>.
        /// </value>
        public bool Fullscreen { get; set; }

        /// <summary>
        /// Gets the timer.
        /// The timer value can be used to measure time that passed since the first call of this property.
        /// </summary>
        /// <value>
        /// The timer.
        /// </value>
        public static float Timer
        {
            get
            {
                if (_watch == null)
                {
                    _watch = new Stopwatch();
                    _watch.Start();
                }
                return ((float)_watch.ElapsedTicks) / ((float)Stopwatch.Frequency);
            }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderCanvasWindowImp" /> class.
        /// </summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        protected RenderCanvasWindowImp(IntPtr windowHandle, int width, int height)
        {
            _major = 1;
            _minor = 0;

            _flags = ContextFlags.Default;
            //_wi = Utilities.CreateWindowsWindowInfo(windowHandle);

            try
            {
                //_mode = new GraphicsMode(32, 24, 0, 8);
                _context = new GLFWGraphicsContext();
            }
            catch
            {
                //_mode = new GraphicsMode(32, 24, 0, 8);
                _context = new GLFWGraphicsContext();
            }

            _context.MakeCurrent();
            //((IGraphicsContextInternal)_context).LoadAll();

            GL.ClearColor(Color.MidnightBlue);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            // Use VSync!
            VerticalSync = true;
            _lastTimeTick = Timer;

            BaseWidth = width;
            BaseHeight = height;
        }
        #endregion

        #region Members

        /// <summary>
        /// Presents the rendered result of this instance. The rendering buffers are flushed and the delta time is recalculated.
        /// Call this function after rendering.
        /// </summary>
        public void Present()
        {
            // Recalculate time tick.
            float newTick = Timer;
            _deltaFrameTime = newTick - _lastTimeTick;
            _lastTimeTick = newTick;


            // _context.MakeCurrent(_wi);
            _context.SwapBuffers();

        }

        /// <summary>
        /// Set the cursor (the mouse pointer image) to one of the predefined types
        /// </summary>
        /// <param name="cursorType">The type of the cursor to set.</param>
        public abstract void SetCursor(CursorType cursorType);

        /// <summary>
        /// Opens the given URL in the user's standard web browser. The link MUST start with "http://" otherwise.
        /// </summary>
        /// <param name="link">The URL to open</param>
        public abstract void OpenLink(string link);


        /// <summary>
        /// Sets the size of the output window for desktop development.
        /// </summary>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        /// <param name="posx">The x position of the window.</param>
        /// <param name="posy">The y position of the window.</param>
        /// <param name="borderHidden">Show the window border or not.</param>
        public void SetWindowSize(int width, int height, int posx = 0, int posy = 0, bool borderHidden = false)
        {
            Width = width;
            Height = height;

            Left = (posx == -1) ? DisplayDevice.Default.Bounds.Width / 2 - width / 2 : posx;
            Top = (posy == -1) ? DisplayDevice.Default.Bounds.Height / 2 - height / 2 : posy;
            // TODO: border settings
        }

        /// <summary>
        /// Closes the GameWindow with a call to OpenTK.
        /// </summary>
        public void CloseGameWindow()
        {
            // TODO: implement something useful here.
        }

        /// <summary>
        /// Runs this application instance.
        /// </summary>
        public abstract void Run();

        private bool _disposed = false;

        //Implement IDisposable.
        /// <summary>
        /// Releases this instance for garbage collection. Do not call this method in frequent updates because of performance reasons.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }

                //Free your own state(unmanaged objects).
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }

                //if (_wi != null)
                //{
                //    _wi.Dispose();
                //    _wi = null;
                //}

                _disposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="RenderCanvasWindowImp"/> class.
        /// </summary>
        ~RenderCanvasWindowImp()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }
        #endregion
    }
    */

    /// <summary>
    /// This is a default render canvas implementation creating its own rendering window.
    /// </summary>
    public class RenderCanvasImp : RenderCanvasImpBase, IRenderCanvasImp
    {
        #region Fields

        //Some tryptichon related variables.
        private bool _videoWallMode = false;
        private int _videoWallMonitorsHor;
        private int _videoWallMonitorsVert;
        private bool _windowBorderHidden = false;

        /// <summary>
        /// Window handle for the window the engine renders to.
        /// </summary>
        public IWindowHandle WindowHandle { get; }

        /// <summary>
        /// Implementation Tasks: Gets and sets the width(pixel units) of the Canvas.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width
        {
            get { return BaseWidth; }
            set
            {
                _gameWindow.Size = new Vector2i(value, _gameWindow.Size.Y);
                BaseWidth = value;
                ResizeWindow();
            }
        }

        /// <summary>
        /// Gets and sets the height in pixel units.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height
        {
            get { return BaseHeight; }
            set
            {
                _gameWindow.Size = new Vector2i(_gameWindow.Size.X, value);
                BaseHeight = value;
                ResizeWindow();
            }
        }

        /// <summary>
        /// Gets and sets the caption(title of the window).
        /// </summary>
        /// <value>
        /// The caption.
        /// </value>
        public string Caption
        {
            get { return (_gameWindow == null) ? "" : _gameWindow.Title; }
            set { if (_gameWindow != null) _gameWindow.Title = value; }
        }

        /// <summary>
        /// Gets the delta time.
        /// The delta time is the time that was required to render the last frame in milliseconds.
        /// This value can be used to determine the frames per second of the application.
        /// </summary>
        /// <value>
        /// The delta time in milliseconds.
        /// </value>
        public float DeltaTime
        {
            get
            {
                if (_gameWindow != null)
                    return _gameWindow.DeltaTime;
                return 0.01f;
            }
        }

        /// <summary>
        /// Gets and sets a value indicating whether [vertical synchronize].
        /// This option is used to reduce "Glitches" during rendering.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [vertical synchronize]; otherwise, <c>false</c>.
        /// </value>
        public bool VerticalSync
        {
            get { return (_gameWindow != null) && (_gameWindow.VSync == VSyncMode.On || _gameWindow.VSync == VSyncMode.Adaptive); }
            set { if (_gameWindow != null) _gameWindow.VSync = VSyncMode.On; }
        }

        /// <summary>
        /// Gets and sets a value indicating whether [enable blending].
        /// Blending is used to render transparent objects.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable blending]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableBlending
        {
            get { return _gameWindow.Blending; }
            set { _gameWindow.Blending = value; }
        }

        /// <summary>
        /// Gets and sets a value indicating whether [fullscreen] is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [fullscreen]; otherwise, <c>false</c>.
        /// </value>
        public bool Fullscreen
        {
            get { return (_gameWindow.WindowState == WindowState.Fullscreen); }
            set { _gameWindow.WindowState = (value) ? WindowState.Fullscreen : WindowState.Normal; }
        }

        /// <summary>
        /// Gets a value indicating whether [focused].
        /// This property is used to identify if this application is the active window of the user.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [focused]; otherwise, <c>false</c>.
        /// </value>
        //public bool Focused
        //{
        //    get { return _gameWindow.Focused; }
        //}

        // Some tryptichon related Fields.

        /// <summary>
        /// Activates (true) or deactivates (false) the video wall feature.
        /// </summary>
        public bool VideoWallMode
        {
            get { return _videoWallMode; }
            set { _videoWallMode = value; }
        }

        /// <summary>
        /// This represents the number of the monitors in a vertical column.
        /// </summary>
        public int TryptMonitorSetupVertical
        {
            get { return _videoWallMonitorsVert; }
            set { _videoWallMonitorsVert = value; }
        }

        /// <summary>
        /// This represents the number of the monitors in a horizontal row.
        /// </summary>
        public int TryptMonitorSetupHorizontal
        {
            get { return _videoWallMonitorsHor; }
            set { _videoWallMonitorsHor = value; }
        }

        internal RenderCanvasGameWindow _gameWindow;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderCanvasImp"/> class.
        /// </summary>
        public RenderCanvasImp() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderCanvasImp"/> class.
        /// </summary>
        /// <param name="appIcon">The icon for the render window.</param>
        public RenderCanvasImp(Icon appIcon)
        {
            try
            {
                _gameWindow = new RenderCanvasGameWindow(this, false);
            }
            catch
            {
                _gameWindow = new RenderCanvasGameWindow(this, false);
            }
            if (appIcon != null)
                _gameWindow.Icon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(appIcon.Width, appIcon.Height, SwapColors(appIcon.ToBitmap(), ChangeColors.SwapBlueAndRed)));

            WindowHandle = new WindowHandle()
            {
                Handle = _gameWindow.Handle
            };

        }

        #region Swap Icon colors
        //ToDo OpenTk4.0 - is there a simpler way to crate the Icon? Do we want to have the swap methods generalized and move them somewhere else?
        private enum ChangeColors
        {
            ShiftRight,
            ShiftLeft,
            SwapBlueAndRed,
            SwapBlueAndGreen,
            SwapRedAndGreen,
            RedOnly,
            GreenOnly,
            BlueOnly,
            AlphaOnly,
            None
        }

        private static Bitmap ToBitmap(byte[] pxData, int width, int height, SDPixelFormat pxFormat)
        {
            Bitmap resultBitmap = new Bitmap(width, height, pxFormat);
            BitmapData resultData = resultBitmap.LockBits(new System.Drawing.Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly, SDPixelFormat.Format32bppArgb);

            Marshal.Copy(pxData, 0, resultData.Scan0, pxData.Length);
            resultBitmap.UnlockBits(resultData);

            return resultBitmap;
        }

        //Will only work for ARGB pixel formats...
        private static byte[] SwapColors(Bitmap originalImage, ChangeColors swapType)
        {
            BitmapData sourceData = originalImage.LockBits(new System.Drawing.Rectangle(0, 0, originalImage.Width, originalImage.Height), ImageLockMode.ReadOnly, originalImage.PixelFormat);

            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];
            Marshal.Copy(sourceData.Scan0, resultBuffer, 0, resultBuffer.Length);
            originalImage.UnlockBits(sourceData);

            byte resultBlue, resultGreen, resultRed, resultAlpha;

            for (int k = 0; k < resultBuffer.Length; k += 4)
            {
                byte sourceBlue = resultBuffer[k];
                byte sourceGreen = resultBuffer[k + 1];
                byte sourceRed = resultBuffer[k + 2];
                byte sourceAlpha = resultBuffer[k + 3];

                switch (swapType)
                {
                    default:
                    case ChangeColors.None:
                        {
                            resultBlue = sourceBlue;
                            resultGreen = sourceGreen;
                            resultRed = sourceRed;
                            resultAlpha = sourceAlpha;
                            break;
                        }
                    case ChangeColors.RedOnly:
                        {
                            resultBlue = 0;
                            resultGreen = 0;
                            resultRed = sourceRed;
                            resultAlpha = sourceAlpha;
                            break;
                        }
                    case ChangeColors.GreenOnly:
                        {
                            resultBlue = 0;
                            resultGreen = sourceGreen;
                            resultRed = 0;
                            resultAlpha = sourceAlpha;
                            break;
                        }
                    case ChangeColors.BlueOnly:
                        {
                            resultBlue = sourceBlue;
                            resultGreen = 0;
                            resultRed = 0;
                            resultAlpha = sourceAlpha;
                            break;
                        }
                    case ChangeColors.AlphaOnly:
                        {
                            resultBlue = 0;
                            resultGreen = 0;
                            resultRed = 0;
                            resultAlpha = sourceAlpha;
                            break;
                        }
                    case ChangeColors.ShiftRight:
                        {
                            resultBlue = sourceGreen;
                            resultRed = sourceBlue;
                            resultGreen = sourceRed;
                            resultAlpha = sourceAlpha;

                            break;
                        }
                    case ChangeColors.ShiftLeft:
                        {
                            resultBlue = sourceRed;
                            resultRed = sourceGreen;
                            resultGreen = sourceBlue;
                            resultAlpha = sourceAlpha;

                            break;
                        }
                    case ChangeColors.SwapBlueAndRed:
                        {
                            resultBlue = sourceRed;
                            resultGreen = sourceGreen;
                            resultRed = sourceBlue;
                            resultAlpha = sourceAlpha;

                            break;
                        }
                    case ChangeColors.SwapBlueAndGreen:
                        {
                            resultRed = sourceRed;
                            resultBlue = sourceGreen;
                            resultGreen = sourceBlue;
                            resultAlpha = sourceAlpha;

                            break;
                        }
                    case ChangeColors.SwapRedAndGreen:
                        {
                            resultRed = sourceGreen;
                            resultGreen = sourceGreen;
                            resultBlue = sourceBlue;
                            resultAlpha = sourceAlpha;

                            break;
                        }
                }

                resultBuffer[k] = resultBlue;
                resultBuffer[k + 1] = resultGreen;
                resultBuffer[k + 2] = resultRed;
                resultBuffer[k + 3] = resultAlpha;
            }

            return resultBuffer;
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderCanvasImp"/> class.
        /// </summary>
        /// <param name="width">The width of the render window.</param>
        /// <param name="height">The height of the render window.</param>
        /// <remarks>The window created by this constructor is not visible. Should only be used for internal testing.</remarks>
        public RenderCanvasImp(int width, int height)
        {
            try
            {
                _gameWindow = new RenderCanvasGameWindow(this, width, height, true);
            }
            catch
            {
                _gameWindow = new RenderCanvasGameWindow(this, width, height, false);
            }
            _gameWindow.Size = new Vector2i(0, 0);
        }

        /// <summary>
        /// Implementation of the Dispose pattern. Disposes of the OpenTK game window.
        /// </summary>
        public void Dispose()
        {
            _gameWindow.Dispose();
        }

        #endregion

        #region Members

        private void ResizeWindow()
        {
            if (!_videoWallMode)
            {
                _gameWindow.WindowBorder = _windowBorderHidden ? WindowBorder.Hidden : WindowBorder.Resizable;
                _gameWindow.Bounds = new Box2i(BaseLeft, BaseTop - BaseHeight, BaseLeft + BaseWidth, BaseTop);
            }
            else
            {

                throw new System.NotImplementedException("Resize window is not implemented for video wall.");
                //var oneScreenWidth = DisplayDevice.Default.Bounds.Width + 16; // TODO: Fix this. This +16 is strange behavior. Border should not make an impact to the width.
                //var oneScreenHeight = DisplayDevice.Default.Bounds.Height;

                //var width = oneScreenWidth * _videoWallMonitorsHor;
                //var height = oneScreenHeight * _videoWallMonitorsVert;

                //_gameWindow.Bounds = new System.Drawing.Rectangle(0, 0, width, height);

                //if (_windowBorderHidden)
                //    _gameWindow.WindowBorder = WindowBorder.Hidden;
            }
        }

        /// <summary>
        /// Changes the window of the application to video wall mode.
        /// </summary>
        /// <param name="monitorsHor">Number of monitors on horizontal axis.</param>
        /// <param name="monitorsVert">Number of monitors on vertical axis.</param>
        /// <param name="activate">Start the window in activated state-</param>
        /// <param name="borderHidden">Start the window with a hidden windows border.</param>
        public void VideoWall(int monitorsHor = 1, int monitorsVert = 1, bool activate = true, bool borderHidden = false)
        {
            VideoWallMode = activate;
            _videoWallMonitorsHor = monitorsHor > 0 ? monitorsHor : 1;
            _videoWallMonitorsVert = monitorsVert > 0 ? monitorsVert : 1;
            _windowBorderHidden = borderHidden;

            ResizeWindow();
        }

        /// <summary>
        /// Sets the size of the output window for desktop development.
        /// </summary>
        /// <param name="width">The width of the window.</param>
        /// <param name="height">The height of the window.</param>
        /// <param name="posx">The x position of the window.</param>
        /// <param name="posy">The y position of the window.</param>
        /// <param name="borderHidden">Show the window border or not.</param>
        public void SetWindowSize(int width, int height, int posx = -1, int posy = -1, bool borderHidden = false)
        {
            BaseWidth = width;
            BaseHeight = height;

            //BaseLeft = (posx == -1) ? (int)_gameWindow.DisplayDeviceResolution.X / 2 - width / 2 : posx;
            //BaseTop = (posy == -1) ? (int)_gameWindow.DisplayDeviceResolution.Y / 2 - height / 2 : posy;

            BaseLeft = (posx == -1) ? 0 : posx;
            BaseTop = (posy == -1) ? 0 : posy;

            _windowBorderHidden = borderHidden;

            // Disable video wall mode for this because it would not make sense.
            _videoWallMode = false;

            ResizeWindow();
        }

        /// <summary>
        /// Closes the GameWindow with a call to OpenTk.
        /// </summary>
        public void CloseGameWindow()
        {
            if (_gameWindow != null)
            {
                _gameWindow.Close();
                _gameWindow.Dispose();
            }
        }

        /// <summary>
        /// Presents this application instance. Call this function after rendering to show the final image. 
        /// After Present is called the render buffers get flushed.
        /// </summary>
        public void Present()
        {
            if (_gameWindow != null)
                _gameWindow.SwapBuffers();
        }

        /// <summary>
        /// Set the cursor (the mouse pointer image) to one of the predefined types
        /// </summary>
        /// <param name="cursorType">The type of the cursor to set.</param>
        public void SetCursor(CursorType cursorType)
        {
            // Currently not supported by OpenTK... Too bad.
        }

        /// <summary>
        /// Opens the given URL in the user's standard web browser. The link MUST start with "http://".
        /// </summary>
        /// <param name="link">The URL to open</param>
        public void OpenLink(string link)
        {
            if (link.StartsWith("http://"))
            {
                //UseShellExecute needs to be set to true in .net 3.0. See:https://github.com/dotnet/corefx/issues/33714
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = link,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
        }

        /// <summary>
        /// Implementation Tasks: Runs this application instance. This function should not be called more than once as its only for initialization purposes.
        /// </summary>
        public void Run()
        {
            if (_gameWindow != null)
                _gameWindow.Run();
        }

        /// <summary>
        /// Creates a bitmap image from the current frame of the application.
        /// </summary>
        /// <param name="width">The width of the window, and therefore image to render.</param>
        /// <param name="height">The height of the window, and therefore image to render.</param>
        /// <returns></returns>
        public Bitmap ShootCurrentFrame(int width, int height)
        {
            this.DoInit();
            this.DoRender();
            this.DoResize(width, height);

            var bmp = new Bitmap(this.Width, this.Height, SDPixelFormat.Format32bppArgb);
            var mem = bmp.LockBits(new System.Drawing.Rectangle(0, 0, Width, Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, SDPixelFormat.Format32bppArgb);
            GL.PixelStore(PixelStoreParameter.PackRowLength, mem.Stride / 4);
            GL.ReadPixels(0, 0, Width, Height, PixelFormat.Bgra, PixelType.UnsignedByte, mem.Scan0);
            bmp.UnlockBits(mem);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }

        #endregion
    }

    /// <summary>
    /// OpenTK implementation of RenderCanvas for the window output.
    /// </summary>
    public class RenderCanvasImpBase
    {
        #region Fields

        /// <summary>
        /// The Width
        /// </summary>
        protected internal int BaseWidth;

        /// <summary>
        /// The Height
        /// </summary>
        protected internal int BaseHeight;

        /// <summary>
        /// The Top Position
        /// </summary>
        protected internal int BaseTop;

        /// <summary>
        /// The Left Position
        /// </summary>
        protected internal int BaseLeft;

        #endregion

        #region Events
        /// <summary>
        /// Occurs when [initialize].
        /// </summary>
        public event EventHandler<InitEventArgs> Init;
        /// <summary>
        /// Occurs when [unload].
        /// </summary>
        public event EventHandler<InitEventArgs> UnLoad;
        /// <summary>
        /// Occurs when [render].
        /// </summary>
        public event EventHandler<RenderEventArgs> Render;
        /// <summary>
        /// Occurs when [resize].
        /// </summary>
        public event EventHandler<Common.ResizeEventArgs> Resize;

        #endregion

        #region Internal Members

        /// <summary>
        /// Does the initialize of this instance.
        /// </summary>
        internal protected void DoInit()
        {
            Init?.Invoke(this, new InitEventArgs());
        }

        /// <summary>
        /// Does the unload of this instance. 
        /// </summary>
        internal protected void DoUnLoad()
        {
            UnLoad?.Invoke(this, new InitEventArgs());
        }

        /// <summary>
        /// Does the render of this instance.
        /// </summary>
        internal protected void DoRender()
        {
            if (Render != null)
                Render(this, new RenderEventArgs());
        }

        /// <summary>
        /// Does the resize on this instance.
        /// </summary>
        internal protected void DoResize(int width, int height)
        {
            Resize?.Invoke(this, new Common.ResizeEventArgs(width, height));
        }

        #endregion
    }

    class RenderCanvasGameWindow : GameWindow
    {
        #region Fields

        private RenderCanvasImp _renderCanvasImp;

        /// <summary>
        /// Gets the delta time.
        /// The delta time is the time that was required to render the last frame in milliseconds.
        /// This value can be used to determine the frames per second of the application.
        /// </summary>
        /// <value>
        /// The delta time in milliseconds.
        /// </value>
        public float DeltaTime { get; private set; }

        /// <summary>
        /// Gets and sets a value indicating whether [blending].
        /// Blending is used to render transparent objects.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [blending]; otherwise, <c>false</c>.
        /// </value>
        public bool Blending
        {
            get { return GL.IsEnabled(EnableCap.Blend); }
            set
            {
                if (value)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                }
                else
                {
                    GL.Disable(EnableCap.Blend);
                }
            }
        }

        public IntPtr Handle
        {
            get
            {
                IntPtr hwnd;
                unsafe
                {
                    hwnd = GLFW.GetWin32Window(WindowPtr);
                }
                return hwnd;
            }
        }

        public Vector2 DisplayDeviceResolution
        {
            get
            {
                Vector2 res;
                unsafe
                {
                    var monitor = CurrentMonitor.ToUnsafePtr<OpenTK.Windowing.GraphicsLibraryFramework.Monitor>();
                    var videoMode = *GLFW.GetVideoMode(monitor);
                    res = new Vector2(videoMode.Width, videoMode.Height);
                }
                return res;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderCanvasGameWindow"/> class.
        /// </summary>
        /// <param name="renderCanvasImp">The render canvas implementation.</param>
        /// <param name="antiAliasing">if set to <c>true</c> [anti aliasing] is on.</param>
        public RenderCanvasGameWindow(RenderCanvasImp renderCanvasImp, bool antiAliasing)
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                Title = "Fusee Engine",
                APIVersion = new Version(4, 2),
                API = ContextAPI.OpenGL
            })
        {
            MakeCurrent(); //Needed with OpenTK 4.0 prev 9.2 and above. See https://github.com/opentk/opentk/issues/1118
            GL.LoadBindings(new GLFWBindingsContext());

            _renderCanvasImp = renderCanvasImp;

            Size = new Vector2i(((int)DisplayDeviceResolution.X) / 2, ((int)DisplayDeviceResolution.Y) / 2);

            _renderCanvasImp.BaseWidth = Size.X;
            _renderCanvasImp.BaseHeight = Size.Y;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderCanvasGameWindow"/> class.
        /// </summary>
        /// <param name="renderCanvasImp">The render canvas implementation.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="antiAliasing">if set to <c>true</c> [anti aliasing] is on.</param>
        public RenderCanvasGameWindow(RenderCanvasImp renderCanvasImp, int width, int height, bool antiAliasing)
            : base(GameWindowSettings.Default, new NativeWindowSettings()
            {
                Size = new Vector2i(width, height),
                Title = "Fusee Engine",
                APIVersion = new Version(4, 2),
                API = ContextAPI.OpenGL
            })
        {
            MakeCurrent(); //Needed with OpenTK 4.0 prev 9.2 and above. See https://github.com/opentk/opentk/issues/1118
            GL.LoadBindings(new GLFWBindingsContext());

            _renderCanvasImp = renderCanvasImp;
            _renderCanvasImp.BaseWidth = Size.X;
            _renderCanvasImp.BaseHeight = Size.Y;
        }

        #endregion

        #region Overrides

        protected override void OnLoad()
        {
            // Check for necessary capabilities
            if (APIVersion.Major < 2)
                throw new InvalidOperationException("You need at least OpenGL 2.0 to run this example. GLSL not supported.");

            GL.ClearColor(Color.MidnightBlue);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            // Use VSync!
            VSync = VSyncMode.On;

            _renderCanvasImp.DoInit();
        }

        protected override void OnUnload()
        {
            _renderCanvasImp.DoUnLoad();
            _renderCanvasImp.Dispose();
        }

        protected override void OnResize(OpenTK.Windowing.Common.ResizeEventArgs e)
        {
            if (_renderCanvasImp != null)
            {
                _renderCanvasImp.BaseWidth = e.Width;
                _renderCanvasImp.BaseHeight = e.Height;
                _renderCanvasImp.DoResize(e.Width, e.Height);
            }
            else
                throw new NullReferenceException("_randerCanvasImp is not initialized!");
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //TODO: OpenTK4.0 - esc will lead to NullReference on Input properties
            //if (KeyboardState[Key.Escape])
            //{
            //    Close();
            //    Dispose();
            //}

            if (KeyboardState[Key.F11])
                WindowState = (WindowState != WindowState.Fullscreen) ? WindowState.Fullscreen : WindowState.Normal;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (KeyboardState[Key.Escape])
            {
                Close();
                Dispose();
                return;
            }

            DeltaTime = (float)e.Time;

            if (_renderCanvasImp != null)
                _renderCanvasImp.DoRender();
        }

        #endregion
    }
}