using System;
using System.Runtime.InteropServices;

public class ComputerScreen
{
    // Declare member variables here.
    public  static int Width => 128;
    public static int Height => 128;

    // private Image _image = new Image();
    // private ImageTexture _imagetexture = new ImageTexture();
    private byte[] _array = new byte[Width*Height];
    private bool _changed = false;
    // Called when the node enters the scene tree for the first time.
    public void _Ready()
    {
        // _array[0] = 1;
        // _array[128*128-1] = 1;
        SetPixel(0,0,1);
        SetPixel(127,127,1);
        Render();
        // IntPtr ptr = CreateIntPtrForArray(_array);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public void _Process(float delta)
    {
        Render();
    }

    // Imgui needs a pointer to the image data, array of argb/rgba probably.
    IntPtr CreateIntPtrForArray(byte[] array)
    {
        IntPtr pointer;
        GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        try
        {
            pointer = handle.AddrOfPinnedObject();
            return pointer;
        }
        finally
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }
    }

    public void SetPixel(int x, int y, byte on)
    {
        if (x < 0 || y < 0 || x > Width || x > Height)
            return;

        var index = x + Width * y;

        if (_array[index] == on)
            return;

        _array[index] = on;
        _changed = true;
    }

    public void Render()
    {
        if (!_changed)
            return;

        // _image.CreateFromData(Width, Height, false, Image.Format.R8, _array);
        // _imagetexture.CreateFromImage(_image, 0);
        // (Material as ShaderMaterial).SetShaderParam("my_array", _imagetexture);
        _changed = false;
    }
}
