using System.Collections.Generic;
using UnityEngine;

public class URPWallFade : MonoBehaviour
{
    [System.Serializable]
    public class PlayerData
    {
        public Transform transform;
        public bool enabled = true;
    }

    [Header("Players")]
    public List<PlayerData> players = new List<PlayerData>();

    [Header("Wall Settings")]
    public LayerMask wallLayer;
    [Range(0.1f, 0.4f)] public float transparentAlpha = 0.25f;
    public float fadeSpeed = 6f;
    public float updateInterval = 0.05f;

    [Header("URP Properties")]
    public string colorProperty = "_BaseColor";
    public string surfaceProperty = "_Surface";

    private Dictionary<Renderer, WallState> wallStates = new Dictionary<Renderer, WallState>();
    private HashSet<Renderer> occludedWalls = new HashSet<Renderer>();
    private float timer;

    private class WallState
    {
        public Renderer renderer;
        public float currentAlpha = 1f;
        public float targetAlpha = 1f;
        public bool isTransparent = false;
        public Material originalMaterial;
    }

    void Start()
    {
        if (players.Count == 0)
        {
            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject obj in playerObjects)
            {
                players.Add(new PlayerData { transform = obj.transform, enabled = true });
            }
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < updateInterval) return;
        timer = 0f;

        DetectOccludingWalls();
        UpdateWallTransparency();
    }

    void DetectOccludingWalls()
    {
        occludedWalls.Clear();

        if (players.Count == 0 || Camera.main == null) return;

        Vector3 cameraPos = Camera.main.transform.position;

        foreach (PlayerData playerData in players)
        {
            if (playerData.transform == null || !playerData.enabled) continue;

            Vector3 playerPos = playerData.transform.position + Vector3.up * 1.5f;
            Vector3 direction = playerPos - cameraPos;
            float distance = direction.magnitude;

            RaycastHit[] hits = Physics.SphereCastAll(
                cameraPos,
                0.3f,
                direction.normalized,
                distance,
                wallLayer
            );

            foreach (RaycastHit hit in hits)
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();
                if (rend != null)
                {
                    occludedWalls.Add(rend);

                    if (!wallStates.ContainsKey(rend))
                    {
                        InitializeWall(rend);
                    }
                }
            }
        }
    }

    void InitializeWall(Renderer rend)
    {
        WallState state = new WallState
        {
            renderer = rend,
            currentAlpha = 1f,
            targetAlpha = 1f,
            originalMaterial = rend.sharedMaterial
        };

        wallStates[rend] = state;
    }

    void UpdateWallTransparency()
    {
        foreach (var kvp in wallStates)
        {
            bool shouldBeTransparent = occludedWalls.Contains(kvp.Key);
            kvp.Value.targetAlpha = shouldBeTransparent ? transparentAlpha : 1f;
        }

        foreach (var kvp in wallStates)
        {
            WallState state = kvp.Value;
            if (state.renderer == null) continue;

            // Smooth fade
            state.currentAlpha = Mathf.Lerp(
                state.currentAlpha,
                state.targetAlpha,
                Time.deltaTime * fadeSpeed
            );

            ApplyURPTransparency(state);
        }
    }

    void ApplyURPTransparency(WallState state)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        state.renderer.GetPropertyBlock(mpb);

        Color currentColor = Color.white;
        if (mpb.HasProperty(colorProperty))
        {
            currentColor = mpb.GetColor(colorProperty);
        }
        else if (state.renderer.sharedMaterial.HasProperty(colorProperty))
        {
            currentColor = state.renderer.sharedMaterial.GetColor(colorProperty);
        }

        currentColor.a = state.currentAlpha;

        if (mpb.HasProperty(colorProperty) || state.renderer.sharedMaterial.HasProperty(colorProperty))
        {
            mpb.SetColor(colorProperty, currentColor);
        }

        state.renderer.SetPropertyBlock(mpb);

        if (state.currentAlpha < 0.95f && !state.isTransparent)
        {
            EnableURPTransparency(state.renderer, true);
            state.isTransparent = true;
        }
        else if (state.currentAlpha >= 0.95f && state.isTransparent)
        {
            EnableURPTransparency(state.renderer, false);
            state.isTransparent = false;
        }
    }

    void EnableURPTransparency(Renderer rend, bool enable)
    {
        foreach (Material mat in rend.sharedMaterials)
        {
            if (mat == null) continue;

            if (enable)
            {
                if (mat.HasProperty(surfaceProperty))
                {
                    mat.SetFloat(surfaceProperty, 1f); 
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                }

                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);

                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                if (mat.HasProperty(surfaceProperty))
                {
                    mat.SetFloat(surfaceProperty, 0f); 
                    mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                }

                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);

                mat.renderQueue = -1;
            }
        }
    }

    void OnDestroy()
    {
        foreach (var kvp in wallStates)
        {
            if (kvp.Key != null)
            {
                EnableURPTransparency(kvp.Key, false);

                kvp.Key.SetPropertyBlock(null);
            }
        }
    }
}