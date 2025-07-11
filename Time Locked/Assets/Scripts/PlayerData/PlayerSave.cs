using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSave
{
    public bool loadSuccess;
    public int characterId;
    public int money;
    public DateTime saveTime;

    public PlayerSave()
    {
        loadSuccess = true;
        characterId = -1;
        money = 0;
        saveTime = DateTime.Now;
    }

    public PlayerSave(bool success)
    {
        loadSuccess = success;
        characterId = -1;
        money = 0;
        saveTime = DateTime.Now;
    }

    public PlayerSave(int charId, int money, DateTime saveTime)
    {
        loadSuccess = true;
        characterId = charId;
        this.money = money;
        this.saveTime = saveTime;
    }
}
