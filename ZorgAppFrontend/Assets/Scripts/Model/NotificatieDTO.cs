using UnityEngine;

[System.Serializable]
public class NotificatieDTO
{
    // Use [field: SerializeField] attributes to control JSON field names
    [SerializeField]
    public string bericht;

    [SerializeField]
    public bool isGelezen;

    [SerializeField]
    public string datumAanmaak;

    [SerializeField]
    public string datumVerloop;
}