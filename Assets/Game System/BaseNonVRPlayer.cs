using System;
using UnityEngine;
using MessagePack;

public class BaseNonVRPlayer : GamePlayer
{
    public NonVRPlayerController PlayerController;

    private void Awake()
    {
        PlayerController = GetComponent<NonVRPlayerController>();

        Invoke("SyncPlayer", 0.2f);
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
        byte[] message = MessagePackSerializer.Serialize(
            new PlayerInfo(
                transform.localPosition, 
                transform.localRotation, 
                PlayerController.playerShadesParent.transform.localRotation));
        SendSyncMessage(message);

        Invoke("SyncPlayer", 0.2f);
    }

    public override void HandleMessage(byte[] data)
    {
        PlayerInfo info = MessagePackSerializer.Deserialize<PlayerInfo>(data);

        transform.localPosition = info.position;
        transform.localRotation = info.rotation;
        PlayerController.playerShadesParent.transform.localRotation = info.lookDir;
    }
}
