using UnityEngine;
using System.Collections;

//All effects in the game must implement one of these interfaces.  
//Most will not use Effect directly, but instead a derivitave such as EffectInstant or EffectWave

//different targeting types.
public enum TargetingType {
	none, 	//this effect does not require the player to select a target
	tower, 	//this effect requires the player to target a tower 
};

//different effect types
public enum EffectType {
	instant,
	wave,
    discard,
    self
};

//base interface
public interface IEffect {
	
	string Name { get; } 				//user-friendly name of this effect
	TargetingType targetingType { get; }//specifies what this card must target when casting, if anything
	EffectType effectType { get; }		//specifies what kind of effect this is
	float strength { get; set; }		//specifies how strong the effect is

}

//for instantaneous effects that happen once and then go away
public interface IEffectInstant : IEffect {

	void trigger(); //called when this effect triggers

};

//for effects that apply to enemy waves, altering their properties
public interface IEffectWave : IEffect {

	WaveData alteredWaveData (WaveData currentWaveData); //alters the current wave data and returns the new values

};

//for effects that apply on discard
public interface IEffectDiscard : IEffect
{

    bool trigger(ref Card c); //called when this effect triggers.  returns true if the card no longer needs discarding

}

//for effects that target the card itself
public interface IEffectSelf : IEffect
{

    void trigger(ref Card card, GameObject card_gameObject); //called when this effect triggers.

}

