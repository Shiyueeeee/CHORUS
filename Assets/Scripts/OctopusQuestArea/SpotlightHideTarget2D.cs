using UnityEngine;

public class SpotlightHideTarget2D : MonoBehaviour
{
    public bool IsHiddenFromSpotlight => hiddenZoneCount > 0;

    private int hiddenZoneCount;

    public void EnterHiddenZone()
    {
        hiddenZoneCount++;
    }

    public void ExitHiddenZone()
    {
        hiddenZoneCount = Mathf.Max(0, hiddenZoneCount - 1);
    }
}
