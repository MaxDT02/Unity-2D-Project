using UnityEngine;

// Hace que un enemigo pueda convertirse en moneda mientras dura el power-up.
// Como moneda: se oculta, deja de moverse y de hacer daño, y Jack lo recoge (puntos en cadena).
// Es un Collectible: la recogida, los puntos y el popup son los de las demás monedas.
// El componente se mantiene desactivado mientras es hostil, así no recibe contactos.
[RequireComponent(typeof(Enemy))]
public class EnemyCoin : Collectible
{
    private SpriteRenderer[] _enemySprites;
    private DamageSystem _damage;
    private EnemyMotion _motion;
    private Collider2D _body;
    private GameObject _coinVisual;
    private bool _isCoin;

    protected override void Awake()
    {
        base.Awake();
        _body = GetComponent<Collider2D>();
        _damage = GetComponent<DamageSystem>();
        _motion = GetComponent<EnemyMotion>();
        _enemySprites = GetComponentsInChildren<SpriteRenderer>(true);   // antes de crear el aspecto de moneda
        enabled = false;
    }

    // Si nace durante el power-up, nace ya como moneda.
    private void Start()
    {
        if (EnemyCoinSystem.CoinsActive) SetCoin(true);
    }

    public void SetCoin(bool on)
    {
        if (_isCoin == on || IsCollected) return;
        _isCoin = on;
        enabled = on;

        foreach (SpriteRenderer sprite in _enemySprites) sprite.enabled = !on;
        if (_damage != null) _damage.enabled = !on;
        if (_motion != null) _motion.enabled = !on;

        if (on)
        {
            ShowCoinVisual();
            SetCoinVisible(EnemyCoinSystem.CoinVisible);

            // Si Jack ya estaba encima, reactivar el collider genera de nuevo el contacto.
            _body.enabled = false;
            _body.enabled = true;
        }
        else if (_coinVisual != null)
        {
            _coinVisual.SetActive(false);
        }
    }

    // Parpadeo: lo dirige EnemyCoinSystem.
    public void SetCoinVisible(bool visible)
    {
        if (_isCoin && _coinVisual != null) _coinVisual.SetActive(visible);
    }

    private void ShowCoinVisual()
    {
        if (_coinVisual == null)
        {
            EnemyCoinSystem system = EnemyCoinSystem.Current;
            if (system == null || system.CoinVisualPrefab == null)
            {
                Debug.LogWarning("EnemyCoin: falta el aspecto de moneda en EnemyCoinSystem.", this);
                return;
            }
            _coinVisual = Instantiate(system.CoinVisualPrefab, transform);
        }

        _coinVisual.transform.position = _body.bounds.center;   // centrada, aunque el origen esté en los pies
        _coinVisual.SetActive(true);
    }

    protected override int GetPoints() =>
        PowerupController.Current != null ? PowerupController.Current.NextChainPoints() : 0;

    protected override bool CanCollect => _isCoin;

    protected override void OnCollected()
    {
        if (EnemyCoinSystem.Current != null) EnemyCoinSystem.Current.PlayCollectEffect(_body.bounds.center);
    }

    protected override void Finish()
    {
        gameObject.SetActive(false);   // sale del registro al instante
        Destroy(gameObject);
    }
}