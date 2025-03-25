using System;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class Notificatie
{
    public int ID { get; set; }

    public string Bericht { get; set; } = string.Empty;

    public bool IsGelezen { get; set; } = false;

    public DateTime DatumAanmaak { get; set; }

    public DateTime DatumVerloop { get; set; }

    public string UserId { get; set; }
}
