# CMDG

![cover image](https://github.com/Byproduct/CMDG/blob/main/cover_image_small.png)

Welcome to the CMD graphics engine!

CMDG makes it easy for artistic C# programmers to draw graphics in the windows command prompt (cmd.exe). 

The aim is to be able to get right into it without having to study its workings for too long. You just need to take a look at the [the simple configuration file](https://github.com/Byproduct/CMDG/blob/main/CMDG/Config.cs), [the scene template](https://github.com/Byproduct/CMDG/blob/main/CMDG/Scenes/SceneTemplate.cs), and [the example scene](https://github.com/Byproduct/CMDG/blob/main/CMDG/Scenes/Example2D.cs) to get going.

In a nutshell, you have a SetPixel function
`Framebuffer.SetPixel(x, y, color))`
in which you specify the pixel coordinates and color, and the engine takes care of drawing that into the console. 

Besides that, you're free to structure your program in any way you like, add any classes, files, directories etc.

You have the [glorious ANSI palette of 16 colors](https://github.com/Byproduct/CMDG/blob/main/CMDG/Help/palette.png) to work with. You specify the colors in RGB bytes (e.g. 255,255,255) and the engine converts them to the nearest ANSI color.

By default the █ character is used for drawing. You can change it to any Unicode character, also while the scene is running, but the entire screen will always be drawn with the same character. (So think of it as pixels rather than ASCII art.)

A homemade 3D engine is also available, featuring .obj/mtl support, object/camera positions and rotations, lighting etc.  

More details are available in the template and example scenes.

If you've done something with this, we'd love to see it! [CMDG has its own Discord server](https://discord.gg/8j8auPha) which is also a good place for any questions you might have. :)
