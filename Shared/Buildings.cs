using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Egladil
{
    public class Buildings
    {
        public static void AddToPlan(string id, string category, string before = null, string after = null)
        {
            var plan = TUNING.BUILDINGS.PLANORDER.Find(x => x.category == category);
            if (plan.category != category) throw new ArgumentException("category");

            var buildings = (IList<string>)plan.data;

            if (before != null)
            {
                int index = buildings.IndexOf(before);
                buildings.Insert(index, id);
            }
            else if (after != null)
            {
                int index = buildings.IndexOf(after) + 1;
                buildings.Insert(index, id);
            }
            else
            {
                buildings.Add(id);
            }
        }

        public static void AddToTech(string id, string category)
        {
            var tech = Database.Techs.TECH_GROUPING[category].ToList();
            tech.Add(id);
            Database.Techs.TECH_GROUPING[category] = tech.ToArray();
        }
    }
}
