using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Type : int
    { 
        Normal,
        Fire,
        Water,
        Plant,
        Poison
        


    }


public class HealthSystem : MonoBehaviour
{
    public float hp;
    public float defense;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }




}

/*
 * Current plan on the design of this
 * Every entity with health, owns their own "HealthSystem" (better name pending... maybe forever)
 * Or there is a single system that keeps track of everythings health.. but I feel like that would be worse. Have entities have their own health.
 
 * Available health types: and notes on them
 * HP - can regen, `bool regenEnabled;` - generic health

 * Defense - decreases overall damage  taken - could also be different types, pokemon-esque 
 - "physical defense" reduces general damage from more real world things
 - "special defense" or otherwise reduces general damage from other damage types
 - ignore defence tags that can be passed in, probably bit-flags?
 - also if defense takes away a flat number, it needs to be a max % of health (def of -10 dmg, 5 dmg dealt, should still deal damage, is it a % total or "1 dmg dealt"?)

 * Damage/Types/Specific defenses - Find a way for users to create their own types, and how they effect each other
 - flat +damage, as well as a *damage taken, maybe make the healing system tie directly into this? so negatives just heal.
 - damage over time

 * Status effects
 - things like extra damage taken/reduced damage dealt/reduced healing/etc that last a temporary amount of time
 - A way to stop the effects early, as well as read what effects are currently effecting the player

 * Req Logic:
 - When updated... how do u update the health of all the players, unless all players update their own health every-
 - ok that actually makes sense..

 * Layout/Design
 - all "types" would be under user defined enums.
 - A script that is user editable with a few 'default' types, that the main system reads from, to control everything
 - This script itself would be part of all entities requiring health, I guess this would also be the one with user defined systems?


 *
 *
 *
 *
 *
 *
 *
 */