using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestGuideSceneReferences : MonoBehaviour
{
    [SerializeField] private List<QuestGuideNpcAnchor> npcAnchors = new List<QuestGuideNpcAnchor>();
    [SerializeField] private List<QuestGuidePortalAnchor> portalAnchors = new List<QuestGuidePortalAnchor>();
    [SerializeField] private Transform defaultPortalTarget;
    [Header("Arrow Override")]
    [SerializeField] private bool overrideArrowDistance;
    [SerializeField] private float arrowDistanceFromPlayer = 1.5f;

    public bool TryGetNpcAnchor(QuestGuideAnchorId anchorId, out Transform target)
    {
        foreach (QuestGuideNpcAnchor anchor in npcAnchors)
        {
            if (anchor.AnchorId != anchorId || anchor.Target == null)
                continue;

            target = anchor.Target;
            return true;
        }

        target = null;
        return false;
    }

    public bool TryGetPortalAnchor(string objectiveSceneName, out Transform target)
    {
        foreach (QuestGuidePortalAnchor anchor in portalAnchors)
        {
            if (!string.Equals(anchor.ObjectiveSceneName, objectiveSceneName, StringComparison.Ordinal) || anchor.Target == null)
                continue;

            target = anchor.Target;
            return true;
        }

        target = null;
        return false;
    }

    public bool TryGetDefaultPortalAnchor(out Transform target)
    {
        target = defaultPortalTarget;
        return target != null;
    }

    public float GetArrowDistance(float fallbackDistance)
    {
        return overrideArrowDistance ? arrowDistanceFromPlayer : fallbackDistance;
    }
}

[Serializable]
public struct QuestGuideNpcAnchor
{
    public QuestGuideAnchorId AnchorId;
    public Transform Target;
}

[Serializable]
public struct QuestGuidePortalAnchor
{
    public string ObjectiveSceneName;
    public Transform Target;
}
