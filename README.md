# Overview
Currently has a 'Package Browser' that allows you to view and extract individual files from PD:TH/PD2/PD2 Linux without having to extract the entire game.

Feel free to fork this repo or make Pull Requests. Code style is kinda all over the place, but I would prefer keeping it to CamelCase.
I've also released [DieselEngineFormats](https://github.com/Luffyyy/DieselEngineFormats) which is used in this project. Built dll found in Libs/

This fork was mostly fixed to run quicker and for it to work on PAYDAY 2 and Raid WW2 I don't intend to continue the developement of it as the GUI framework used is awful to use. If were to ever to do something serious, it would be a complete rewrite of this program maybe in a different programming language even.

This has basic scripting support. Includes a 'Heist Extractor' script which can be used to extract all files associated with a particular heist. This is what was used to create the No Mercy port by ViciousWalrus.
Some other scripts are also included

In the build the Hashlist should be split into paths, exts and others with extra full hashlists being put in an 'Extra' folder. All inside the 'Hashlists' directory. Development seperated hashlists can be found in the 'Hashlist Example' directory.

# Credits
* Thanks to Simon W for creating the original program.
* Thanks to David Anson for the basis of the virtual drag drop implementation. Found [here](http://dlaa.me/blog/post/9913083)
* Thanks to Yusuke Kamiyamane for the icons used from [Fugue Icons](http://p.yusukekamiyamane.com/)
* Thanks to all the developers of the Nuget packages used
