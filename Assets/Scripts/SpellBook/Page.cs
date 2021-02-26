using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Page", menuName = "ScriptableObjects/PageScriptableObject", order = 1)]
public class Page : ScriptableObject
{
    public string spellName;

    public string element;

    [TextArea(2,5)]
    public string description;

    public Image imageHowTo;

}
