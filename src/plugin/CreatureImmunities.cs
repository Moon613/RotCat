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
                        if (self?.grasps[j] != null && self?.grasps[j].grabbedChunk.owner == self.room.PlayersInRoom[i]) {
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
                    if(self?.grabChunk?.owner?.GetType() == typeof(Player)) {
                        self.grabChunk = null;
                    }
                }
            }
        }
    }
}