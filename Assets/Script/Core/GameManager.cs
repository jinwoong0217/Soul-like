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

    protected override void OnInitialize()
    {
        base.OnInitialize();
        player = FindAnyObjectByType<Player>();
        virtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
    }
}
