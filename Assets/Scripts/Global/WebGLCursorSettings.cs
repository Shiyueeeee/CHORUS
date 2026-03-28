using UnityEngine;

[CreateAssetMenu(fileName = "WebGLCursorSettings", menuName = "CHORUS/WebGL Cursor Settings")]
public class WebGLCursorSettings : ScriptableObject
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;
    public int webCursorSize = 64;
}
