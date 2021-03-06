﻿using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelSelectScript : BaseBehaviour
{
    public string       levelDir;         //directory levels are stored in
    public string       modLevelDir;      //directory mod levels are stored in
    public GameObject   buttonPrefab;     //prefab used to create buttons
    public GameObject   menuRoot;         //object to be destroyed when the menu is no longer needed

    //colors to be used on various types of buttons
    public Color        menuButtonColor;  //misc. menu buttons such as back, quit, etc.
    public Color        baseLevelColor;   //base game levels
    public Color        modLevelColor;    //modded game levels
    public Color        levelDeckColor;   //the level deck
    public Color        premadeDeckColor; //premade decks
    public Color        playerDeckColor;  //player decks

    //list of created menu buttons
    private List<GameObject> menuButtons;

    //temp storage for player menu selections
    private FileInfo chosenLevelFile;

    // Use this for initialization
    private void Start()
    {
        //the menu image is disabled to hide it in the editor, but we want it to be visible in game
        //so we turn it on again at runtime
        gameObject.GetComponent<UnityEngine.UI.Image>().enabled = true;

        //force menu image to be at least as tall as the UI canvas
        //we can't just use Screen.height because that is the height of the window itself and doesnt account for scaling
        //this is especially true of playing in the editor
        gameObject.GetComponent<UnityEngine.UI.LayoutElement>().minHeight = Screen.height / transform.root.gameObject.GetComponent<Canvas>().transform.localScale.y;

        //create an empty list to hold the buttons in
        menuButtons = new List<GameObject>();

        //start on a level select prompt
        StartCoroutine(setupLevelButtons());
    }

    //creates buttons to be used as a level select
    private IEnumerator setupLevelButtons()
    {
        //base game levels
        DirectoryInfo dir = new DirectoryInfo (Path.Combine (Application.dataPath, levelDir));  //find level folder
        FileInfo[] files = dir.GetFiles ("*.xml");                                              //get list of .xml files from it
        foreach (FileInfo f in files)                           //for each level file...
        {
            GameObject fButton = Instantiate(buttonPrefab);     //create a new button
            fButton.SendMessage("setLevel", f);                 //tell it what level it belongs to
            fButton.SendMessage("setColor", baseLevelColor);    //set the button color
            fButton.transform.SetParent(this.transform, false); //add it to the menu without altering scaling settings
            menuButtons.Add(fButton);                           //and add it to the list of buttons
        }

        //modded levels
        dir = new DirectoryInfo(Path.Combine(Application.dataPath, modLevelDir));   //find level folder
        files = dir.GetFiles("*.xml");                                              //get list of .xml files from it
        foreach (FileInfo f in files)                           //for each level file
        {
            GameObject fButton = Instantiate(buttonPrefab);     //create a new button
            fButton.SendMessage("setLevel", f);                 //tell it what level it belongs to
            fButton.SendMessage("setColor", modLevelColor);     //set the button color
            fButton.transform.SetParent(this.transform, false); //add it to the menu without altering scaling settings
            menuButtons.Add(fButton);                           //and add it to the list of buttons
        }

        //throw in a "quit" button also to exit the game with, if we are not in the editor or a web build (both of which ignore Application.Quit() anyway)
        if ((Application.isEditor == false) && (Application.isWebPlayer == false))
        {
            GameObject qButton = Instantiate(buttonPrefab);     //create a new button
            qButton.SendMessage("setButtonText", "Quit");       //set the text
            qButton.SendMessage("setColor", menuButtonColor);   //and the color
            qButton.transform.SetParent(this.transform, false); //and it to the menu
            menuButtons.Add(qButton);                           //and add it to the list of buttons
        }

        yield return null; //give the scrollRect a frame to catch up
        gameObject.transform.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1; //scroll the menu to the top after adding all these buttons
    }

    //creates buttons to be used as a deck select
    private IEnumerator setupDeckButtons()
    {
        //create a button for using the default level deck
        GameObject ldButton = Instantiate(buttonPrefab);             //create a new button
        ldButton.SendMessage("setButtonText", "Default Level Deck"); //set the text
        ldButton.SendMessage("setColor", levelDeckColor);            //and the color
        ldButton.transform.SetParent(this.transform, false);         //add it to the menu
        menuButtons.Add(ldButton);                                    //and add it to the list of buttons
        
        //buttons for all the player decks
        foreach (XMLDeck pd in DeckManagerScript.instance.playerDecks.decks)
        {
            GameObject pdButton = Instantiate(buttonPrefab);     //create a new button
            pdButton.SendMessage("setDeck", pd);                 //set the deck
            pdButton.SendMessage("setColor", playerDeckColor);   //and the color
            pdButton.transform.SetParent(this.transform, false); //and add it to the menu
            menuButtons.Add(pdButton);                           //and add it to the list of buttons
        }

        //buttons for all the premade decks
        foreach (XMLDeck pd in DeckManagerScript.instance.premadeDecks.decks)
        {
            GameObject pdButton = Instantiate(buttonPrefab);     //create a new button
            pdButton.SendMessage("setDeck", pd);                 //set the deck
            pdButton.SendMessage("setColor", premadeDeckColor);  //and the color
            pdButton.transform.SetParent(this.transform, false); //and add it to the menu
            menuButtons.Add(pdButton);                           //and add it to the list of buttons
        }

        //a button to open the editor...
        GameObject eButton = Instantiate(buttonPrefab);      //create a new button
        eButton.SendMessage("setButtonText", "Deck Editor"); //set the text
        eButton.SendMessage("setColor", menuButtonColor);    //and the color
        eButton.transform.SetParent(this.transform, false);  //and add it to the menufor returning to the level select
        menuButtons.Add(eButton);                            //and add it to the list of buttons

        //and a back button 
        GameObject backButton = Instantiate(buttonPrefab);     //create a new button
        backButton.SendMessage("setButtonText", "Back");       //set the text
        backButton.SendMessage("setColor", menuButtonColor);   //and the color
        backButton.transform.SetParent(this.transform, false); //and add it to the menufor returning to the level select
        menuButtons.Add(backButton);                           //and add it to the list of buttons

        yield return null; //give the scrollRect a frame to catch up
        gameObject.transform.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1; //scroll the menu to the top after adding all these buttons
    }

    //destroys all the menu buttons so a different menu can be shown
    private void destroyButtons()
    {
        foreach (GameObject button in menuButtons)
            Destroy(button);
        menuButtons.Clear();
    }

    //callback from level buttons
    private void LevelSelected(FileInfo levelFile)
    {
        chosenLevelFile = levelFile;        //save the chosen level
        destroyButtons();                   //get rid of the level menu
        StartCoroutine(setupDeckButtons()); //present the deck menu
    }

    //callback from deck buttons
    private void DeckSelected(XMLDeck deck)
    {
        DeckManagerScript.instance.SendMessage("SetDeck", deck); //send deck manager the chosen deck
        DeckManagerScript.instance.Shuffle(); //always shuffle the deck, regardless of what the level file says, if the deck did not come from the level file
        LevelManagerScript.instance.SendMessage("loadLevel", chosenLevelFile.FullName); //load the previously chosen level
        Destroy(menuRoot); //we are done with this menu.  Destroy it.
    }

    //callback from text buttons
    private void TextButtonSelected(string buttonText)
    {
        switch(buttonText)
        {
            case "Deck Editor":
                SceneManager.LoadScene("Deck Editor");
                break;
            case "Default Level Deck":
                //player wants to use the predefined deck for this level.  Load the level immediately and then let the level manager load the deck for us
                LevelManagerScript.instance.SendMessage("loadLevel", chosenLevelFile.FullName);
                Destroy(menuRoot); //we are done with this menu.  Destroy it.
                break;
            case "Quit":
                //player wants to quit.
                Application.Quit();
                break;
            case "Back":
                //player wants to go back to beginning
                chosenLevelFile = null;
                destroyButtons();
                StartCoroutine(setupLevelButtons());
                break;
            default:
                Debug.LogError("LevelSelectScript doesnt know how to handle this button!");
                break;
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}