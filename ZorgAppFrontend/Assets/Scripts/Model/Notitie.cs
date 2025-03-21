[System.Serializable]
public class Notitie
{
    // Match these property names exactly with the JSON field names
    public int id;
    public string titel;
    public string tekst;
    public string userId;
    public string datumAanmaak;

    // Properties to maintain compatibility with existing code
    public string Titel { get { return titel; } set { titel = value; } }
    public string Tekst { get { return tekst; } set { tekst = value; } }
    public string DatumAanmaak { get { return datumAanmaak; } set { datumAanmaak = value; } }
}