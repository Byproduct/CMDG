# CMDG

![cover image](https://github.com/Byproduct/CMDG/blob/main/cover_image_small.png)

Welcome to the commandline graphics engine!

CMDG makes it easy for artistic C# programmers to draw graphics in the windows command terminal (cmd.exe). 

The aim is to get right into it without having to study some mysterious mechanics too much. The aim is that you only need to take a look at [the simple configuration file](https://github.com/Byproduct/CMDG/blob/main/CMDG/Config.cs), [the scene template](https://github.com/Byproduct/CMDG/blob/main/CMDG/Scenes/SceneTemplate.cs), and [the example scene](https://github.com/Byproduct/CMDG/blob/main/CMDG/Scenes/Example2D.cs) to get going.

In a nutshell, you have a SetPixel function
`Framebuffer.SetPixel(x, y, color))`
in which you specify the pixel coordinates and color (rgb in bytes),
and the engine takes care of turning that into a character on screen. 

Besides that, you're free to structure your program in any way you like, add any files, directories etc.

More details are available in the template and example scenes, and 3D tutorial is also in the works.

If you've done something with this, I'd love to see it!

If you have any questions, [http://discordapp.com/users/244907716231954442](drop me a message on Discord).
