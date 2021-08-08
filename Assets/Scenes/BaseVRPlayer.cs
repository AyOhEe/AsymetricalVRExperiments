using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using MessagePack;

public class BaseVRPlayer : GamePlayer
{
    public VRController controller;

    public Transform Head;
    public Transform LeftHand;
    public Transform RightHand;

    public Transform CamrigHead;
    public Transform CamrigLeftHand;
    public Transform CamrigRightHand;

    public void Start()
    {
        //make ourselves a cool colour
        int hue = Random.Range(0, 767);
        Color32 color = new Color32(
            (byte)Mathf.Clamp(hue, 0, 255), 
            (byte)(Mathf.Clamp(hue, 256, 511) - 256), 
            (byte)(Mathf.Clamp(hue, 512, 767) - 256), 
            (byte)255);
        Material material = new Material(Head.GetComponent<MeshRenderer>().material);
        material.SetColor("_Color", color);

        Head.GetComponent<MeshRenderer>().material = material;
        LeftHand.GetComponent<MeshRenderer>().material = material;
        RightHand.GetComponent<MeshRenderer>().material = material;

        StartCoroutine(_start());
    }

    private IEnumerator _start()
    {
        controller = GetComponent<VRController>();


        //if the vr object is locally owned, force load openvr
        if (LocalOwned)
        {
            //enable vr
            XRSettings.enabled = true;
            XRSettings.LoadDeviceByName("OpenVR");

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Valve.VR.SteamVR.Initialize(true);
            controller.cameraRig.SetActive(true);
            
            Invoke("SyncPlayer", 0.1f);
        }

        yield return null;
    }

    [MessagePackObject]
    public struct PlayerInfo
    {
        [Key(0)]
        public Vector3 lP;
        [Key(1)]
        public Quaternion lR;
        [Key(2)]
        public Vector3 hP;
        [Key(3)]
        public Quaternion hR;
        [Key(4)]
        public Vector3 rP;
        [Key(5)]
        public Quaternion rR;

        public PlayerInfo(Transform left, Transform right, Transform head)
        {
            lP = left.localPosition;
            lR = left.localRotation;
            rP = right.localPosition;
            rR = right.localRotation;
            hP = head.localPosition;
            hR = head.localRotation;
        }
    }

    public override void SyncPlayer()
    {
        byte[] message = MessagePackSerializer.Serialize(new PlayerInfo(CamrigLeftHand, CamrigRightHand, CamrigHead));
        SendSyncMessage(message);
        
        LeftHand.localPosition = CamrigLeftHand.localPosition;
        LeftHand.localRotation = CamrigLeftHand.localRotation;
        RightHand.localPosition = CamrigRightHand.localPosition;
        RightHand.localRotation = CamrigRightHand.localRotation;
        Head.localPosition = CamrigHead.localPosition;
        Head.localRotation = CamrigHead.localRotation;

        Invoke("SyncPlayer", 0.05f);
    }

    public override void HandleMessage(byte[] data)
    {
        PlayerInfo info = MessagePackSerializer.Deserialize<PlayerInfo>(data);

        LeftHand.localPosition = info.lP;
        LeftHand.localRotation = info.lR;
        RightHand.localPosition = info.rP;
        RightHand.localRotation = info.rR;
        Head.localPosition = info.hP;
        Head.localRotation = info.hR;
    }
}
