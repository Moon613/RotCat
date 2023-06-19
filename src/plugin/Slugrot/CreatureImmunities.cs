using MoreSlugcats;

namespace Chimeric
{
    public class CreatureImmunities {
        public static CreatureTemplate.Relationship DaddyAIImmune(On.DaddyAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, DaddyAI self, RelationshipTracker.DynamicRelationship dRelation) {
            CreatureTemplate.Relationship relationship = orig(self, dRelation);
            Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
            if (self.daddy.Template.type != MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs && trackedCreature is Player player && Plugin.tenticleStuff.TryGetValue(player, out var something) && something.isRot) {
                //Debug.Log("Made it to changing relationship");
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f);
            }
            return relationship;
        }
        public static void DaddyAIImmune2(On.DaddyAI.orig_Update orig, DaddyAI self) {
            if (self.daddy.Template.type != MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs) {
                if (self.preyTracker.currentPrey != null && self.preyTracker.currentPrey.critRep.representedCreature.realizedCreature is Player player && Plugin.tenticleStuff.TryGetValue(player, out var something) && something.isRot) {
                    self.preyTracker.currentPrey = null;
                    self.behavior = DaddyAI.Behavior.Idle;
                }
                self.daddy.eatObjects.RemoveAll(c => c.chunk.owner is Player player && Plugin.tenticleStuff.TryGetValue(player, out var something) && something.isRot);
            }
            orig(self);
        }
        public static void DaddyTentacleImmune(On.DaddyTentacle.orig_Update orig, DaddyTentacle self) {
            orig(self);
            if (self.daddy.Template.type != MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs) {
                if (self.grabChunk != null && self.grabChunk.owner is Player player && Plugin.tenticleStuff.TryGetValue(player, out var something) && something.isRot) {
                    self.grabChunk = null;
                }
                if (self.huntCreature != null && self.huntCreature.representedCreature.realizedCreature is Player player1 && Plugin.tenticleStuff.TryGetValue(player1, out var something1) && something1.isRot) {
                    self.SwitchTask(DaddyTentacle.Task.Locomotion);
                }
            }
        }
        public static void WallCystImmune1(On.DaddyCorruption.LittleLeg.orig_Update orig, DaddyCorruption.LittleLeg self, bool eu) {
            if (self.grabChunk?.owner is Creature crit && crit is Player player && Plugin.tenticleStuff.TryGetValue(player, out var something) && something.isRot) {
                player.GrabbedByDaddyCorruption = false;
                self.grabChunk = null;
                self.pullInPrey = 0f;
            }
            orig(self, eu);
        }
        public static void WallCystImmune2(On.DaddyCorruption.orig_Update orig, DaddyCorruption self, bool eu) {
            self.eatCreatures.RemoveAll(c => c.creature is Player player && Plugin.tenticleStuff.TryGetValue(player, out var something) && something.isRot);
            if (self.bulbs != null) {
                for (int i = 0; i < self.bulbs.GetLength(0); i++) {
                    for (int j = 0; j < self.bulbs.GetLength(1); j++) {
                        if (self.bulbs[i,j] != null) {
                            for (int k = 0; k < self.bulbs[i,j].Count; k++) {
                                if (self.bulbs[i,j][k].eatChunk != null && self.bulbs[i,j][k].eatChunk.owner is Creature crit1 && crit1 is Player player1 && Plugin.tenticleStuff.TryGetValue(player1, out var something1) && something1.isRot)
                                self.bulbs[i,j][k].eatChunk = null;
                            }
                        }
                    }
                }
            }
            orig(self, eu);
        }
    }
}