using CMDG;

Util.Initialize();
Util.DrawBorder();

// Change this value to select the source scene file. (Adding more scenes to choose from is fine.)
int sceneChoice = 2; 
switch (sceneChoice)
{
    case 1:  // default "moving pixels" scene
        Scene.Run();
        break;
    case 2:  // plasma scene by DeepThink
        Scene2.Run();
        break;
    case 3:
        SceneTemplate.Run();  // empty scene template
        break;
}
