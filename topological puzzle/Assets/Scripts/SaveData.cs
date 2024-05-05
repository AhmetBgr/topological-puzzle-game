using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int levelProgressIndex;

    // first char is final level,
    // rest is all levels from 0 to final,
    // 0 = incomplete, 1 = completed
    //string levels = "000000000000000000000000000000000000";
    //public int lastLevel;
}
