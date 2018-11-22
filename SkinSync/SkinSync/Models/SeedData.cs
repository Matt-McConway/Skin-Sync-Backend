using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkinSync.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new SkinSyncContext(
                serviceProvider.GetRequiredService<DbContextOptions<SkinSyncContext>>()))
            {
                // Look for any lesion items
                if (context.LesionItem.Count() > 0)
                {
                    return;   // DB has been seeded
                }

                context.LesionItem.AddRange(
                    new LesionItem
                    {
                        Url = "https://www.skincancer.org/Media/Default/Page/atypical-mole.png",
                        Location = "Left Hand",
                        Diameter = 7,
                        Uploaded = "22-11-18 4:20T18:25:43.511Z",
                        Width = "314",
                        Height = "272"
                    }


                );
                context.SaveChanges();
            }
        }
    }
}
