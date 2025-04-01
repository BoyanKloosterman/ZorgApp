using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Model
{
    public class Patient
    {
        public int id;
        public string voornaam;
        public string achternaam;
        public string? oudervoogdid;
        public int? trajectid;
        public int? artsid;
        public int? avatarId;
        public string userid;
        public DateTime? Geboortedatum;
        public string email;
        public string password;
    }

}
