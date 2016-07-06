﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Vexe.Runtime.Types;

//all effects in this file trigger when an enemy is damaged.  The effect itself could be attached either to the attacking tower or the defending enemy

//targets enemy in range with the highest armor, breaking ties by proximity to goal
public class EffectTargetArmor : IEffectTowerTargeting
{
    //generic interface
    [Hide] public TargetingType targetingType { get { return TargetingType.noCast; } } //this effect should never be on a card, and thus should never be cast
    [Hide] public EffectType effectType { get { return EffectType.towerTargeting; } }  //effect type
    [Hide] public float strength { get; set; }                                         //effect strength (unused in this effect)
    [Hide] public string argument { get; set; }                                        //effect argument (unused in this effect)

    //this effect
    [Hide] public string Name { get { return "Target: highest armor"; } } //returns name and strength
    [Show] public string XMLName { get { return "tagetArmor"; } } //name used to refer to this effect in XML

    public List<GameObject> findTargets(Vector2 towerPosition, float towerRange)
    {
        List<GameObject> enemiesInRange = EnemyManagerScript.instance.enemiesInRange(towerPosition, towerRange); //get a list of all valid targets
        if ((enemiesInRange == null) || (enemiesInRange.Count == 0))
            return enemiesInRange;
        float highestArmor = -1;
        GameObject target = null;
        foreach (GameObject curEnemy in enemiesInRange)
        {
            //fetch enemy armor value
            float curArmor = 0;
            EffectData curEnemyEffects = curEnemy.GetComponent<EnemyScript>().effectData;
            if (curEnemyEffects != null)
                foreach (IEffect curEffect in curEnemyEffects.effects)
                    if (curEffect.XMLName == "armor")
                        curArmor += curEffect.strength;

            //if this is the highest, update vars
            if (curArmor > highestArmor)
            {
                highestArmor = curArmor;
                target = curEnemy;
            }
        }

        //return a list with just the chosen target
        enemiesInRange.Clear();
        enemiesInRange.Add(target);
        return enemiesInRange;
    }
}

//targets all enemies in range
public class EffectTargetAll : IEffectTowerTargeting
{
    [Hide] public TargetingType targetingType { get { return TargetingType.noCast; } } //this effect should never be on a card, and thus should never be cast
    [Hide] public EffectType effectType { get { return EffectType.towerTargeting; } }  //effect type
    [Hide] public float strength { get; set; }                                         //effect strength (unused in this effect)
    [Hide] public string argument { get; set; }                                        //effect argument (unused in this effect)

    //this effect
    [Hide] public string Name { get { return "Target: highest armor"; } } //returns name and strength
    [Show] public string XMLName { get { return "tagetArmor"; } } //name used to refer to this effect in XML

    public List<GameObject> findTargets(Vector2 towerPosition, float towerRange)
    {
        return EnemyManagerScript.instance.enemiesInRange(towerPosition, towerRange); //simply returns all targets in range
    }
}