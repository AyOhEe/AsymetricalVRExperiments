using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class BaseNonVRPlayer : GamePlayer
{
    public NonVRPlayerController PlayerController;

    private new void Awake()
    {
        base.Awake();
        PlayerController = GetComponent<NonVRPlayerController>();
    }

    [Serializable]
    private struct PlayerInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public Quaternion lookDir;

        public PlayerInfo(Vector3 _pos, Quaternion _rot, Quaternion _lookDir)
        {
            position = _pos;
            rotation = _rot;
            lookDir = _lookDir;
        }
    }

    public override void SyncPlayer()
    {
        string message = JsonUtility.ToJson(
            new PlayerInfo(
                transform.localPosition, 
                transform.localRotation, 
                PlayerController.playerShadesParent.transform.localRotation));
        SendSyncMessage(message);

        Invoke("SyncPlayer", 0.2f);
    }

    public override void HandleMessage(string data)
    {
        PlayerInfo info = JsonUtility.FromJson<PlayerInfo>(data);

        transform.localPosition = info.position;
        transform.localRotation = info.rotation;
        PlayerController.playerShadesParent.transform.localRotation = info.lookDir;
    }
}
