using UnityEngine;
using TMPro;

public class PlayerStatusDisplay : MonoBehaviour // status effects for the player
{
    public TMP_Text statusText;

    private PlayerState currentPlayer;

    void Start()
    {
        if (GameSession.gs != null && GameSession.gs.playerMember != null)
        {
            currentPlayer = GameSession.gs.playerMember;
        }
    }


    void Update()
    {
        if (currentPlayer == null && GameSession.gs != null)
        {
            currentPlayer = GameSession.gs.playerMember;
        }

        UpdateStatusDisplay();
    }

    
    private void UpdateStatusDisplay()
    {
        if (currentPlayer == null || statusText == null)
        {
            if (statusText != null) statusText.text = "";
            return;
        }

        string statusDisplay = "";


        int fullIcons = currentPlayer.pushTurnHalves / 2;
        int halfIcons = currentPlayer.pushTurnHalves % 2;
        statusDisplay += $"Turn Icons: {fullIcons}";
        if (halfIcons > 0)
        {
            statusDisplay += ".5";
        }
        statusDisplay += "\n";


        statusDisplay += $"Mana: {currentPlayer.currentMana}\n";


        if (currentPlayer.currentBurnStacks > 0)
        {
            statusDisplay += $"Burn x{currentPlayer.currentBurnStacks}\n";
        }


        if (currentPlayer.currentSlowStacks > 0)
        {
            statusDisplay += $"Slow x{currentPlayer.currentSlowStacks}\n";
        }

        statusText.text = statusDisplay;
    }
}
