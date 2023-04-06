using BepInEx;
using UnityEngine;
using Noise;
using MoreSlugcats;
using RWCustom;
using System.Security;
using System.Security.Permissions;
using System;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using SlugBase.Features;
using SlugBase;

namespace RotCat
{
    public class CalmTentacles
    {
        public static void CalmNewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom) {
            orig(self, newRoom);
            RotCat.tenticleStuff.TryGetValue(self, out var something);
            if (something.isRot) {
                for (int i = 0; i < something.tentacles.Length; i++) {
                    for (int j = something.tentacles[i].pList.Length-1; j >= 0; j--) {
                        something.tentacles[i].pList[j].position = self.mainBodyChunk.pos - new Vector2(0,j);
                        something.tentacles[i].pList[j].prevPosition = self.mainBodyChunk.pos - new Vector2(0,j);
                    }
                }
            }
        }
        public static void CalmSpitOutOfShortCut(On.Player.orig_SpitOutOfShortCut orig, Player self, IntVector2 pos, Room newRoom, bool spitOutAllStacks) {
            orig(self, pos, newRoom, spitOutAllStacks);
            RotCat.tenticleStuff.TryGetValue(self, out var something);
            if (something.isRot) {
                for (int i = 0; i < something.tentacles.Length; i++) {
                    for (int j = something.tentacles[i].pList.Length-1; j >= 0; j--) {
                        something.tentacles[i].pList[j].position = self.mainBodyChunk.pos - new Vector2(0,j);
                        something.tentacles[i].pList[j].prevPosition = self.mainBodyChunk.pos - new Vector2(0,j);
                    }
                }
            }
        }
    }
}