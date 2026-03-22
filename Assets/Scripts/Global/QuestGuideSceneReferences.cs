using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestGuideSceneReferences : MonoBehaviour
{
    [SerializeField] private List<QuestGuideNpcAnchor> npcAnchors = new List<QuestGuideNpcAnchor>();
    [SerializeField] private List<QuestGuidePortalAnchor> portalAnchors = new List<QuestGuidePortalAnchor>();

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
