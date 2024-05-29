# 2.1.0
- Added vanilla style stealth meter with out of the box compatibility for InsanityDisplay, EladsHUD, and ShyHUD
- Improved local config syncing when changed via LethalConfig ingame
- (Internal) Updated project to NET Standard 2.1

# 2.0.11
- Fixed accidental hard dependancy on InputUtils

# 2.0.10
- Fixed minor transpilation conflict with LethalCompanyVR
- Added config to ignore custom keybinds for compatibility with unconventional controller schemes when InputUtils is installed

# 2.0.9
- Fixed bug stopping interaction when a mask is worn when the player is ejected

# 2.0.8
- Added safety checks to try and keep any errors self contained within the mod's effects

# 2.0.7
- Added minimum time to stay hidden for collision ignoring effects to kick in

# 2.0.6
- Reverted all changes to the way player ids are handled

# 2.0.5
- Reverted player id extension using the wrong field due to way more issues

# 2.0.4
- Patched the network events that were missed in 2.0.3 causing issues for clients
- Fixed player id extension using the wrong field
- Added NRE fix for exposed vanilla thumper collision detection

# 2.0.3
- Add config option to ignore enemy collisions events when using the mask

# 2.0.2
- Add compatibility to HotbarPlus and similar mods
- Added mask grab and drop sounds to Drama

# 2.0.1
- Switched main supported version to v50 with addition of version specific enemy patches

# 2.0.0
- Added better support for hiding from all enemies
- Added configs to specify which enemy groups and overrides to use
- Improved config for specifying moon overrides with special values
- Added dropdown selection for applicable config fields in LethalConfig

# 1.3.5
- Fix typo causing QE interactions to have duplicate keybinds if only one is rebinded

# 1.3.4
- Fix item QE interactions due to v50 inputUtils

# 1.3.3
- Fix recently introduced incompatibility with MoreCompany with item QE interactions
- Added FAQ and improved some config descriptions (e.g. showing options in LethalConfig)

# 1.3.2
- Fix inability to interact with items when holding a mask due to bad handling of InputUtils on default controls
- Allow mask eyes to be toggled when looking at interactable if the keybind doesn't clash with interact's

# 1.3.1
- Added config to disable mask possession when attached

# 1.3.0
- Allowed players to get possessed when wearing the Comedy or Tragedy masks
- Fixed using terminal while wearing mask showing a fake mask in the players' hand
- Allow mask view to be different based on holding up or attaching
- Added translucent mask option on usage
- Fixed phantom bodies remaining on death for clients
- Added config syncing through the use of CSync (Sigurd)
- Added config for instant local mask actions for clients
- Added compatibility for Lethal Config to change configs mid-game
- Added compatibility for Input Utils to rebind mask controls

# 1.2.4
- Fixed masks rotating every time they are worn
- Fixed bar colour config not working as intended

# 1.2.3
- Fixed mask size and positioning to match Masked enemies
- Fixed bug where wearing mask won't always hide you
- Added config to set spawn rarity, including a fine grain option for specific moons

# 1.2.2
- Fixed being softlocked in the mask attached state if worn on teleport, centipede attack, or ship retuning to orbit
- Reduced duplication of masks on disconnection
- Fixed control tips being overridden when other players attach their masks

# 1.2.1
- Fixed recently introduced incompatibility issues with MoreEmotes

# 1.2.0
- Added behaviours to mimic the Masked (allowing masks to be attached, and activation of the eyes)
- Fixed Drama mask displaying incorrectly at far distances

# 1.1.3
- Fixed incompatibility issues with BiggerLobby

# 1.1.2
- Fix to stop server host seeing other player's masks as outlines if they have the setting enabled

# 1.1.1
- Added option to only show an outline when equipping masks for better visibility

# 1.1.0
- Added a regenerative stealth meter for the masks to allow hiding mechanic to be balanced (configurable)

# 1.0.0
- Added the Drama mask item
- Allowed configured masks (defaults to all) to hide you from configured enemies (defaults to Masked)