using UnityEngine;

[CreateAssetMenu(fileName = "CrabTrashCleanupSettings", menuName = "CHORUS/Crab Trash Cleanup Settings")]
public class CrabTrashCleanupSettings : ScriptableObject
{
    [SerializeField] private Sprite[] frames;
    [Min(0.01f)]
    [SerializeField] private float framesPerSecond = 12f;
    [SerializeField] private Vector3 effectOffset = Vector3.zero;
    [SerializeField] private Vector3 effectScale = new Vector3(0.05f, 0.05f, 1f);
    [Tooltip("Multiplier applied to the cleanup crab animation scale. Increase this to make the effect larger.")]
    [Min(0.01f)]
    [SerializeField] private float cleanupAnimationScaleMultiplier = 2f;
    [SerializeField] private int sortingOrderOffset = 1;

    public Sprite[] Frames => frames;
    public float FramesPerSecond => framesPerSecond;
    public Vector3 EffectOffset => effectOffset;
    public Vector3 EffectScale => new Vector3(
        effectScale.x * cleanupAnimationScaleMultiplier,
        effectScale.y * cleanupAnimationScaleMultiplier,
        effectScale.z);
    public int SortingOrderOffset => sortingOrderOffset;
}
