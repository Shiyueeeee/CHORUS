using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CrabRushManager : MonoBehaviour
{
    [SerializeField] private float rushDuration = 60f;
    [SerializeField] private String sceneName;
    [SerializeField] private bool startOnSceneLoad = true;

    private float timer;
    private bool finished = false;
    private bool isRunning;

    private void Awake()
    {
        timer = rushDuration;
    }

    private void Start()
    {
        if (startOnSceneLoad)
        {
            BeginRush();
        }
    }

    private void Update()
    {
        if (finished || !isRunning) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            finished = true;
            EndRush();
        }
    }

    private void EndRush()
    {
        PlayerEnterState.movementMode = PlayerMovementMode.Free;
        SceneTransition.LoadScene(sceneName);
    }

    public void BeginRush()
    {
        if (finished || isRunning)
        {
            return;
        }

        isRunning = true;
    }
}
