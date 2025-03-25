[System.Serializable]
public class Notificatie
{
    public int id;
    public string bericht;
    public bool isGelezen;
    public string datumAanmaak;
    public string datumVerloop;
    public string userId;

    public int ID { get { return id; } set { id = value; } }
    public string Bericht { get { return bericht; } set { bericht = value; } }
    public bool IsGelezen { get { return isGelezen; } set { isGelezen = value; } }
    public string DatumAanmaak { get { return datumAanmaak; } set { datumAanmaak = value; } }
    public string DatumVerloop { get { return datumVerloop; } set { datumVerloop = value; } }
    public string UserId { get { return userId; } set { userId = value; } }
}
