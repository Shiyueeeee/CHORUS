using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class ClipboardImageUtility
{
    public static bool IsCopySupported
    {
        get
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return true;
#else
            return false;
#endif
        }
    }

    public static bool TryCopyTextureToClipboard(Texture2D texture)
    {
        if (texture == null) return false;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return WindowsClipboard.TryCopyTexture(texture);
#else
        Debug.LogWarning("ClipboardImageUtility: Copying images to clipboard is only implemented for Windows.");
        return false;
#endif
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private static class WindowsClipboard
    {
        private const uint CF_DIB = 8;
        private const uint GMEM_MOVEABLE = 0x0002;
        private const uint BI_RGB = 0;

        [StructLayout(LayoutKind.Sequential)]
        private struct BitmapInfoHeader
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr newOwner);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint format, IntPtr mem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint flags, UIntPtr bytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr mem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GlobalUnlock(IntPtr mem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalFree(IntPtr mem);

        public static bool TryCopyTexture(Texture2D texture)
        {
            if (texture == null) return false;

            try
            {
                byte[] dibBytes = CreateDib(texture);
                IntPtr globalHandle = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)dibBytes.Length);
                if (globalHandle == IntPtr.Zero) return false;

                IntPtr lockedHandle = IntPtr.Zero;

                try
                {
                    lockedHandle = GlobalLock(globalHandle);
                    if (lockedHandle == IntPtr.Zero) return false;

                    Marshal.Copy(dibBytes, 0, lockedHandle, dibBytes.Length);
                }
                finally
                {
                    if (lockedHandle != IntPtr.Zero) GlobalUnlock(globalHandle);
                }

                if (!OpenClipboard(IntPtr.Zero))
                {
                    GlobalFree(globalHandle);
                    return false;
                }

                try
                {
                    if (!EmptyClipboard())
                    {
                        GlobalFree(globalHandle);
                        return false;
                    }

                    IntPtr setResult = SetClipboardData(CF_DIB, globalHandle);
                    if (setResult == IntPtr.Zero)
                    {
                        GlobalFree(globalHandle);
                        return false;
                    }

                    // Ownership is transferred to the clipboard on success.
                    globalHandle = IntPtr.Zero;
                    return true;
                }
                finally
                {
                    CloseClipboard();
                    if (globalHandle != IntPtr.Zero) GlobalFree(globalHandle);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"ClipboardImageUtility: Failed to copy image to clipboard. {ex.Message}");
                return false;
            }
        }

        private static byte[] CreateDib(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;
            int rowStride = width * 4;
            int pixelBytesLength = rowStride * height;
            int headerLength = Marshal.SizeOf<BitmapInfoHeader>();

            byte[] bytes = new byte[headerLength + pixelBytesLength];

            BitmapInfoHeader header = new BitmapInfoHeader
            {
                biSize = (uint)headerLength,
                biWidth = width,
                biHeight = height,
                biPlanes = 1,
                biBitCount = 32,
                biCompression = BI_RGB,
                biSizeImage = (uint)pixelBytesLength,
                biXPelsPerMeter = 3780,
                biYPelsPerMeter = 3780,
                biClrUsed = 0,
                biClrImportant = 0
            };

            IntPtr headerPtr = Marshal.AllocHGlobal(headerLength);
            try
            {
                Marshal.StructureToPtr(header, headerPtr, false);
                Marshal.Copy(headerPtr, bytes, 0, headerLength);
            }
            finally
            {
                Marshal.FreeHGlobal(headerPtr);
            }

            Color32[] pixels = texture.GetPixels32();
            int destOffset = headerLength;

            // Unity pixels are already laid out bottom-up, which matches a positive-height DIB.
            for (int y = 0; y < height; y++)
            {
                int srcRowStart = y * width;
                int dstRowStart = destOffset + (y * rowStride);

                for (int x = 0; x < width; x++)
                {
                    Color32 pixel = pixels[srcRowStart + x];
                    int index = dstRowStart + (x * 4);
                    bytes[index] = pixel.b;
                    bytes[index + 1] = pixel.g;
                    bytes[index + 2] = pixel.r;
                    bytes[index + 3] = pixel.a;
                }
            }

            return bytes;
        }
    }
#endif
}
