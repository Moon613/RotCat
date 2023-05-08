using BepInEx.Logging;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace RotCat;
public class RotCatOptions : OptionInterface {
    private readonly ManualLogSource Logger;
    public readonly Configurable<KeyCode> tentMovementUp = new Configurable<KeyCode>(KeyCode.W);
    public readonly Configurable<KeyCode> tentMovementDown = new Configurable<KeyCode>(KeyCode.S);
    public readonly Configurable<KeyCode> tentMovementLeft = new Configurable<KeyCode>(KeyCode.A);
    public readonly Configurable<KeyCode> tentMovementRight = new Configurable<KeyCode>(KeyCode.D);
    public readonly Configurable<KeyCode> tentMovementEnable = new Configurable<KeyCode>(KeyCode.LeftAlt);
    public readonly Configurable<KeyCode> tentMovementAutoEnable = new Configurable<KeyCode>(KeyCode.LeftControl);
    public readonly Configurable<bool> enableVignette = new Configurable<bool>(true);
    private UIelement[] UIArrPlayerOptions;

    public RotCatOptions(RotCat pluginInstance, ManualLogSource logSource) {
        Logger = logSource;
        tentMovementUp = config.Bind("tentacleMovementUp", KeyCode.W);
        tentMovementDown = config.Bind("tentacleMovementDown", KeyCode.S);
        tentMovementLeft = config.Bind("tentacleMovementLeft", KeyCode.A);
        tentMovementRight = config.Bind("tentacleMovementRight", KeyCode.D);
        tentMovementEnable = config.Bind("tentacleMovementEnable", KeyCode.LeftAlt);
        tentMovementAutoEnable = config.Bind("tentacleMovementAutoEnable", KeyCode.LeftControl);
        enableVignette = config.Bind("enableVignette", true);
    }

    public override void Initialize() {
        OpTab opTab = new OpTab(this, "Options");
        this.Tabs = new[]
        {
            opTab
        };

        UIArrPlayerOptions = new UIelement[]
        {
            new OpLabel(260f, 570f, "Options", true),

            new OpLabel(200f, 520f, "Movement Keybinds", true),
            new OpLabel(50f, 485f, "Movement Tentacles Up", false),
            new OpKeyBinder(tentMovementUp, new Vector2(350f, 480f), new Vector2(120f, 30f)) {description = "Tentacle Up"},
            new OpLabel(50f, 450f, "Movement Tentacles Down", false),
            new OpKeyBinder(tentMovementDown, new Vector2(350f, 445f), new Vector2(120f, 30f)) {description = "Tentacle Down"},
            new OpLabel(50f, 415f, "Movement Tentacles Left", false),
            new OpKeyBinder(tentMovementLeft, new Vector2(350f, 410f), new Vector2(120f, 30f)) {description = "Tentacle Left"},
            new OpLabel(50f, 380f, "Movement Tentacles Right", false),
            new OpKeyBinder(tentMovementRight, new Vector2(350f, 375f), new Vector2(120f, 30f)) {description = "Tentacle Right"},
            new OpLabel(50f, 345f, "Activate the tentacle movement", false),
            new OpKeyBinder(tentMovementEnable, new Vector2(350f, 340f), new Vector2(120f, 30f)) {description = "Enable movement via tentacles"},
            new OpLabel(50f, 310f, "Activate automatic tentacle movement", false),
            new OpKeyBinder(tentMovementAutoEnable, new Vector2(350f, 305f), new Vector2(120f, 30f)) {description = "Activte tentacles without choosing an initial surface"},
            new OpLabel(50f, 275f, "Enable Vignette effect", false),
            new OpCheckBox(enableVignette, new Vector2(400f, 270f))
        };
        opTab.AddItems(UIArrPlayerOptions);
    }
}