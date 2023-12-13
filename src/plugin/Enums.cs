using System.Diagnostics.CodeAnalysis;

namespace Chimeric;
public sealed class CreatureTemplateType {
    [AllowNull] public static CreatureTemplate.Type BabyAquapede = new(nameof(BabyAquapede), true);
}
public static class SandboxUnlockID {
    [AllowNull] public static MultiplayerUnlocks.SandboxUnlockID BabyAquapede = new(nameof(BabyAquapede), true);
}
public static class SoundEnums
{
    public static void RegisterValues() {
        Silence = new SoundID("Silence", true);
    }
    [AllowNull] public static SoundID Silence;
}