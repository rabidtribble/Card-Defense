﻿using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

//a Color class that is nicely formatted in xml
[System.Serializable]
public class XMLColor
{
    [XmlAttribute] public float r;
    [XmlAttribute] public float g;
    [XmlAttribute] public float b;
    [XmlAttribute] public float a;

    //returns a unity color
    public Color toColor()
    {
        return new Color(r, g, b, a);
    }

    //returns a hex string for use with rich text formatting and such
    public string toHex()
    {
        //each value is multiplied by 255, rounded off, and then added to the result string as a hex value
        return
            Mathf.RoundToInt(r * 255).ToString("X2") +
            Mathf.RoundToInt(g * 255).ToString("X2") +
            Mathf.RoundToInt(b * 255).ToString("X2") +
            Mathf.RoundToInt(a * 255).ToString("X2");
    }
};

//contains everything needed to define an enemy type
[System.Serializable]
public class EnemyData
{
    [XmlAttribute] public string     name;       //used to identify this enemy type
    [XmlAttribute] public int        spawnCost;	 //used for wave generation: more expensive enemies spawn in smaller numbers
    [XmlAttribute] public int        damage;     //number of charges knocked off if the enemy reaches the goal
    [XmlAttribute] public int        maxHealth;  //max health
    [XmlAttribute] public float      unitSpeed;  //speed, measured in distance/second
                   public XMLColor   unitColor;  //used to colorize the enemy sprite
                   public EffectData effectData; //specifies which effects are attached to this enemy type and what their parameters are
};

public class EnemyScript : MonoBehaviour
{
    public List<Vector2> path;               //list of points this unit must go to
    public int           currentDestination; //index in the path that indicates the current destination
    public EnemyData     data;               //contains all the data specific to this type of enemy
    public Vector2       startPos;           //position where this enemy was spawned
    public int           curHealth;          //current health
    public int           expectedHealth;     //what health will be after all active shots reach this enemy

    private Transform   parentTransform;    //reference to the transform of this enemy

    //used for health bar
    public Color    healthyColor; //color when healthy
    public Color    dyingColor;   //color when near death

    // Use this for initialization
    private void Start()
    {
        //init vars
        curHealth = data.maxHealth;
        expectedHealth = data.maxHealth;
        parentTransform = GetComponentInParent<Transform>();
        startPos = parentTransform.position;
    }

    // LateUpdate is called once per frame, after other objects have done a regular Update().  We use LateUpdate to make sure bullets get to move first this frame.
    private void LateUpdate()
    {
        Vector2 curLocation = parentTransform.position; //fetch current location
        Vector2 newLocation = Vector2.MoveTowards (curLocation, path[currentDestination], data.unitSpeed * Time.deltaTime); //calculate new location

        //save position
        parentTransform.position = new Vector3(newLocation.x, newLocation.y, parentTransform.position.z);

        //if reached the current destination, attempt to move to the next one
        if (curLocation == newLocation)
        {
            currentDestination++;

            if (path.Count == currentDestination)
            {
                //reached the end.  damage player...
                DeckManagerScript.instance.SendMessage("Damage", data.damage);

                //...and go back to start for another lap
                parentTransform.position = startPos;
                currentDestination = 0;

                //if the enemy is not expected to die, update the enemy list with the new pathing info
                if (expectedHealth > 0)
                    EnemyManagerScript.instance.SendMessage("EnemyPathChanged", gameObject);
            }
        }

        //update health bar fill and color
        Image healthbar = gameObject.GetComponentInChildren<Image> ();
        float normalizedHealth = (float)curHealth / (float)data.maxHealth;
        healthbar.color = Color.Lerp(dyingColor, healthyColor, normalizedHealth);
        healthbar.fillAmount = normalizedHealth;
    }

    //tracks damage that WILL arrive so that towers dont keep shooting something that is about to be dead
    public void onExpectedDamage(ref DamageEventData e)
    {
        //deal with effects that need to happen when we expect damage
        if (data.effectData != null)
            foreach (IEffect i in data.effectData.effects)
                if (i.effectType == EffectType.enemyDamaged)
                    ((IEffectEnemyDamaged)i).expectedDamage(ref e);

        //expect to take damage
        expectedHealth -= Mathf.CeilToInt(e.rawDamage);

        //if a death is expected, report self as dead so towers ignore this unit
        if (expectedHealth <= 0)
            EnemyManagerScript.instance.SendMessage("EnemyExpectedDeath", gameObject);
    }

    //receives damage
    private void OnDamage(DamageEventData e)
    {
        //dont bother if we are already dead
        if (curHealth <= 0)
            return;

        //deal with effects that need to happen when we actually take damage
        if (data.effectData != null)
            foreach (IEffect i in data.effectData.effects)
                if (i.effectType == EffectType.enemyDamaged)
                    ((IEffectEnemyDamaged)i).actualDamage(ref e);

        //take damage
        int damage = Mathf.CeilToInt(e.rawDamage);
        damage = System.Math.Min(damage, curHealth);
        curHealth -= damage;
        LevelManagerScript.instance.WaveTotalRemainingHealth -= damage;

        if (curHealth <= 0)
        {
            //if dead, report the kill to the tower that shot it
            LevelManagerScript.instance.deadThisWave++;
            Destroy(gameObject);
        }
    }

    //stores a new path for this unit to follow
    private void SetPath(List<Vector2> p)
    {
        path = p;               //save path
        currentDestination = 0; //go towards the first destination
    }

    //stores the data specific to this type of enemy
    private void SetData(EnemyData d)
    {
        data = d;
        this.GetComponent<SpriteRenderer>().color = d.unitColor.toColor();
    }

    //returns the distance from this enemy's current position to the goal, following its current path
    public float distanceToGoal()
    {
        float result = Vector2.Distance(transform.position, path[currentDestination]); //start with distance to the current destination...

        //..and add the length of each subsequent segment
        for (int segment = currentDestination + 1; segment < path.Count; segment++)
            result += Vector2.Distance(path[segment - 1], path[segment]);

        return result;
    }
}