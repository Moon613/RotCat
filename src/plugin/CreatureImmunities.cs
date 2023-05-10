using System.Collections.Generic;
using System.Linq;
using MoreSlugcats;
using UnityEngine;

namespace RotCat
{
    public class CreatureImmunities {
        public static CreatureTemplate.Relationship DaddyAIImmune(On.DaddyAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, DaddyAI self, RelationshipTracker.DynamicRelationship dRelation) {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            var trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (self.daddy.Template.type != MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs && trackedCreature is Player player && RotCat.tenticleStuff.TryGetValue(player, out var something) && something.isRot) {
                //Debug.Log("Made it to changing relationship");
                relationship.type = CreatureTemplate.Relationship.Type.StayOutOfWay;
                relationship.intensity = 1f;
            }
            return relationship;
        }
        public static void DaddyAIImmune2(On.DaddyAI.orig_Update orig, DaddyAI self) {
            if (self.daddy.Template.type != MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs) {
                if (self.preyTracker.currentPrey != null && self.preyTracker.currentPrey.critRep.representedCreature.realizedCreature is Player player && RotCat.tenticleStuff.TryGetValue(player, out var something) && something.isRot) {
                    self.preyTracker.currentPrey = null;
                    self.behavior = DaddyAI.Behavior.Idle;
                }
                self.daddy.eatObjects.RemoveAll(c => c.chunk.owner is Player player && RotCat.tenticleStuff.TryGetValue(player, out var something) && something.isRot);
            }
            orig(self);
        }
        public static void DaddyTentacleImmune(On.DaddyTentacle.orig_Update orig, DaddyTentacle self) {
            orig(self);
            if (self.grabChunk != null && self.grabChunk.owner is Player player && RotCat.tenticleStuff.TryGetValue(player, out var something) && something.isRot) {
                self.grabChunk = null;
            }
            if (self.huntCreature != null && self.huntCreature.representedCreature.realizedCreature is Player player1 && RotCat.tenticleStuff.TryGetValue(player1, out var something1) && something1.isRot) {
                self.huntCreature = null;
            }
        }
        public static void WallCystImmune(On.DaddyCorruption.LittleLeg.orig_Update orig, DaddyCorruption.LittleLeg self, bool eu) {
            self.owner.eatCreatures.RemoveAll(c => c.creature is Player player && RotCat.tenticleStuff.TryGetValue(player, out var something) && something.isRot);
            if (self.grabChunk?.owner is Creature crit && crit is Player player && RotCat.tenticleStuff.TryGetValue(player, out var something) && something.isRot) {
                (self.grabChunk.owner as Creature).GrabbedByDaddyCorruption = false;
                self.grabChunk = null;
                self.pullInPrey = 0f;
            }
            for (int i = 0; i < self.owner.bulbs.GetLength(0); i++) {
                for (int j = 0; j < self.owner.bulbs.GetLength(1); j++) {
                    if (self.owner.bulbs[i,j] != null) {
                        for (int k = 0; k < self.owner.bulbs[i,j].Count; k++) {
                            if (self.owner.bulbs[i,j][k].eatChunk != null && self.owner.bulbs[i,j][k].eatChunk.owner is Creature crit1 && crit1 is Player player1 && RotCat.tenticleStuff.TryGetValue(player1, out var something1) && something1.isRot)
                            self.owner.bulbs[i,j][k].eatChunk = null;
                        }
                    }
                }
            }
            orig(self, eu);
        }
    }
}