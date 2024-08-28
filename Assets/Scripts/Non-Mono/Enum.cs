using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Enum
{
    public enum SelectStatus
    {
        NONE,
        CRAB,
        SIEGE,
        MIXED
    }
    
    public enum CrabSpecies
    {
        ROCK, 
        FIDDLER, 
        TGIANT, 
        SPIDER, 
        COCONUT, 
        HORSESHOE, 
        SEAWEED, 
        CALICO, 
        TRILOBITE, 
        KAKOOTA
    };

    public enum WallUpgradeType
    {
        NORMAL, 
        WOOD, 
        STONE
    }

    public enum AIStage
    {
        START, 
        MID, 
        END
    };
}
