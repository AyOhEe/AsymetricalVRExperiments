using System;
using UnityEngine;
using MessagePack;

public class BaseNonVRPlayer : GamePlayer
{
    public NonVRPlayerController PlayerController;

    private void Awake()
    {
        PlayerController = GetComponent<NonVRPlayerController>();

        Invoke("SyncPlayer", 0.05f);
    }

    [MessagePackObject]
    public struct PlayerInfo
    {
        [Key(0)]
        public Vector3 p;
        [Key(1)]
        public Quaternion r;
        [Key(2)]
        public Quaternion l;

        public PlayerInfo(Vector3 _pos, Quaternion _rot, Quaternion _lookDir)
        {
            p = _pos;
            r = _rot;
            l = _lookDir;
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

        Invoke("SyncPlayer", 0.05f);
    }

    public override void HandleMessage(byte[] data)
    {
        PlayerInfo info = MessagePackSerializer.Deserialize<PlayerInfo>(data);

        transform.localPosition = info.p;
        transform.localRotation = info.r;
        PlayerController.playerShadesParent.transform.localRotation = info.l;
    }
}
