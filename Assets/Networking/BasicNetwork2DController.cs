using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicNetwork2DController : MonoBehaviour
{
    public float playerSpeed;
    SyncedObject syncedObject;

    private void Start()
    {
        syncedObject = GetComponent<SyncedObject>();
        GetComponent<MeshRenderer>().material = new Material(GetComponent<MeshRenderer>().material);
        GetComponent<MeshRenderer>().material.SetColor("_Color", new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)255));
    }

    // Update is called once per frame
    void Update()
    {
        if (syncedObject.localOwned)
        {
            transform.Translate(new Vector3(
                ((Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0)) * playerSpeed * Time.deltaTime,
                0,
                ((Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0)) * playerSpeed * Time.deltaTime));
        }
    }
}
