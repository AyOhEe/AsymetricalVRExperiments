using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SceneInfo : ScriptableObject
{
    public string SceneName;
    public int index;
    public List<GameObject> PlayerPrefabs;
}
