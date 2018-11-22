using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkinSync.Models
{
    public class LesionImageItem
    {
        public string Location { get; set; }
        public string Diameter { get; set; }
        public IFormFile Image { get; set; }
    }
}
