# APK test vs Play Store distribution

The requested test artifact is an installable APK. This project contains `Neon Apocalypse > Build Android APK (Test)`, but this execution environment has no Unity Editor/Android SDK/ADB, so an actual APK is not fabricated.

For the eventual Google Play release, a large game should use Android App Bundle + Play Asset Delivery rather than trying to ship the whole world as one legacy APK. Current Google Play documentation lists a 500 MB base-module download limit and 1.5 GB per asset pack, with up to 4 GB for the cumulative total of modules and install-time asset packs; legacy APK publishing is subject to a 100 MB limit. See the official Google Play documentation referenced in the project release notes.
