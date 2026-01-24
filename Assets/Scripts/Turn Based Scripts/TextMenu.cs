using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using Unity.VisualScripting;

public class TextMenu : MonoBehaviour
{
    public MenuActions menuActions;

    public TMP_Text menuText;

    void Awake()
    {
        menuActions.menuChanged += updateMenu;
    }

    void OnDisable()
    {
        menuActions.menuChanged -= updateMenu;
    }

    private void updateMenu(List<string> newMenu)
    {
        string displayText = "";

        for (int i = 0; i < newMenu.Count; i++)
        {
            displayText += newMenu[i] + "\n";
        }

        menuText.text = displayText;
    }
}
