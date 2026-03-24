using UnityEngine;

namespace Seahorse
{
    public class SeahorseDashCooldownIndicator : MonoBehaviour
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

        [Header("Availability")]
        [Tooltip("Override this on a scene instance to hide the dash cooldown indicator in that scene.")]
        [SerializeField] private bool indicatorEnabled = true;

        [Header("Target")]
        [SerializeField] private SeahorseDash targetDash;
        [SerializeField] private Transform followTarget;
        [SerializeField] private Renderer sortingReference;

        [Header("Placement")]
        [SerializeField] private float verticalPadding = 0.12f;
        [SerializeField] private float minimumRadius = 0.22f;
        [SerializeField] private float radiusScale = 0.33f;
        [SerializeField] private float thicknessRatio = 0.28f;
        [SerializeField] private int sortingOrderOffset = 8;

        [Header("Appearance")]
        [SerializeField] private int segments = 72;
        [SerializeField] private Color backgroundColor = new(1f, 1f, 1f, 0.18f);
        [SerializeField] private Color fillColor = new(0.26f, 0.92f, 1f, 0.95f);
        [SerializeField] private Color readyColor = new(0.94f, 1f, 0.84f, 1f);

        private SpriteRenderer[] targetSprites = System.Array.Empty<SpriteRenderer>();
        private SpriteRenderer primarySprite;
        private Material runtimeMaterial;
        private MaterialPropertyBlock backgroundBlock;
        private MaterialPropertyBlock fillBlock;
        private Mesh backgroundMesh;
        private Mesh fillMesh;
        private MeshRenderer backgroundRenderer;
        private MeshRenderer fillRenderer;
        private SeahorseDash cachedDash;
        private float lastRadius = -1f;
        private float lastThickness = -1f;
        private float lastProgress = -1f;
        private int lastSegmentCount = -1;

        private void Awake()
        {
            EnsureVisuals();
            ResolveTarget();
            RefreshIndicator(forceMeshRebuild: true);
        }

        private void LateUpdate()
        {
            ResolveTarget();
            RefreshIndicator(forceMeshRebuild: false);
        }

        private void OnDestroy()
        {
            if (backgroundMesh != null)
                Destroy(backgroundMesh);

            if (fillMesh != null)
                Destroy(fillMesh);

            if (runtimeMaterial != null)
                Destroy(runtimeMaterial);
        }

        private void OnDisable()
        {
            SetVisible(false);
        }

        private void ResolveTarget()
        {
            SeahorseDash resolvedDash = ResolveTargetDash();
            if (resolvedDash != targetDash)
                targetDash = resolvedDash;

            if (targetDash != cachedDash)
            {
                cachedDash = targetDash;
                followTarget = null;
                sortingReference = null;
                CacheTargetVisuals();
                lastRadius = -1f;
                lastThickness = -1f;
                lastProgress = -1f;
                lastSegmentCount = -1;
            }

            if (followTarget == null && targetDash != null)
                followTarget = targetDash.transform;

            if (sortingReference == null)
            {
                if (primarySprite != null)
                    sortingReference = primarySprite;
                else if (targetSprites.Length > 0)
                    sortingReference = targetSprites[0];
            }
        }

        private void CacheTargetVisuals()
        {
            if (targetDash == null)
            {
                targetSprites = System.Array.Empty<SpriteRenderer>();
                primarySprite = null;
                sortingReference = null;
                return;
            }

            targetSprites = targetDash.GetComponentsInChildren<SpriteRenderer>();
            SeahorseVisual seahorseVisual = targetDash.GetComponentInChildren<SeahorseVisual>();
            primarySprite = seahorseVisual != null ? seahorseVisual.GetComponent<SpriteRenderer>() : null;

            if (sortingReference == null)
            {
                if (primarySprite != null)
                    sortingReference = primarySprite;
                else if (targetSprites.Length > 0)
                    sortingReference = targetSprites[0];
            }
        }

        private SeahorseDash ResolveTargetDash()
        {
            if (IsUsableDash(targetDash))
                return targetDash;

            SeahorseDash scopedDash = FindDashInLocalScope();
            if (scopedDash != null)
                return scopedDash;

            SeahorseDash[] sceneDashes = FindObjectsByType<SeahorseDash>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            SeahorseDash sceneDash = SelectBestDash(sceneDashes);
            return sceneDash != null ? sceneDash : targetDash;
        }

        private SeahorseDash FindDashInLocalScope()
        {
            for (Transform scope = transform.parent; scope != null; scope = scope.parent)
            {
                SeahorseDash[] scopedDashes = scope.GetComponentsInChildren<SeahorseDash>(true);
                SeahorseDash bestDash = SelectBestDash(scopedDashes);
                if (bestDash != null)
                    return bestDash;
            }

            return null;
        }

        private SeahorseDash SelectBestDash(SeahorseDash[] candidates)
        {
            SeahorseDash bestDash = null;
            int bestScore = int.MinValue;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < candidates.Length; i++)
            {
                SeahorseDash candidate = candidates[i];
                if (candidate == null)
                    continue;

                int score = GetDashScore(candidate);
                float distance = (candidate.transform.position - transform.position).sqrMagnitude;
                if (score < bestScore)
                    continue;

                if (score == bestScore && distance >= bestDistance)
                    continue;

                bestDash = candidate;
                bestScore = score;
                bestDistance = distance;
            }

            return bestDash;
        }

        private static bool IsUsableDash(SeahorseDash dash)
        {
            return dash != null && dash.isActiveAndEnabled;
        }

        private static int GetDashScore(SeahorseDash dash)
        {
            if (dash == null)
                return int.MinValue;

            if (dash.isActiveAndEnabled)
                return 3;

            if (dash.gameObject.activeInHierarchy)
                return 2;

            if (dash.enabled)
                return 1;

            return 0;
        }

        private void EnsureVisuals()
        {
            if (backgroundRenderer != null && fillRenderer != null)
                return;

            Shader indicatorShader = Shader.Find("Sprites/Default");
            if (indicatorShader == null)
                indicatorShader = Shader.Find("Universal Render Pipeline/Unlit");

            if (indicatorShader == null)
            {
                Debug.LogWarning("Seahorse dash cooldown indicator could not find a compatible shader.", this);
                enabled = false;
                return;
            }

            runtimeMaterial = new Material(indicatorShader)
            {
                name = "SeahorseDashCooldownIndicator (Runtime)"
            };

            backgroundBlock = new MaterialPropertyBlock();
            fillBlock = new MaterialPropertyBlock();

            backgroundMesh = new Mesh { name = "CooldownIndicatorBackground" };
            backgroundMesh.MarkDynamic();
            fillMesh = new Mesh { name = "CooldownIndicatorFill" };
            fillMesh.MarkDynamic();

            CreateRingChild("CooldownBackground", backgroundMesh, out backgroundRenderer);
            CreateRingChild("CooldownFill", fillMesh, out fillRenderer);
        }

        private void CreateRingChild(string childName, Mesh mesh, out MeshRenderer meshRenderer)
        {
            GameObject child = new(childName);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;

            MeshFilter meshFilter = child.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            meshRenderer = child.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = runtimeMaterial;
        }

        private void RefreshIndicator(bool forceMeshRebuild)
        {
            if (backgroundRenderer == null || fillRenderer == null)
                return;

            if (!indicatorEnabled || targetDash == null || followTarget == null || !targetDash.DashEnabled)
            {
                SetVisible(false);
                return;
            }

            bool isCoolingDown = targetDash.CooldownRemaining > 0f;
            if (!isCoolingDown)
            {
                SetVisible(false);
                return;
            }

            Bounds targetBounds;
            if (!TryGetTargetBounds(out targetBounds))
                targetBounds = new Bounds(followTarget.position, Vector3.zero);

            float radius = Mathf.Max(minimumRadius, targetBounds.size.x * radiusScale);
            float thickness = Mathf.Clamp(radius * thicknessRatio, 0.04f, radius - 0.02f);

            transform.position = new Vector3(
                targetBounds.center.x,
                targetBounds.max.y + verticalPadding + radius,
                followTarget.position.z);

            UpdateSorting();

            int clampedSegments = Mathf.Max(12, segments);
            float progress = Mathf.Clamp01(targetDash.CooldownProgress01);
            bool rebuildBackground = forceMeshRebuild
                || !Mathf.Approximately(radius, lastRadius)
                || !Mathf.Approximately(thickness, lastThickness)
                || clampedSegments != lastSegmentCount;

            if (rebuildBackground)
                BuildRingMesh(backgroundMesh, radius, thickness, 1f, clampedSegments);

            if (rebuildBackground || Mathf.Abs(progress - lastProgress) > 0.001f)
                BuildRingMesh(fillMesh, radius, thickness, progress, clampedSegments);

            SetVisible(true);
            ApplyColor(backgroundRenderer, backgroundBlock, backgroundColor);
            ApplyColor(fillRenderer, fillBlock, progress >= 0.999f ? readyColor : fillColor);
            fillRenderer.enabled = progress > 0.001f;

            lastRadius = radius;
            lastThickness = thickness;
            lastProgress = progress;
            lastSegmentCount = clampedSegments;
        }

        private void UpdateSorting()
        {
            if (sortingReference == null)
                return;

            backgroundRenderer.sortingLayerID = sortingReference.sortingLayerID;
            fillRenderer.sortingLayerID = sortingReference.sortingLayerID;
            backgroundRenderer.sortingOrder = sortingReference.sortingOrder + sortingOrderOffset;
            fillRenderer.sortingOrder = sortingReference.sortingOrder + sortingOrderOffset + 1;
        }

        private void ApplyColor(Renderer renderer, MaterialPropertyBlock propertyBlock, Color color)
        {
            propertyBlock.Clear();
            propertyBlock.SetColor(ColorPropertyId, color);
            propertyBlock.SetColor(BaseColorPropertyId, color);
            renderer.SetPropertyBlock(propertyBlock);
        }

        private bool TryGetTargetBounds(out Bounds bounds)
        {
            if (primarySprite != null && primarySprite.enabled)
            {
                bounds = primarySprite.bounds;
                return true;
            }

            if (sortingReference is SpriteRenderer referenceSprite && referenceSprite.enabled)
            {
                bounds = referenceSprite.bounds;
                return true;
            }

            bool hasBounds = false;
            bounds = default;

            for (int i = 0; i < targetSprites.Length; i++)
            {
                SpriteRenderer sprite = targetSprites[i];
                if (sprite == null || !sprite.enabled)
                    continue;

                if (!hasBounds)
                {
                    bounds = sprite.bounds;
                    hasBounds = true;
                    continue;
                }

                bounds.Encapsulate(sprite.bounds);
            }

            if (hasBounds)
                return true;

            Collider2D targetCollider = targetDash != null ? targetDash.GetComponent<Collider2D>() : null;
            if (targetCollider != null)
            {
                bounds = targetCollider.bounds;
                return true;
            }

            if (followTarget != null)
            {
                bounds = new Bounds(followTarget.position, Vector3.zero);
                return true;
            }

            return false;
        }

        private void SetVisible(bool visible)
        {
            if (backgroundRenderer != null)
                backgroundRenderer.enabled = visible;

            if (fillRenderer != null)
                fillRenderer.enabled = visible && lastProgress > 0.001f;
        }

        private static void BuildRingMesh(Mesh mesh, float radius, float thickness, float fillAmount, int segmentCount)
        {
            mesh.Clear();

            fillAmount = Mathf.Clamp01(fillAmount);
            if (fillAmount <= 0f || radius <= 0f || thickness <= 0f)
                return;

            int arcSegments = Mathf.Max(1, Mathf.CeilToInt(segmentCount * fillAmount));
            float innerRadius = Mathf.Max(0.001f, radius - thickness);
            float startAngle = 90f;
            float endAngle = startAngle - (360f * fillAmount);

            Vector3[] vertices = new Vector3[(arcSegments + 1) * 2];
            Vector2[] uv = new Vector2[vertices.Length];
            int[] triangles = new int[arcSegments * 6];

            for (int i = 0; i <= arcSegments; i++)
            {
                float t = arcSegments == 0 ? 0f : i / (float)arcSegments;
                float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;
                Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
                int vertexIndex = i * 2;

                vertices[vertexIndex] = direction * radius;
                vertices[vertexIndex + 1] = direction * innerRadius;
                uv[vertexIndex] = new Vector2(t, 1f);
                uv[vertexIndex + 1] = new Vector2(t, 0f);

                if (i == arcSegments)
                    continue;

                int triangleIndex = i * 6;
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 2;
                triangles[triangleIndex + 2] = vertexIndex + 1;
                triangles[triangleIndex + 3] = vertexIndex + 1;
                triangles[triangleIndex + 4] = vertexIndex + 2;
                triangles[triangleIndex + 5] = vertexIndex + 3;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
    }
}
