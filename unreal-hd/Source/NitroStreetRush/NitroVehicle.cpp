#include "NitroVehicle.h"
#include "Components/StaticMeshComponent.h"

ANitroVehicle::ANitroVehicle()
{
    PrimaryActorTick.bCanEverTick = true;

    Root = CreateDefaultSubobject<USceneComponent>(TEXT("Root"));
    SetRootComponent(Root);

    VisualBody = CreateDefaultSubobject<UStaticMeshComponent>(TEXT("VisualBody"));
    VisualBody->SetupAttachment(Root);
}

void ANitroVehicle::Tick(float DeltaSeconds)
{
    Super::Tick(DeltaSeconds);

    const float BaseBoost = bNitro && Nitro > 0.f ? 1.9f : 1.f;
    if (bNitro && Nitro > 0.f)
        Nitro = FMath::Max(0.f, Nitro - NitroDrainPerSecond * DeltaSeconds);
    else
        Nitro = FMath::Min(100.f, Nitro + 7.f * DeltaSeconds);

    const float SpeedKph = GetVelocity().Size() * 0.036f;
    const float SteeringResponse = FMath::Clamp(SpeedKph / MaxSpeedKph, 0.15f, 1.f);
    AddActorLocalRotation(FRotator(0.f, Steer * 55.f * SteeringResponse * DeltaSeconds, 0.f));
    AddActorLocalOffset(GetActorForwardVector() * (Acceleration * BaseBoost * DeltaSeconds), true);
}

void ANitroVehicle::SetSteerInput(float Value)
{
    Steer = FMath::Clamp(Value, -1.f, 1.f);
}

void ANitroVehicle::SetNitroInput(bool bEnabled)
{
    bNitro = bEnabled;
}
