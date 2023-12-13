using System;
using System.Runtime.CompilerServices;
using MonoMod.Cil;
using RWCustom;
using UnityEngine;
using OpCodes = Mono.Cecil.Cil.OpCodes;
using Random = UnityEngine.Random;

//TODO: Make sure that sprites stay when things get abstracted and then realized

namespace Chimeric
{
    public class CorruptionRotSpritesControl
    {
        private static ConditionalWeakTable<PhysicalObject.BodyChunkConnection, StrongBox<float>> ConnectionCWT = new();
        public static void Apply() {
            On.AbstractCreature.ctor += AddCWTToAbstractCreature;
            On.PhysicalObject.BodyChunkConnection.ctor += BodyChunkConnection_ctor;
            On.Player.EatMeatUpdate += PlayerEatsCreature;
            On.Player.TossObject += TossAndRemoveCreature;
            On.GraphicsModule.DrawSprites += DrawRotYumSprites;
            On.Creature.SpitOutOfShortCut += ReassignRotSprites;
            On.Creature.SuckedIntoShortCut += SuckIntoPipe;
            On.Creature.SpitOutOfShortCut += SpitOutOfPipe;
            On.Player.Update += CompressCreature;
            On.Room.Update += Room_Update;
            On.StaticWorld.InitCustomTemplates += StaticWorld_InitCustomTemplates;
            IL.Player.EatMeatUpdate += ReplaceEatingSound;
            // These two should skip the template meatpoints check and use a value from CreatureCWT, but used with slugrot causes errors
            // On.Player.EatMeatUpdate += Player_EatMeatUpdate;
            // IL.Player.GrabUpdate += Player_GrabUpdate;
        }
        public static void StaticWorld_InitCustomTemplates(On.StaticWorld.orig_InitCustomTemplates orig) {
            orig();
            Array.Find(StaticWorld.creatureTemplates, i => i.type == CreatureTemplate.Type.Deer).meatPoints = 15;
            Array.Find(StaticWorld.creatureTemplates, i => i.type == CreatureTemplate.Type.BigEel).meatPoints = 30;
        }
        ///<summary>Gives each creature a CWT to hold info on the rot sprites upon creation</summary>
        public static void AddCWTToAbstractCreature(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);
            Plugin.CreatureCWT.Add(self, new CreatureEx());
            Plugin.CreatureCWT.TryGetValue(self, out var thing);
            thing.maxNumOfSprites = Random.Range(5, 16);
        }
        public static void BodyChunkConnection_ctor(On.PhysicalObject.BodyChunkConnection.orig_ctor orig, PhysicalObject.BodyChunkConnection self, BodyChunk chunk1, BodyChunk chunk2, float distance, PhysicalObject.BodyChunkConnection.Type type, float elasticity, float weightSymmetry) {
            orig(self, chunk1, chunk2, distance, type, elasticity, weightSymmetry);
            ConnectionCWT.Add(self, new StrongBox<float>(distance));
        }
        public static void PlayerEatsCreature(On.Player.orig_EatMeatUpdate orig, Player self, int graspIndex)
        {
            orig(self, graspIndex);
            if (self.eatMeat > 20 && Plugin.playerCWT.TryGetValue(self, out var something) && something.isRot) {
                Creature creature;
                if (self.grasps[graspIndex].grabbedChunk.owner is not Creature crit) { return; } 
                else { creature = crit; }
                if (Plugin.CreatureCWT.TryGetValue(creature.abstractCreature, out CreatureEx thing)) {
                    if (creature.State.meatLeft != creature.Template.meatPoints /*&& creature.Template.meatPoints > 0*/ && !thing.isBeingEaten) {
                        thing.isBeingEaten = true;
                        something.eating = true;
                        // Debug.Log("Thing is being eaten");
                    }
                    if (self.eatMeat % 10 == 2) {
                        thing.addNewSprite = true;
                    }
                    else {
                        thing.addNewSprite = false;
                    }
                }
            }
        }
        ///<summary>Kill the creature when you toss it</summary>
        public static void TossAndRemoveCreature(On.Player.orig_TossObject orig, Player self, int grasp, bool eu) {
            orig(self, grasp, eu);
            if (Plugin.playerCWT.TryGetValue(self, out var something) && something.isRot && self.grasps[grasp].grabbed is Creature crit) {
                something.eating = false;
                if (crit.State.meatLeft <= 0 && crit.Template.meatPoints > 0) { // Probably fix by removing Template check here
                    crit.Destroy();
                    return;
                }
                if (Plugin.CreatureCWT.TryGetValue(crit.abstractCreature, out var thing)) {
                    thing.isBeingEaten = false;
                }
                foreach (var connection in crit.bodyChunkConnections) {
                    if (ConnectionCWT.TryGetValue(connection, out var originalValue)) {
                        connection.distance = originalValue.Value;
                    }
                }
            }
        }
        ///<summary>Adds sprites onto the creature when conditions are met, and assigns them new sprites to follow</summary>
        public static void DrawRotYumSprites(On.GraphicsModule.orig_DrawSprites orig, GraphicsModule self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.owner is Creature creature && Plugin.CreatureCWT.TryGetValue(creature.abstractCreature, out CreatureEx thing)) {
                if (thing.addNewSprite) {
                    while (Random.Range(0,2) <= 0) {
                        thing.maxNumOfSprites++;
                    }
                    thing.addNewSprite = false;
                }
                if (thing.isBeingEaten && thing.numOfCosmeticSprites <= thing.maxNumOfSprites) {
                    Debug.Log($"Adding cosmetic sprite num {thing.numOfCosmeticSprites}");
                    thing.numOfCosmeticSprites++;
                    FSprite? randSprite = null;
                    while (randSprite == null || randSprite.GetPosition() == Vector2.zero || !randSprite.isVisible || randSprite.color.maxColorComponent < 0.05f || randSprite.color.a < 0.05f) {
                        randSprite = sLeaser.sprites[Random.Range(0, sLeaser.sprites.Length)];
                    }
                    Color accentColor;
                    foreach (var grasp in creature.grabbedBy) {
                        if (grasp != null && grasp.grabber is Player p && Plugin.playerCWT.TryGetValue(p, out var something)) {
                            accentColor = SlugBase.DataTypes.PlayerColor.GetCustomColor(p.graphicsModule as PlayerGraphics, 2);
                            goto Skip;
                        }
                    }
                    accentColor = Color.blue;
                    Skip:
                    CreatureCorruptionBulb newCorruptionBulb = new CreatureCorruptionBulb(Random.Range(0.125f, 0.22f), randSprite.color, Color.Lerp(randSprite.color, accentColor, 0.9f), randSprite.GetPosition(), randSprite, creature.abstractCreature, sLeaser.sprites.IndexOf(randSprite));
                    self.owner.room.AddObject(newCorruptionBulb);
                    thing.corruptionBulbs.Add(newCorruptionBulb);
                }
                if (thing.redrawRotSprites) {
                    foreach (CreatureCorruptionBulb rotBulb in thing.corruptionBulbs)
                    {
                        rotBulb.ReassignSprites(sLeaser.sprites[rotBulb.indexInArray]);
                        Debug.Log($"Reassigned to: {rotBulb.indexInArray}");
                        rotBulb.hideSpritesInPipe = false;
                    }
                    thing.redrawRotSprites = false;
                }
                if (self?.owner?.room != null) {
                    foreach (var leaser in self.owner.room.game.cameras[0].spriteLeasers) {
                        if (leaser.drawableObject is CreatureCorruptionBulb bulb) {
                            bulb.DrawSprites(leaser, self.owner.room.game.cameras[0], timeStacker, camPos);
                        }
                    }
                }
            }
        }
        public static void CompressCreature(On.Player.orig_Update orig, Player self, bool eu) {
            orig(self, eu);
            if (Plugin.playerCWT.TryGetValue(self, out var something) && something.isRot) {
                foreach (var grasp in self.grasps) {
                    if (grasp != null && grasp.grabbed is Creature creature && Plugin.CreatureCWT.TryGetValue(creature.abstractCreature, out var crit) && crit.isBeingEaten && something.eating) {
                        foreach (PhysicalObject.BodyChunkConnection connection in creature.bodyChunkConnections) {
                            if (ConnectionCWT.TryGetValue(connection, out StrongBox<float>? originalDistance)) {
                                connection.distance = Mathf.Lerp(Mathf.Max(connection.chunk1.rad, connection.chunk2.rad)/2f, originalDistance.Value, (float)creature.State.meatLeft/(float)creature.Template.meatPoints);
                            }
                        }
                    }
                }
            }
        }
        ///<summary>Makes the sprites follow the newly generated sprites</summary>
        public static void ReassignRotSprites(On.Creature.orig_SpitOutOfShortCut orig, Creature self, IntVector2 pos, Room newRoom, bool spitOutAllSticks)
        {
            orig(self, pos, newRoom, spitOutAllSticks);
            if (Plugin.CreatureCWT.TryGetValue(self.abstractCreature, out var thing))
            {
                thing.redrawRotSprites = true;
            }
        }
        ///<summary>Hides the extra rot sprites when dragging a creature into a pipe</summary>
        public static void SuckIntoPipe(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
        {
            if (self is Player player) {
                for (int i = 0; i < player.grasps.Length; i++) {
                    if (player.grasps[i] != null && player.grasps[i].grabbedChunk != null && player.grasps[i].grabbedChunk.owner is Creature crit && Plugin.CreatureCWT.TryGetValue(crit.abstractCreature, out var thing)) {
                        foreach (CreatureCorruptionBulb rotBulb in thing.corruptionBulbs) {
                            rotBulb.hideSpritesInPipe = true;
                            //Debug.Log("Hidden sprites");
                            self.room.RemoveObject(rotBulb);
                        }
                    }
                }
            }
            orig(self, entrancePos, carriedByOther);
        }
        public static void SpitOutOfPipe(On.Creature.orig_SpitOutOfShortCut orig, Creature self, IntVector2 pos, Room newRoom, bool spitOutAllSticks) {
            orig(self, pos, newRoom, spitOutAllSticks);
            if (self is Player player) {
                for (int i = 0; i < player.grasps.Length; i++) {
                    if (player.grasps[i] != null && player.grasps[i].grabbedChunk != null && player.grasps[i].grabbedChunk.owner is Creature crit && Plugin.CreatureCWT.TryGetValue(crit.abstractCreature, out var thing)) {
                        thing.redrawRotSprites = true;
                        foreach (CreatureCorruptionBulb rotBulb in thing.corruptionBulbs) {
                            self.room.AddObject(rotBulb);
                        }
                    }
                }
            }
        }
        public static void Room_Update(On.Room.orig_Update orig, Room self) {
            orig(self);
            foreach (var obj in self.updateList) {
                if (obj is Creature crit && Plugin.CreatureCWT.TryGetValue(crit.abstractCreature, out var thing)) {
                    thing.redrawRotSprites = true;  // This could cause lag potentially
                    foreach (CreatureCorruptionBulb rotBulb in thing.corruptionBulbs) {
                        if (!self.drawableObjects.Contains(rotBulb)) {
                            self.AddObject(rotBulb);
                        }
                    }
                }
            }
        }
        ///<summary>IL hook that replaces the sound of eating with daddy sounds</summary>
        public static void ReplaceEatingSound(ILContext il) {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdsfld<SoundID>("Slugcat_Eat_Meat_A"))) {
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((SoundID sound, Player self) => {
                if (Plugin.playerCWT.TryGetValue(self, out var something) && something.isRot) {
                    Debug.Log("Replacing sound");
                    //Debug.Log(sound);
                    sound = SoundID.Daddy_And_Bro_Tentacle_Grab_Creature;
                    //Debug.Log(sound);
                }
                return sound;
            });
            if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchLdsfld<SoundID>("Slugcat_Eat_Meat_B"))) {
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((SoundID sound, Player self) => {
                // Rather then make a label and skip to it, replacing the sound with the conditions it easier, does the same thing, and has less IL steps
                if (Plugin.playerCWT.TryGetValue(self, out var something) && something.isRot) {
                    Debug.Log("Replacing 2nd sound");
                    //Debug.Log(sound);
                    if (!something.eating) {
                        sound = SoundID.Daddy_Digestion_Init;
                    }
                    else {
                        sound = SoundEnums.Silence;
                    }
                    //Debug.Log(sound);
                }
                return sound;
            });
        }
        // Is supposed to make Deer able to be eaten, but this somehow results in an error in the sprite index of the head or arm.  Few days later, this might be because of something else.
        public static void Player_GrabUpdate(ILContext il) {
            var cursor = new ILCursor(il);
            var label = il.DefineLabel();
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.meatPoints)))) {
                return;
            }
            if (!cursor.TryGotoPrev(MoveType.Before, i => i.MatchLdarg(0))) {
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc, 8);
            cursor.EmitDelegate((Player self, int grasp) => {
                return ((Creature)self.grasps[grasp].grabbed).Template.meatPoints == 0;
            });
            cursor.Emit(OpCodes.Brtrue, label);
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchBle(out var _))) {
                return;
            }
            cursor.MarkLabel(label);
        }
        public static void Player_EatMeatUpdate(On.Player.orig_EatMeatUpdate orig, Player self, int graspIndex) {
            if (self.grasps[graspIndex].grabbedChunk.owner is Creature crit && Plugin.CreatureCWT.TryGetValue(crit.abstractCreature, out var thing) && thing.meat > 0 && crit.Template.meatPoints == 0) {
                crit.State.meatLeft = thing.meat;
            }
            orig(self, graspIndex);
            if (self.grasps[graspIndex].grabbedChunk.owner is Creature crit1 && Plugin.CreatureCWT.TryGetValue(crit1.abstractCreature, out var thing1) && thing1.meat > 0 && crit1.Template.meatPoints == 0) {
                thing1.meat = crit1.State.meatLeft;
                crit1.State.meatLeft = 0;
            }
        }
    }
}