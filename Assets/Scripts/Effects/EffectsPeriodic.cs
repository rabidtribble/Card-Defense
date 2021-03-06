﻿using UnityEngine;
using System.Collections;
using Vexe.Runtime.Types;
using System;

//all effects in this file trigger when the object they are attached to is updated.  They can be applied to multiple types of entities

//reduces incoming damage by a fixed amount (but attacks always do at least 1 damage)
public class EffectRegeneration : IEffectPeriodic
{
    [Hide] public TargetingType targetingType { get { return TargetingType.noCast; } } //this effect should never be on a card, and thus should never be cast
    [Hide] public EffectType effectType { get { return EffectType.periodic; } }        //effect type
    [Show, Display(2)] public float strength { get; set; }                             //how much health is healed per second
    [Hide] public string argument { get; set; }                                        //effect argument (unused in this effect)

    [Hide] public string Name { get { return "Regeneration: " + strength; } } //returns name and strength

    [Show, Display(1)] public string XMLName { get { return "regeneration"; } } //name used to refer to this effect in XML

    [Hide] private float carryOver; //enemy health is an int, and rounding every frame will cause issues, so fractions are carried to the next frame

    public EffectRegeneration () { carryOver = 0; }  //default constructor inits carryover to 0

    public void UpdateEnemy(EnemyScript e, float deltaTime)
    {
        //skip if the enemy already expects to die, so we dont screw with targeting in all manner of broken ways
        if (e.expectedHealth <= 0)
            return;

        float healAmount = strength * deltaTime; //scale by time
        healAmount += carryOver; //   

        //prevent over heal
        int healthMissing = e.maxHealth - e.curHealth;
        if (healAmount > healthMissing)
            healAmount = healthMissing;

        //carry fractions to avoid rounding errors
        int healThisFrame = Mathf.FloorToInt(healAmount);
        carryOver = healAmount - healThisFrame;

        //heal
        e.curHealth += healThisFrame;
        e.expectedHealth += healThisFrame;
        LevelManagerScript.instance.WaveTotalRemainingHealth += healThisFrame;
    }
}