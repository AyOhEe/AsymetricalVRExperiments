using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SceneInfo : ScriptableObject
{
    public string SceneName;
    public List<GameObject> PlayerPrefabs;
}
