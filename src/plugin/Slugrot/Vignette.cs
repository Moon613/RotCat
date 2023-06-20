using UnityEngine;
using Noise;
using RWCustom;
using Random = UnityEngine.Random;
using Menu;

namespace Chimeric
{
    public class Vignette
    {
        public static float defaultVignetteSize = 0.7f;
        public static void GameSessionStartup(On.GameSession.orig_ctor orig, GameSession self, RainWorldGame game) {
            orig(self, game);
            Debug.Log("Ran Session Startup");
            Futile.stage.AddChild(Plugin.darkContainer);
            if (!Plugin.appliedVignette && ChimericOptions.enableVignette.Value) {
                Plugin.vignetteEffect = new FSprite("Futile_White", true);
                Functions.UpdateVignette(new Color(0.5f, 0.5f, 0.55f, defaultVignetteSize), false);  //r & g are the position of the vignette inside the square. b is the intensity/radius and a is the alpha/fade radius
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
        public static void QuitModeBackToMenu(On.RainWorldGame.orig_ExitToMenu orig, RainWorldGame self) {
            orig(self);
            Plugin.darkContainer.RemoveAllChildren();
            Plugin.vignetteEffect = null;
            Plugin.appliedVignette = false;
        }
        public static void CystsReacts(On.Room.orig_InGameNoise orig, Room self, InGameNoise noise) {
            orig(self, noise);
            if (self.game.session.characterStats.name.ToString().ToLower() == "slugrot" || !self.game.IsStorySession) {
                PhysicalObject source = noise.sourceObject; 
                //Debug.Log($"{source} and {source.GetType()}");
                for (int i = 0; i < self.PlayersInRoom.Count; i++) {
                    if (source != self.PlayersInRoom[i] && (Plugin.tenticleStuff.TryGetValue(self.PlayersInRoom[i], out PlayerEx player) && player.isRot) && (player.hearingCooldown <= 0 || CalculateMod5PlusMinus1(player.hearingCooldown) || player.hearingCooldown==40) && Custom.Dist(self.PlayersInRoom[i].mainBodyChunk.pos, noise.pos) <= noise.strength*1.5f) {
                        if (ChimericOptions.enableVignette.Value) {
                            self.AddObject(new CreaturePing(noise.pos, Color.white, noise.strength/75f, self));
                            Debug.Log($"name/id is: {self.roomSettings.name}");
                        }
                        //Debug.Log($"Add Ping Effect number {i}");
                        for (int j = 0; j < Random.Range(3,5); j++) {
                            self.AddObject(new RotCatBubble(self.PlayersInRoom[i], Custom.DirVec(self.PlayersInRoom[0].mainBodyChunk.pos, noise.pos) * 12f / (1f + j * 0.2f), Random.Range(0.85f, 1.15f), Random.value, Random.Range(0f, 0.5f)));
                            self.AddObject(new RotCatRipple(self.PlayersInRoom[i], noise.pos, default(Vector2), Random.Range(0.7f, 0.8f), Color.white));
                        }
                        player.hearingCooldown = 40;
                    }
                }
            }
        }
        public static void CystsReacts2(On.VirtualMicrophone.orig_PlaySound_SoundID_PositionedSoundEmitter_bool_float_float_bool orig, VirtualMicrophone self, SoundID soundID, PositionedSoundEmitter controller, bool loop, float vol, float pitch, bool randomStartPosition) {
            orig(self, soundID, controller, loop, vol, pitch, randomStartPosition);
            if (self.room != null && (self.room.game.session.characterStats.name.ToString().ToLower() == "slugrot" || !self.room.game.IsStorySession)) {
                //Debug.Log($"Volume is: {vol}. Pitch is: {pitch}. SoundID is: {soundID}");
                if (self.room != null && controller is ChunkSoundEmitter c && c.chunk.owner is Creature) {
                    if (c.chunk.owner is Player p && p is not null && Plugin.tenticleStuff.TryGetValue(p, out var player) && player.isRot) {
                        return;
                    }
                    for (int i = 0; i < self.room.PlayersInRoom.Count; i++) {
                        if (ChimericOptions.enableVignette.Value && Plugin.tenticleStuff.TryGetValue(self.room.PlayersInRoom[i], out var player1) && player1.isRot && Custom.Dist(self.room.PlayersInRoom[i].mainBodyChunk.pos, controller.pos) < 650 && CalculateMod5PlusMinus1(player1.smolHearingCooldown)) {
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
            if (Plugin.vignetteEffect != null && self is Player p && Plugin.tenticleStuff.TryGetValue(p, out var player) && player.isRot && ChimericOptions.enableVignette.Value) {
                Functions.UpdateVignette(new Color(Plugin.vignetteEffect.color.r, Plugin.vignetteEffect.color.g, Plugin.vignetteEffect.color.b, 0f));
            }
        }
        public static void RotCatSpitOutOfShortcut(On.Creature.orig_SpitOutOfShortCut orig, Creature self, IntVector2 pos, Room newRoom, bool spitOutAllSticks) {
            orig(self, pos, newRoom, spitOutAllSticks);
            if (Plugin.vignetteEffect != null && self is Player p && Plugin.tenticleStuff.TryGetValue(p, out var player) && player.isRot && ChimericOptions.enableVignette.Value) {
                Functions.UpdateVignette(new Color(Plugin.vignetteEffect.color.r, Plugin.vignetteEffect.color.g, Plugin.vignetteEffect.color.b, defaultVignetteSize));
            }
        }
        public static void CleanDarkContainerOnSleepAndDeathScreen(On.Menu.SleepAndDeathScreen.orig_ctor orig, SleepAndDeathScreen self, ProcessManager manager, ProcessManager.ProcessID ID) {
            orig(self, manager, ID);
            Plugin.darkContainer.RemoveAllChildren();
            Futile.stage.RemoveChild(Plugin.darkContainer);
            Plugin.appliedVignette = false;
        }
        public static void GameOver(On.Player.orig_Die orig, Player self) {
            orig(self);
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