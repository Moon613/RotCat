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
            Plugin.tenticleStuff.TryGetValue(self, out var something);
            if (something.isRot) {
                for (int i = 0; i < something.tentacles.Length; i++) {
                    for (int j = something.tentacles[i].pList.Length-1; j >= 0; j--) {
                        something.tentacles[i].pList[j].position = self.mainBodyChunk.pos - new Vector2(0,j);
                        something.tentacles[i].pList[j].prevPosition = self.mainBodyChunk.pos - new Vector2(0,j);
                        something.targetPos[i].foundSurface = false;    //Could put a check that determines the position of player and sets startPos behind them
                        something.tentacles[i].iWantToGoThere = self.mainBodyChunk.pos;
                        //Functions.TentaclesFindPositionToGoTo(something, self, Functions.FindPos(something.overrideControls, self, RotCat.staticOptions));
                        something.targetPos[i].targetPosition = self.mainBodyChunk.pos;
                    }
                }
            }
        }
        public static void CalmSpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, IntVector2 pos, Room newRoom, bool spitOutAllStacks) {
            orig(self, pos, newRoom, spitOutAllStacks);
            Plugin.tenticleStuff.TryGetValue(self, out var something);
            if (something.isRot) {
                for (int i = 0; i < something.tentacles.Length; i++) {
                    for (int j = something.tentacles[i].pList.Length-1; j >= 0; j--) {
                        something.tentacles[i].pList[j].position = self.mainBodyChunk.pos - new Vector2(0,j);
                        something.tentacles[i].pList[j].prevPosition = self.mainBodyChunk.pos - new Vector2(0,j);
                        something.targetPos[i].foundSurface = false;
                        something.tentacles[i].iWantToGoThere = self.mainBodyChunk.pos;
                        //Functions.TentaclesFindPositionToGoTo(something, self, Functions.FindPos(something.overrideControls, self, RotCat.staticOptions));
                        something.targetPos[i].targetPosition = self.mainBodyChunk.pos;
                    }
                }
            }
        }
    }
}