﻿//based on tutorial found here: http://wiki.unity3d.com/index.php?title=Saving_and_Loading_Data:_XmlSerializer

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using Vexe.Runtime.Types;

//maintains the collection of card types, including saving/loading to XML
[XmlRoot("CardTypes")]
[System.Serializable]
public class CardTypeCollection
{
    //list of different card types
    [XmlArray("Cards")]
    [XmlArrayItem("Card")]
    public List<CardData> cardTypes = new List<CardData>();

    public void Save(string path)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(CardTypeCollection));

        using (StreamWriter stream = new StreamWriter(path, false, Encoding.GetEncoding("UTF-8")))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static CardTypeCollection Load(string path)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(CardTypeCollection));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return serializer.Deserialize(stream) as CardTypeCollection;
        }
    }
}

public class CardTypeManagerScript : BaseBehaviour
{
    //singleton instance
    public static CardTypeManagerScript instance;

    public string path;                     //path of base game cards
    public string modPath;                  //path of modded cards
    public CardTypeCollection types;		//contains all card types

    //set ALL THREE of these to true to save any debugger card data changes back to the XML
    public bool saveCardChanges;

    public bool reallySaveCardChanges;
    public bool reallyReallySaveCardChanges;

    // Use this for initialization
    private void Awake()
    {
        instance = this;

        //load base game cards
        types = CardTypeCollection.Load(Path.Combine(Application.dataPath, path));
        foreach (CardData baseCard in types.cardTypes)
            baseCard.isModded = false; //flag base game cards as being from the base game

        //integrate mod files
        CardTypeCollection modTypes;                                                                //temp storage of mod cards
        DirectoryInfo modDir =  new DirectoryInfo (Path.Combine (Application.dataPath, modPath));   //mod folder
        FileInfo[] modFiles = modDir.GetFiles ("*.xml");                                            //file list

        foreach (FileInfo f in modFiles)
        {
            modTypes = CardTypeCollection.Load(f.FullName); //load file
            foreach (CardData moddedCard in modTypes.cardTypes)
            {
                moddedCard.isModded = true; //flag it as from a mod

                //find the existing version of this card
                CardData existingCard = null;
                foreach (CardData baseCard in types.cardTypes)
                {
                    if (baseCard.cardName == moddedCard.cardName)
                    {
                        existingCard = baseCard;
                        break;
                    }
                }

                //replace the card if it exists already, and add it if it doesnt
                if (existingCard != null)
                {
                    types.cardTypes.Remove(existingCard);
                    types.cardTypes.Add(moddedCard);
                    Debug.Log("Overwriting card: " + existingCard.cardName);
                }
                else
                {
                    types.cardTypes.Add(moddedCard);
                }
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (saveCardChanges && reallySaveCardChanges && reallyReallySaveCardChanges)
        {
            types.Save(Path.Combine(Application.dataPath, path));
            saveCardChanges = false;
            reallySaveCardChanges = false;
            reallyReallySaveCardChanges = false;
            Debug.Log("Card changes saved.");
        }
    }

    //returns a random card type from the database
    public CardData getRandomCardType()
    {
        //get random index
        int index = Mathf.RoundToInt (Random.Range (0.0f, types.cardTypes.Count-1));

        //return card at that index
        return types.cardTypes[index];
    }

    //returns the card from the database with the given name
    public CardData getCardByName(string name)
    {
        foreach (CardData cd in types.cardTypes)
        {
            if (cd.cardName.Equals(name))
            {
                return cd;
            }
        }

        Debug.LogError("Could not find card type " + name + ".");
        return null;
    }
}