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
using System.IO;
using Menu;
using MonoMod.Cil;
using Mono.Cecil.Cil;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace RotCat;

[BepInPlugin("rotcat", "RotCat", "0.0.2")]
public class RotCat : BaseUnityPlugin
{
    bool init = false;
    public static FContainer darkContainer = new FContainer();
    public static FSprite? vignetteEffect;
    public static ConditionalWeakTable<Player, PlayerEx> tenticleStuff = new();
    public static ConditionalWeakTable<Spark, SparkEx> sparkLayering = new();
    public static bool appliedVignette = false;
    public static RotCatOptions RotOptions;
    bool configWorking = false;
    public int timer = 0;
    public void OnEnable()
    {
        On.RainWorld.OnModsInit += Init;
        
        On.Player.NewRoom += CalmTentacles.CalmNewRoom;

        On.DaddyLongLegs.Update += CreatureImmunities.DaddyImmune;

        On.DaddyCorruption.LittleLeg.Update += CreatureImmunities.WallCystImmune;

        On.PlayerGraphics.AddToContainer += PlayerGraphicsHooks.RotAddToContainer;

        On.PlayerGraphics.InitiateSprites += PlayerGraphicsHooks.RotInitiateSprites;

        On.PlayerGraphics.DrawSprites += PlayerGraphicsHooks.RotDrawSprites;

        On.Player.SpitOutOfShortCut += CalmTentacles.CalmSpitOutOfShortCut;

        On.GameSession.ctor += Vignette.GameSessionStartup;

        On.RainWorldGame.ExitToMenu += Vignette.QuitModeBackToMenu;

        On.Room.InGameNoise += Vignette.CystsReacts;

        On.Creature.SuckedIntoShortCut += Vignette.RotCatSuckIntoShortcut;

        On.Creature.SpitOutOfShortCut += Vignette.RotCatSpitOutOfShortcut;

        On.VirtualMicrophone.PlaySound_SoundID_PositionedSoundEmitter_bool_float_float_bool += Vignette.LocateSmallSounds;

        On.Menu.SleepAndDeathScreen.ctor += Vignette.CleanDarkContainerOnSleepAndDeathScreen;

        On.Spark.AddToContainer += (orig, self, sLeaser, rCam, newContainer) => {
            if (sparkLayering.TryGetValue(self, out SparkEx spark) && spark.isHearingSpark) {
                newContainer = darkContainer;
            }
            orig(self, sLeaser, rCam, newContainer);
        };

        On.Player.ctor += (orig, self, abstractCreature, world) =>
        {
            orig(self, abstractCreature, world);
            tenticleStuff.Add(self, new PlayerEx());
            tenticleStuff.TryGetValue(self, out var something);
            if (self.slugcatStats.name.ToString() == "slugrot") {
                something.isRot = true;
            }
            if (something.isRot) {
                self.abstractCreature.tentacleImmune = true;
                something.totalCircleSprites = something.circleAmmount * 4;
                something.tentacleOne = new Line();
                something.tentacleTwo = new Line();
                something.tentacleThree = new Line();
                something.tentacleFour = new Line();
                something.tentacles[0] = something.tentacleOne;
                something.tentacles[1] = something.tentacleTwo;
                something.tentacles[2] = something.tentacleThree;
                something.tentacles[3] = something.tentacleFour;
                something.decorativeTentacles[0] = new Line();
                something.decorativeTentacles[1] = new Line();
                something.randomPosOffest = new Vector2[something.decorativeTentacles.Length*2];
                for (int i = 0; i < something.randomPosOffest.Length; i++) {
                    if (i%2==0) {
                        something.randomPosOffest[i] = new Vector2(Random.Range(0,-8f),Random.Range(-4f,1f));
                        //something.randomPosOffest[i] = new Vector2(-8f,1f);
                    }
                    else {
                        something.randomPosOffest[i] = new Vector2(Random.Range(8f,0),Random.Range(-15f,-4f));
                        //something.randomPosOffest[i] = new Vector2(8f, -15f);
                    }
                }
                //Adds points and sticks to the grabby tentacles
                foreach (var tentacle in something.tentacles) {
                    tentacle.pList = new Point[something.segments];
                    for (int i = 0; i < something.segments; i++) {
                        tentacle.pList[i] = new Point(new Vector2(self.mainBodyChunk.pos.x, self.mainBodyChunk.pos.y-1-i), i==0);
                        tentacle.pList[i].prevPosition = new Vector2(self.mainBodyChunk.pos.x, self.mainBodyChunk.pos.y-i);
                    }
                    tentacle.sList = new Stick[something.segments-1];
                    for (int i = 0; i < tentacle.pList.Length-1; i++) {
                        tentacle.sList[i] = new Stick(tentacle.pList[i], tentacle.pList[i+1], 9.25f);
                    }
                    tentacle.cList = new Circle[something.circleAmmount];//Hard-coded bumps here      Make sure to actually change the index they're being put in if copying
                    //base.Logger.LogDebug(tentacle.pList.Length);
                    tentacle.cList[0] = new Circle(tentacle.pList[8], tentacle.pList[9], new Vector2(3f,10f), true, false, 0.4f);
                    tentacle.cList[1] = new Circle(tentacle.pList[19], tentacle.pList[20], new Vector2(0f,6f), true, false, 0.4f);
                    tentacle.cList[2] = new Circle(tentacle.pList[15], tentacle.pList[16], new Vector2(-1.15f, 3.43f), true, false, 1f, scaleX:0.386f, scaleY:0.54f);
                    tentacle.cList[3] = new Circle(tentacle.pList[8], tentacle.pList[9], new Vector2(3f,10f), false, false, 0.3f);
                    tentacle.cList[4] = new Circle(tentacle.pList[19], tentacle.pList[20], new Vector2(0f,6f), false, false, 0.3f);
                    tentacle.cList[5] = new Circle(tentacle.pList[22], tentacle.pList[23], new Vector2(1,5f), false, false, 0.4f, scaleY:0.9f);
                    tentacle.cList[6] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(-4f,4f), false, false, 0.55f);
                    tentacle.cList[7] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(5f,12f), false, false, 0.55f);
                    tentacle.cList[8] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(-2f,12.3f), false, false, 0.475f);
                    tentacle.cList[9] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(3f,4f), false, false, 0.25f);
                    tentacle.cList[10] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(0.5f,9.1f), false, false, 0.445f);
                    tentacle.cList[11] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(2.25f,-0.6f), false, false, 0.389f);
                    tentacle.cList[12] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(-0.9f,-3f), false, false, 0.385f);
                    tentacle.cList[13] = new Circle(tentacle.pList[21], tentacle.pList[22], new Vector2(-2.15f,3.35f), false, false, 1f, scaleX:0.13f, scaleY:0.36f, lightgrayscale: true);
                    tentacle.cList[14] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(-0.5f,9.3f), false, true, 0.455f);
                    tentacle.cList[15] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(-4f,4f), false, true, 0.35f);
                    tentacle.cList[16] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(5f,12f), false, true, 0.35f);
                    tentacle.cList[17] = new Circle(tentacle.pList[23], tentacle.pList[24], new Vector2(2.25f,-0.6f), false, true, 0.229f);
                    tentacle.cList[18] = new Circle(tentacle.pList[21], tentacle.pList[22], new Vector2(-0.95f,3.1f), false, true, 1f, scaleX:0.23f, scaleY:0.66f);

                }
                //Adds points and sticks to the decorative tentacles
                foreach (var tentacle in something.decorativeTentacles) {
                    tentacle.pList = new Point[something.decorationSegments];
                    for (int i = 0; i < tentacle.pList.Length; i++) {
                        tentacle.pList[i] = new Point(new Vector2(self.mainBodyChunk.pos.x, self.mainBodyChunk.pos.y-1-i), i==0||i==(tentacle.pList.Length-1)?true:false);
                    }
                    tentacle.sList = new Stick[tentacle.pList.Length-1];
                    for (int i = 0; i < tentacle.pList.Length-1; i++) {
                        tentacle.sList[i] = new Stick(tentacle.pList[i], tentacle.pList[i+1], 5f);
                    }
                    tentacle.decoPushDirection = Vector2.right * Random.Range(-0.7f,0.7f) * 0;  //Curently unused because it doesn't look the best. To enable change the 0 to something.decorativeTentacles[i].decoPushDirection *I think
                }
                self.slugcatStats.runspeedFac = 0.65f;
                self.slugcatStats.corridorClimbSpeedFac = 1.1f;
            }
        };

        /*On.Player.EatMeatUpdate += (orig, self, graspIndex) => {
            orig(self, graspIndex);
            tenticleStuff.TryGetValue(self, out var something);
            Creature creature;
            if (self.grasps[graspIndex].grabbedChunk.owner is not Creature) {return;} else {creature = (self.grasps[graspIndex].grabbedChunk.owner as Creature);}
            if (creature.bodyChunks.Length < 2) {return;}
            else if (!creature.dead) {
                Vector2 vector = creature.bodyChunks[0].pos - creature.bodyChunks[1].pos;
                Vector2 normalVector = vector.normalized;
                Vector2 perpVector = Custom.PerpendicularVector(normalVector);
            }
        };*/

        On.Player.Update += (orig, self, eu) =>
        {
            orig(self, eu);
            tenticleStuff.TryGetValue(self, out var something);
            if (something.isRot && self != null) {
                self.scavengerImmunity = 9999;  //Might want to add a system that calculates this based on rep, since you should be able to become friends if you want
                something.overrideControls = Input.GetKey(RotOptions.tentMovementLeft.Value) || Input.GetKey(RotOptions.tentMovementRight.Value) || Input.GetKey(RotOptions.tentMovementDown.Value) || Input.GetKey(RotOptions.tentMovementUp.Value);
                if (something.grabWallCooldown > 0) { //Doesn't do anything right now, need to get it to play nice with the logic first     //New note, this will probably go unused.
                    something.grabWallCooldown -= 0.5f;
                }
                if (something.hearingCooldown > 0) {
                    something.hearingCooldown--;
                }
                if (something.smolHearingCooldown > 0) {
                    something.smolHearingCooldown--;
                }
                //This whole bit controls the lengthening of the tentacles when the player is standing still, or moving too much
                Functions.TentacleRetraction(self, something, RotOptions);

                //The same, but for the decorative tentacles which have slightly different parameters to follow.
                foreach (var tentacle in something.decorativeTentacles) {
                    int pointer = Array.IndexOf(something.decorativeTentacles, tentacle);
                    Vector2 direction = self.mainBodyChunk.pos - self.bodyChunks[1].pos;
                    Vector2 dirNormalized = direction.normalized;
                    Vector2 perpendicularVector = Custom.PerpendicularVector(direction);
                    foreach (Point p in tentacle.pList)
                    {
                        if (!p.locked && self.room != null) {
                            Vector2 positionBeforeUpdate = p.position;
                            p.position += (p.position - p.prevPosition) * Random.Range(0.9f,1.1f);
                            p.position += (Vector2.down * self.room.gravity * Random.Range(0.9f,1.1f));
                            p.prevPosition = positionBeforeUpdate;
                        }
                        if (Array.IndexOf(tentacle.pList, p) == 0) {
                            p.locked = true;
                            p.position = self.mainBodyChunk.pos + (dirNormalized * something.randomPosOffest[pointer*2].y) + (perpendicularVector * something.randomPosOffest[pointer*2].x);
                        }
                        if (Array.IndexOf(tentacle.pList, p) == tentacle.pList.Length-1) {
                            p.locked = true;
                            p.position = self.mainBodyChunk.pos + (dirNormalized * something.randomPosOffest[(pointer*2)+1].y) + (perpendicularVector * something.randomPosOffest[(pointer*2)+1].x);
                        }
                    }
                }

                if (Input.GetKey(RotOptions.tentMovementEnable.Value) || Input.GetKey(RotOptions.tentMovementAutoEnable.Value)) {
                    Functions.PrimaryTentacleAndPlayerMovement(something, self, RotOptions);
                    float startPos = Functions.FindPos(something.overrideControls, self, RotOptions);    //Finds the position around the player to start, based on Sine and Cosine intervals of pi/4
                    Functions.TentaclesFindPositionToGoTo(something, self, startPos);
                    Functions.MoveTentacleToPosition(something, self);
                }
                else {
                    something.automateMovement = false;
                }

                //Physics for the individual points and Rot Bulbs
                int numIterations = 10;
                foreach (var tentacle in something.tentacles) {
                    foreach (Point p in tentacle.pList)
                    {
                        p.Update(something, self, RotOptions, tentacle);
                    }
                
                    //base.Logger.LogDebug("Offset here");
                    foreach (Circle spot in tentacle.cList) {
                        spot.Update();
                    }
                }
                //Physics for the sticks of all tentacles, which affects the points
                for (int i = 0; i < numIterations; i++) {
                    Line[][] totalTentacles = {something.tentacles, something.decorativeTentacles};
                    Functions.StickCalculations(totalTentacles);
                }
            }
        };

        #region Don't Mind this lol
        /*On.RoomCamera.ctor += (orig, self, game, cameraNumber) => {   //Leftover from when I was doing a slight amount of trolling :3
            orig(self, game, cameraNumber);
            FSprite replacementBackground = new FSprite("bgreplace", false);
            replacementBackground.x = 700f;
            //self.backgroundGraphic = replacementBackground;
            //replacementBackground.anchorX = 350f;
            //self.ReturnFContainer("Foreground").RemoveChild(self.backgroundGraphic);
            //self.backgroundGraphic = new FSprite("bgreplace", false);
            //self.backgroundGraphic.shader = game.rainWorld.Shaders["Background"];
            //self.backgroundGraphic.anchorX = 0f;
            //self.backgroundGraphic.anchorY = 0f;
            //self.backgroundGraphic.scale = 4f;
            self.ReturnFContainer("Shadows").AddChild(replacementBackground);
            replacementBackground.MoveToBack();
        };*/
        /*On.RoomCamera.ApplyPositionChange += (orig, self) => {
            orig(self);
            Texture2D newTexture = new Texture2D(400, 225, TextureFormat.ARGB32, false);
            var filePath = AssetManager.ResolveFilePath("textures/thumbnail.png");
            if(File.Exists(filePath)) {
                var rawData = File.ReadAllBytes(filePath);
                newTexture.LoadImage(rawData);
                Debug.Log(self.bkgwww + " " + newTexture);
                self.bkgwww.LoadImageIntoTexture(newTexture);
            }
            else {
                Debug.Log("File path " + filePath + " does not exist!");
            }
        };*/
        /*On.RoomCamera.Update += (orig, self) => {
            orig(self);
            backgroundAtlas._elementsByName.TryGetValue("lightgrayscalesprite", out var element);
            Debug.Log(element.name);
            self.backgroundGraphic.element = element;
            self.backgroundGraphic.color = Color.white;
            self.backgroundGraphic.shader = self.game.rainWorld.Shaders["Basic"];
            self.backgroundGraphic.scale = 80f;
            self.backgroundGraphic.SetPosition(20, 20);
        };*/
        /*On.RoomCamera.Update += (orig, self) => {
            orig(self);
            //self.backgroundGraphic.SetAnchor(-0.4f, -0.2f);
            if (self.bkgwww != null) {
                self.bkgwww.Dispose();
                self.bkgwww = null;
            }
            string filePath = AssetManager.ResolveFilePath("textures/blank.png");
            self.bkgwww = new WWW(filePath);


            int updatePerFrame = 5;
            int amountOfFrames = 7;
            string frame = "frame" + (timer/updatePerFrame).ToString();
            Debug.Log(frame);
            self.backgroundGraphic.SetElementByName(frame);
            Debug.Log(self.backgroundGraphic.GetAnchor());
            Debug.Log("timer is: " + timer);
            timer++;
            if (timer>(amountOfFrames*updatePerFrame)-1) {
                timer = 0;
                Debug.Log("Reset timer");
            }
        };*/
        /*On.RoomCamera.Update += (orig, self) => {
            orig(self);
            int updatePerFrame = 5;
            int amountOfFrames = 7;
            string text = AssetManager.ResolveFilePath("textures/frame"+timer+".png");
            if (File.Exists(text) && self.www == null && self.quenedTexture != text) {
                self.www = new WWW(text);
                self.applyPosChangeWhenTextureIsLoaded = true;
            }
            timer++;
            if (timer>(amountOfFrames*updatePerFrame)-1) {
                timer = 0;
                Debug.Log("Reset timer");
            }
        };*/
        #endregion
    }
    
    private void Init(On.RainWorld.orig_OnModsInit orig, RainWorld self) {
        orig(self);

        if (!init) {
            init = true;
            try {
                //AssetManager.ResolveFilePath("atlases/mane.png");
                //Futile.atlasManager.LoadAtlas("textures/beecatboing");
                Futile.atlasManager.LoadAtlas("atlases/lightgrayscalesprite");
                Futile.atlasManager.LoadAtlas("atlases/grayscalesprite");
                Futile.atlasManager.LoadAtlas("atlases/roteyeeye");
                Futile.atlasManager.LoadAtlas("atlases/vignette");
                Futile.atlasManager.LoadAtlas("atlases/RotFace");
                Futile.atlasManager.LoadAtlas("atlases/roteye");
                //Futile.atlasManager.LoadAtlas("atlases/body");
                //Futile.atlasManager.LoadAtlas("atlases/mane");
                //Futile.atlasManager.LoadAtlas("atlases/bgreplace");
                RotOptions = new RotCatOptions(this, Logger);
                MachineConnector.SetRegisteredOI("rotcat", RotOptions);
                configWorking = true;
            } catch (Exception err) {
                base.Logger.LogError(err);
                configWorking = false;
            }
        }
    }
}
public class PlayerEx
{
    public Line tentacleOne;    //These honestly don't need to be here, since they are immediantly grouped into the tentacles array
    public Line tentacleTwo;    //These honestly don't need to be here, since they are immediantly grouped into the tentacles array
    public Line tentacleThree;  //These honestly don't need to be here, since they are immediantly grouped into the tentacles array
    public Line tentacleFour;   //These honestly don't need to be here, since they are immediantly grouped into the tentacles array
    public Line[] tentacles = new Line[4];  //Array that stores the logic bits of the movement tentacles
    public Line[] decorativeTentacles = new Line[2];    //Array that stores the logic bits of the decorative tentacles
    public int initialDecoLegSprite;    //The position of the initial decorative tentacle in the sLeaser array
    public float retractionTimer = 80;
    public Vector2 previousPosition;    //The previous position of the player, used to control if the tentacles should be retracted based on movement
    public int initialLegSprite;    //The position of the initial movement tentacle in the sLeaser array
    public int segments = 25;   //The amount of points in the movement tentacles.Might be aassumed as 10 in some places, can't remember, so changing this might break something
    public int decorationSegments = 10; //The amount of points in the decorative tentacles. Might be aassumed as 10 in some places, can't remember, so changing this might break something
    public Vector2 potentialGrapSpot;
    public int totalCircleSprites;  //Assigned later, keeps track of the total amount of circle sprites on all the tentacles (Is used solely for resizing the sLeaser array)
    public TargetPos[] targetPos = new TargetPos[4] {new TargetPos(), new TargetPos(), new TargetPos(), new TargetPos()};
    public bool automateMovement = false;   //Determines whether the tentacles will guide themselves toward a wall/pole
    public float grabWallCooldown = 0;  //Currently unused, honestly can't remember what I wanted to do with it
    public Vector2[] randomPosOffest;   //Currently not used, applies a force the the 2 decorative tentacles behind the scug
    public bool overrideControls = false;
    public int circleAmmount = 19;  //Change this to alter the amount of circle sprites that can be made
    public int initialCircleSprite;     //Stores the position of the initial tantacle circle sprite in the sprite list
    public int initialBodyRotSprite;    //Stores the position of the initial bodyRot sprite in the sprite list
    public int bodyRotSpriteAmount = 8;     //Change this whenever I change the amount of Rot sprites on the body
    public BodyRot[] rList;     //An array that stores the logic for the body rot sprites
    public Color rotEyeColor;   //Should store and control the color of the X sprites from the slugbase custom color
    public FAtlas faceAtlas;
    public bool eating = false;
    public float timer = 0f;
    public int hearingCooldown = 0;
    public int smolHearingCooldown = 0;
    public class TargetPos {    //Movement tentacle targeting logic, probably very messing in implementation
        public Vector2 targetPosition = new Vector2(0,0);
        public bool newConnectionSpotFound = false;
        public bool foundSurface = false;
        public bool isPole = false;
    }
    public bool isRot = false;  //Is set to true if the Slugrot character is selected, so it doesn't apply anything to non-rot characters
}
public class Functions {
    public static float FindPos(bool flag, Player self, RotCatOptions options) {
        if ((Input.GetKey(options.tentMovementRight.Value) && flag) || (!flag && self.input[0].x == 1)) {
            if ((Input.GetKey(options.tentMovementUp.Value) && flag) || (!flag && self.input[0].y == 1)) {
                return 0;
            }
            else {
                return 7*(float)Math.PI/4;
            }
        }
        else if ((Input.GetKey(options.tentMovementUp.Value) && flag) || (!flag && self.input[0].y == 1)) {
            if ((Input.GetKey(options.tentMovementLeft.Value) && flag) || (!flag && self.input[0].x == -1)) {
                return (float)Math.PI/2;
            }
            else {
                return (float)Math.PI/4;
            }
        }
        else if ((Input.GetKey(options.tentMovementLeft.Value) && flag) || (!flag && self.input[0].x == -1)) {
            if ((Input.GetKey(options.tentMovementDown.Value) && flag) || (!flag && self.input[0].y == -1)) {
                return (float)Math.PI;
            }
            else {
                return 3*(float)Math.PI/4;
            }
        }
        else if ((Input.GetKey(options.tentMovementDown.Value) && flag) || (!flag && self.input[0].y == -1)) {
            if ((Input.GetKey(options.tentMovementRight.Value) && flag) || (!flag && self.input[0].x == 1)) {
                return 3*(float)Math.PI/2;
            }
            else {
                return 5*(float)Math.PI/4;
            }
        }
        return 0;
    }
    public static void StickCalculations(Line[][] totalTentacles) {
        foreach (Line[] tentacleList in totalTentacles) {
            foreach (Line tentacle in tentacleList) {
                foreach (Stick stick in tentacle.sList) {
                    stick.Update();
                }
            }
        }
    }
    public static void TentacleRetraction(Player self, PlayerEx something, RotCatOptions RotOptions) {
        
        if (Input.GetKey(RotOptions.tentMovementAutoEnable.Value) && something.retractionTimer < 40) {
            something.retractionTimer += 5;
        }

        bool notTooFarNotTooClose = (Custom.Dist(something.previousPosition, self.mainBodyChunk.pos) > 1f && Custom.Dist(something.previousPosition, self.mainBodyChunk.pos) < 3.5f);

        if ((notTooFarNotTooClose || Input.GetKey(RotOptions.tentMovementEnable.Value)) && something.retractionTimer < 60) {//Change limits back to 1f and 4.5f once testing is done
            something.retractionTimer += 0.5f;
        }
        else if (something.retractionTimer > -20 && !Input.GetKey(RotOptions.tentMovementEnable.Value)) {
            something.retractionTimer -= 0.5f;
        }
        if (something.retractionTimer <= 40 && something.retractionTimer > 0) {
            foreach (var tentacle in something.tentacles) {
                foreach (var stick in tentacle.sList)
                {
                    stick.length = Mathf.Lerp(0.15f, 10, something.retractionTimer/40);
                }
            }
        }
        something.previousPosition = self.mainBodyChunk.pos;
    }
    public static void PrimaryTentacleAndPlayerMovement(PlayerEx something, Player self, RotCatOptions RotOptions) {
        if (Input.GetKey(RotOptions.tentMovementAutoEnable.Value) && !Input.GetKey(RotOptions.tentMovementEnable.Value)) {
            something.automateMovement = true;
        }
        if (self.room != null && !(self.room.GetTile(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position).Solid || self.room.GetTile(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position).AnyBeam) && !something.automateMovement) {
            
            int upDown = (Input.GetKey(RotOptions.tentMovementUp.Value)? 1:0) + (Input.GetKey(RotOptions.tentMovementDown.Value)? -1:0);
            int rightLeft = (Input.GetKey(RotOptions.tentMovementRight.Value)? 1:0) + (Input.GetKey(RotOptions.tentMovementLeft.Value)? -1:0);
            
            if (Custom.Dist(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position + new Vector2(3*(something.overrideControls? rightLeft:self.input[0].x), 3*(something.overrideControls? upDown:self.input[0].y)), self.mainBodyChunk.pos) < 300f) {
                
                something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position += new Vector2(3*(something.overrideControls? rightLeft:self.input[0].x), 3*(something.overrideControls? upDown:self.input[0].y));
            }
        }
        else {
            //self.canJump = 0;
            //self.wantToJump = 0;
            self.airFriction = 0.85f;
            self.customPlayerGravity = 0.2f;
            if (self.feetStuckPos != null)  //Stop feet from magneting to the ground
                self.bodyChunks[1].pos = self.feetStuckPos.Value+Vector2.up*2f;
            if (!something.automateMovement) {  //If the tentacle is making first contact, make it go to that position
                something.tentacles[0].iWantToGoThere = something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position;
                something.targetPos[0].foundSurface = true;
                something.targetPos[0].newConnectionSpotFound = true;
            }
            something.automateMovement = true;
            int connectionsToSurface = 0;   //Get how many tentacles are attatched to the terrain/poles
            foreach (var tentacle in something.tentacles) {
                if (tentacle.isAttatchedToSurface == 1) {
                    connectionsToSurface += 1;
                }
            }
            if (connectionsToSurface == 0 && !(Input.GetKey(RotOptions.tentMovementAutoEnable.Value))) {
                something.automateMovement = false;
                //self.controller = new Player.NullController();
            }
            
            #region BodyChunkMovements
            Debug.Log(something.timer);
            if (connectionsToSurface == 0 && Input.GetKey(RotOptions.tentMovementAutoEnable.Value) && !Input.GetKey(RotOptions.tentMovementEnable.Value)) {
                self.customPlayerGravity = 0f;
                self.mainBodyChunk.vel -= new Vector2(0f, 0.1f);
            }
            else if (!Input.GetKey(RotOptions.tentMovementAutoEnable.Value) || Input.GetKey(RotOptions.tentMovementEnable.Value)) {
                if (self.input[0].x != 0 || self.input[0].y != 0 && something.timer < 1f) {
                    something.timer += 0.1f;
                }
                else if (something.timer > 0f) {
                    something.timer -= 0.1f;
                }
                self.mainBodyChunk.vel = Vector2.Lerp(new Vector2(0,0.86f), new Vector2(2.3f*connectionsToSurface*self.input[0].x, 2.3f*connectionsToSurface*self.input[0].y), something.timer);
                foreach (var chunk in self.bodyChunks)
                {
                    if (chunk != self.mainBodyChunk)
                    {
                        //base.Logger.LogDebug(self.room.GetTile(chunk.pos + new Vector2(0,-1f)).Solid);
                        if (self.room != null && !self.room.GetTile(chunk.pos + Vector2.down).Solid)
                        {
                            chunk.vel = Vector2.down * self.customPlayerGravity * self.room.gravity / self.airFriction;
                        }
                        else
                        {
                            chunk.vel = new Vector2(0, 0.88f);
                        }
                    }
                }
            }
            #endregion
        }
    }
    public static void TentaclesFindPositionToGoTo(PlayerEx something, Player self, float startPos) {
        for (int i = 0; i < something.targetPos.Length; i++) {
            if (something.targetPos[i].foundSurface && (Custom.Dist(self.mainBodyChunk.pos, something.targetPos[i].targetPosition) >= 250 || Custom.Dist(self.mainBodyChunk.pos, something.tentacles[i].iWantToGoThere) >= 250)) {
                something.targetPos[i].foundSurface = false;
                something.targetPos[i].newConnectionSpotFound = false;
            }
            for (float k = 0; k < 200; k++) {
                for (float j = startPos + (float)Math.PI/8*(i); j < startPos + (float)Math.PI/8*(i+1); j+=((float)Math.PI)/256f) {
                    if (self.room != null && (self.room.GetTile(new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y)).Solid || self.room.GetTile(new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y)).AnyBeam) && !something.targetPos[i].foundSurface) {
                        if (self.room.GetTile(new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y)).AnyBeam) {
                            something.targetPos[i].isPole = true;
                        }
                        else {
                            something.targetPos[i].isPole = false;
                        }   //These two are technically the same I think
                        //something.targetPos[i].isPole = (self.room.GetTile(new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y)).AnyBeam);
                        something.targetPos[i].targetPosition = new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y) + (something.targetPos[i].isPole? (new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y)-self.mainBodyChunk.pos).normalized * 5 : new Vector2(0,0));
                        something.targetPos[i].foundSurface = true;
                    }
                    /*else if (!something.targetPos[i].foundSurface) {
                        something.targetPos[i].targetPosition = self.mainBodyChunk.pos - Vector2.down * 5;
                    }*/
                }
            }
            //base.Logger.LogDebug(something.targetPos[i]);
            //self.room.AddObject(new Spark(something.targetPos[i].targetPosition, new Vector2(-5,5), Color.green, null, 10, 20));
            //self.room.AddObject(new Spark(something.targetPos[i].targetPosition, new Vector2(5,5), Color.green, null, 10, 20));
        }
    }
    public static void MoveTentacleToPosition(PlayerEx something, Player self) {
        for (int i = 0; i < something.tentacles.Length; i++) {
            //base.Logger.LogDebug(something.targetPos[i].isPipe);
            //base.Logger.LogDebug(Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, something.tentacles[i].iWantToGoThere));
            if (!something.targetPos[i].newConnectionSpotFound && something.automateMovement) {
                something.targetPos[i].newConnectionSpotFound = true;
                something.tentacles[i].iWantToGoThere = something.targetPos[i].targetPosition;
                //self.room.AddObject(new Spark(something.tentacles[i].iWantToGoThere, new Vector2(5,5), Color.blue, null, 10, 20));  //Testing
            }
            if (Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, something.tentacles[i].iWantToGoThere) > (something.targetPos[i].isPole? 0f:5f) && something.automateMovement) {
                something.tentacles[i].isAttatchedToSurface = 0;
                Vector2 direction = (something.tentacles[i].iWantToGoThere - something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position);
                
                something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position += direction / ((Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, something.tentacles[i].iWantToGoThere) > 5f)? 9f:1f); //Tentacle Tip, controls speed tentacles move to their target pos
                
                something.tentacles[i].canPlaySound = true;
                //base.Logger.LogDebug(direction);
            }
            if (self.room != null && Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, something.tentacles[i].iWantToGoThere) < 15f) {    //Casually giving the player some lenience
                something.tentacles[i].isAttatchedToSurface = 1;
                if (something.tentacles[i].canPlaySound) {
                    self.room.PlaySound(SoundID.Daddy_And_Bro_Tentacle_Grab_Terrain, something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, 1f, 1f);
                    something.tentacles[i].canPlaySound = false;
                }
            }
        }
    }
    public static void DrawFace(PlayerEx something, RoomCamera.SpriteLeaser sLeaser, string name) {
        if (name != null && name.StartsWith("Face") && something.faceAtlas._elementsByName.TryGetValue("Rot" + name, out var element)) {
            sLeaser.sprites[9].element = element;
            //base.Logger.LogDebug(element.name);
            //base.Logger.LogDebug(sLeaser.sprites[9].scaleX);
            if(sLeaser.sprites[9].scaleX < 0) {
                if(element.name == "RotFaceA0" || element.name == "RotFaceA8" || element.name == "RotFaceB0" || element.name == "RotFaceB8" || element.name == "RotFaceStunned") {
                    sLeaser.sprites[9].scaleX = 1f;
                }
                else if(element.name.StartsWith("RotFaceA")) {
                    char num = element.name[8];
                    //base.Logger.LogDebug(element.name.Substring(0,7));
                    //base.Logger.LogDebug(num);
                    sLeaser.sprites[9].element = something.faceAtlas._elementsByName[element.name.Substring(0,7)+"E"+num];
                    sLeaser.sprites[9].scaleX = 1f;
                }
                else if(element.name.StartsWith("RotFaceB")) {
                    char num = element.name[8];
                    //base.Logger.LogDebug(element.name.Substring(0,7));
                    //base.Logger.LogDebug(num);
                    sLeaser.sprites[9].element = something.faceAtlas._elementsByName[element.name.Substring(0,7)+"F"+num];
                    sLeaser.sprites[9].scaleX = 1f;
                }
            }
        }
    }
    public static void DrawTentacleCircles(PlayerEx something, Vector2 camPos, FSprite[] tentacle1Circles, FSprite[] tentacle2Circles, FSprite[] tentacle3Circles, FSprite[] tentacle4Circles) {
        //Set the circle positions on the tentacles and color them
        for (int i = 0; i < something.tentacles.Length; i++) {
            for (int j = 0; j < something.tentacles[i].cList.Length; j++) {
                //base.Logger.LogDebug("Drawsprites");
                //base.Logger.LogDebug(something.tentacles[i].cList[j].position);
                if (i == 0) {
                    Vector2 vector = (something.tentacles[i].cList[j].pointA.position-something.tentacles[i].cList[j].pointB.position).normalized;
                    bool rotationSide = vector.x < 0;
                    tentacle1Circles[j].SetPosition(something.tentacles[i].cList[j].position - camPos);
                    tentacle1Circles[j].color = GetColor(something.rotEyeColor, something.tentacles[i].cList[j].brightBackground, something.tentacles[i].cList[j].darkBackground);
                    tentacle1Circles[j].rotation = Mathf.Rad2Deg * -Mathf.Atan(vector.y / vector.x) - 90 + (rotationSide? -180f:0f) + something.tentacles[i].cList[j].rotation;
                    if (something.tentacles[i].cList[j].grayscale) {
                        tentacle1Circles[j].element = Futile.atlasManager.GetElementWithName("lightgrayscalesprite");
                    }
                }
                if (i == 1) {
                    Vector2 vector = (something.tentacles[i].cList[j].pointA.position-something.tentacles[i].cList[j].pointB.position).normalized;
                    bool rotationSide = vector.x < 0;
                    tentacle2Circles[j].SetPosition(something.tentacles[i].cList[j].position - camPos);
                    //self.player.room.AddObject(new Spark(tentacle2Circles[j].GetPosition() - new Vector2(20f,13f), new Vector2(-5,5), Color.cyan, null, 10, 20));
                    tentacle2Circles[j].color = GetColor(something.rotEyeColor, something.tentacles[i].cList[j].brightBackground, something.tentacles[i].cList[j].darkBackground);
                    tentacle2Circles[j].rotation = Mathf.Rad2Deg * -Mathf.Atan(vector.y / vector.x) - 90 + (rotationSide? -180f:0f);
                    if (something.tentacles[i].cList[j].grayscale) {
                        tentacle2Circles[j].element = Futile.atlasManager.GetElementWithName("lightgrayscalesprite");
                    }
                }
                if (i == 2) {
                    Vector2 vector = (something.tentacles[i].cList[j].pointA.position-something.tentacles[i].cList[j].pointB.position).normalized;
                    bool rotationSide = vector.x < 0;
                    tentacle3Circles[j].SetPosition(something.tentacles[i].cList[j].position - camPos);
                    //self.player.room.AddObject(new Spark(tentacle3Circles[j].GetPosition() - new Vector2(20f,13f), new Vector2(-5,5), Color.cyan, null, 10, 20));
                    tentacle3Circles[j].color = GetColor(something.rotEyeColor, something.tentacles[i].cList[j].brightBackground, something.tentacles[i].cList[j].darkBackground);
                    tentacle3Circles[j].rotation = Mathf.Rad2Deg * -Mathf.Atan(vector.y / vector.x) - 90 + (rotationSide? -180f:0f);
                    if (something.tentacles[i].cList[j].grayscale) {
                        tentacle3Circles[j].element = Futile.atlasManager.GetElementWithName("lightgrayscalesprite");
                    }
                }
                if (i == 3) {
                    Vector2 vector = (something.tentacles[i].cList[j].pointA.position-something.tentacles[i].cList[j].pointB.position).normalized;
                    bool rotationSide = vector.x < 0;
                    tentacle4Circles[j].SetPosition(something.tentacles[i].cList[j].position - camPos);
                    //self.player.room.AddObject(new Spark(tentacle4Circles[j].GetPosition() - new Vector2(20f,13f), new Vector2(-5,5), Color.cyan, null, 10, 20));
                    tentacle4Circles[j].color = GetColor(something.rotEyeColor, something.tentacles[i].cList[j].brightBackground, something.tentacles[i].cList[j].darkBackground);
                    tentacle4Circles[j].rotation = Mathf.Rad2Deg * -Mathf.Atan(vector.y / vector.x) - 90 + (rotationSide? -180f:0f);
                    if (something.tentacles[i].cList[j].grayscale) {
                        tentacle4Circles[j].element = Futile.atlasManager.GetElementWithName("lightgrayscalesprite");
                    }
                }
            }
        }
    }
    public static void UpdateVignette(RainWorld game, Player self, Color col, float vignetteRad, Vector2 camPos) {
        float rVar = (self.mainBodyChunk.pos.x-camPos.x)/game.screenSize.x;
        float gVar = (self.mainBodyChunk.pos.y-camPos.y)/game.screenSize.y;
        RotCat.vignetteEffect.color = new Color(rVar, gVar, col.b, vignetteRad);
        //Debug.Log($"Update Vignette. rVar: {rVar} gVar: {gVar} bodyX: {self.mainBodyChunk.pos.x-camPos.x} bodyY: {self.mainBodyChunk.pos.y-camPos.y}");
    }
    public static Color GetColor(Color rotEyeColor, bool brightBackground, bool darkBackground) {
        float r = rotEyeColor.r, g = rotEyeColor.g, b = rotEyeColor.b;
        if (brightBackground) {
            return rotEyeColor; //Default value: 27/255, 11/255, 253/255
        }
        else if (darkBackground) {
            if (r > g && r > b) {
                r /= 5f;
            }
            else if (g > r && g > b) {
                g /= 5f;
            }
            else if (b > r && b > g) {
                b /= 5f;
            }
            return new Color(r, g, b); //new Color((float)27/255, (float)11/255, (float)55/255);
        }
        else {
            if (r > g && r > b) {
                r /= 1.64f;
            }
            else if (g > r && g > b) {
                g /= 1.64f;
            }
            else if (b > r && b > g) {
                b /= 1.64f;
            }
            return new Color(r, g, b); //new Color((float)27/255, (float)11/255, (float)153/255);
        }
    }
}
public class SparkEx {
    public bool isHearingSpark;
    public SparkEx(bool flag) {
        this.isHearingSpark = flag;
    }
}

//These bits are for the extra sprite logic/data storage
public class Point
{
    public Vector2 position = new Vector2(700,200);
    public Vector2 prevPosition = new Vector2(701,200);
    public bool locked = false;
    public Point(Vector2 position, bool locked) {
        this.position = position;
        this.locked = locked;
    }
    public void Update(PlayerEx something, Player self, RotCatOptions RotOptions, Line tentacle) {
        if (Array.IndexOf(tentacle.pList, this) == tentacle.pList.Length-1 && ((Input.GetKey(RotOptions.tentMovementEnable.Value) || Input.GetKey(RotOptions.tentMovementAutoEnable.Value)) && something.targetPos[Array.IndexOf(something.tentacles, tentacle)].foundSurface && ((Array.IndexOf(something.tentacles, tentacle) == 0) || something.automateMovement))) {  //If it is the very last point in the list, the tentacle tip
            this.locked = true;
        }
        else {
            this.locked = false;
        }
        if (!this.locked && self.room != null) {
            Vector2 positionBeforeUpdate = this.position;
            this.position += (this.position - this.prevPosition) * Random.Range(0.9f,1.1f);
            this.position += Vector2.down * self.room.gravity * Random.Range(0.15f,0.3f);
            this.prevPosition = positionBeforeUpdate;
        }
        if (Array.IndexOf(tentacle.pList, this) == 0) {
            this.position = self.mainBodyChunk.pos;
        }
    }
}
public class Stick {
    public Point pointA, pointB;
    public float length;
    public Stick(Point pointA, Point pointB, float length) {
        this.pointA = pointA;
        this.pointB = pointB;
        this.length = length;
    }
    public void Update() {
        Vector2 stickCenter = (this.pointA.position + this.pointB.position)/2;
        Vector2 stickDir = (this.pointA.position - this.pointB.position).normalized;
        if (!this.pointA.locked)
            this.pointA.position = stickCenter + stickDir * this.length / 2;
        if (!this.pointB.locked)
            this.pointB.position = stickCenter - stickDir * this.length / 2;
        }
}
public class Line {
    public Point[] pList;
    public Stick[] sList;
    public Circle[] cList;
    public Vector2 iWantToGoThere;
    public int isAttatchedToSurface = 0;
    public Vector2 decoPushDirection = new Vector2(0,0);
    public bool canPlaySound = true;
}
public class BodyRot {
    public BodyRot (FSprite chunk1, FSprite chunk2, Vector2 offset, float scale/*, int bodyRotEyePosInSpriteList = null*/) {
        this.chunk1 = chunk1;
        this.chunk2 = chunk2;
        this.offset = offset;
        this.scale = scale;
        /*this.bodyRotEyePosInSpriteList = bodyRotEyePosInSpriteList*/
    }
    public void Update() {
        
    }
    public FSprite chunk1;
    public FSprite chunk2;
    public Vector2 offset;
    public float scale;
    //public int bodyRotEyePosInSpriteList;
}
public class Circle {
    public Circle (Point pointA, Point pointB, Vector2 offset, bool background, bool brightBackground, float scale, float scaleX = 1f, float scaleY = 1f, bool lightgrayscale = false, float rotation = 0) {
        this.pointA = pointA;
        this.pointB = pointB;
        this.offset = offset;
        this.darkBackground = background;
        this.scale = scale;
        this.brightBackground = brightBackground;
        this.grayscale = lightgrayscale;
        this.rotation = rotation;
        if (scaleX == 1) {
            this.scaleX = scale;
        }
        else {
            this.scaleX = scaleX;
        }
        if (scaleY == 1) {
            this.scaleY = scale;
        }
        else {
            this.scaleY = scaleY;
        }
    }
    public void Update() {
        Vector2 direction = this.pointB.position - this.pointA.position;
        Vector2 dirNormalized = direction.normalized;
        Vector2 perpendicularVector = Custom.PerpendicularVector(direction);
        this.position = this.pointA.position + (dirNormalized * this.offset.y) + (perpendicularVector * this.offset.x);
    }
    public Point pointA;
    public Point pointB;
    public Vector2 offset;
    public bool darkBackground;
    public bool brightBackground;
    public Vector2 position;
    public Vector2 newPosition;
    public float scale;
    public float scaleX;
    public float scaleY;
    public bool grayscale;
    public float rotation;
}
//End of file