using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SkinSync.Models
{
    public class SkinSyncContext : DbContext
    {
        public SkinSyncContext (DbContextOptions<SkinSyncContext> options)
            : base(options)
        {
        }

        public DbSet<SkinSync.Models.LesionItem> LesionItem { get; set; }
    }
}
