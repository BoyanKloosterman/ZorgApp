using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Model
{
    public class Patient
    {
        public int id { get; set; }
        public string voornaam { get; set; }
        public string achternaam { get; set; }
        public int oudervoogd_id { get; set; }
        public int trajectid { get; set; }
        public int? artsid { get; set; }
        public string userid { get; set; }
    }
}
