using Fisobs.Creatures;
using Fisobs.Core;
using System.Collections.Generic;
using Fisobs.Sandbox;
using static PathCost.Legality;
using UnityEngine;
using DevInterface;
using RWCustom;
using Fisobs.Properties;
using MoreSlugcats;

namespace Chimeric
{
    sealed class BabyAquapede : Critob{
        public BabyAquapede() : base(CreatureTemplateType.BabyAquapede)
        {
            Icon = new SimpleIcon("Kill_Centipede1", Color.blue);
            RegisterUnlock(KillScore.Configurable(25), SandboxUnlockID.BabyAquapede);
            SandboxPerformanceCost = new(3f, 1.5f);
            LoadedPerformanceCost = 50f;
            ShelterDanger = ShelterDanger.Safe;
            BabyAquapedeHooks.Apply();
        }
        public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allow)
        {
            base.ConnectionIsAllowed(map, connection, ref allow);
        }
        public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allow)
        {
            base.TileIsAllowed(map, tilePos, ref allow);
        }
        public override int ExpeditionScore() => 20;
        public override Color DevtoolsMapColor(AbstractCreature acrit) => Color.blue;
        public override string DevtoolsMapName(AbstractCreature acrit) => "smolaqua";
        public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] {RoomAttractivenessPanel.Category.LikesInside, RoomAttractivenessPanel.Category.LikesWater, RoomAttractivenessPanel.Category.Swimming, RoomAttractivenessPanel.Category.All};
        public override IEnumerable<string> WorldFileAliases() => new[] {"babyaquapede"};
        public override CreatureTemplate CreateTemplate()
        {
            var t = new CreatureFormula(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, Type, "BabyAquapede")
            {
                DefaultRelationship = new(CreatureTemplate.Relationship.Type.Afraid, 1f),
                HasAI = true,
                Pathing = PreBakedPathing.Ancestral(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
            }.IntoTemplate();
            t.smallCreature = true;
            t.shortcutColor = Color.blue;
            return t;
        }
        public override void EstablishRelationships()
        {
            Relationships centi = new Relationships(Type);
            centi.IsInPack(Type, 0.85f);
            centi.PlaysWith(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti, 0.5f);
            centi.Eats(CreatureTemplate.Type.Fly, 0.8f);
        }
        public override ArtificialIntelligence? CreateRealizedAI(AbstractCreature acrit) => new CentipedeAI(acrit, acrit.world);
        public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Centipede(acrit, acrit.world);
        public override CreatureState CreateState(AbstractCreature acrit) => new Centipede.CentipedeState(acrit);
        public override void LoadResources(RainWorld rainWorld) {}
        public override CreatureTemplate.Type? ArenaFallback() => CreatureTemplate.Type.SmallCentipede;
    }
}