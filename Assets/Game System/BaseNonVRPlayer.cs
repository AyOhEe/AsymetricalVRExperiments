using System;
using UnityEngine;
using MessagePack;

public class BaseNonVRPlayer : GamePlayer
{
    public NonVRPlayerController PlayerController;

    private void Awake()
    {
        PlayerController = GetComponent<NonVRPlayerController>();

#if UNITY_EDITOR
        Debug.Log(String.Format("Player {0} Spawned in {1}", ClientID, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));
#endif

        Invoke("SyncPlayer", 0.05f);
    }

    [MessagePackObject]
    public struct PlayerInfo
    {
        [Key(0)]
        public Vector3 position;
        [Key(1)]
        public Quaternion rotation;
        [Key(2)]
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

        Invoke("SyncPlayer", 0.05f);
    }

    public override void HandleMessage(byte[] data)
    {
        PlayerInfo info = MessagePackSerializer.Deserialize<PlayerInfo>(data);

        transform.localPosition = info.position;
        transform.localRotation = info.rotation;
        PlayerController.playerShadesParent.transform.localRotation = info.lookDir;
    }

#if UNITY_EDITOR
    public void OnDestroy()
    {
        Debug.Log(String.Format("Player {0} Destroyed in {1}", ClientID, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name));
    }
#endif
}
