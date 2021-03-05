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

[RequireComponent(typeof(AudioSource))]
public class SpellBook : MonoBehaviour
{
    public TMP_Text title;
    public TMP_Text element;
    public TMP_Text description;
    public Image imageHowTo;

    public Page currentPage;
    public int currentPageNumber;
    public Pages[] pages;

    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentPage = pages[0].page;
        currentPageNumber = 0;
        display();
    }

    public void displayNextPage() {
        audioSource.Play();
        int length = pages.Length;

        if(currentPageNumber + 1 < length) {
            currentPage = pages[currentPageNumber + 1].page;
            currentPageNumber++;
        }else {
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
