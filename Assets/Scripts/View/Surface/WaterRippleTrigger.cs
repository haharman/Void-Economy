using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WaterRippleTrigger : MonoBehaviour
{
    private const int MaxRipples = 8;

    [SerializeField]
    [Tooltip("波紋を適用するSpriteRenderer(未設定ならこのGameObjectのRendererを使用)")]
    private SpriteRenderer targetRenderer;

    [SerializeField]
    [Tooltip("プレイヤーが接触したときに波紋を発生させるか")]
    private bool triggerOnPlayerContact = true;

    [SerializeField]
    [Tooltip("プレイヤーGameObject(自動検出されなかった場合に指定)")]
    private GameObject playerObject;

    private Material _material;
    private readonly Vector4[] _rippleData = new Vector4[MaxRipples];
    private int _writeIndex;
    private int _activeCount;
    private Bounds _spriteWorldBounds;

    private void OnEnable()
    {
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<SpriteRenderer>();
        }
        _material = targetRenderer.material;

        if (playerObject == null)
        {
            playerObject = GameObject.FindWithTag("Player");
            if (playerObject == null)
            {
                var playerByName = GameObject.Find("Player");
                if (playerByName != null)
                {
                    playerObject = playerByName;
                }
            }
        }
    }

    private void OnDisable()
    {
        if (_material != null)
        {
            _material.SetInt("_RippleCount", 0);
        }
    }

    private void Update()
    {
        if (!triggerOnPlayerContact || playerObject == null)
            return;

        _spriteWorldBounds = targetRenderer.bounds;
        if (_spriteWorldBounds.Contains(playerObject.transform.position))
        {
            TriggerRippleAtWorldPoint(playerObject.transform.position);
        }
    }

    public void TriggerRipple(Vector2 uv)
    {
        uv.x = Mathf.Clamp01(uv.x);
        uv.y = Mathf.Clamp01(uv.y);

        _rippleData[_writeIndex] = new Vector4(uv.x, uv.y, Time.time, 0f);
        _writeIndex = (_writeIndex + 1) % MaxRipples;
        _activeCount = Mathf.Min(_activeCount + 1, MaxRipples);

        _material.SetVectorArray("_RippleData", new System.Collections.Generic.List<Vector4>(_rippleData));
        _material.SetInt("_RippleCount", _activeCount);
    }

    public void TriggerRippleAtWorldPoint(Vector3 worldPoint)
    {
        Bounds bounds = targetRenderer.bounds;
        Vector3 localPoint = worldPoint - bounds.min;
        Vector2 uv = new Vector2(
            localPoint.x / bounds.size.x,
            localPoint.y / bounds.size.y
        );
        TriggerRipple(uv);
    }
}
