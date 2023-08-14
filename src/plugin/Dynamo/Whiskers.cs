using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using RWCustom;

namespace Chimeric
{
    internal class DynamoWhiskers
    {
        public static void Hooks()
        {
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
        }

        public static ConditionalWeakTable<Player, Whiskerdata> whiskerStorage = new ConditionalWeakTable<Player, Whiskerdata>();
        public class Whiskerdata
        {
            public bool ready = false;
            public int initialWhiskerIndex; //initial location for each sprite!!
            public string spriteName = "LizardScaleA0"; //just for changing out what sprite is used
            public Scale[] gills = new Scale[4];
            public Whiskerdata(PlayerGraphics playerG) //sets up a weak reference to the player.
            {
                for (int i = 0; i < gills.Length; i++)
                {
                    gills[i] = new Scale(playerG, new Vector2(i<gills.Length/2? 0.7f : -0.7f, i<gills.Length/2? 0.035f : 0.026f));
                }
            }
            //scales are literaly stolen from rivulet's gill scales :]
            public class Scale : BodyPart
            {
                public float length = 10f;
                public Vector2 positionAroundHead = new ();
                public Scale(GraphicsModule cosmetics, Vector2 position) : base(cosmetics)
                {
                    positionAroundHead = position;
                }
                public override void Update()
                {
                    base.Update();
                    if (this.owner.owner.room.PointSubmerged(this.pos))
                    {
                        this.vel *= 0.5f;
                    }
                    else
                    {
                        this.vel *= 0.9f;
                    }
                    this.lastPos = this.pos;
                    this.pos += this.vel;
                }
            }
            public int WhiskerSprite(int side, int pair)
            {
                return initialWhiskerIndex + side + gills.Length/2*pair;
            }
        }

        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (Plugin.tenticleStuff.TryGetValue(self.player, out var something) && something.isDynamo)
            {
                whiskerStorage.Add(self.player, new Whiskerdata(self)); //setup the CWT
            }
        }
        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (Plugin.tenticleStuff.TryGetValue(self.player, out var something) && something.isDynamo && whiskerStorage.TryGetValue(self.player, out var whiskerData)) {
                whiskerData.initialWhiskerIndex = sLeaser.sprites.Length;
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 4);  //add on more space for our sprites
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        sLeaser.sprites[whiskerData.WhiskerSprite(i, j)] = new FSprite(whiskerData.spriteName) {
                            scaleX = i == 1 ? 0.5f : -0.5f,
                            scaleY = 0.875f,
                            anchorY = 0.1f
                        };
                    }
                }
                whiskerData.ready = true; //say that we're ready to add these to the container!
                self.AddToContainer(sLeaser, rCam, null); //then add em!
            }
        }
        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (Plugin.tenticleStuff.TryGetValue(self.player, out var something) && something.isDynamo && whiskerStorage.TryGetValue(self.player, out Whiskerdata whiskerData) && whiskerData.ready) //make sure to check that we're ready, same as checking for the length of the sprite array.
            {                
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        sLeaser.sprites[whiskerData.WhiskerSprite(i, j)].RemoveFromContainer();
                        rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[whiskerData.WhiskerSprite(i, j)]);
                        sLeaser.sprites[whiskerData.WhiskerSprite(i, j)].MoveBehindOtherNode(sLeaser.sprites[9]);
                    }
                }
                whiskerData.ready = false; //set ready to false for next time.
            }
        }
        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (Plugin.tenticleStuff.TryGetValue(self.player, out var something) && something.isDynamo && whiskerStorage.TryGetValue(self.player, out Whiskerdata whiskerData))
            {
                //oh god i need to rewrite all of this into one for loop. <-- past me is right, you probably want to do this better. its a mess.    Thanks Leo.
                int index = 0;
                for (int i = 0; i < 2; i++) //as i said before, basically just rivy's code.
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Vector2 vector = sLeaser.sprites[9].GetPosition();
                        vector.x += i==0? -5f : 5f;
                        sLeaser.sprites[whiskerData.WhiskerSprite(i, j)].SetPosition(vector);
                        sLeaser.sprites[whiskerData.WhiskerSprite(i, j)].rotation = Custom.AimFromOneVectorToAnother(vector + camPos, Vector2.Lerp(whiskerData.gills[index].lastPos, whiskerData.gills[index].pos, timeStacker)) + (i==0? 0f : 180f);
                        sLeaser.sprites[whiskerData.WhiskerSprite(i, j)].color = SlugBase.DataTypes.PlayerColor.GetCustomColor(self, 2);
                        index++;
                    }
                }
            }
        }
        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            if (Plugin.tenticleStuff.TryGetValue(self.player, out var something) && something.isDynamo && whiskerStorage.TryGetValue(self.player, out Whiskerdata whiskerData))
            {
                int index = 0; // once again we are in horrid loop hell. ew.
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Vector2 bodyPos = self.owner.bodyChunks[0].pos; //the lost got a whisker transplant from rivvy... now rivvy has no whiskers so sad....
                        float rotationOffset = 90f / (whiskerData.gills.Length / 2f);   // always 45, for now
                        bodyPos.x += i==0? -5f : 5f;
                        bodyPos.x -= (self.player.bodyMode == Player.BodyModeIndex.Crawl || self.player.bodyMode == Player.BodyModeIndex.CorridorClimb)? 5f : 0;    // Fix gitteriness when crawling to the left.
                        Vector2 a = Custom.rotateVectorDeg(Vector2.up, j * rotationOffset + 90f / (whiskerData.gills.Length / 2f)); // Left side whiskers?
                        // When j is 0, this is (0.7,0.7). When j is 1, this is (1,0)
                        Vector2 vector = Custom.rotateVectorDeg(Vector2.up, j * rotationOffset - 90f / (whiskerData.gills.Length / 2f));    // Right side whiskers?
                        // When j is 0, this is (-0.7,0.7), when j is 1, this is (0,1)
                        Vector2 a2 = Custom.DirVec(self.owner.bodyChunks[1].pos, bodyPos);  // Starts as the direction from the feet bodyChunk to the head bodyChunk (+ offset).
                        if (whiskerData.gills[index].positionAroundHead.y < 0.2f)  // Always true, for some reason lol.
                        {
                            a2 -= a * Mathf.Pow(Mathf.InverseLerp(0.2f, 0f, whiskerData.gills[index].positionAroundHead.y), 2f) * 2f;
                        }
                        a2 = Vector2.Lerp(a2, vector, 0.0875f).normalized;
                        Vector2 vector2 = bodyPos + a2 * whiskerData.gills.Length;
                        if (!Custom.DistLess(whiskerData.gills[index].pos, vector2, whiskerData.gills[index].length / 2f))
                        {
                            Vector2 a3 = Custom.DirVec(whiskerData.gills[index].pos, vector2);
                            float num5 = Vector2.Distance(whiskerData.gills[index].pos, vector2);
                            float num6 = whiskerData.gills[index].length / 2f;
                            whiskerData.gills[index].pos += a3 * (num5 - num6);
                            whiskerData.gills[index].vel += a3 * (num5 - num6);
                        }
                        whiskerData.gills[index].vel += Vector2.ClampMagnitude(vector2 - whiskerData.gills[index].pos, 10f) / Mathf.Lerp(5f, 1.5f, 0.5873646f);
                        whiskerData.gills[index].vel *= Mathf.Lerp(1f, 0.8f, 0.5873646f);
                        whiskerData.gills[index].ConnectToPoint(bodyPos, whiskerData.gills[index].length, true, 0f, new Vector2(0f, 0f), 0f, 0f);
                        whiskerData.gills[index].Update();
                        index++;
                    }
                }
            }
        }
    }
}