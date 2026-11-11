using UnityEngine;

// Efecto que reproduce su animación una vez y se destruye. Sirve para chispas, destellos...
[RequireComponent(typeof(Animator))]
public class OneShotEffect : MonoBehaviour
{
    private Animator _animator;

    public static OneShotEffect Play(OneShotEffect prefab, Vector3 position) =>
        prefab == null ? null : Instantiate(prefab, position, Quaternion.identity);

    private void Awake() => _animator = GetComponent<Animator>();

    private void Update()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) Destroy(gameObject);
    }
}