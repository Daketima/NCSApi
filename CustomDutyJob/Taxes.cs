using DataLayer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NCS.DTO
{
    //[XmlElement("Taxes")]
    public class Taxes
    {
        public int Id { get; set; }
        [XmlElement("Tax")]
        public List<Tax> Tax { get; set; }
    }
}
