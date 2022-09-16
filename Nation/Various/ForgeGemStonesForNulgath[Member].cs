//cs_include Scripts/CoreBots.cs
//cs_include Scripts/CoreFarms.cs
//cs_include Scripts/Nation/CoreNation.cs

using Skua.Core.Interfaces;

public class ForgeGemStonesForNulgath
{

    public IScriptInterface Bot => IScriptInterface.Instance;
    public CoreBots Core => CoreBots.Instance;
    public CoreFarms Farm = new();
    public CoreNation Nation = new();



    public void ScriptMain(IScriptInterface bot)
    {
        Core.SetOptions();

        Nation.GemStonesOfnulgath();

        Core.SetOptions(false);
    }
}