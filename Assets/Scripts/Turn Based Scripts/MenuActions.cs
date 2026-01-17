using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuActions : MonoBehaviour
{


    public PlayerAction playerAction;
    public RectTransform arrow;
    public event Action<List<string>> menuChanged;
    public event Action<List<(string text, bool canAfford)>> spellMenuChanged; // New event for spell menu with affordability info
    public event Action<string> playerActionFired;
    private int currentSelection = 0;
    public Stack<List<string>> currentMenuStack = new Stack<List<string>>();

    public PlayerState currentPlayer;

    private List<string> itemMenu = new List<string>();

    private List<float> menuLocations = new List<float>{
        110.7f,
        35.7f,
        -40.7f,
        -115.7f
    };

    private List<string> mainMenu;
    private List<string> spellMenu;

    void Awake()
    {
        playerAction.menuDisplay += menuDisplay;
    }

    void OnDestroy()
    {
        playerAction.menuDisplay -= menuDisplay;
    }

    private int menuUp() => currentSelection == 0 ? currentMenuStack.Peek().Count - 1 : currentSelection - 1;
    private int menuDown() => currentSelection == currentMenuStack.Peek().Count - 1 ? 0 : currentSelection + 1;

    private void menuDisplay(bool show)
    {
        currentPlayer = GameSession.gs.playerMember;

        // Build spell menu
        spellMenu = currentPlayer.playerStats.spellInfo.Keys.ToList();

        // Build main menu dynamically based on available options
        mainMenu = new List<string> { "Attack" };

        if (spellMenu.Count > 0)
        {
            mainMenu.Add("Spells");
        }

        if (itemMenu.Count > 0)
        {
            mainMenu.Add("Items");
        }

        mainMenu.Add("Run");

        // Reset menu stack with new main menu
        currentMenuStack.Clear();
        currentMenuStack.Push(mainMenu);
        currentSelection = 0;
        menuChanged?.Invoke(mainMenu);

        gameObject.SetActive(show);
    }

    void handleChoice()
    {
        if (currentMenuStack.Count == 1)
        {
            string selectedOption = mainMenu[currentSelection];

            if (selectedOption == "Attack")
            {
                playerActionFired?.Invoke("attack");
            }
            else if (selectedOption == "Spells")
            {
                currentMenuStack.Push(spellMenu);
                currentSelection = 0;

                // Send spell menu with affordability info
                var spellsWithAffordability = spellMenu.Select(spell =>
                    (spell, currentPlayer.canCastSpell(spell))
                ).ToList();
                spellMenuChanged?.Invoke(spellsWithAffordability);
            }
            else if (selectedOption == "Items")
            {
                currentMenuStack.Push(itemMenu);
                currentSelection = 0;
            }
            // "Run" option can be handled here if needed
        }
        else
        {
            if (currentMenuStack.Peek().SequenceEqual(spellMenu))
            {
                string selectedSpell = currentMenuStack.Peek()[currentSelection];
                if (currentPlayer.canCastSpell(selectedSpell))
                {
                    playerActionFired?.Invoke(selectedSpell);
                }
                else
                {

                }
            }
            else if (currentMenuStack.Peek().SequenceEqual(itemMenu))
            {
                playerActionFired?.Invoke(currentMenuStack.Peek()[currentSelection]);
            }
        }
    }


    void Update()
    {
        UnityEngine.Vector2 pos = arrow.anchoredPosition;

        if (Input.GetKeyDown(KeyCode.W))
        {
            currentSelection = menuUp();
            arrow.anchoredPosition = new UnityEngine.Vector2(pos.x, menuLocations[currentSelection]);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            currentSelection = menuDown();
            arrow.anchoredPosition = new UnityEngine.Vector2(pos.x, menuLocations[currentSelection]);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            handleChoice();
            menuChanged?.Invoke(currentMenuStack.Peek());
            arrow.anchoredPosition = new UnityEngine.Vector2(pos.x, menuLocations[currentSelection]);
        }

        if (Input.GetKeyDown(KeyCode.K) && currentMenuStack.Count > 1)
        {
            currentSelection = 0;
            currentMenuStack.Pop();
            menuChanged?.Invoke(currentMenuStack.Peek());
            arrow.anchoredPosition = new UnityEngine.Vector2(pos.x, menuLocations[currentSelection]);
        }
    }
}
