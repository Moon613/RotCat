using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using MoreSlugcats;
using static RelationshipTracker;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Chimeric;

public class Dynamo
{
    public static void Apply() {
        DynamoGraphics.Apply();
        On.Player.Update += DynoUpdate;
        On.Creature.Update += FearDyno;
        On.CentipedeAI.IUseARelationshipTracker_UpdateDynamicRelationship += AquaFriendDyno;
        On.Player.UpdateBodyMode += DynoUpdateBodyMode;
        IL.Player.GrabUpdate += DynamoCanEatUnderwater;
    }
    public static void DynoCtor(Player self, PlayerEx something)
    {
        self.slugcatStats.runspeedFac = 0.95f;
    }
    public static CreatureTemplate.Relationship AquaFriendDyno(On.CentipedeAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, CentipedeAI self, DynamicRelationship dRelation) {
        Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
        if ((self.centipede.Template.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti || self.centipede.Template.type == CreatureTemplate.Type.Centiwing) && trackedCreature is Player player && Plugin.playerCWT.TryGetValue(player, out var something) && something.isDynamo) {
            //Debug.Log("Made it to changing relationship");
            if (self.centipede.abstractCreature?.abstractAI?.RealAI?.preyTracker?.currentPrey?.critRep?.representedCreature != null && self.centipede.abstractCreature.abstractAI.RealAI.preyTracker.currentPrey.critRep.representedCreature.realizedCreature == player) {
                self.centipede.abstractCreature.abstractAI.RealAI.preyTracker.prey.RemoveAll(c => c.critRep.representedCreature.realizedCreature == player);
                self.centipede.abstractCreature.abstractAI.RealAI.preyTracker.currentPrey = null;
            }
            foreach (Creature.Grasp grasp in self.centipede.grasps) {
                if (grasp != null && grasp.grabbedChunk.owner == player) {
                    grasp.Release();
                }
            }
            return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f);
        }
        return orig(self, dRelation);
    }
    public static void FearDyno(On.Creature.orig_Update orig, Creature self, bool eu)
    {
        orig(self, eu);
        if (self is not Player && self.abstractCreature?.abstractAI?.RealAI is IUseARelationshipTracker && self.room != null) {
            for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++) {
                Creature crit = self.room.abstractRoom.creatures[i].realizedCreature;
                //Debug.Log($"randNum is {randNum}");
                if (Plugin.CreatureCWT.TryGetValue(self.abstractCreature, out var thing) && thing.shouldFearDynamo && crit is Player player && Plugin.playerCWT.TryGetValue(player, out var something) && something.isDynamo && thing.fearTime > 0) {
                    List<DynamicRelationship>? relationships = self.abstractCreature.abstractAI?.RealAI.relationshipTracker.relationships;
                    thing.fearTime--;
                    //Debug.Log($"Feartime is: {thing.fearTime}");
                    for (int j = 0; j < relationships?.Count; j++) {
                        if (relationships[j].trackerRep?.representedCreature?.realizedCreature == player && self.Template.type != MoreSlugcatsEnums.CreatureTemplateType.AquaCenti && self.Template.type != CreatureTemplate.Type.Centiwing) {
                            //Debug.Log("Creature is afraid");
                            relationships[j].currentRelationship.type = CreatureTemplate.Relationship.Type.Afraid;
                            relationships[j].currentRelationship.intensity = 1f;
                            if (self.abstractCreature.abstractAI?.RealAI.preyTracker?.currentPrey?.critRep?.representedCreature != null && self.abstractCreature.abstractAI.RealAI.preyTracker.currentPrey?.critRep?.representedCreature?.realizedCreature == player) {
                                self.abstractCreature.abstractAI.RealAI.preyTracker.prey = self.abstractCreature.abstractAI.RealAI.preyTracker.prey.Where(c => c.critRep.representedCreature.realizedCreature != player).ToList();
                                self.abstractCreature.abstractAI.RealAI.preyTracker.currentPrey = null;
                            }
                            //else{Debug.Log($"null value or creature tracked is not player");}
                            self.abstractCreature.abstractAI?.RealAI.threatTracker?.AddThreatCreature(relationships[j].trackerRep);
                            //self.abstractCreature.abstractAI.RealAI.threatTracker.mostThreateningCreature = relationships[j].trackerRep;
                        }
                    }
                }
                else if ((!thing.shouldFearDynamo && crit is Player player1 && Plugin.playerCWT.TryGetValue(player1, out var something1) && something1.isDynamo && self.grasps != null && !self.grasps.Any<Creature.Grasp>(x => x?.grabbed is VultureMask)) || thing.fearTime == 0) {
                    self.abstractCreature.abstractAI?.RealAI.threatTracker?.RemoveThreatCreature(crit.abstractCreature);
                    if (thing.fearTime > 0) {
                        thing.fearTime = 0;
                    }
                    //Debug.Log("Removed player from threat list");
                    //self.abstractCreature.abstractAI.RealAI.threatTracker.mostThreateningCreature = null;
                }
            }
        }
        if (self is not Centipede centi || (!centi.AquaCenti && !centi.Centiwing) || self.grasps == null || self.grasps.Length <= 0) { return; }
        foreach (Creature.Grasp? grasp in self.grasps) {
            if (grasp == null || grasp.grabbedChunk.owner is not Player player || (Plugin.playerCWT.TryGetValue(player, out var something) && !something.isDynamo))
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
        if (Plugin.playerCWT.TryGetValue(self, out var something) && something.isDynamo) {
            if (something.canPlayShockSound > 0) {
                something.canPlayShockSound--;
            }
            //Debug.Log(something.swimTimer);
            
            self.airInLungs = 1f;   // Don't drown
            self.swimForce = 0f;    // idek
            self.waterFriction = 0.9f;  // About normal friction idk    I think 1 is 0 friction and 0 is literally can't move friction
            self.buoyancy = 0f;   // doesn't do what I want it to I think but also doesn't do anything

            #region Counters lmao
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
            foreach (Fin fin in something.fList)
                if (self.bodyMode == Player.BodyModeIndex.CorridorClimb && fin.corriderTimer < 20f) {
                    fin.corriderTimer++;
                }
                else if (self.bodyMode != Player.BodyModeIndex.CorridorClimb && fin.corriderTimer > 0f) {
                    fin.corriderTimer--;
                }
            #endregion

            #region Swimming Velocity Stuff
            if (self.animation == Player.AnimationIndex.SurfaceSwim && !self.input[1].jmp && self.input[0].jmp) {
                self.mainBodyChunk.vel.y += 20f;
            }
            if (self.submerged) {
                if ((self.input[0].x != 0 || self.input[0].y != 0) && something.swimTimer < 8) {
                    something.swimTimer += 2;
                }
                else if (something.swimTimer > 0) {
                    something.swimTimer = 0;
                }

                #region Set stuff to 0 if it's too low
                if (Mathf.Abs(something.slowX) < 0.01f) {
                    something.slowX = 0;
                    //Debug.Log("Set slowX to 0");
                }
                if (Mathf.Abs(something.slowY) < 0.01f) {
                    something.slowY = 0;
                    //Debug.Log("Set slowY to 0");
                }
                if (something.swimTimer < 0) {
                    something.swimTimer = 0;
                }
                #endregion
                #region Evil Maths Stuff beware (dont touch it bites)
                // wtf are these maths even, idk what I was thinking anymore, but they do seem to work ok...
                if (self.input[0].x != 0 && (Mathf.Abs(something.slowX) < 1f || Mathf.Sign(something.prevInput.x) != Mathf.Sign(self.input[0].x))) {
                    something.slowX += (self.input[0].x>0? 0.1f : -0.1f) * Mathf.Lerp(1.5f, 1f, Mathf.Abs(something.slowX)) * (Mathf.Sign(something.slowX)!=Mathf.Sign(self.input[0].x)? 10f:1f);
                }
                else if (self.input[0].x == 0 && something.slowX != 0) {
                    something.slowX += Mathf.Lerp(0, something.slowX>0? -0.2f : 0.2f, Mathf.Abs(something.slowX)); //Mathf.Abs(something.slowX) < 0.5f? (something.slowX>0? -0.05f : (something.slowX==0? 0 : 0.05f)) : (something.slowX>0? -0.1f : (something.slowX==0? 0 : 0.1f));
                }
                if (self.input[0].y != 0 && (Mathf.Abs(something.slowY) < 1f || Mathf.Sign(something.prevInput.y) != Mathf.Sign(self.input[0].y))) {
                    something.slowY += (self.input[0].y>0? 0.1f : -0.1f) * Mathf.Lerp(1.5f, 1f, Mathf.Abs(something.slowY)) * (Mathf.Sign(something.slowY)!=Mathf.Sign(self.input[0].y)? 10f:1f);
                }
                else if (self.input[0].y == 0 && something.slowY != 0) {
                    something.slowY += Mathf.Lerp(0, something.slowY>0? -0.2f : 0.2f, Mathf.Abs(something.slowY)); //Mathf.Abs(something.slowY) < 0.5f? (something.slowY>0? -0.05f : (something.slowY==0? 0 : 0.05f)) : (something.slowY>0? -0.1f : (something.slowY==0? 0 : 0.1f));
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
                #endregion
                #region Set stuff to 1 if it's too high
                if (Mathf.Abs(something.slowX) > 1) {
                    something.slowX = 1 * Mathf.Sign(something.slowX);
                }
                if (Mathf.Abs(something.slowY) > 1) {
                    something.slowY = 1 * Mathf.Sign(something.slowY);
                }
                #endregion

                //Debug.Log($"slowX & slowY: {something.slowX*20f}, {something.slowY*20f} animation: {self.animation} timeInWater: {something.timeInWaterUpTo80/80f} Multiple: {Mathf.Pow(something.swimTimer/6.25f, 2) + 1}");
                // Rotund world stuff affecting swimming movement
                float rotundFactor = Plugin.rotund? (self.TotalMass >= 1.7f ? self.TotalMass : 0f) : 0f;
                self.mainBodyChunk.vel = new Vector2(something.slowX*20*(something.timeInWaterUpTo80/80f)/(rotundFactor == 0? 1f:(rotundFactor*4f)),something.slowY*20*(something.timeInWaterUpTo80/80f)/(rotundFactor == 0? 1f:(rotundFactor*4f))-(rotundFactor/2f)) * (Mathf.Pow(something.swimTimer/6.25f, 2) + 1);
                //Debug.Log($"Result: {self.mainBodyChunk.vel}\n");
            }
            else {
                something.swimTimer = 0;
                for (int i = 0; i < something.fList.Count; i++) {
                    something.fList[i].swimCycle = Mathf.PI/6f;
                }
            }
            #endregion
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
                PlayerGraphics gModule = (PlayerGraphics)self.graphicsModule;
                gModule.tail[gModule.tail.Length-1].vel += new Vector2(-16f * self.flipDirection, -34f);
                something.crawlToRoll = false;
            }
            something.prevInput = new Vector2(self.input[0].x, self.input[0].y);
            #endregion
        }

        orig(self, eu);

        if (something.isDynamo) {
            if (self.submerged && self.bodyChunks[0].vel.magnitude < 1f) {
                Debug.Log($"vel after: {self.bodyChunks[1].vel}");
                self.bodyChunks[1].vel -= self.bodyChunks[1].vel;
            }
            //Debug.Log($"Roll: {self.superLaunchJump}");   //ITS SUPERLAUNCHJUMP DAMNIT    // Oh yeah for the thing (I forgor what :3) // Oh yeah for the pounce jump
            if (!self.submerged && startSubmerged) {
                // Debug.Log("Lol I stopped the veolcity, how goofy what a goofy thing I just did there guys lmao");
                something.slowX = 0;
                something.slowY = 0;
                // self.bodyChunks[0].vel /= 1.1f;
                // self.bodyChunks[1].vel /= 1.1f;
            }
            if (self.submerged && !startSubmerged) {
                for (int i = 0; i < something.fList.Count; i++) {
                    something.fList[i].swimCycle = something.fList[i].startSwimCycle;
                }
                self.bodyChunks[0].vel = Vector2.down * 0.8f;
                self.bodyChunks[1].vel = Vector2.down * 0.8f;
            }
            //Debug.Log($"Is it this? {self.superLaunchJump}");
            // If Dynamo is crawling, and either getting ready to pounce and the player is holding down, or they already did this and haven't moved yet, make them ready to pounce.
            if (self.bodyMode == Player.BodyModeIndex.Crawl && ((self.superLaunchJump >= 1 && self.input[0].y < 0) || something.crawlToRoll) /*&& self.input[0].x == 0*/) {
                something.crawlToRoll = true;
                self.superLaunchJump = 20;
            }
            else if (self.bodyMode == Player.BodyModeIndex.Stand || self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam || self.bodyMode == Player.BodyModeIndex.CorridorClimb || self.bodyMode == Player.BodyModeIndex.ClimbIntoShortCut) {
                something.crawlToRoll = false;
            }
        }
    }
    public static void DynoUpdateBodyMode(On.Player.orig_UpdateBodyMode orig, Player self) {
        orig(self);
        if (Plugin.playerCWT.TryGetValue(self, out var something) && something.isDynamo) {
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
    public static void DynamoCanEatUnderwater(ILContext il) {
        var cursor = new ILCursor(il);
        var label = il.DefineLabel();
        int start = cursor.Index;   // Store the start index of the cursor

        // Move after the first call to the isRivulet property
        if (!cursor.TryGotoNext(MoveType.After, 
            i => i.MatchCall<Player>("get_isRivulet"))) {
            return;
        }
        // Now go to right before the instruction that marks a true value
        if (!cursor.TryGotoNext(MoveType.Before,
            i => i.MatchLdcI4(1))) {
            return;
        }

        // aaaaand save it's location in the label created previously, then go back to the start
        cursor.MarkLabel(label);
        cursor.Index = start;

        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchCall<Player>("get_isRivulet"))) {
            return;
        }
        if (!cursor.TryGotoPrev(MoveType.Before, 
            i => i.MatchLdarg(0))) {
            return;
        }

        // Grab the player from the top of the stack, and evaluate if it is Dynamo
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((Player player) => {
            if (Plugin.playerCWT.TryGetValue(player, out var something) && something.isDynamo) {
                //Debug.Log("Doing things!");
                return true;
            }
            return false;
        });

        // Now, if the scug is Rivulet OR Dynamo, the cursor moves to the label, so Dynamo can eat underwater (among a few other unknown things I think)
        cursor.Emit(OpCodes.Brtrue_S, label);
    }
}