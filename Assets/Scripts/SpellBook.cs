using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellBook : MonoBehaviour
{
    public Text title;
    public Text description;

    public Image image;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void displayStoneSpell() {
        title.text = "Steinfall";
        description.text = "Du beschwörst einen großen Stein über deinem Gegner. Der Aufprall ist so mächtig, dass der Gegner für eine kurze Zeit nicht angreifen kann.";
        // element = "";
    }
}
