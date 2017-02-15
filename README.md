# Overview
Currently has a 'Package Browser' that allows you to view and extract individual files from PD:TH/PD2/PD2 Linux without having to extract the entire game.

I don't really have any plans to work on this as I've completely lost interest in doing anything with Payday. I may do some bits here and there in the future. Feel free to fork this repo or make Pull Requests. Code style is kinda all over the place, but I would prefer keeping it to CamelCase.
I've also released [DieselEngineFormats](https://github.com/simon-wh/DieselEngineFormats) which is used in this project. Built dll found in Libs/

This has basic scripting support. Includes a 'Heist Extractor' script which can be used to extract all files associated with a particular heist. This is what was used to create the No Mercy port by ViciousWalrus.
Some other scripts are also included

In the build the Hashlist should be split into paths, exts and others with extra full hashlists being put in an 'Extra' folder. All inside the 'Hashlists' directory. Development seperated hashlists can be found in the 'Hashlist Example' directory.

# Credits
* Thanks to David Anson for the basis of the virtual drag drop implementation. Found [here](http://dlaa.me/blog/post/9913083)
* Thanks to Yusuke Kamiyamane for the icons used from [Fugue Icons](http://p.yusukekamiyamane.com/)
* Thanks to all the developers of the Nuget packages used