using Windows.Foundation.Collections;

using EGLDisplay = System.IntPtr;
using EGLContext = System.IntPtr;
using EGLConfig = System.IntPtr;
using EGLSurface = System.IntPtr;
using System.Runtime.InteropServices;

namespace SkiaSharp.Views.GlesInterop;

class GlesContext : IDisposable
{
    public GlesContext()
    {
        eglConfig = Egl.EGL_NO_CONFIG;
        eglContext = Egl.EGL_NO_CONTEXT;
        eglSurface = Egl.EGL_NO_SURFACE;
        InitializeDisplay();
        Initialize();
    }

    public bool HasSurface
    {
        get
        {
            return eglSurface != Egl.EGL_NO_SURFACE;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            DestroySurface();
            Cleanup();
            isDisposed = true;
        }
    }

    ~GlesContext()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void CreateSurface(SwapChainPanel panel, Size? renderSurfaceSize, float? resolutionScale)
    {
        if (panel == null)
        {
            throw new ArgumentNullException("SwapChainPanel parameter is invalid");
        }
        IntPtr surface = Egl.EGL_NO_SURFACE;
        int[] surfaceAttributes = new int[]
        {
            12344
        };
        PropertySet surfaceCreationProperties = new PropertySet();
        surfaceCreationProperties.Add("EGLNativeWindowTypeProperty", panel);
        if (renderSurfaceSize != null)
        {
            surfaceCreationProperties.AddSize("EGLRenderSurfaceSizeProperty", renderSurfaceSize.Value);
        }
        if (resolutionScale != null)
        {
            surfaceCreationProperties.AddSingle("EGLRenderResolutionScaleProperty", resolutionScale.Value);
        }
        surface = Egl.eglCreateWindowSurface(eglDisplay, eglConfig, surfaceCreationProperties, surfaceAttributes);
        if (surface == Egl.EGL_NO_SURFACE)
        {
            throw new Exception("Failed to create EGL surface");
        }
        eglSurface = surface;
    }

    public void GetSurfaceDimensions(out int width, out int height)
    {
        Egl.eglQuerySurface(eglDisplay, eglSurface, 12375, out width);
        Egl.eglQuerySurface(eglDisplay, eglSurface, 12374, out height);
    }

    public void SetViewportSize(int width, int height)
    {
        Gles.glViewport(0, 0, width, height);
    }

    public void DestroySurface()
    {
        if (eglDisplay != Egl.EGL_NO_DISPLAY && eglSurface != Egl.EGL_NO_SURFACE)
        {
            Egl.eglDestroySurface(eglDisplay, eglSurface);
            eglSurface = Egl.EGL_NO_SURFACE;
        }
    }

    public void MakeCurrent()
    {
        if (Egl.eglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext) == 0)
        {
            throw new Exception("Failed to make EGLSurface current");
        }
    }

    public bool SwapBuffers()
    {
        return Egl.eglSwapBuffers(eglDisplay, eglSurface) == 1;
    }

    public void Reset()
    {
        Cleanup();
        Initialize();
    }

    private void InitializeDisplay()
    {
        if (eglDisplay != Egl.EGL_NO_DISPLAY)
        {
            return;
        }
        int[] defaultDisplayAttributes = new int[]
        {
            12803,
            12808,
            13220,
            13226,
            12815,
            1,
            12344
        };
        int[] fl9_3DisplayAttributes = new int[]
        {
            12803,
            12808,
            12804,
            9,
            12805,
            3,
            13220,
            13226,
            12815,
            1,
            12344
        };
        int[] warpDisplayAttributes = new int[]
        {
            12803,
            12808,
            12809,
            12811,
            13220,
            13226,
            12815,
            1,
            12344
        };
        IntPtr config = IntPtr.Zero;
        eglDisplay = Egl.eglGetPlatformDisplayEXT(12802U, Egl.EGL_DEFAULT_DISPLAY, defaultDisplayAttributes);
        if (eglDisplay == Egl.EGL_NO_DISPLAY)
        {
            throw new Exception("Failed to get EGL display");
        }
        int major;
        int minor;
        if (Egl.eglInitialize(eglDisplay, out major, out minor) == 0)
        {
            eglDisplay = Egl.eglGetPlatformDisplayEXT(12802U, Egl.EGL_DEFAULT_DISPLAY, fl9_3DisplayAttributes);
            if (eglDisplay == Egl.EGL_NO_DISPLAY)
            {
                throw new Exception("Failed to get EGL display");
            }
            if (Egl.eglInitialize(eglDisplay, out major, out minor) == 0)
            {
                eglDisplay = Egl.eglGetPlatformDisplayEXT(12802U, Egl.EGL_DEFAULT_DISPLAY, warpDisplayAttributes);
                if (eglDisplay == Egl.EGL_NO_DISPLAY)
                {
                    throw new Exception("Failed to get EGL display");
                }
                if (Egl.eglInitialize(eglDisplay, out major, out minor) == 0)
                {
                    throw new Exception("Failed to initialize EGL");
                }
            }
        }
    }

    public void Initialize()
    {
        int[] configAttributes = new int[]
        {
            12324,
            8,
            12323,
            8,
            12322,
            8,
            12321,
            8,
            12325,
            8,
            12326,
            8,
            12344
        };
        int[] contextAttributes = new int[]
        {
            12440,
            2,
            12344
        };
        IntPtr[] configs = new IntPtr[1];
        int numConfigs;
        if (Egl.eglChooseConfig(eglDisplay, configAttributes, configs, configs.Length, out numConfigs) == 0 || numConfigs == 0)
        {
            throw new Exception("Failed to choose first EGLConfig");
        }
        eglConfig = configs[0];
        eglContext = Egl.eglCreateContext(eglDisplay, eglConfig, Egl.EGL_NO_CONTEXT, contextAttributes);
        if (eglContext == Egl.EGL_NO_CONTEXT)
        {
            throw new Exception("Failed to create EGL context");
        }
    }

    private void Cleanup()
    {
        if (eglDisplay != Egl.EGL_NO_DISPLAY && eglContext != Egl.EGL_NO_CONTEXT)
        {
            Egl.eglDestroyContext(eglDisplay, eglContext);
            eglContext = Egl.EGL_NO_CONTEXT;
        }
    }

    private static IntPtr eglDisplay = Egl.EGL_NO_DISPLAY;

    private bool isDisposed;

    private IntPtr eglContext;

    private IntPtr eglSurface;

    private IntPtr eglConfig;
}
internal static class PropertySetExtensions
{
    private const string libInterop = "SkiaSharp.Views.Interop.UWP.dll";

    public static void AddSingle(this PropertySet properties, string key, float value)
    {
        PropertySet_AddSingle(properties, key, value);
    }

    public static void AddSize(this PropertySet properties, string key, Size size)
    {
        PropertySet_AddSize(properties, key, (float)size.Width, (float)size.Height);
    }

    public static void AddSize(this PropertySet properties, string key, float width, float height)
    {
        PropertySet_AddSize(properties, key, width, height);
    }

    [DllImport(libInterop)]
    private static extern void PropertySet_AddSingle(
        [MarshalAs(UnmanagedType.IInspectable)] object properties,
        [MarshalAs(UnmanagedType.HString)] string key,
        float value);

    [DllImport(libInterop)]
    private static extern void PropertySet_AddSize(
        [MarshalAs(UnmanagedType.IInspectable)] object properties,
        [MarshalAs(UnmanagedType.HString)] string key,
        float width, float height);
}