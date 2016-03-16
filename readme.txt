================== 
Roguelike
================== 

The game is a turnbased, multiplayer roguelike. This means that the game is intended to be slow-paced and even just moving around is quite slow, especially if more than one character moves simultaneously.

================== 
Basic mechanics
================== 

Each turn you have a couple of seconds to give orders for the next turn, after which there is a visualization phase that takes from 0 to several seconds depending on how much stuff happened during the turn. The visualization is shown in the order in which events took place on the server's game simulation, although some parts haven't been implemented yet (death animation, hitpoint change on attack, item pickup.) 

The order-giving phase is indicated by an area around your character being highlighted in green, showing you how far you can move in one turn. You should only be able to control your character during this phase, during the visualization phase input is disabled.

================== 
Basic controls
================== 

You control your character mostly using your mouse. Right clicking on an empty tile tells your character to move to that tile, possibly over several turns if it's too far for a single turn. Right clicking on enemies gives an attack command, and your character attempts to move to a tile adjacent to the targeted enemy and attack it. If you move over an item at any point during your turn, you will pick it up unless your inventory is full (currently limited at 5 items.)


================== 
Starting a game
================== 

You start the game in the main menu. To get to the actual game, you have to either host a local server yourself or join a remote server. If you host a local game, you can either start the game right away by pressing ready, in which case you will start a single-player session, or you can wait for more players to join your server to play in multiplayer. To join a server, press join game and either select a detected server in the local network servers list, or enter an IP address to a remote server if you know it. The dedicated server doesn't work currently as it's running an older version.

================== 
Items
================== 

There are two main kinds of equipment items that are currently implemented. Melee weapons, eg. the sword, give you an attack damage bonus for melee attacks. Ranged weapons, eg. the bow, allow you to attack enemies from afar. 

To use melee weapons, you must equip it using the equipment screen (press C to open both inventory and equipment screens.) Drag the sword you picked up into the weapon slot in the equipment screen, and you will receive bonus attack damage, as well as a different attack sound effect. 

For bows, you must drag the bow to a slot on the action bar to use it. Either click the action bar slot using your left mouse button or press the number key indicated on the slot to enter bow aiming mode. In this mode, right clicking an enemy will tell your character to shoot the bow at the enemy. Right clicking on an empty tile or pressing esc will cancel aiming.

Potions are consumable items that you can drink to restore lost hit points. Using is identical to bows, except that the potion will be used immideately after activation instead of entering aiming mode.

================== 
Combat
================== 

You can attack enemies by using the attack command (see basic controls) or shooting them with a bow (see items.) In either case, your attacks will deplete the enemy's hit points, and the enemy will die once they reach 0. You can attack other players or the test NPC that's placed near the middle of the scene. The NPC will also attack you once you go within three tiles of it.

================== 
Chat
================== 

The game has an in-game chat feature. Press enter to open the chat window. You can type in messages and send them to all connected players by using the text field. Currently your client ID number is displayed in place of your chosen nickname.

================== 
Minimap
================== 

You can use the minimap in the top left corner to orienteer yourself and keep track of nearby visible enemies or items. It only shows tiles you have seen at some point. Dynamic objects are only shown if currently visible, other tiles are displayed as long as you've seen them at some point in the past. The minimap uses color-coding for presenting a simplified version of the scene:

Grey = unknown tile, tiles you haven't seen so far
Light grey = open tile
Black = walls
Pink = highlight for unknown tiles adjacent to open tiles
Red = character, player or NPC
Gold = item

================== 
Console
================== 

The test executable is built as a developer version, so any errors occurring during play will be printed in a small window near the bottom of the screen. This should help you in determining whether or not something is a bug or intended behaviour. At startup a single error message is printed to the console, but it doesn't affect anything (see known issues.)

================== 
Known issues
================== 

-"Failed to find an accessible position" error printed at startup. Doesn't affect anything, and not really an error.
-After dying, using the chat or trying to move will generate errors.
-Exiting a game and restarting a new server or joining another game breaks everything.
-Two characters attacking and killing a third character simultaneously will generate an error.
-Two characters moving over an item and trying to pick it up simultaneously will generate an error.
-Item tooltips don't always work. Sometimes clicking the inventory screen will make them appear.
-Items dropped on death don't work.