using System;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
[System.Serializable]
public class Notificatie
{
    public int ID;

    public string Bericht;

    public bool IsGelezen;

    public string DatumAanmaak;

    public string DatumVerloop;

    public string UserId;
}
