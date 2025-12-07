using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuActions : MonoBehaviour
{


    public PlayerAction playerAction;
    public RectTransform arrow;
    public event Action<List<string>> menuChanged;
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
    private List<string> mainMenu = new List<string> { "Attack", "Spells", "Items", "Run" };

    private List<string> spellMenu;
    private int currentID;

    void Awake()
    {
        playerAction.menuDisplay += menuDisplay;
    }

    void OnDestroy()
    {
        playerAction.menuDisplay -= menuDisplay;
    }

    void OnEnable()
    {
        currentMenuStack = new Stack<List<string>>();
        currentMenuStack.Push(mainMenu);

        menuChanged?.Invoke(mainMenu);
    }

    private int menuUp() => currentSelection == 0 ? currentMenuStack.Peek().Count - 1 : currentSelection - 1;
    private int menuDown() => currentSelection == currentMenuStack.Peek().Count - 1 ? 0 : currentSelection + 1;

    private void menuDisplay(int id, bool show) {
        currentID = id;

        // the new spell menu should be based on ID now

        spellMenu = GameSession.gs.partyMembers[id].playerStats.spellInfo.Keys.ToList();

        currentPlayer = GameSession.gs.partyMembers[id];

        gameObject.SetActive(show);
    }

    void handleChoice()
    {
        if (currentMenuStack.Count == 1)
        {
            if (currentSelection == 0)
            {
                playerActionFired?.Invoke("attack");
            }
            else if (currentSelection == 1)
            {
                currentMenuStack.Push(spellMenu); // goes to the spell menu
                currentSelection = 0;
            }
            else if (currentSelection == 2)
            {
                currentMenuStack.Push(itemMenu); // goes to the item menu next time
                currentSelection = 0;
            }
        }
        else
        {
            if (currentMenuStack.Peek().SequenceEqual(spellMenu) &&
            currentPlayer.playerStats.spellInfo[currentMenuStack.Peek()[currentSelection]].manaCost <= currentPlayer.currentMana)
            {
                playerActionFired?.Invoke(currentMenuStack.Peek()[currentSelection]);
            } else
            {
                print("Not enough mana!");
            }

            if (currentMenuStack.Peek().SequenceEqual(itemMenu))
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
