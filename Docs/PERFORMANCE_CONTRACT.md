# Performance Contract

## Targets
- Primary target: stable 60 FPS on a representative upper-midrange Android device.
- Fallback: stable 30 FPS on lower-tier supported devices.
- Avoid frame spikes caused by per-shot allocations, repeated global searches, and uncontrolled enemy spawning.
- Camera follow is updated in LateUpdate and uses unscaled, frame-rate-independent interpolation.

## Runtime rules
- Pool gameplay actors wherever lifetime is frequent/repetitive.
- Avoid Instantiate/Destroy in hot combat loops.
- Avoid Find* calls inside per-frame AI paths.
- Prefer shared materials and static batching for repeated world geometry.
- Use LODs, occlusion/streaming and scalable effects for high-density areas.
- Profile on actual Android hardware before claiming jitter-free behavior.

## Diagnostic
Press F3 in the development build to show FPS, frame time, render scale, and active enemy count.
