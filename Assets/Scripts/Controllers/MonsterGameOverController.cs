using UnityEngine;
using System;

public class MonsterGameOverController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 3f;

    private Transform _player;
    private Action _onReachedPlayer;
    private bool _isMoving = false;

    public void StartMovingToPlayer(Transform player, Action onReachedPlayer)
    {
        _player = player;
        _onReachedPlayer = onReachedPlayer;
        _isMoving = true;
    }

    private void Update()
    {
        if (!_isMoving || _player == null) return;

        Vector3 direction = (_player.position - transform.position).normalized;
        transform.position += direction * _moveSpeed * Time.deltaTime;

        float distance = Vector3.Distance(transform.position, _player.position);
        if (distance <= 0.8f)
        {
            _isMoving = false;
            OnReachedPlayer();
        }
    }

    private void OnReachedPlayer()
    {
        // 🔥 TAM PLAYER'A GELDİĞİ AN 🔥
        _onReachedPlayer?.Invoke();
    }
}
