using UnityEngine;
using RWCustom;

namespace Chimeric
{
    public class CalmTentacles
    {
        public static void Apply() {
            On.Player.NewRoom += CalmNewRoom;
            On.Player.SpitOutOfShortCut += CalmSpitOutOfShortCut;
        }
        public static void CalmNewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom) {
            orig(self, newRoom);
            if (Plugin.tenticleStuff.TryGetValue(self, out var something) && something.isRot) {
                foreach (Tentacle tentacle in something.tentacles) {
                    tentacle.Reset(self.mainBodyChunk.pos);
                }
                foreach (Tentacle decoTentacle in something.decorativeTentacles) {
                    foreach (Point point in decoTentacle.pList) {
                        point.Reset(self.mainBodyChunk.pos);
                    }
                }
            }
        }
        public static void CalmSpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, IntVector2 pos, Room newRoom, bool spitOutAllStacks) {
            orig(self, pos, newRoom, spitOutAllStacks);
            if (Plugin.tenticleStuff.TryGetValue(self, out var something) && something.isRot) {
                foreach (Tentacle tentacle in something.tentacles) {
                    tentacle.Reset(self.mainBodyChunk.pos);
                }
                foreach (Tentacle decoTentacle in something.decorativeTentacles) {
                    foreach (Point point in decoTentacle.pList) {
                        point.Reset(self.mainBodyChunk.pos);
                    }
                }
            }
        }
    }
}