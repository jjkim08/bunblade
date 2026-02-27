using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ShopSystem : MonoBehaviour // basic shop purchasing system which displays and implements the shop upgrades
{
    [Header("UI References")]
    public TMP_Text goldText;
    public RectTransform arrow;

    [Header("Upgrade Costs")]
    public int attackUpgradeCost = 50;
    public int defenseUpgradeCost = 50;
    public int spellDamageUpgradeCost = 50;

    [Header("Upgrade Amounts")]
    public int attackIncreaseAmount = 5;
    public int defenseIncreaseAmount = 5;
    public int spellDamageIncreaseAmount = 5;

    private int currentSelection = 0;
    private List<string> shopOptions = new List<string>
    {
        
        "Buy Attack (+5)",
        "Buy Defense (+5)",
        "Buy Ability Power (+5)",
        "Next Battle"
    };


    private List<float> arrowXPositions = new List<float>
    {
        -5.22f,
        -1.23f,
        2.5f,
        7.5f
    };

    void Start()
    {
        UpdateUI();
        UpdateArrowPosition();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            currentSelection = MenuLeft();
            UpdateArrowPosition();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            currentSelection = MenuRight();
            UpdateArrowPosition();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            HandleChoice();
        }
    }

    private int MenuLeft() => currentSelection == 0 ? shopOptions.Count - 1 : currentSelection - 1;
    private int MenuRight() => currentSelection == shopOptions.Count - 1 ? 0 : currentSelection + 1;

    void UpdateArrowPosition()
    {
        if (arrow != null && currentSelection < arrowXPositions.Count)
        {
            Vector2 pos = arrow.anchoredPosition;
            pos.x = arrowXPositions[currentSelection];
            arrow.anchoredPosition = pos;
        }
    }

    void UpdateUI()
    {
        if (GameSession.gs != null)
        {
            
            goldText.text = $"Gold Remaining: {GameSession.gs.currentGold}";
        }
    }

    void HandleChoice()
    {
        switch (currentSelection)
        {
            case 0:
                BuyAttackUpgrade();
                break;
            case 1:
                BuyDefenseUpgrade();
                break;
            case 2:
                BuySpellDamageUpgrade();
                break;
            case 3:
                LeaveShop();
                break;
        }
    }

    void BuyAttackUpgrade()
    {
        if (GameSession.gs == null)
        {
            return;
        }

        if (GameSession.gs.SpendGold(attackUpgradeCost))
        {
            GameSession.gs.playerMember.currentAttackDamage += attackIncreaseAmount;
            UpdateUI();
        }
    }

    void BuyDefenseUpgrade()
    {
        if (GameSession.gs == null)
        {
            return;
        }

        if (GameSession.gs.SpendGold(defenseUpgradeCost))
        {
            GameSession.gs.playerMember.currentDefense += defenseIncreaseAmount;
            UpdateUI();
        }
    }

    void BuySpellDamageUpgrade()
    {
        if (GameSession.gs == null)
        {
            return;
        }

        if (GameSession.gs.SpendGold(spellDamageUpgradeCost))
        {
            GameSession.gs.playerMember.currentAbilityPower += spellDamageIncreaseAmount;
            UpdateUI();
        }
    }

    void LeaveShop()
    {
        if (GameSession.gs != null)
        {

            GameSession.gs.InitializeEnemy();
        }


        UnityEngine.SceneManagement.SceneManager.LoadScene("Turn Based");
    }
}
