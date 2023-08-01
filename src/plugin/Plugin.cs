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
        public static ConditionalWeakTable<AbstractCreature, CreatureEx> creatureYummersSprites = new ConditionalWeakTable<AbstractCreature, CreatureEx>();
        public static FContainer darkContainer = new FContainer();
        public static FSprite? vignetteEffect;
        public static bool appliedVignette = false;
        public static ChimericOptions? ChimericOptions;
        bool configWorking = false;
        public static bool loaded = false;
        public const string ROT_NAME = "slugrot";
        public const string DYNAMO_NAME = "dynamo";
        public const string DRACO_NAME = "draco";
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
                    FSprite sprite = new FSprite("remixmenubreakline", false);
                    sprite.SetPosition(new Vector2(300-(1366-Futile.screen.pixelWidth)/2f, 240));
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
            else if (self.slugcatStats.name.value == DRACO_NAME) {
                something.isDragon = true;
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
                    AssetBundle? bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/red"));
                    if (bundle != null) {
                        self.Shaders["Red"] = FShader.CreateShader("red", bundle.LoadAsset<Shader>("Assets/red.shader"));
                    }
                    else {
                        Debug.Log("Shitfuckfcukdamnitnotagain");
                    }
                } catch (Exception err) {
                    base.Logger.LogError(err);
                    configWorking = false;
                }
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
    }
}