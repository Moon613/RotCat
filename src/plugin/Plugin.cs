using BepInEx;
using UnityEngine;
using RWCustom;
using System.Security;
using System.Security.Permissions;
using System;
using System.Runtime.CompilerServices;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using Steamworks;
using Fisobs.Core;
using MoreSlugcats;
using MonoMod.Cil;
using System.IO;
using MonoMod.RuntimeDetour;
using static System.Reflection.BindingFlags;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete

namespace Chimeric
{
    [BepInPlugin("moon.chimeric", "RotCat", "0.0.2")]
    public class Plugin : BaseUnityPlugin
    {
        bool init = false;
        public static ConditionalWeakTable<Player, PlayerEx> tenticleStuff = new ConditionalWeakTable<Player, PlayerEx>();
        public static ConditionalWeakTable<Spark, SparkEx> sparkLayering = new ConditionalWeakTable<Spark, SparkEx>();
        public static ConditionalWeakTable<Creature, CreatureEx> creatureYummersSprites = new ConditionalWeakTable<Creature, CreatureEx>();
        public static FContainer darkContainer = new FContainer();
        public static FSprite? vignetteEffect;
        public static bool appliedVignette = false;
        public static ChimericOptions? ChimericOptions;
        bool configWorking = false;
        public static bool loaded = false;
        public const string ROT_NAME = "slugrot";
        public const string DYNAMO_NAME = "dynamo";
        public void OnEnable()
        {
            ConversationOverrides.Hooks();  // Won't do anything until the bool in the slugbase json is changed to true
            ConversationID.RegisterValues();
            SoundEnums.RegisterValues();
            Content.Register(new BabyAquapede());
            On.RainWorld.OnModsInit += OnModsInit;
            CreatureImmunities.Apply();
            RotGraphicsHooks.Apply();
            Vignette.Apply();
            DigestionRotSprites.Apply();
            CalmTentacles.Apply();
            SlugRot.Apply();
            Dynamo.Apply();
            DynamoWhiskers.Hooks();
            On.Player.ctor += PlayerCtor;



            // Can be used to change sleep screen stuff, may end up using this later
            /*On.Menu.MenuScene.BuildScene += (orig, self) => {
                orig(self);
                if (self.sceneID == MenuScene.SceneID.SleepScreen)
                {
                    self.depthIllustrations[3] = new Menu.MenuDepthIllustration(self.menu, self, "Scenes" + Path.DirectorySeparatorChar.ToString() + "Sleep Screen - white", "Sleep - 2 - white", new Vector2(0f, 0f), 1.7f, Menu.MenuDepthIllustration.MenuShader.Normal);
                }
            };*/
            
            On.RainWorld.LoadResources += (orig, self) => {
                orig(self);
                if (!loaded)
                {
                    AssetBundle? bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("mods/Chimerical/assetbundles/red"));
                    if (bundle != null) {
                        self.Shaders["Red"] = FShader.CreateShader("red", bundle.LoadAsset<Shader>("Assets/red.shader"));
                    }
                    else {
                        Debug.Log("Shitfuckfcukdamnitnotagain");
                    }
                    loaded = true;
                }
            };

            On.Spark.AddToContainer += (orig, self, sLeaser, rCam, newContainer) => {   //Shouldn't ever cause issues, just adds Spark sprites to the darkContainer if they are created by CreaturePing
                if (sparkLayering.TryGetValue(self, out SparkEx spark) && spark.isHearingSpark) {
                    newContainer = darkContainer;
                }
                orig(self, sLeaser, rCam, newContainer);
            };

            On.Menu.MainMenu.ctor += (orig, self, manager, showRegionSpecificBkg) => {
                orig(self, manager, showRegionSpecificBkg);
                //self.AddMainMenuButton(new Menu.SimpleButton(self, self.pages[0], self.Translate("ROTCAT"), "ROTCAT", Vector2.zero, new Vector2(100f, 30f)), OpenSaysMe, 0);
            };
            
            On.Menu.Remix.MixedUI.OpTab.ctor += (orig, self, owner, name) => {
                orig(self, owner, name);
                if (name == "Slugrot") {
                    //Debug.Log("Yippee!");
                    Debug.Log($"Canvas scale is: {self.CanvasSize}");
                    var sprite = new FSprite("remixmenubreakline", false);
                    //sprite.width = 626f;
                    //sprite.height = 1.1f;
                    sprite.SetPosition(new Vector2(300, 275));
                    self._container.AddChild(sprite);
                }
            };

            //POMDarkness.RegisterDarkness();

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
            /*private static void AddIntroRollImage(ILContext il)
            {
                var cursor = new ILCursor(il);
                if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloc(3)))
                {
                    return;
                }
                cursor.RemoveRange(8);
                if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchLdstr("Intro_Roll_C_")))
                {
                    return;
                }
                cursor.EmitDelegate((string str) =>
                {
                    Debug.Log("Removed instructions and made it to destination, please remain seated with your seatbelts on.");
                    List<string> strArr = new List<string> {
                        "gourmand",
                        "rivulet",
                        "spear",
                        "artificer",
                        "saint"
                    };
                    int prevLength = strArr.Count;
                    strArr.Add("abc123");
                    int randNum = Random.Range(0, strArr.Count);
                    return (randNum > prevLength)? strArr[randNum] : $"Intro_Roll_C_{strArr[randNum]}";
                });
            }*/
            #endregion
        }
        public static void PlayerCtor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world) {
            orig(self, abstractCreature, world);
            Plugin.tenticleStuff.Add(self, new PlayerEx());
            Plugin.tenticleStuff.TryGetValue(self, out var something);
            if (self.slugcatStats.name.value == ROT_NAME) {
                something.isRot = true;
                SlugRot.RotCtor(self, something);
            }
            else if (self.slugcatStats.name.value == DYNAMO_NAME) {
                something.isDynamo = true;
                Dynamo.DynoCtor(self, something);
            }
            else if (self.slugcatStats.name.value == "nine") {
                something.isNine = true;
            }
        }
        public void OpenSaysMe()
        {
            if (SteamManager.Initialized && SteamUtils.IsOverlayEnabled()) {
                SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/filedetails/?id=2949461454");
            }
            else {
                Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=2949461454");
            }
        }
        private void OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self) {
            orig(self);

            if (!init) {
                init = true;
                try {
                    //AssetManager.ResolveFilePath("atlases/mane.png");
                    //Futile.atlasManager.LoadAtlas("textures/beecatboing");
                    Futile.atlasManager.LoadAtlas("atlases/lightgrayscalesprite");
                    Futile.atlasManager.LoadAtlas("atlases/remixmenubreakline");
                    Futile.atlasManager.LoadAtlas("atlases/grayscalesprite");
                    //Futile.atlasManager.LoadAtlas("atlases/bgreplace");
                    Futile.atlasManager.LoadAtlas("atlases/DynamoFins");
                    Futile.atlasManager.LoadAtlas("atlases/roteyeeye");
                    //Futile.atlasManager.LoadAtlas("atlases/vignette");
                    Futile.atlasManager.LoadAtlas("atlases/RotFace");
                    //Futile.atlasManager.LoadAtlas("atlases/mane");
                    //Futile.atlasManager.LoadAtlas("atlases/body");
                    Futile.atlasManager.LoadAtlas("atlases/roteye");
                    ChimericOptions = new ChimericOptions(this, Logger);
                    MachineConnector.SetRegisteredOI("moon.chimeric", ChimericOptions);
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
        public Line[] tentacles = new Line[4];  //Array that stores the logic bits of the movement tentacles
        public Line[] decorativeTentacles = new Line[2];    //Array that stores the logic bits of the decorative tentacles
        public int initialDecoLegSprite;    //The position of the initial decorative tentacle in the sLeaser array
        public int endOfsLeaser;
        public float retractionTimer = 80;
        public Vector2 previousPosition;    //The previous position of the player, used to control if the tentacles should be retracted based on movement
        public int initialLegSprite;    //The position of the initial movement tentacle in the sLeaser array
        public int segments = 25;   //The amount of points in the movement tentacles.Might be aassumed as 10 in some places, can't remember, so changing this might break something
        public int decorationSegments = 10; //The amount of points in the decorative tentacles. Might be aassumed as 10 in some places, can't remember, so changing this might break something
        public Vector2 potentialGrapSpot;   //IT'S NOT USED LMAOOOOOO
        public int totalCircleSprites;  //Assigned later, keeps track of the total amount of circle sprites on all the tentacles (Is used solely for resizing the sLeaser array)
        public TargetPos[] targetPos = new TargetPos[4] {new TargetPos(), new TargetPos(), new TargetPos(), new TargetPos()};
        public bool automateMovement = false;   //Determines whether the tentacles will guide themselves toward a wall/pole
        public float grabWallCooldown = 0;  //Currently unused, honestly can't remember what I wanted to do with it
        [AllowNull] public Vector2[] randomPosOffest;   //Currently not used, applies a force the the 2 decorative tentacles behind the scug
        public bool overrideControls = false;   //If the player starts using custom movement keys instead of the default scug movement, this is set to true and false otherwise
        public int circleAmmount = 19;  //Change this to alter the amount of circle sprites that can be made
        public int initialCircleSprite;     //Stores the position of the initial tantacle circle sprite in the sprite list
        public int initialBodyRotSprite;    //Stores the position of the initial bodyRot sprite in the sprite list
        public int bodyRotSpriteAmount = 8;     //Change this whenever I change the amount of Rot sprites on the body
        [AllowNull] public BodyRot[] rList;     //An array that stores the logic for the body rot sprites
        public Color rotEyeColor;   //Should store and control the color of the X sprites from the slugbase custom color
        [AllowNull] public FAtlas faceAtlas;
        public bool eating = false;
        public float timer = 0f;
        public int hearingCooldown = 0;
        public int smolHearingCooldown = 0;
        public bool canPlayShockSound = true;
        public int swimTimer = 0;
        public float slowX = 0;
        public float slowY = 0;
        public Vector2 prevInput = Vector2.zero;
        public int initialFinSprite;
        public List<Fin> fList = new List<Fin>();
        public int timeInWater;
        public int timeInWaterUpTo80;
        [AllowNull] public AbstractOnTentacleStick stuckCreature;
        public bool crawlToRoll;
        public class TargetPos {    //Movement tentacle targeting logic, probably very messing in implementation. Should also probably be merged into the Line class instead of shoved here
            public Vector2 targetPosition = new Vector2(0,0);
            public bool hasConnectionSpot = false;
            public bool foundSurface = true;
            public bool isPole = false;
        }
        public bool isRot = false;  //Is set to true if the Slugrot character is selected, so it doesn't apply anything to non-rot characters
        public bool isDynamo = false;
        public bool isNine = false;
    }
    public class Functions {
        ///<summary> Find the position for tentacles to start finding connection points at </summary>
        public static float FindPos(bool flag, Player self) {
            if ((Input.GetKey(ChimericOptions.tentMovementRight.Value) && flag) || (!flag && self.input[0].x == 1)) {
                if ((Input.GetKey(ChimericOptions.tentMovementUp.Value) && flag) || (!flag && self.input[0].y == 1)) {
                    return 0;
                }
                else {
                    return 7*(float)Math.PI/4;
                }
            }
            else if ((Input.GetKey(ChimericOptions.tentMovementUp.Value) && flag) || (!flag && self.input[0].y == 1)) {
                if ((Input.GetKey(ChimericOptions.tentMovementLeft.Value) && flag) || (!flag && self.input[0].x == -1)) {
                    return (float)Math.PI/2;
                }
                else {
                    return (float)Math.PI/4;
                }
            }
            else if ((Input.GetKey(ChimericOptions.tentMovementLeft.Value) && flag) || (!flag && self.input[0].x == -1)) {
                if ((Input.GetKey(ChimericOptions.tentMovementDown.Value) && flag) || (!flag && self.input[0].y == -1)) {
                    return (float)Math.PI;
                }
                else {
                    return 3*(float)Math.PI/4;
                }
            }
            else if ((Input.GetKey(ChimericOptions.tentMovementDown.Value) && flag) || (!flag && self.input[0].y == -1)) {
                if ((Input.GetKey(ChimericOptions.tentMovementRight.Value) && flag) || (!flag && self.input[0].x == 1)) {
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
        public static void TentacleRetraction(Player self, PlayerEx something) {
            if (Input.GetKey(ChimericOptions.tentMovementAutoEnable.Value) && something.retractionTimer < 40) {
                something.retractionTimer += 5;
            }

            //bool notTooFarNotTooClose = (Custom.Dist(something.previousPosition, self.mainBodyChunk.pos) > 1f && Custom.Dist(something.previousPosition, self.mainBodyChunk.pos) < 3.5f);

            if ((self.dead || /*notTooFarNotTooClose ||*/ Input.GetKey(ChimericOptions.tentMovementEnable.Value)) && something.retractionTimer < 60) {//Change limits back to 1f and 3.5f once testing is done
                something.retractionTimer += 0.5f;
            }
            else if (something.retractionTimer > 7/*-20*/ && !Input.GetKey(ChimericOptions.tentMovementEnable.Value) && !self.dead) {
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
        public static void PrimaryTentacleAndPlayerMovement(PlayerEx something, Player self) {
            if (Input.GetKey(ChimericOptions.tentMovementAutoEnable.Value) && !Input.GetKey(ChimericOptions.tentMovementEnable.Value)) {
                something.automateMovement = true;
            }
            if (something.stuckCreature != null) {
                Debug.Log($"Caught Creature is {something.stuckCreature.PhysObject}");
                something.stuckCreature.Update();
            }
            if (self.room != null && !(self.room.GetTile(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position).Solid || self.room.GetTile(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position).AnyBeam) && !something.automateMovement) {
                
                something.targetPos[0].foundSurface = true;
                
                int upDown = (Input.GetKey(ChimericOptions.tentMovementUp.Value)? 1:0) + (Input.GetKey(ChimericOptions.tentMovementDown.Value)? -1:0);
                int rightLeft = (Input.GetKey(ChimericOptions.tentMovementRight.Value)? 1:0) + (Input.GetKey(ChimericOptions.tentMovementLeft.Value)? -1:0);
                
                float dist = Custom.Dist(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position + new Vector2(3*(something.overrideControls? rightLeft:self.input[0].x), 3*(something.overrideControls? upDown:self.input[0].y)), self.mainBodyChunk.pos);
                if (dist < 300f) {
                    something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position += new Vector2(3f*(something.overrideControls? rightLeft:self.input[0].x), 6f*(something.overrideControls? upDown:self.input[0].y));
                }
                for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++) {
                    if (something.stuckCreature == null && Custom.DistLess(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, 15f) && self.room.abstractRoom.creatures[i].realizedCreature != self) {
                        something.stuckCreature = new AbstractOnTentacleStick(self.abstractCreature, self.room.abstractRoom.creatures[i]);
                        something.stuckCreature.ChangeOverlap(false);
                    }
                }
                /*else if (dist > 305f) {
                    something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position = Vector2.MoveTowards(something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position, self.mainBodyChunk.pos, 1*self.mainBodyChunk.vel.magnitude);
                }*/
            }
            else {
                //self.canJump = 0;
                //self.wantToJump = 0;
                self.airFriction = 0.85f;
                self.customPlayerGravity = 0.2f;
                if (self.feetStuckPos != null && !Input.GetKey(ChimericOptions.tentMovementAutoEnable.Value))  //Stop feet from magnetising to the ground
                    self.bodyChunks[1].pos = self.feetStuckPos.Value+Vector2.up*2f;
                if (!something.automateMovement) {  //If the tentacle is making first contact, make it go to that position
                    something.tentacles[0].iWantToGoThere = something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position;
                    something.targetPos[0].foundSurface = true;
                    something.targetPos[0].hasConnectionSpot = true;
                }
                something.automateMovement = true;
                int connectionsToSurface = 0;   //Get how many tentacles are attached to the terrain/poles
                foreach (var tentacle in something.tentacles) {
                    if (tentacle.isAttatchedToSurface == 1) {
                        connectionsToSurface += 1;
                    }
                }
                if (connectionsToSurface == 0 && !(Input.GetKey(ChimericOptions.tentMovementAutoEnable.Value))) {
                    something.automateMovement = false;
                    //self.controller = new Player.NullController();
                }

#region BodyChunkMovements
                //Debug.Log(something.timer);
                if (connectionsToSurface == 0 && Input.GetKey(ChimericOptions.tentMovementAutoEnable.Value) && !Input.GetKey(ChimericOptions.tentMovementEnable.Value)) {
                    self.customPlayerGravity = 0.2f;
                    //self.mainBodyChunk.vel -= new Vector2(0f, 0.2f);
                }
                else if (!Input.GetKey(ChimericOptions.tentMovementAutoEnable.Value) || Input.GetKey(ChimericOptions.tentMovementEnable.Value)) {
                    if (self.input[0].x != 0 || self.input[0].y != 0 && something.timer < 1f) {
                        something.timer += 0.1f;
                    }
                    else if (something.timer > 0f) {
                        something.timer -= 0.1f;
                    }
                    self.mainBodyChunk.vel = Vector2.Lerp(new Vector2(0,0.84f*(self.room==null? 0 : self.room.gravity)), new Vector2(2.3f*connectionsToSurface*self.input[0].x, 2.3f*connectionsToSurface*self.input[0].y), something.timer);
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
                                chunk.vel = new Vector2(0, 0.84f);
                            }
                        }
                    }
                }
#endregion
            }
        }
        public static void TentaclesFindPositionToGoTo(PlayerEx something, Player self, float startPos) {
            int numerations = (self.room != null && self.room.game.IsStorySession && (self.room.world.region.name=="RM" || self.room.world.region.name=="SS" || self.room.world.region.name=="DM"))? 100 : 200;
            float multiple = numerations==100? 2f : 1f;
            for (int i = 0; i < something.tentacles.Length; i++) {
                if (something.targetPos[i].foundSurface && (Custom.Dist(self.mainBodyChunk.pos, something.targetPos[i].targetPosition) >= 250 || Custom.Dist(self.mainBodyChunk.pos, something.tentacles[i].iWantToGoThere) >= 250) && something.targetPos[i].hasConnectionSpot) {
                    something.targetPos[i].foundSurface = false;
                    something.targetPos[i].hasConnectionSpot = false;
                }
                // These two for loops make a part of a circle, where k is the radius and j is the relative angle
                for (float k = 0; k < numerations; k++) {
                    for (float j = startPos + (float)Math.PI/8*(i); j < startPos + (float)Math.PI/8*(i+1); j+=((float)Math.PI/256f)*multiple) {
                        Vector2 position = new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y);
                        var tile = self.room?.GetTile(new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y));
                        if (!something.targetPos[i].foundSurface && self.room != null && tile != null && (tile.Solid || tile.AnyBeam)) {
                            if (tile.AnyBeam) {
                                something.targetPos[i].isPole = true;
                            }
                            else {
                                something.targetPos[i].isPole = false;
                            }   //These two are technically the same I think
                            //something.targetPos[i].isPole = (self.room.GetTile(new Vector2((Mathf.Cos(j)*(k * 2))+self.mainBodyChunk.pos.x,(Mathf.Sin(j)*(k * 2))+self.mainBodyChunk.pos.y)).AnyBeam);
                            something.targetPos[i].targetPosition = position + (something.targetPos[i].isPole? (position-self.mainBodyChunk.pos).normalized * 5 : new Vector2(0,0));
                            something.targetPos[i].foundSurface = true;
                            goto End;   // If this tentacle found a valid position, skip doing the rest of the math and go to the next one
                        }
                        /*else if (!something.targetPos[i].foundSurface && self.room != null && (tile == null || (!tile.Solid && !tile.AnyBeam))) {
                            something.targetPos[i].foundSurface = false;
                            something.targetPos[i].targetPosition = self.mainBodyChunk.pos - Vector2.down * 5;
                        }*/
                    }
                }
                End:;
            }
        }
        public static void MoveTentacleToPosition(PlayerEx something, Player self) {
            for (int i = 0; i < something.tentacles.Length; i++) {
                //base.Logger.LogDebug(something.targetPos[i].isPipe);
                //base.Logger.LogDebug(Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, something.tentacles[i].iWantToGoThere));
                if (!something.targetPos[i].hasConnectionSpot && something.automateMovement) {
                    something.targetPos[i].hasConnectionSpot = true;
                    Debug.Log($"Please god help me here: {something.tentacles[i].iWantToGoThere} and {something.targetPos[i].targetPosition}");
                    something.tentacles[i].iWantToGoThere = something.targetPos[i].targetPosition;
                    //self.room.AddObject(new Spark(something.tentacles[i].iWantToGoThere, new Vector2(5,5), Color.blue, null, 10, 20));  //Testing
                }
                if (Custom.Dist(something.tentacles[i].pList[something.tentacles[i].pList.Length-1].position, something.tentacles[i].iWantToGoThere) > (something.targetPos[i].isPole? 5f:5f/*Can be adjusted maybe, rn it plays multiple times for poles*/) && something.automateMovement) {
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
        ///<summary> Replaced the normal face with Slugrot's face </summary>
        public static void DrawFace(PlayerEx something, RoomCamera.SpriteLeaser sLeaser, string? name) {
            if (name != null && name != null && name.StartsWith("Face") && something.faceAtlas._elementsByName.TryGetValue("Rot" + name, out var element)) {
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
        ///<summary> Update the color of the vignette sprite, and use inputs to do math to determine the center position. r + g are replaced, b + a are passed through </summary>
        public static void UpdateVignette(RainWorld game, Player self, Color col, Vector2 camPos, bool visible = true) {
            if (Plugin.vignetteEffect != null) {
                float rVar = (self.mainBodyChunk.pos.x-camPos.x)/(game.screenSize.x);
                float gVar = (self.mainBodyChunk.pos.y/**0.8f+80f*/-camPos.y)/(game.screenSize.y)/(86f/48f)+0.22f;    //Math numbers are gotten by doing the best thing ever, complete guesswork!
                //Debug.Log($"{rVar} and {gVar} and {col.b} and {col.a}");
                Plugin.vignetteEffect.color = new Color(rVar, gVar, col.b, col.a);
                Plugin.vignetteEffect.isVisible = visible;
                //Debug.Log($"Update Vignette. rVar: {rVar} gVar: {gVar} bodyX: {self.mainBodyChunk.pos.x-camPos.x} bodyY: {self.mainBodyChunk.pos.y-camPos.y}");
            }
            else {
                Debug.LogWarning("VignetteEffect was null!");
            }
        }
        ///<summary> Hard-replacement of the Vignette color </summary>
        public static void UpdateVignette(Color col, bool visible = true) {
            if (Plugin.vignetteEffect != null) {
                Plugin.vignetteEffect.color = new Color(col.r, col.g, col.b, col.a);
                Plugin.vignetteEffect.isVisible = visible;
                //Debug.Log($"Default Color is: {Shader.GetGlobalColor("_InputColorA")}");
            }
            else {
                Debug.LogWarning("VignetteEffect was null!");
            }
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
        //Thanks Niko
        public static Vector2 RotateAroundPoint(Vector2 center, Vector2 offset, float degrees)
            {
                offset += center;
                float radians = (float)(degrees * Math.PI / 180.0);
                float cos = (float)Math.Cos(radians);
                float sin = (float)Math.Sin(radians);
                float x = (offset.x - center.x) * cos - (offset.y - center.y) * sin + center.x;
                float y = (offset.x - center.x) * sin + (offset.y - center.y) * cos + center.y;
                return new Vector2(x, y);
            }
    }
    public class SparkEx {
        public bool isHearingSpark;
        public SparkEx(bool flag) {
            this.isHearingSpark = flag;
        }
    }
    public class CreatureEx {
        public bool isBeingEaten;
        public bool redrawRotSprites;
        public bool addNewSprite;
        public int numOfCosmeticSprites;
        public int maxNumOfSprites;
        public bool shouldFearDynamo = false;
        public int fearTime = -1;
        public List<EatingRot> yummersRotting = new List<EatingRot>();
    }
    ///<summary>The pivot points for the tentacles, where they can bend. To be replaced with tailSegments</summary>
    public class Point
    {
        public Vector2 position = new Vector2(700,200);
        public Vector2 prevPosition = new Vector2(701,200);
        public bool locked = false;
        public Point(Vector2 position, bool locked) {
            this.position = position;
            this.locked = locked;
        }
        public void Update(PlayerEx something, Player self, Line tentacle) {
            if (Array.IndexOf(tentacle.pList, this) == tentacle.pList.Length-1 && (
                    (Input.GetKey(ChimericOptions.tentMovementEnable.Value) || Input.GetKey(ChimericOptions.tentMovementAutoEnable.Value)) && 
                    something.targetPos[Array.IndexOf(something.tentacles, tentacle)].foundSurface && 
                    ((Array.IndexOf(something.tentacles, tentacle) == 0) || something.automateMovement))) {  //If it is the very last point in the list, the tentacle tip
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
    ///<summary>The connections between points</summary>
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
        [AllowNull] public Point[] pList;
        [AllowNull] public Stick[] sList;
        [AllowNull] public Circle[] cList;
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
    public class Fin {
        ///<summary>For creating fin sprites and holding their data. BodyChunk constructor. Negative rotation makes them angled down more, and positive is up. The X and Y scales are the other, because the initial sprite is vertical. swimRange is how much the fins rotate in either direction while swimming</summary>
        public Fin(FSprite connectedSprite, Vector2 posOffset, float additionalRotation, float scaleX, float scaleY, bool flipped = false, float foldRotation = -80f, List<float>? swimRange = null, float swimCycle = Mathf.PI/6f, float startSwimCycle = Mathf.PI/6f) {
            this.connectedSprite = connectedSprite;
            this.posOffset = posOffset;
            this.additionalRotation = additionalRotation;
            this.startAdditionalRotation = additionalRotation;
            this.flipped = flipped;
            this.scaleX = scaleX;
            this.scaleY = scaleY;
            this.foldRotation = foldRotation;
            if (swimRange == null) {
                this.swimRange = new List<float>(){0, 0};
            }
            else if (swimRange != null) {
                this.swimRange.AddRange(swimRange);
            }
            this.swimCycle = swimCycle;
            this.startSwimCycle = startSwimCycle;
        }
        ///<summary>For creating fin sprites and holding their data. tailSegment constructor, alternate version for attatching sprites to the tail. When adding offset to the tailSegments, y is inverted (negative is up and positive is down). Negative rotation makes them angled down more, and positive is up. The X and Y scales are the other, because the initial sprite is vertical. swimRange is how much the fins rotate in either direction while swimming</summary>
        public Fin(TailSegment connectedTailSegment, Vector2 posOffset, float additionalRotation, float scaleX, float scaleY, bool flipped = false, float foldRotation = -80f, List<float>? swimRange = null, float swimCycle = Mathf.PI/6f, float startSwimCycle = Mathf.PI/6f) {
            this.connectedTailSegment = connectedTailSegment;
            this.posOffset = posOffset;
            this.additionalRotation = additionalRotation;
            this.startAdditionalRotation = additionalRotation;
            this.flipped = flipped;
            this.scaleX = scaleX;
            this.scaleY = scaleY;
            this.foldRotation = foldRotation;
            if (swimRange == null) {
                this.swimRange = new List<float>(){0, 0};
            }
            else if (swimRange != null) {
                this.swimRange.AddRange(swimRange);
            }
            this.swimCycle = swimCycle;
            this.startSwimCycle = startSwimCycle;
        }
        [AllowNull] public TailSegment connectedTailSegment;
        [AllowNull] public FSprite connectedSprite;
        public Vector2 posOffset;
        public float additionalRotation;
        public bool flipped;
        public float scaleX;
        public float scaleY;
        public float startAdditionalRotation;
        public float foldRotation;
        public List<float> swimRange = new List<float>{};
        public float swimCycle;
        public float startSwimCycle;
        public float corriderTimer;
    }
    public class AbstractOnTentacleStick : AbstractPhysicalObject.AbstractObjectStick
    {
        [AllowNull] public AbstractPhysicalObject Player
        {
            get
            {
                return this.A;
            }
            set
            {
                this.A = value;
            }
        }
        [AllowNull] public AbstractPhysicalObject PhysObject
        {
            get
            {
                return this.B;
            }
            set
            {
                this.B = value;
            }
        }
        public AbstractOnTentacleStick(AbstractPhysicalObject player, AbstractPhysicalObject creature) : base(player, creature)
        {
        }
        public override string SaveToString(int roomIndex)
        {
            return string.Concat(new string[]
            {
                roomIndex.ToString(),
                "<stkA>sprOnBackStick<stkA>",
                this.A.ID.ToString(),
                "<stkA>",
                this.B.ID.ToString()
            });
        }
        public void Update() {
            Creature? crit = (this.PhysObject.realizedObject as Creature);
            Player? player = (this.Player.realizedObject as Player);
            if (crit != null && player != null) {
                if (Plugin.tenticleStuff.TryGetValue(player, out var something) && something.isRot) {
                    crit.mainBodyChunk.pos = something.tentacles[0].pList[something.tentacles[0].pList.Length-1].position;
                }
                foreach (var chunk in crit.bodyChunks) {
                    chunk.vel = Vector2.zero;
                }
            }
        }
        public void ChangeOverlap(bool newOverlap)
        {
            Creature? crit = (this.PhysObject.realizedObject as Creature);
            Player? player = (this.Player.realizedObject as Player);
            //crit.CollideWithObjects = newOverlap;
            //crit.canBeHitByWeapons = newOverlap;
            if (crit != null) {
                crit.GoThroughFloors = newOverlap;
            }
            /*if (crit.graphicsModule == null || player.room == null)
            {
                return;
            }
            for (int i = 0; i < player.room.game.cameras.Length; i++)
            {
                player.room.game.cameras[i].MoveObjectToContainer(crit.graphicsModule, player.room.game.cameras[i].ReturnFContainer((!newOverlap) ? "Background" : "Midground"));
            }*/
        }
    }
}