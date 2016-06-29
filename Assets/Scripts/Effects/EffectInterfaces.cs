using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

//represents an effect in XML
[System.Serializable]
public class XMLEffect : System.Object
{
    [XmlAttribute]
    public string name;
    [XmlAttribute]
    public float strength;
    [XmlAttribute]
    public string argument;
}

//represents everything needed to apply effects to an object
[System.Serializable]
public class EffectData : System.Object
{
    //list of effects from xml
    [XmlArray("Effects")]
    [XmlArrayItem("Effect")]
    public List<XMLEffect> XMLeffects = new List<XMLEffect>();
    [XmlIgnore]
    public bool effectsSpecified //hide effect list if it is empty
    {
        get { return XMLeffects.Count > 0; }
        set { }
    }

    [XmlIgnore]
    public List<IEffect> effects = new List<IEffect>(); //list of effect objects
    [XmlIgnore]
    public TargetingType targetingType
    {
        get
        {
            if (effects.Count == 0) parseEffects(); //make sure we have actual code references to the effects

            //return the target type of the first effect that requires a target.  no card can have effects that target two different types of things
            foreach (IEffect e in effects)
            {
                if (e.targetingType != TargetingType.none)
                    return e.targetingType;
            }
            return TargetingType.none; //if no effect needs a target, return none
        }
    }

    //translates the XMLeffects into code references
    public void parseEffects()
    {
        foreach (XMLEffect xe in XMLeffects)
        {
            IEffect ie = EffectTypeManagerScript.instance.parse(xe);
            if (ie != null)
                effects.Add(ie);
        }
    }

}

//All effects in the game must implement one of these interfaces.  
//Most will not use Effect directly, but instead a derivitave such as EffectInstant or EffectWave

//different targeting types.
public enum TargetingType
{
	none, 	//this effect does not require the player to select a target
	tower, 	//this effect requires the player to target a tower 
    noCast  //this effect does not support being cast (used for effects that are not meant to be applied to a card)
};

//different effect types
public enum EffectType {
	instant,        //effect triggers instantly without the need for a target
	wave,           //effect alters the current wave
    discard,        //effect triggers when the card it is attached to is discarded
    self,           //effect affects the card it is attached to (i.e.: to gain/lose charges when cast)
    enemyDamaged    //effect triggers when an enemy is damaged.  Could be attached to the attacking tower or the defending enemy
};

//base interface
public interface IEffect {
	
	string Name { get; } 				//user-friendly name of this effect
	TargetingType targetingType { get; }//specifies what this card must target when casting, if anything
	EffectType effectType { get; }		//specifies what kind of effect this is
	float strength { get; set; }		//specifies how strong the effect is.  not used in every effect.
    string argument { get; set; }       //specifies any other information the effect requires.  not used in every effect.

}

//effect triggers instantly without the need for a target
public interface IEffectInstant : IEffect {

	void trigger(); //called when this effect triggers

};

//effect alters the current wave
public interface IEffectWave : IEffect {

	WaveData alteredWaveData (WaveData currentWaveData); //alters the current wave data and returns the new values

};

//effect triggers when the card it is attached to is discarded
public interface IEffectDiscard : IEffect
{

    bool trigger(ref Card c); //called when this effect triggers.  returns true if the card no longer needs discarding

}

//effect affects the card it is attached to (i.e.: to gain/lose charges when cast)
public interface IEffectSelf : IEffect
{

    void trigger(ref Card card, GameObject card_gameObject); //called when this effect triggers.

}

//effect triggers when an enemy is damaged.  Could be attached to the attacking tower or the defending enemy
public interface IEffectEnemyDamaged : IEffect
{

    void expectedDamage(DamageEventData d); //called when it is expected to deal damage (so targeting etc. can account for damage amount changes)
    void actualDamage(DamageEventData d); //called when damage actually happens (for things that should happen when the actual hit occurs)

}

