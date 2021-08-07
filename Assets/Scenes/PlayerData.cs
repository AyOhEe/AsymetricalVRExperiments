using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct PlayerData
{
    public string PlayerName;
    public TeamSystem.Team Team;
    public GamePlayer Player;
    public InputMethod InputMethod;

    public void FillUIElement(PlayerDescriptionUI PlayerUI)
    {
        PlayerUI.PlayerName.text = PlayerName;
        PlayerUI.InputMethod.text = InputMethod == InputMethod.NonVR ? "Non-VR" : "VR";
    }
}
