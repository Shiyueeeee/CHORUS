using System;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Collider2D))]
public class FinalRushSceneTrigger : MonoBehaviour
{
    private Collider2D _collider;
    [SerializeField] private string rushSceneName;
    

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerEnterState.movementMode = PlayerMovementMode.VerticalOnly;
            SceneManager.LoadScene(rushSceneName);
        }
    }
}
