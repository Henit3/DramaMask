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