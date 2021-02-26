using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public struct Pages
{
    public Page page;
}

public class SpellBook : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text element;
    public TMP_Text description;
    public Image imageHowTo;

    public Page currentPage;
    public int currentPageNumber;
    public Pages[] pages;

    // Start is called before the first frame update
    void Start()
    {
        currentPage = pages[0].page;
        currentPageNumber = 0;
        display();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            displayNextPage();
        }
    }
    void displayNextPage() {
        int length = pages.Length;

        if(currentPageNumber !=null)
        {
            if(currentPageNumber + 1 < length) {
                currentPage = pages[currentPageNumber + 1].page;
                currentPageNumber++;
            }else {
                currentPage = pages[0].page;
                currentPageNumber = 0;
            }
        
        } else
        {
            currentPage = pages[0].page;
            currentPageNumber = 0;
        }
        display();
    }

    void display() {
        title.text = currentPage.spellName;
        element.text = currentPage.element;
        description.text = currentPage.description;
        imageHowTo.sprite = currentPage.imageHowTo.sprite;
    }
}
