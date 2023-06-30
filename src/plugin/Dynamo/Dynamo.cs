using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Linq;
using MoreSlugcats;
using static RelationshipTracker;

namespace Chimeric;

public partial class Dynamo
{
    public static void DynoCtor(Player self, PlayerEx something)
    {
        if (something.isDynamo) {
            //self.slugcatStats.lungsFac = 0.000000001f;
        }
    }
    public static CreatureTemplate.Relationship AquaFriendDyno(On.CentipedeAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, CentipedeAI self, RelationshipTracker.DynamicRelationship dRelation) {
        var trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
        if ((self.centipede.Template.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti || self.centipede.Template.type == CreatureTemplate.Type.Centiwing) && trackedCreature is Player player && Plugin.tenticleStuff.TryGetValue(player, out var something) && something.isDynamo) {
            //Debug.Log("Made it to changing relationship");
            if (self.centipede.abstractCreature?.abstractAI?.RealAI?.preyTracker?.currentPrey?.critRep?.representedCreature != null && self.centipede.abstractCreature.abstractAI.RealAI.preyTracker.currentPrey.critRep.representedCreature.realizedCreature == player) {
                self.centipede.abstractCreature.abstractAI.RealAI.preyTracker.prey.RemoveAll(c => c.critRep.representedCreature.realizedCreature == player);
                self.centipede.abstractCreature.abstractAI.RealAI.preyTracker.currentPrey = null;
            }
            if (self.centipede.grasps != null) {
                foreach (var grasp in self.centipede.grasps) {
                    if (grasp != null && grasp.grabbedChunk.owner == player) {
                        grasp.Release();
                    }
                }
            }
            return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f);
        }
        return orig(self, dRelation);
    }
    public static void FearDyno(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);
        if (self.room?.game?.StoryCharacter?.value == Plugin.DYNAMO_NAME && self != null && self.abstractCreature != null && self.abstractCreature.abstractAI != null && self.abstractCreature.abstractAI.RealAI != null && self.room != null && self.abstractCreature.abstractAI.RealAI is IUseARelationshipTracker) {
            for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++) {
                Creature crit = self.room.abstractRoom.creatures[i].realizedCreature;
                float randNum = Random.Range(0, 201);
                //Debug.Log($"randNum is {randNum}");
                if (Plugin.creatureYummersSprites.TryGetValue(self, out var thing) && thing.shouldFearDynamo && crit is Player player && Plugin.tenticleStuff.TryGetValue(player, out var something) && something.isDynamo && thing.fearTime > 0 && randNum != 150) {
                    List<DynamicRelationship>? relationships = self.abstractCreature.abstractAI?.RealAI.relationshipTracker.relationships;
                    thing.fearTime--;
                    //Debug.Log($"Feartime is: {thing.fearTime}");
                    for (int j = 0; j < relationships?.Count; j++) {
                        if (relationships[j].trackerRep.representedCreature.realizedCreature == player && (self.Template.type != MoreSlugcatsEnums.CreatureTemplateType.AquaCenti && self.Template.type != CreatureTemplate.Type.Centiwing)) {
                            //Debug.Log("Creature is afraid");
                            relationships[j].currentRelationship.type = CreatureTemplate.Relationship.Type.Afraid;
                            relationships[j].currentRelationship.intensity = 0.5f;
                            if (self.abstractCreature.abstractAI?.RealAI.preyTracker?.currentPrey.critRep.representedCreature != null && self.abstractCreature.abstractAI.RealAI.preyTracker.currentPrey.critRep.representedCreature.realizedCreature == player) {
                                self.abstractCreature.abstractAI.RealAI.preyTracker.prey = self.abstractCreature.abstractAI.RealAI.preyTracker.prey.Where(c => c.critRep.representedCreature.realizedCreature != player).ToList();
                                self.abstractCreature.abstractAI.RealAI.preyTracker.currentPrey = null;
                            }
                            //else{Debug.Log($"null value or creature tracked is not player");}
                            self.abstractCreature.abstractAI?.RealAI.threatTracker?.AddThreatCreature(relationships[j].trackerRep);
                            //self.abstractCreature.abstractAI.RealAI.threatTracker.mostThreateningCreature = relationships[j].trackerRep;
                            //Debug.Log($"Current threat level is: {self.abstractCreature.abstractAI.RealAI.threatTracker.currentThreat}");
                        }
                        //else {Debug.Log($"Searching for player loop {j}");}
                    }
                }
                else if ((!thing.shouldFearDynamo && crit is Player player1 && Plugin.tenticleStuff.TryGetValue(player1, out var something1) && something1.isDynamo && self.grasps != null && !self.grasps.Any<Creature.Grasp>(x => x?.grabbed is VultureMask)) || thing.fearTime == 0 || randNum == 150) {
                    self.abstractCreature.abstractAI?.RealAI.threatTracker?.RemoveThreatCreature(crit.abstractCreature);
                    if (thing.fearTime > 0) {
                        thing.fearTime = 0;
                    }
                    //Debug.Log("Removed player from threat list");
                    //self.abstractCreature.abstractAI.RealAI.threatTracker.mostThreateningCreature = null;
                }
            }
        }

        if (self is not Centipede centi || (!centi.AquaCenti && !centi.Centiwing) || self.grasps == null ||
            self.grasps.Length <= 0)
        {
            return;
        }
        foreach (Creature.Grasp? grasp in self.grasps)
        {
            if (grasp == null || grasp.grabbedChunk == null || grasp.grabbedChunk.owner == null || grasp.grabbedChunk.owner is not Player player || !Plugin.tenticleStuff.TryGetValue(player, out var something) || !something.isDynamo)
            {
                continue;
            }
            grasp.Release();
            Debug.Log("Removed grasp");
        }
        //else {Debug.Log($"Not even a tracker {self.Template.type.value}");}
    }
    public static void DynoUpdate(On.Player.orig_Update orig, Player self, bool eu) {
        var prevVel = self.mainBodyChunk.vel;
        bool startSubmerged = self.submerged;
        if (self.submerged) {
            Debug.Log($"Velocity is: {self.mainBodyChunk.vel}, Magnitude is: {self.mainBodyChunk.vel.magnitude}  {self.slugcatStats.name.value}");
        }
        if (Plugin.tenticleStuff.TryGetValue(self, out var something) && something.isDynamo) {
            #region Swimming Velocity Stuff
            //Debug.Log(something.swimTimer);
            
            #region Don't Drown and do have good friction
            self.airInLungs = 1f;
            self.swimForce = 0f;
            self.waterFriction = 0.9f;
            #endregion

            if (self.animation == Player.AnimationIndex.SurfaceSwim || self.animation == Player.AnimationIndex.DeepSwim || self.submerged) {
                if (something.timeInWater < 40) {
                    something.timeInWater++;
                }
                if (something.timeInWaterUpTo80 < 80) {
                    something.timeInWaterUpTo80++;
                }
            }
            else if (self.animation != Player.AnimationIndex.SurfaceSwim && self.animation != Player.AnimationIndex.DeepSwim) {
                if (something.timeInWater > 0) {
                    something.timeInWater--;
                }
                if (something.timeInWaterUpTo80 > 0) {
                    something.timeInWaterUpTo80 = 0;
                }
            }
            if (self.animation == Player.AnimationIndex.SurfaceSwim && !self.input[1].jmp && self.input[0].jmp) {
                self.mainBodyChunk.vel.y += 20f;
            }
            if (self.submerged) {
                //self.buoyancy = -10f;
                if ((self.input[0].x != 0 || self.input[0].y != 0) && something.swimTimer < 8) {
                    something.swimTimer+=2;
                }
                else if (something.swimTimer > 0) {
                    something.swimTimer=0;
                }

                #region Set stuff to 0 if it's too low
                if (Mathf.Abs(something.slowX) < 0.1f) {
                    something.slowX = 0;
                    Debug.Log("Set slowX to 0");
                }
                if (Mathf.Abs(something.slowY) < 0.1f) {
                    something.slowY = 0;
                    Debug.Log("Set slowY to 0");
                }
                if (something.swimTimer < 0) {
                    something.swimTimer = 0;
                }
                #endregion

                if (self.input[0].x != 0 && (Mathf.Abs(something.slowX) < 1f || (something.prevInput.x==1 && self.input[0].x==-1) || (something.prevInput.x==-1 && self.input[0].x==1))) {
                    something.slowX += self.input[0].x>0? 0.1f : -0.1f * Mathf.Lerp(1f, 6f, Mathf.Abs(something.slowX));
                }
                else if (self.input[0].x == 0 && something.slowX != 0) {
                    something.slowX += Mathf.Lerp(0, something.slowX>0? -0.2f : 0.2f, Mathf.InverseLerp(0, 1f, Mathf.Abs(something.slowX))); //Mathf.Abs(something.slowX) < 0.5f? (something.slowX>0? -0.05f : (something.slowX==0? 0 : 0.05f)) : (something.slowX>0? -0.1f : (something.slowX==0? 0 : 0.1f));
                }
                if (self.input[0].y != 0 && (Mathf.Abs(something.slowY) < 1f || (something.prevInput.y==1 && self.input[0].y==-1) || (something.prevInput.y==-1 && self.input[0].y==1))) {
                    something.slowY += self.input[0].y>0? 0.1f : -0.1f * Mathf.Lerp(1f, 6f, Mathf.Abs(something.slowY));
                }
                else if (self.input[0].y == 0 && something.slowY != 0) {
                    something.slowY += Mathf.Lerp(0, something.slowY>0? -0.2f : 0.2f, Mathf.InverseLerp(0, 1f, Mathf.Abs(something.slowY))); //Mathf.Abs(something.slowY) < 0.5f? (something.slowY>0? -0.05f : (something.slowY==0? 0 : 0.05f)) : (something.slowY>0? -0.1f : (something.slowY==0? 0 : 0.1f));
                }
                for (int i = 0; i < something.fList.Count; i++) {
                    if (something.slowX != 0 || something.slowY != 0) {
                        something.fList[i].swimCycle += Mathf.PI/12f;
                    }
                    else {
                        something.fList[i].swimCycle += Mathf.PI/96f;
                    }
                    //Debug.Log($"Added to swimCycle: {something.fList[i].swimCycle}");
                    if (something.fList[i].swimCycle >= Mathf.PI) {
                        something.fList[i].swimCycle = 0;
                        //Debug.Log("Reset swimCycle");
                    }
                }

                #region Set stuff to 1 if it's too high
                if (Mathf.Abs(something.slowX) > 1) {
                    something.slowX = 1 * Mathf.Sign(something.slowX);
                }
                if (Mathf.Abs(something.slowY) > 1) {
                    something.slowY = 1 * Mathf.Sign(something.slowY);
                }
                #endregion

                //Debug.Log($"Slow x: {something.slowX} Slow y: {something.slowY}");
                Debug.Log($"slowX & slowY: {something.slowX*20f}, {something.slowY*20f} animation: {self.animation.ToString()} timeInWater: {something.timeInWaterUpTo80/80f} Multiple: {Mathf.Pow(something.swimTimer/6.25f, 2) + 1}");
                self.mainBodyChunk.vel = new Vector2(something.slowX*20*(something.timeInWaterUpTo80/80f),something.slowY*20*(something.timeInWaterUpTo80/80f)) * (Mathf.Pow(something.swimTimer/6.25f, 2) + 1);
                Debug.Log($"Result: {self.mainBodyChunk.vel}\n");
            }
            if (!self.submerged) {
                something.swimTimer = 0;
                for (int i = 0; i < something.fList.Count; i++) {
                    something.fList[i].swimCycle = Mathf.PI/6f;
                }
            }
            #endregion
            something.prevInput = new Vector2(self.input[0].x, self.input[0].y);
            #region Funny Roll
            if (something.crawlToRoll && self.bodyMode == Player.BodyModeIndex.Default || self.animation == Player.AnimationIndex.BellySlide) {
                //self.mainBodyChunk.vel.y = 0;
                //self.allowRoll = 50;
                self.rollDirection = self.flipDirection;
                self.rollCounter = 0;
                self.standing = false;
                self.animation = Player.AnimationIndex.Roll;
                self.bodyChunks[0].vel.x = 8.75f * self.flipDirection;
                self.bodyChunks[1].vel.x = 8.75f * self.flipDirection;
                PlayerGraphics gModule = ((PlayerGraphics)self.graphicsModule);
                gModule.tail[gModule.tail.Length-1].vel += new Vector2(-16f * self.flipDirection, -34f);
                something.crawlToRoll = false;
            }
            #endregion
        }

        orig(self, eu);
        if (something.isDynamo) {
            //Debug.Log($"Roll: {self.superLaunchJump}");   //ITS SUPERLAUNCHJUMP DAMNIT
            if (!self.submerged && startSubmerged) {
                Debug.Log("Lol I stopped the veolcity, how goofy what a goofy thing I just did there guys lmao");
                something.slowX = 0;
                something.slowY = 0;
                self.bodyChunks[0].vel /= 1.1f;
                self.bodyChunks[1].vel /= 1.1f;
            }
            if (self.submerged && !startSubmerged) {
                for (int i = 0; i < something.fList.Count; i++) {
                    something.fList[i].swimCycle = something.fList[i].startSwimCycle;
                    self.bodyChunks[0].vel = Vector2.down * 0.8f;
                    self.bodyChunks[1].vel = Vector2.down * 0.8f;
                }
            }
            //Debug.Log($"Is it this? {self.superLaunchJump}");
            if (self.bodyMode == Player.BodyModeIndex.Crawl && ((self.superLaunchJump >= 1 && self.input[0].y < 0) || something.crawlToRoll) /*&& self.input[0].x == 0*/) {
                something.crawlToRoll = true;
                self.superLaunchJump = 20;
            }
            else if (self.bodyMode != Player.BodyModeIndex.Crawl && (self.bodyMode == Player.BodyModeIndex.Stand || self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam || self.bodyMode == Player.BodyModeIndex.CorridorClimb || self.bodyMode == Player.BodyModeIndex.ClimbIntoShortCut)) {
                something.crawlToRoll = false;
            }
        }
    }
    public static void DynoUpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self) {
        orig(self);
        if (Plugin.tenticleStuff.TryGetValue(self, out var something) && something.isDynamo) {
            if (self.animation == Player.AnimationIndex.Roll) {
                if (self.rollCounter <= 10) {
                    self.bodyChunks[0].vel *= 0.8f;
                    self.bodyChunks[1].vel *= 0.8f;
                }
                else {
                    self.bodyChunks[0].vel *= 1.3f;
                    self.bodyChunks[1].vel *= 1.3f;
                }
                if (self.rollCounter > 15) {
                    self.rollCounter = 200;
                }
            }
        }
    }
}