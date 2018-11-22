using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkinSync.Models
{
    public class LesionItem
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Location { get; set; }
        public int Diameter { get; set; }
        public string Uploaded { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
    }
}
