using MonoMod.Cil;
using RWCustom;
using UnityEngine;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace Chimeric
{
    public class DigestionRotSprites
    {
        public static void Apply() {
            On.AbstractCreature.ctor += AddCWTToCreature;
            On.Player.EatMeatUpdate += PlayerEatsCreature;
            On.Player.TossObject += TossAndRemoveCreature;
            On.GraphicsModule.DrawSprites += DrawRotYumSprites;
            On.Creature.SpitOutOfShortCut += ReassignRotSprites;
            On.Creature.SuckedIntoShortCut += SuckIntoPipe;
            IL.Player.EatMeatUpdate += ReplaceEatingSound;
        }
        ///<summary>Gives each creature a CWT to hold info on the rot sprites upon creation</summary>
        public static void AddCWTToCreature(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);
            Plugin.creatureYummersSprites.Add(self, new CreatureEx());
            Plugin.creatureYummersSprites.TryGetValue(self, out var extraSprites);
            extraSprites.maxNumOfSprites = Random.Range(5, 16);
        }
        public static void PlayerEatsCreature(On.Player.orig_EatMeatUpdate orig, Player self, int graspIndex)
        {
            orig(self, graspIndex);
            if (Plugin.tenticleStuff.TryGetValue(self, out var something) && something.isRot) {
                Creature creature;
                if (self.grasps[graspIndex].grabbedChunk.owner is not Creature crit) {return;} 
                else { creature = crit; }
                if (Plugin.creatureYummersSprites.TryGetValue(creature.abstractCreature, out CreatureEx thing)) {
                    if (creature.State.meatLeft != creature.Template.meatPoints /*&& creature.Template.meatPoints > 0*/ && !thing.isBeingEaten) {
                        thing.isBeingEaten = true;
                        something.eating = true;
                        Debug.Log("Thing is being eaten");
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
        public static void TossAndRemoveCreature(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
        {
            orig(self, grasp, eu);
            if (Plugin.tenticleStuff.TryGetValue(self, out var something) && something.isRot && self.grasps[grasp].grabbed is Creature crit && crit.State.meatLeft <= 0 && crit.Template.meatPoints > 0) {
                something.eating = false;
                crit.Destroy();
            }
        }
        ///<summary>Adds sprites onto the creature when conditions are met, and assigns them new sprites to follow</summary>
        public static void DrawRotYumSprites(On.GraphicsModule.orig_DrawSprites orig, GraphicsModule self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.owner is Creature creature && Plugin.creatureYummersSprites.TryGetValue(creature.abstractCreature, out CreatureEx thing)) {
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
                    while (randSprite == null || randSprite.GetPosition() == Vector2.zero || !randSprite.isVisible || randSprite.color.maxColorComponent < 0.08f) {
                        randSprite = sLeaser.sprites[Random.Range(0, sLeaser.sprites.Length)];
                    }
                    EatingRot newRotYum = new EatingRot(Random.Range(0.125f, 0.22f), randSprite.color, Color.blue, randSprite.GetPosition(), randSprite, creature.abstractCreature, sLeaser.sprites.IndexOf(randSprite));
                    self.owner.room.AddObject(newRotYum);
                    thing.yummersRotting.Add(newRotYum);
                }
                if (thing.redrawRotSprites) {
                    foreach (EatingRot rotBulb in thing.yummersRotting)
                    {
                        rotBulb.ReassignSprites(sLeaser.sprites[rotBulb.indexInArray]);
                        Debug.Log($"Reassigned to: {rotBulb.indexInArray}");
                        rotBulb.hideSpritesInPipe = false;
                    }
                    thing.redrawRotSprites = false;
                }
            }
        }
        public static void CompressCreature(On.Creature.orig_Update orig, Creature self, bool eu)
        {
            orig(self, eu);
            if (Plugin.creatureYummersSprites.TryGetValue(self.abstractCreature, out var thing)) {
                
            }
        }
        ///<summary>Makes the sprites follow the newly generated sprites</summary>
        public static void ReassignRotSprites(On.Creature.orig_SpitOutOfShortCut orig, Creature self, IntVector2 pos, Room newRoom, bool spitOutAllSticks)
        {
            orig(self, pos, newRoom, spitOutAllSticks);
            if (Plugin.creatureYummersSprites.TryGetValue(self.abstractCreature, out var thing))
            {
                thing.redrawRotSprites = true;
            }
        }
        ///<summary>Hides the extra rot sprites when dragging a creature into a pipe</summary>
        public static void SuckIntoPipe(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
        {
            if (self is Player player) {
                for (int i = 0; i < player.grasps.Length; i++) {
                    if (player.grasps[i] != null && player.grasps[i].grabbedChunk != null && player.grasps[i].grabbedChunk.owner is Creature crit && Plugin.creatureYummersSprites.TryGetValue(crit.abstractCreature, out var thing))
                    {
                        foreach (EatingRot rotBulb in thing.yummersRotting)
                        {
                            rotBulb.hideSpritesInPipe = true;
                            //Debug.Log("Hidden sprites");
                        }
                    }
                    else {/*Debug.Log("Failed to hide any sprites");*/}
                }
            }
            orig(self, entrancePos, carriedByOther);
        }
        ///<summary>IL hook that replaces the sound of eating with daddy sounds</summary>
        public static void ReplaceEatingSound(ILContext il)
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdsfld<SoundID>("Slugcat_Eat_Meat_A"))) {
                return;
            }
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate((SoundID sound, Creature self) => {
                if (self is Player player && Plugin.tenticleStuff.TryGetValue(player, out var something) && something.isRot)
                {
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
            cursor.EmitDelegate((SoundID sound, Creature self) => {
                if (self is Player player && Plugin.tenticleStuff.TryGetValue(player, out var something) && something.isRot)
                {
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
    }
}