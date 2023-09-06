
# Description

For This Patcher to work you Need BepinEx https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21

You also need to Open up the WorkShop Menu Atlease once so it creates a modconfig.xml you do not have to do this if you use this plugin https://github.com/ihatetn931/WorkShopPatches/releases

<img src="https://github.com/ihatetn931/StationeersBepinExModLoader/assets/2037352/52c0f1c0-4e46-4412-9b8d-b08cf65e84cc" width="150" height="80">

<img src="https://github.com/ihatetn931/StationeersBepinExModLoader/assets/2037352/92c7560d-9f76-4535-ba06-7461264cabe9" width="250" height="300">

Due to the limitiation of this being a BepinEx patcher I can not do anything a plugin does, so as of now if you subscribe to a mod you must load the game and go to the Workshop mod menu so it updates the files, restart your game and everything will work fine
only needed when you add new mods I am trying to find a way to work around this if I can't then I will prolly release a seperate mod. fixed with https://github.com/ihatetn931/WorkShopPatches/releases

This Mod Loader will allow you to load BepinEx Mods from the Steam Workshop and also your Local Mod Folder that is in Documents.

## Install
Just put the dll in your BepinEx/patchers folder

When it loads for the first time it creates a ModLoaderSettings.xml file in the same folder rocketstation.exe is, this file contains if you want StationeersMods to load BepinEx mods and some other mod info to make it easy to change values

If you also use StationeersMod https://github.com/jixxed/StationeersMods you can change LoadWithStationeersMod to true in ModLoaderSettings.xml and the Mod will be loaded by stationeers mods and not this modloader

### Recommended Plugins
This plugin will update the modconfig.xml on game load instead of when you open the workshop menu this is mostly so mods that you just added from the workshop will load the first time instead of having to go to the workshop 
menu<br>
https://github.com/ihatetn931/WorkShopPatches/releases

#### The Config
ModLoaderSettings.xml
This is a example of what the ModLoader Creates for the mods it detects
```xml
<BepinExMods>
  <Mod LoadWithStationeersMod="false"> <!--- Change this to true if you want StationeersMods to load this Mod --->
    <ModName>Deadly Toxins [StationeersMods]</ModName> <!--- The Mods Name got from the About.xml in the mod folder NOT CHANGEABLE --->
    <WorkshopId>3004378085</WorkshopId> <!--- The Mods WorkShop Id got from the About.xml in the mod folder, if it has no id it is 0 NOT CHANGEABLE --->
    <IsEnabled>true</IsEnabled> <!--- If mod is enabled in the workshop menu NOT CHANGEABLE --->
    <ModPath>C:\Program Files (x86)\Steam\steamapps\workshop\content\544550\3004378085</ModPath> <!--- The Mods ModPath the path the mod is NOT CHANGEABLE --->
  </Mod>
</BepinExMods>
```

##### Plans
1: Make it so the list in the WorkShop menu also sets the load order for mods.

##### Info
This Patcher is a modified version of https://github.com/BepInEx/BepInEx.MultiFolderLoader to work with Stationeers
