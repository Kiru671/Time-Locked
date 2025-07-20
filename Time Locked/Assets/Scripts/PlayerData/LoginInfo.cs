using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginInfo
{
    public bool success;
    public string token;
    public string username;
    public UserType type;
    public UserLevel level;
    public UserLanguage Language;
    public int userId;

    public LoginInfo()
    {
        success = false;
        username = "TempUser";
        type = UserType.Private;
        level = UserLevel.PreSchool;
        Language = UserLanguage.Turkish;
    }

    public LoginInfo(bool success)
    {
        this.success = success;
    }

    public LoginInfo(string username, UserType type, UserLevel level, UserLanguage language, int userId)
    {
        success = true;
        this.username = username;
        this.type = type;
        this.level = level;
        this.Language = language;
        this.userId = userId;
    }
}
public class LoginResponse
{
    public string Token { get; set; }
}

public enum UserType
{
    Bau,
    Ugur,
    OguzKaan,
    Private
}

public enum UserLevel
{
    PreSchool,
    FirstGrade,
    SecondGrade,
    ThirdGrade,
    FourthGrade,
    FifthGrade
}

public enum UserLanguage
{
    Turkish,
    English
}