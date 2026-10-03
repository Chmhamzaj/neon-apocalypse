#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Pawn.h"
#include "NitroVehicle.generated.h"

UCLASS()
class NITROSTREETRUSH_API ANitroVehicle : public APawn
{
    GENERATED_BODY()

public:
    ANitroVehicle();

    virtual void Tick(float DeltaSeconds) override;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Handling")
    float MaxSpeedKph = 320.f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Handling")
    float Acceleration = 4200.f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Nitro")
    float Nitro = 100.f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category="Nitro")
    float NitroDrainPerSecond = 26.f;

    UFUNCTION(BlueprintCallable)
    void SetSteerInput(float Value);

    UFUNCTION(BlueprintCallable)
    void SetNitroInput(bool bEnabled);

protected:
    UPROPERTY(VisibleAnywhere)
    TObjectPtr<USceneComponent> Root;

    UPROPERTY(VisibleAnywhere)
    TObjectPtr<UStaticMeshComponent> VisualBody;

private:
    float Steer;
    bool bNitro;
};
