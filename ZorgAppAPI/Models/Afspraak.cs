namespace ZorgAppAPI.Models
{
    public class Afspraak
    {
        public int ID { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ArtsID { get; set; }
        public string Datumtijd { get; set; }
        public string Naam { get; set; }
    }
}
