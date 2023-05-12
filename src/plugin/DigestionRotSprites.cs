using System.Reflection.Emit;
using MonoMod.Cil;
using RWCustom;
using UnityEngine;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace RotCat;

public class DigestionRotSprites
{
    public static void AddCWTToCreature(On.Creature.orig_ctor orig, Creature self, AbstractCreature abstractCreature, World world)
    {
        orig(self, abstractCreature, world);
        RotCat.creatureYummersSprites.Add(self, new CreatureEx());
        RotCat.creatureYummersSprites.TryGetValue(self, out var extraSprites);
        extraSprites.maxNumOfSprites = Random.Range(5, 16);
    }
    public static void PlayerEatsCreature(On.Player.orig_EatMeatUpdate orig, Player self, int graspIndex)
    {
        orig(self, graspIndex);
        if (RotCat.tenticleStuff.TryGetValue(self, out var something) && something.isRot) {
            Creature creature;
            if (self.grasps[graspIndex].grabbedChunk.owner is not Creature crit) {return;} 
            else {
                creature = crit;
            }
            if (RotCat.creatureYummersSprites.TryGetValue(creature, out CreatureEx thing)) {
                if (creature.State.meatLeft != creature.Template.meatPoints /*&& creature.Template.meatPoints > 0*/ && !thing.isBeingEaten) {
                    thing.isBeingEaten = true;
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
    public static void TossAndRemoveCreature(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
    {
        orig(self, grasp, eu);
        if (RotCat.tenticleStuff.TryGetValue(self, out var something) && something.isRot && self.grasps[grasp].grabbed is Creature crit && crit.State.meatLeft <= 0 && crit.Template.meatPoints > 0) {
            crit.Destroy();
        }
    }
    public static void DrawRotYumSprites(On.GraphicsModule.orig_DrawSprites orig, GraphicsModule self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (self.owner is Creature creature && RotCat.creatureYummersSprites.TryGetValue(creature, out CreatureEx thing)) {
            if (thing.addNewSprite) {
                thing.maxNumOfSprites++;
                thing.addNewSprite = false;
            }
            if (thing.isBeingEaten && thing.numOfCosmeticSprites <= thing.maxNumOfSprites) {
                Debug.Log($"Adding cosmetic sprite num {thing.numOfCosmeticSprites}");
                thing.numOfCosmeticSprites++;
                FSprite? randSprite = null;
                while (randSprite == null || randSprite.GetPosition() == Vector2.zero || !randSprite.isVisible || randSprite.color.maxColorComponent < 0.08f) {
                    randSprite = sLeaser.sprites[Random.Range(0, sLeaser.sprites.Length)];
                }
                EatingRot newRotYum = new EatingRot(Random.Range(0.125f, 0.22f), randSprite.color, Color.blue, randSprite.GetPosition(), randSprite, creature, sLeaser.sprites.IndexOf(randSprite));
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
    public static void ReassignRotSprites(On.Creature.orig_SpitOutOfShortCut orig, Creature self, IntVector2 pos, Room newRoom, bool spitOutAllSticks)
    {
        orig(self, pos, newRoom, spitOutAllSticks);
        if (RotCat.creatureYummersSprites.TryGetValue(self, out var thing))
        {
            thing.redrawRotSprites = true;
        }
    }
    public static void SuckIntoPipe(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
    {
        if (self is Player player) {
            for (int i = 0; i < player.grasps.Length; i++) {
                if (player.grasps[i] != null && player.grasps[i].grabbedChunk != null && player.grasps[i].grabbedChunk.owner is Creature crit && RotCat.creatureYummersSprites.TryGetValue(crit, out var thing))
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
    public static void ReplaceEatingSound(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdsfld<SoundID>("Slugcat_Eat_Meat_A"))) {
            return;
        }
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((SoundID sound, Creature self) => {
            if (self is Player player && RotCat.tenticleStuff.TryGetValue(player, out var something) && something.isRot)
            {
                Debug.Log("Replacing sound");
                Debug.Log(sound);
                sound = SoundID.Daddy_And_Bro_Tentacle_Grab_Creature;
                Debug.Log(sound);
            }
            return sound;
        });
        if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchLdsfld<SoundID>("Slugcat_Eat_Meat_B"))) {
            return;
        }
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((SoundID sound, Creature self) => {
            if (self is Player player && RotCat.tenticleStuff.TryGetValue(player, out var something) && something.isRot)
            {
                Debug.Log("Replacing 2nd sound");
                Debug.Log(sound);
                sound = SoundID.Daddy_Digestion_Init;
                Debug.Log(sound);
            }
            return sound;
        });
    }
}