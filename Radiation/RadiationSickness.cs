using Harmony;
using Klei.AI;
using Klei.AI.DiseaseGrowthRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Egladil
{
    public class RadiationSickness : Sickness
    {
        static RadiationSickness() {
            var list = TUNING.GERM_EXPOSURE.TYPES.ToList();
            list.Add(new ExposureType()
            {
                germ_id = RadiationGerms.ID,
                sickness_id = RadiationSickness.ID,
                exposure_threshold = 100,
                base_resistance = -2,
            });
            TUNING.GERM_EXPOSURE.TYPES = list.ToArray();
        }

        public const string ID = "RadiationSickness";

        public static readonly HashedString HashedId = (HashedString)ID;

        private static Sickness instance;
        public static Sickness Instance => instance ?? (instance = Db.Get().Sicknesses.TryGet(ID));

        public RadiationSickness()
        : base(ID, SicknessType.Pathogen, Severity.Major, 0.001f, 
              new List<InfectionVector> { InfectionVector.Contact, InfectionVector.Digestion, InfectionVector.Inhalation, },
              20 * 600f)
        {
            fatalityDuration = 10 * 600f;

            AddSicknessComponent(new CommonSickEffectSickness());

            var name = Strings.Get($"STRINGS.DUPLICANTS.DISEASES.{ID.ToUpper()}.NAME");
            AddSicknessComponent(new AttributeModifierSickness(new AttributeModifier[]
                {
                    new AttributeModifier(Db.Get().Amounts.Stamina.deltaAttribute.Id, -0.5f, name, false, false, true),
                    new AttributeModifier(Db.Get().Amounts.Breath.deltaAttribute.Id, -0.5f, name, false, false, true),
                    new AttributeModifier(Db.Get().Amounts.Bladder.deltaAttribute.Id, 0.5f, name, false, false, true),
                    new AttributeModifier(Db.Get().Attributes.Athletics.Id, -3, name, false, false, true),
                    new AttributeModifier(Db.Get().Attributes.Strength.Id, -3, name, false, false, true),
                    new AttributeModifier(Db.Get().Attributes.GermResistance.Id, -2, name, false, false, true),
                    new AttributeModifier(Db.Get().Attributes.ToiletEfficiency.Id, -0.5f, name, false, false, true),
                }));

            AddSicknessComponent(new AnimatedSickness(new HashedString[1] { "anim_idle_sick_kanim" }, Db.Get().Expressions.Sick));

            AddSicknessComponent(new PeriodicEmoteSickness("anim_idle_sick_kanim", new HashedString[3]
                {
                "idle_pre",
                "idle_default",
                "idle_pst"
                }, 5f));
        }


        [HarmonyPatch(typeof(Database.Sicknesses), MethodType.Constructor, typeof(ResourceSet))]
        public class Sicknesses_Constructor_Patch
        {
            public static void Postfix(Database.Sicknesses __instance)
            {
                __instance.Add(new RadiationSickness());
            }
        }
    }
}
