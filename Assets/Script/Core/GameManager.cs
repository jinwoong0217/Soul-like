using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    Player player;
    public Player Player
    {
        get
        {
            if(player == null)
            {
                OnInitialize();
            }
            return player;
        }
    }

    CinemachineVirtualCamera virtualCamera;
    public CinemachineVirtualCamera VirtualCamera
    {
        get
        {
            if(virtualCamera == null)
            {
                OnInitialize();
            }
            return virtualCamera;
        }
    }

    Enemy enemy;
    public Enemy Enemy
    {
        get
        {
            if(enemy == null)
            {
                OnInitialize();
            }
            return enemy;
        }
    }

    protected override void OnInitialize()
    {
        base.OnInitialize();
        player = FindAnyObjectByType<Player>();
        enemy = FindAnyObjectByType<Enemy>();
        virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
    }
}
