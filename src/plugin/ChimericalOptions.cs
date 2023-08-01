using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace Chimeric
{
    public class ChimericOptions : OptionInterface {
        private readonly ManualLogSource Logger;
        [AllowNull] public static Configurable<KeyCode> tentMovementUp { get; set; }
        [AllowNull] public static Configurable<KeyCode> tentMovementDown { get; set; }
        [AllowNull] public static Configurable<KeyCode> tentMovementLeft { get; set; }
        [AllowNull] public static Configurable<KeyCode> tentMovementRight { get; set; }
        [AllowNull] public static Configurable<KeyCode> tentMovementEnable { get; set; }
        [AllowNull] public static Configurable<KeyCode> tentMovementAutoEnable { get; set; }
        [AllowNull] public static Configurable<KeyCode> grabButton { get; set;}
        [AllowNull] public static Configurable<bool> enableVignette { get; set; }
        [AllowNull] public static Configurable<int> scareDuration { get; set; }
        [AllowNull] public static Configurable<int> yeetusMagnitude { get; set; }
        [AllowNull] private UIelement[] UIRotArrPlayerOptions;
        [AllowNull] private UIelement[] UIDynoArrPlayerOptions;

        public ChimericOptions(Plugin pluginInstance, ManualLogSource logSource) {
            Logger = logSource;
            tentMovementUp = config.Bind("tentacleMovementUp", new KeyCode());
            tentMovementDown = config.Bind("tentacleMovementDown", new KeyCode());
            tentMovementLeft = config.Bind("tentacleMovementLeft", new KeyCode());
            tentMovementRight = config.Bind("tentacleMovementRight", new KeyCode());
            tentMovementEnable = config.Bind("tentacleMovementEnable", new KeyCode());
            tentMovementAutoEnable = config.Bind("tentacleMovementAutoEnable", new KeyCode());
            grabButton = config.Bind("grabButton", new KeyCode());
            enableVignette = config.Bind("enableVignette", true);
            scareDuration = config.Bind("scareDuration", 120, new ConfigAcceptableRange<int>(40, 120));
            yeetusMagnitude = config.Bind("yeetusMagnitude", 22, new ConfigAcceptableRange<int>(5, 100));
        }

        public override void Initialize() {
            OpTab rotTab = new OpTab(this, "Slugrot");
            OpTab dynoTab = new OpTab(this, "Dynamo");
            this.Tabs = new[]
            {
                rotTab,
                dynoTab
            };

            UIelement[]? UIRotArrPlayerOptions = new UIelement[]
            {
                new OpLabel(260f, 570f, "Options", true),

                new OpLabel(200f, 520f, Translate("Movement Keybinds"), true),
                new OpLabel(50f, 485f, Translate("Movement Tentacles Up"), false),
                new OpKeyBinder(tentMovementUp, new Vector2(350f, 480f), new Vector2(120f, 30f)) {description = Translate("Tentacle Up.")},
                new OpLabel(50f, 450f, Translate("Movement Tentacles Down"), false),
                new OpKeyBinder(tentMovementDown, new Vector2(350f, 445f), new Vector2(120f, 30f)) {description = Translate("Tentacle Down.")},
                new OpLabel(50f, 415f, Translate("Movement Tentacles Left"), false),
                new OpKeyBinder(tentMovementLeft, new Vector2(350f, 410f), new Vector2(120f, 30f)) {description = Translate("Tentacle Left.")},
                new OpLabel(50f, 380f, Translate("Movement Tentacles Right"), false),
                new OpKeyBinder(tentMovementRight, new Vector2(350f, 375f), new Vector2(120f, 30f)) {description = Translate("Tentacle Right.")},
                new OpLabel(50f, 345f, Translate("Activate the tentacle movement"), false),
                new OpKeyBinder(tentMovementEnable, new Vector2(350f, 340f), new Vector2(120f, 30f)) {description = Translate("Enable movement via tentacles.")},
                new OpLabel(50f, 310f, Translate("Activate automatic tentacle movement"), false),
                new OpKeyBinder(tentMovementAutoEnable, new Vector2(350f, 305f), new Vector2(120f, 30f)) {description = Translate("Activate tentacles without choosing an initial surface.")},
                new OpLabel(50f, 275f, Translate("Grab creatures"), false),
                new OpKeyBinder(grabButton, new Vector2(350f, 270f), new Vector2(120f, 30f)) {description = Translate("Grab creatures with your tentacle when they are near enough.")},


                new OpLabel(200f, 190f, Translate("Graphical Options"), true),
                new OpLabel(50f, 150f, Translate("Enable Vignette effect"), false),
                new OpCheckBox(enableVignette, new Vector2(400f, 145f)) {description = Translate("Play blinded, unable to see far from yourself.")}
            };
            UIelement[]? UIDynoArrPlayerOptions = new UIelement[]
            {
                new OpLabel(260f, 570f, Translate("Options"), true),

                new OpLabel(150f, 520f, Translate("Mimicry effectiveness"), false),
                new OpSlider(scareDuration, new Vector2(330f, 515f), 80, false) {description = Translate("How effective the shock mimicry is.")},
                new OpLabel(150f, 485f, Translate("Force of Tail"), false),
                new OpSlider(yeetusMagnitude, new Vector2(330f, 480f), 80, false) {description = Translate("How much yeeting the tail does.")}
            };
            rotTab.AddItems(UIRotArrPlayerOptions);
            dynoTab.AddItems(UIDynoArrPlayerOptions);
        }
    }
}