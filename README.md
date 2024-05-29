[![Release Version](https://img.shields.io/github/v/release/Henit3/DramaMask?style=for-the-badge&logo=github)](https://github.com/Henit3d/DramaMask/releases)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/necrowing/DramaMask?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/necrowing/DramaMask/)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/necrowing/DramaMask?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/necrowing/DramaMask/)

# Stealthy Masks

Enables masks to be worn to hide from enemies by blending in with the Masked!
Also adds a new type of mask, Drama, which is not haunted unlike its Tragedy and Comedy counterparts, allowing you to use this feature safely!

They can also be attached to players' faces, allowing you to look identical to Masked enemies and spook your friends!
To balance this mechanic out, there is a stealth meter that is used while attempting to hide.

### Compatibility
* **[LethalLevelLoader](https://thunderstore.io/c/lethal-company/p/IAmBatby/LethalLevelLoader/)**: If playing on v50+ and LLL is being used, ensure it is on 1.2.0 or higher to avoid conflicts with most mods; including this one.
* **[MaskedAiRevamp](https://thunderstore.io/c/lethal-company/p/Piggy/MaskedAIRevamp/)**: Ignoring collision with Masked enemies will not work; it is recommended to use [LethalIntelligence](https://thunderstore.io/c/lethal-company/p/VirusTLNR/LethalIntelligenceExperimental/) instead as this is a successor with more updates.

Works on v45-v50 until 2.0.0 and v50+ with 2.0.1+ with no other known incompatibilities; supports controller and [LethalCompanyVR](https://thunderstore.io/c/lethal-company/p/DaXcess/LethalCompanyVR/)!

> Formerly known as DramaMask

## Config Options
* Which masks can be used by the player to hide **[Host]**
* The selection of enemies that can be hidden from & specific enemy overrides **[Host]**
* The selection of enemies that players won't trigger collision events for when hiding **[Host]**
* The minimum time players need to be hidden to start ignoring collision events **[Host]**
* Potentially increase custom enemy compatibility at the cost of performance **[Host]**
* Disable the stealth meter balancing mechanic **[Host]**
* Adjust stealth meter behaviour (can be made different if attached) **[Host]**
* Adjust stealth meter appearance (colour and vanilla meter offset) **[Client]**
* Stealth meter visibility **[OptionalSync]**
* Remove mask on meter depletion **[Host]**
* Adjust drama mask spawn rates with a multiplier & specific moon overrides **[Host]**
* Change mask action keybinds if Input Utils has been installed **[Client]**
* Disable mask possession when attached **[Host]**
* Change mask view on usage (can be made different if attached) **[OptionalSync]**
* Stop mask actions applying immediately on non-host clients, as it can cause temporary visual desync on rapid changes **[Client]**
* Ignore custom keybinds with InputUtils installed for compatibility with unconventional control schemes like with LethalCompanyVR **[Client]**

## FAQ
### Where can I find the Drama mask?
The Drama mask can be found as scrap with its spawn locations and rates matching that of the Comedy and Tragedy masks (detailed in the table below).
This can be adjusted to match your preferences with the base spawn rate multiplier (applied on the default rates) and custom spawn config settings (for granular customisation per moon).
Special values for this include [All, Vanilla, Modded] in line with what is provided by LethalLib, and all values are evaluated left to right (for overriding).

|Moon		|Spawn Chance	|
|-----------|:-------------:|
|Assurance 	|3				|
|Rend      	|40				|
|Dine      	|40				|
|Titan		|40				|
|Modded		|40				|

To also make it available in the store, check out @megapiggy's [BuyableDramaMask](https://thunderstore.io/c/lethal-company/p/MegaPiggy/BuyableDramaMask/) mod.

### What enemies are supported with hide from all enemies?
While the Masked are the main targets to hide from, proper support for other enemies has been added as of 1.4.0 (detailed in the table below).
Some of these enemies can be considered to have their own agenda, so they have been classified as "Not Natural".
Players will not be hidden from these enemies if the "Enemies Hidden From" config is set to "Natural".
For example, Hoarding Bugs would not discriminate against a """Masked""" if they decide to start stealing from their nest.

Extra overrides for these can be specified using the "Enemy Hiding Overrides" config to enable or disable for specific enemies using their names in the code (exclusions prioritised).

|Enemy			|Status					|
|---------------|:---------------------:|
|Masked 		|Supported - Always		|
|Thumper      	|Supported				|
|Spore Lizard	|Supported				|
|Coil-Head		|Supported				|
|Earth Leviathan|Supported - Not Natural|
|Forest Giant	|Supported				|
|Baboon Hawk	|Supported - Not Natural|
|Hygrodere		|Supported				|
|Bunker Spider	|Supported - Not Natural|
|Jester			|Supported				|
|Bracken		|Supported				|
|Circuit Bee	|Supported - Not Natural|
|Ghost Girl		|Supported - Not Natural|
|Snare Flea		|Supported				|
|Nutcracker		|Supported				|
|Hoarding Bug	|Supported - Not Natural|
|Butler			|Supported				|
|Masked Bee		|Supported - Not Natural|
|Old Bird		|Supported - Not Natural|
|Tulip Snake	|Supported				|
|Eyeless Dog	|No						|
|Manticoil		|No						|
|Roaming Locust	|No						|
|Company		|No						|
|Turret			|Supported				|
|Modded			|Compatibility Dependent|

### Which modded enemies are supported? / How can I make my custom enemy support hiding?
The current method of hiding depends on references to `StartOfRound.allPlayerScripts` being patched to only include entries of visible (and pre-existing invalid) players,
when the base methods for detecting players in `EnemyAI` are called.
Vanilla enemies that make use of their own methods for player detection are additionally patched, so they may also be invoked by custom enemies.
References to `PlayerIsTargetable` are also patched for calls made outside `MeetsStandardPlayerCollisionConditions`, if the "Increased Custom Enemy Compatibility" setting is enabled.
Any storage or cross-referencing done with an index to `allPlayerScripts` (like in `ButlerEnemyAI`) should instead make use of `playerClientId` on the player,
since the position in the array may be affected by this mod's patches.

Enemies can also be added to `DramaMask.Config.EnemyTargetHandler.NaturalExceptions` to stop them being hidden from if the "Enemies Hidden From" setting is "Natural".

Aside from this, explicit patches for custom enemies will _not_ be provided by this mod.

### Why can I not use items with a mask attached?
This is intentional behaviour and is currently integral to how the mod's mask attaching features work. Changing this could mean rewriting the entire codebase to accomodate this, so it is currently not supported. This is feasible though, and may be considered for an update in the distant future.

## Roadmap (development to be paused)
* Sound and visuals support for VR in place of the stealth bar
* Allow the player to have their hands out like the Masked (unlikely without more help)

## Credits
Made on request from @tkcool and @pedro9006.

#### Feature Proposers:
* Stealth meter: @mintiivanilla
	* UI inspired by @megapiggy's
	[InsanityMeter](https://thunderstore.io/c/lethal-company/p/MegaPiggy/InsanityMeter/) mod
* Mask visibility on use: @star0138
* Config based on usage type: @sagey08
* Disabling collision events based on visibility: @alancrytex, @qwbarch, @autumnis

#### Bug Finders:
* (Pre-Release) Networking: @roshposhtosh, @sabzy, and @saintshekzz
* BiggerLobby incompatibilities: @TheRealMrKam (via GitHub)
* MoreEmotes incompatibilities: @sagey08 and @sashimi_express
* Stuck Mask on orbit: @Regnareb and @sashimi_express
* Rotating masks & colour config: @sashimi_express
* Phantom bodies on death client-side: @ValiusV (via GitHub) and @purefpszac
* Cannot interact (later QE) when holding mask: @purefpszac
* Cannot QE interact when not holding mask (v50): @VirusTLNR (via GitHub)
* HotbarPlus incompatibilities: @mari0no1
* Thumper near player detection NRE: @jontep5543
* Issues on client crash: @mari0no1
* No interaction on game over with mask worn: @virustlnr
* Transpilation incompatibility with LethalVR: @daxcess

### Contact
For requesting new features or highlighting issues/bugs found, please post them in the mod's
[Dedicated Discord Channel](https://discord.com/channels/1168655651455639582/1209275419505860719)
on the [Official Lethal Company Modding Discord](https://discord.gg/XeyYqRdRGC).

* [Thunderstore](https://thunderstore.io/c/lethal-company/p/necrowing/DramaMask/)
* [GitHub](https://github.com/Henit3/DramaMask)