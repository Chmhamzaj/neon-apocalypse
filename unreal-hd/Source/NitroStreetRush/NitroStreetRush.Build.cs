using UnrealBuildTool;

public class NitroStreetRush : ModuleRules
{
    public NitroStreetRush(ReadOnlyTargetRules Target) : base(Target)
    {
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;
        PublicDependencyModuleNames.AddRange(new[]
        {
            "Core", "CoreUObject", "Engine", "InputCore",
            "EnhancedInput", "ChaosVehicles"
        });
    }
}
