[![Release Version](https://img.shields.io/github/v/release/Henit3/DramaMask?style=for-the-badge&logo=github)](https://github.com/Henit3d/DramaMask/releases)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/necrowing/DramaMask?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/necrowing/DramaMask/)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/necrowing/DramaMask?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/necrowing/DramaMask/)

# Stealthy Masks

Wearing masks makes you blend in with the other Masked!
Also adds a new type of mask, Drama, which is not haunted unlike its Tragedy and Comedy counterparts, allowing you to use this feature safely!

They can also be attached to players' faces, allowing you to look identical to Masked enemies and spook your friends!
To balance this mechanic out, there is a stealth meter that is used while attempting to hide.

Due to the current transitioning of mod libraries from v49 to v50, LethalLevelLoader may be broken until v50 comes out.
This is only the case if you have both LLL and LLLFixed enabled; disabling LLL should fix this issue.
Works on v45-v50 with no other known incompatibilities; supports controller and LethalCompanyVR!

> Formerly known as DramaMask

## Config Options
* Which masks can be used by the player to hide [Host]
* The player can hide from all enemies (Experimental) [Host]
* Disable the stealth meter balancing mechanic [Host]
* Adjust stealth meter behaviour (can be made different if attached) [Host]
* Adjust stealth meter appearance (position and colour) [Client]
* Stealth meter visibility [OptionalSync]
* Remove mask on meter depletion [Host]
* Adjust drama mask spawn rates with a multiplier, and on specific moons [Host]
* Change mask action keybinds if Input Utils has been installed [Client]
* Disable mask possession when attached [Host]
* Change mask view on usage (can be made different if attached) [OptionalSync]

## FAQ
### Where can I find the Drama mask?
The Drama mask can be found as scrap with its spawn locations and rates matching that of the Comedy and Tragedy masks (detailed in the table below). This can be adjusted to match your preferences with the base spawn rate multiplier (applied on the default rates) and custom spawn config settings (for granular customisation per moon).

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
|Butler			|Planned				|
|Masked Bee		|Planned - Not Natural	|
|Old Bird		|Planned - Not Natural	|
|Tulip Snake	|Planned				|
|Eyeless Dog	|No						|
|Manticoil		|No						|
|Roaming Locust	|No						|
|Company		|No						|
|Turret			|Supported				|

### Why can I not use items with a mask attached?
This is intentional behaviour and is currently integral to how the mod's mask attaching features work. Changing this could mean rewriting the entire codebase to accomodate this, so it is currently not Supported. This is feasible though and may be considered for an update in the distant future.

## Roadmap (development to be paused)
* Add better support for all the experimentatal all enemy hiding behaviour
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

#### Bug Finders:
* (Pre-Release) Networking: @roshposhtosh, @sabzy, and @saintshekzz
* BiggerLobby incompatibilities: @TheRealMrKam (via GitHub)
* MoreEmotes incompatibilities: @sagey08 and @sashimi_express
* Stuck Mask on orbit: @Regnareb and @sashimi_express
* Rotating masks & colour config: @sashimi_express
* Phantom bodies on death client-side: @ValiusV (via GitHub) and @purefpszac
* Cannot interact (later QE) when holding mask: @purefpszac

### Contact
For requesting new features or highlighting issues/bugs found, please post them in the mod's
[Dedicated Discord Channel](https://discord.com/channels/1168655651455639582/1209275419505860719)
on the [Official Lethal Company Modding Discord](https://discord.gg/XeyYqRdRGC).

* [Thunderstore](https://thunderstore.io/c/lethal-company/p/necrowing/DramaMask/)
* [GitHub](https://github.com/Henit3/DramaMask)