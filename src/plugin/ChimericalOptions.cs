using System.Diagnostics.CodeAnalysis;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace Chimeric
{
    public class ChimericOptions : OptionInterface {
        [AllowNull] public static Configurable<KeyCode> TentMovementUp { get; set; }
        [AllowNull] public static Configurable<KeyCode> TentMovementDown { get; set; }
        [AllowNull] public static Configurable<KeyCode> TentMovementLeft { get; set; }
        [AllowNull] public static Configurable<KeyCode> TentMovementRight { get; set; }
        [AllowNull] public static Configurable<KeyCode> TentMovementEnable { get; set; }
        [AllowNull] public static Configurable<KeyCode> TentMovementAutoEnable { get; set; }
        [AllowNull] public static Configurable<KeyCode> GrabButton { get; set; }
        [AllowNull] public static Configurable<bool> EnableVignette { get; set; }
        [AllowNull] public static Configurable<int> ScareDuration { get; set; }
        [AllowNull] public static Configurable<int> YeetusMagnitude { get; set; }
        [AllowNull] public static Configurable<bool> FriendlyFire { get; set; }
        //public OpSimpleButton ButtonWorkPlease = new OpSimpleButton(new Vector2(200,300),new Vector2(20,20),"Rotcat");

        public ChimericOptions() {
            TentMovementUp = config.Bind("tentacleMovementUp", new KeyCode());
            TentMovementDown = config.Bind("tentacleMovementDown", new KeyCode());
            TentMovementLeft = config.Bind("tentacleMovementLeft", new KeyCode());
            TentMovementRight = config.Bind("tentacleMovementRight", new KeyCode());
            TentMovementEnable = config.Bind("tentacleMovementEnable", new KeyCode());
            TentMovementAutoEnable = config.Bind("tentacleMovementAutoEnable", new KeyCode());
            GrabButton = config.Bind("grabButton", new KeyCode());
            EnableVignette = config.Bind("enableVignette", true);
            ScareDuration = config.Bind("scareDuration", 120, new ConfigAcceptableRange<int>(40, 120));
            YeetusMagnitude = config.Bind("yeetusMagnitude", 22, new ConfigAcceptableRange<int>(5, 100));
            FriendlyFire = config.Bind("friendlyFire", true);
            // var OnClick = typeof(OpSimpleButton).GetEvent("OnClick");
            // OnClick.AddEventHandler(ButtonWorkPlease, Delegate.CreateDelegate(OnClick.EventHandlerType, typeof(Plugin), typeof(Plugin).GetMethod("OpenSaysMe")));
        }

        public override void Initialize() {
            OpTab rotTab = new (this, "Slugrot");
            OpTab dynoTab = new (this, "Dynamo");
            Tabs = new[]
            {
                rotTab,
                dynoTab
            };

            UIelement[]? UIRotArrPlayerOptions = new UIelement[]
            {
                new OpLabel(new Vector2(250f, 570f), new Vector2(100f, 20f), "Slugrot Options", FLabelAlignment.Center, true),

                new OpLabel(new Vector2(250f, 525f), new Vector2(100f, 20f), Translate("Movement Keybinds"), FLabelAlignment.Center, true),
                new OpLabel(100f, 485f, Translate("Movement Tentacles Up"), false),
                new OpKeyBinder(TentMovementUp, new Vector2(400f, 480f), new Vector2(120f, 30f)) {description = Translate("Tentacle Up.")},
                new OpLabel(100f, 450f, Translate("Movement Tentacles Down"), false),
                new OpKeyBinder(TentMovementDown, new Vector2(400f, 445f), new Vector2(120f, 30f)) {description = Translate("Tentacle Down.")},
                new OpLabel(100f, 415f, Translate("Movement Tentacles Left"), false),
                new OpKeyBinder(TentMovementLeft, new Vector2(400f, 410f), new Vector2(120f, 30f)) {description = Translate("Tentacle Left.")},
                new OpLabel(100f, 380f, Translate("Movement Tentacles Right"), false),
                new OpKeyBinder(TentMovementRight, new Vector2(400f, 375f), new Vector2(120f, 30f)) {description = Translate("Tentacle Right.")},
                new OpLabel(100f, 345f, Translate("Activate the tentacle movement"), false),
                new OpKeyBinder(TentMovementEnable, new Vector2(400f, 340f), new Vector2(120f, 30f)) {description = Translate("Enable movement via tentacles.")},
                new OpLabel(100f, 310f, Translate("Activate automatic tentacle movement"), false),
                new OpKeyBinder(TentMovementAutoEnable, new Vector2(400f, 305f), new Vector2(120f, 30f)) {description = Translate("Activate tentacles without choosing an initial surface.")},
                // new OpLabel(100f, 275f, Translate("Grab creatures"), false),
                // new OpKeyBinder(GrabButton, new Vector2(400f, 270f), new Vector2(120f, 30f)) {description = Translate("Grab creatures with your tentacle when they are near enough.")},

                new OpLabel(new Vector2(250f, 190f), new Vector2(100f, 20f), Translate("Graphical Options"), FLabelAlignment.Center, true),
                new OpLabel(100f, 150f, Translate("Enable Vignette effect"), false),
                new OpCheckBox(EnableVignette, new Vector2(450f, 145f)) {description = Translate("Play blinded, unable to see far from yourself.")}
            };
            UIelement[]? UIDynoArrPlayerOptions = new UIelement[]
            {
                new OpLabel(new Vector2(250f, 570f), new Vector2(100f, 20f), Translate("Dynamo Options"), FLabelAlignment.Center, true),

                new OpLabel(100f, 520f, Translate("Mimicry Effectiveness"), false),
                new OpSlider(ScareDuration, new Vector2(420f, 515f), 80, false) {description = Translate("How effective the shock mimicry is.")},
                new OpLabel(100f, 485f, Translate("Force of Tail"), false),
                new OpSlider(YeetusMagnitude, new Vector2(420f, 480f), 80, false) {description = Translate("How much yeeting the tail does.")},
                new OpLabel(100f, 450f, Translate("Friendly Fire?"), false),
                new OpCheckBox(FriendlyFire, new Vector2(450f, 445f)) {description = Translate("Can Dynamo's tail hurt other players?")}
            };
            rotTab.AddItems(UIRotArrPlayerOptions);
            dynoTab.AddItems(UIDynoArrPlayerOptions);
        }
    }
}