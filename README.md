# StationeersBepinExModLoader

For This Patcher to work you Need BepinEx https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21

This Mod Loader will allow you to load BepinEx Mods from the Steam Work Shop and also your Local Mod Folder that is in Documents.

Just put the dll in your BepinEx/patchers folder

When it loads for the first time it creates a StationeersModLoader.xml file in the same folder rocketstation.exe is, this file contains if you want StationeersMods to load BepinEx mods 

If you also use StationeersMod https://github.com/jixxed/StationeersMods it is reconmended to leave "StationeersModsLoadsBepinExMod" to false so BepinEx mods do not load twice

This Patcher is a modified version of https://github.com/BepInEx/BepInEx.MultiFolderLoader to work with Stationeers

# Plans
Make it so the list in the WorkShop menu also sets the load order for mods
If mod is disable in the Workshop menu the Modloader will not load it.
I might add where you can set each mod if it will load with stationeersmods or not so you can pick and choose which mod StationeersMods load and which mods my ModLoader will load
