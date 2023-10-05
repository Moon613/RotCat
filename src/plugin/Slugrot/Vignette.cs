using UnityEngine;
using Noise;
using RWCustom;
using Random = UnityEngine.Random;
using Menu;
using System;

namespace Chimeric
{
    public class Vignette
    {
        public static void Apply() {
            On.GameSession.ctor += GameSessionStartup;
            On.ProcessManager.RequestMainProcessSwitch_ProcessID += SwitchGameProcess;
            On.Room.InGameNoise += CystsReacts;
            On.VirtualMicrophone.PlaySound_SoundID_PositionedSoundEmitter_bool_float_float_bool += CystsReacts2;
            On.Creature.SuckedIntoShortCut += RotCatSuckIntoShortcut;
            On.Creature.SpitOutOfShortCut += RotCatSpitOutOfShortcut;
        }
        public static void GameSessionStartup(On.GameSession.orig_ctor orig, GameSession self, RainWorldGame game) {
            orig(self, game);
            Debug.Log("Ran Session Startup");
            Futile.stage.AddChild(Plugin.darkContainer);
            if (ChimericOptions.EnableVignette == null) {
                Debug.LogWarning("Vignette Option was null, attempting to give it a band-aid\nCheck Remix Options, or disable and re-enable the mod");
                try {
                    ChimericOptions.EnableVignette = Plugin.ChimericOptions?.config.Bind("enableVignette", false);
                }
                catch {
                    Debug.LogWarning($"Attempt failed, retrying...");
                    try {
                        ChimericOptions.EnableVignette = new Configurable<bool>(false);
                    } catch (Exception err) {
                        Debug.LogError($"Vignette did not like the band-aid:\n{err}");
                        Debug.LogException(err);
                    }
                }
            }
            if (!Plugin.appliedVignette && ChimericOptions.EnableVignette?.Value == true && self is StoryGameSession && !self.game.rainWorld.safariMode) {
                Plugin.vignetteEffect = new FSprite("Futile_White", true);
                Functions.UpdateVignette(new Color(0f, 0f, 0f, 1), new Color(0, 0, 0, 0.2f), false);
                Plugin.vignetteEffect.SetPosition(new Vector2(game.rainWorld.screenSize.x/2f, game.rainWorld.screenSize.y/2f));
                Plugin.vignetteEffect.scaleX = 86f; //90    //86
                Plugin.vignetteEffect.scaleY = 86f; //60    //48
                Plugin.vignetteEffect.shader = game.rainWorld.Shaders["Red"];
                Plugin.darkContainer.AddChild(Plugin.vignetteEffect);
                Plugin.darkContainer.MoveBehindOtherNode(game.cameras[0].ReturnFContainer("HUD"));
                Plugin.appliedVignette = true;
                Debug.Log("Set Sprite of darkness");
            }
        }
        public static void SwitchGameProcess(On.ProcessManager.orig_RequestMainProcessSwitch_ProcessID orig, ProcessManager self, ProcessManager.ProcessID nextProcessID)
        {
            Plugin.darkContainer.RemoveAllChildren();
            Plugin.vignetteEffect = null;
            Plugin.appliedVignette = false;
            orig(self, nextProcessID);
        }
        public static void CystsReacts(On.Room.orig_InGameNoise orig, Room self, InGameNoise noise) {
            orig(self, noise);
            PhysicalObject source = noise.sourceObject; 
            //Debug.Log($"{source} and {source.GetType()}");
            for (int i = 0; i < self.PlayersInRoom.Count; i++) {
                if (source != self.PlayersInRoom[i] && Plugin.tenticleStuff.TryGetValue(self.PlayersInRoom[i], out PlayerEx player) && player.isRot && (player.hearingCooldown <= 0 || CalculateMod5PlusMinus1(player.hearingCooldown) || player.hearingCooldown==40) && Custom.Dist(self.PlayersInRoom[i].mainBodyChunk.pos, noise.pos) <= noise.strength*1.5f) {
                    if (ChimericOptions.EnableVignette.Value) {
                        self.AddObject(new CreaturePing(noise.pos, Color.white, noise.strength/75f, self));
                    }
                    //Debug.Log($"Add Ping Effect number {i}");
                    for (int j = 0; j < Random.Range(6,12); j++) {
                        self.AddObject(new RotCatBubble(self.PlayersInRoom[i], Custom.DirVec(self.PlayersInRoom[0].mainBodyChunk.pos, noise.pos) * 12f / (1f + j * 0.2f), Random.Range(0.85f, 1.15f), Random.value, Random.Range(0f, 0.5f)));
                        self.AddObject(new RotCatRipple(self.PlayersInRoom[i], noise.pos, default(Vector2), Random.Range(0.7f, 0.8f), SlugBase.DataTypes.PlayerColor.GetCustomColor(self.PlayersInRoom[i].graphicsModule as PlayerGraphics, 1)));
                    }
                    player.hearingCooldown = 40;
                }
            }
        }
        public static void CystsReacts2(On.VirtualMicrophone.orig_PlaySound_SoundID_PositionedSoundEmitter_bool_float_float_bool orig, VirtualMicrophone self, SoundID soundID, PositionedSoundEmitter controller, bool loop, float vol, float pitch, bool randomStartPosition) {
            orig(self, soundID, controller, loop, vol, pitch, randomStartPosition);
            if (self.room != null && (self.room.game.session.characterStats.name.value == Plugin.ROT_NAME || !self.room.game.IsStorySession)) {
                //Debug.Log($"Volume is: {vol}. Pitch is: {pitch}. SoundID is: {soundID}");
                if (self.room != null && controller is ChunkSoundEmitter c && c.chunk.owner is Creature) {
                    if (c.chunk.owner is Player p && p is not null && Plugin.tenticleStuff.TryGetValue(p, out var player) && player.isRot) {
                        return;
                    }
                    for (int i = 0; i < self.room.PlayersInRoom.Count; i++) {
                        if (ChimericOptions.EnableVignette.Value && Plugin.tenticleStuff.TryGetValue(self.room.PlayersInRoom[i], out var player1) && player1.isRot && Custom.Dist(self.room.PlayersInRoom[i].mainBodyChunk.pos, controller.pos) < 650 && CalculateMod5PlusMinus1(player1.smolHearingCooldown)) {
                            self.room.AddObject(new CreaturePing(controller.pos, Color.white, vol*1.5f, self.room));
                            player1.smolHearingCooldown = 10;
                            break;
                        }
                    }
                }
            }
        }
        public static void RotCatSuckIntoShortcut(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther) {
            orig(self, entrancePos, carriedByOther);
            if (Plugin.vignetteEffect != null && self is Player p && Plugin.tenticleStuff.TryGetValue(p, out var player) && player.isRot && ChimericOptions.EnableVignette.Value) {
                Functions.UpdateVignette(Color.black, Color.black);
            }
        }
        public static void RotCatSpitOutOfShortcut(On.Creature.orig_SpitOutOfShortCut orig, Creature self, IntVector2 pos, Room newRoom, bool spitOutAllSticks) {
            orig(self, pos, newRoom, spitOutAllSticks);
            if (Plugin.vignetteEffect != null && self is Player p && Plugin.tenticleStuff.TryGetValue(p, out var player) && player.isRot && ChimericOptions.EnableVignette.Value) {
                Functions.UpdateVignette(Color.black, new Color(0f, 0f, 0f, 0.2f));
            }
        }
        public static bool CalculateMod5PlusMinus1(int num) {
            if (num%5==0 || (num-1)%5==0 || (num+1)%5==0) {
                return true;
            }
            else {
                return false;
            }
        }
    }
}