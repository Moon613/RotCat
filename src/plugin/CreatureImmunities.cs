using MoreSlugcats;

namespace RotCat
{
    public class CreatureImmunities {
        public static void DaddyImmune(On.DaddyLongLegs.orig_Update orig, DaddyLongLegs self, bool eu) {
            for(int i = 0; i < self?.room?.PlayersInRoom?.Count; i++) {
                RotCat.tenticleStuff.TryGetValue(self.room.PlayersInRoom[i], out var something);
                if (something.isRot && self.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs) {
                    self.room.PlayersInRoom[i].abstractCreature.tentacleImmune = false;
                }
            }
            orig(self, eu);
            for(int i = 0; i < self?.room?.PlayersInRoom?.Count; i++) {
                RotCat.tenticleStuff.TryGetValue(self.room.PlayersInRoom[i], out var something);
                if (something.isRot && self.Template.type != MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs) {
                    for(int j = 0; j < self?.grasps?.Length; j++) {
                        if (self?.grasps[j] != null && self?.grasps[j].grabbedChunk.owner is Player) {
                            self.grasps[j] = null;
                        }
                    }
                    self.room.PlayersInRoom[i].abstractCreature.tentacleImmune = true;
                }
            }
        }
        public static void WallCystImmune(On.DaddyCorruption.LittleLeg.orig_Update orig, DaddyCorruption.LittleLeg self, bool eu) {
            orig(self, eu);
            for(int i = 0; i < self?.room?.PlayersInRoom?.Count; i++) {
                RotCat.tenticleStuff.TryGetValue(self.room.PlayersInRoom[i], out var something);
                if (something.isRot) {
                    if(self?.grabChunk?.owner is Player) {
                        self.grabChunk = null;
                    }
                }
            }
        }
    }
}