using BepInEx;
using BepInEx.Configuration;
using RoR2;

namespace CheaperCauldrons
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.MagnusMagnuson.CheaperCauldrons", "CheaperCauldrons", "1.0.0")]
    public class CheaperCauldrons : BaseUnityPlugin
    {

        public static ConfigWrapper<int> greenCost;
        public static ConfigWrapper<int> redCost;
        public void Awake()
        {
            initConfig();

            On.RoR2.PurchaseInteraction.Awake += (orig, self) =>
            {
                orig(self);
                if(self.name.StartsWith("LunarCauldron"))
                {
                    if(self.costType == CostTypeIndex.WhiteItem)
                    {
                        self.cost = greenCost.Value;
                        self.Networkcost = greenCost.Value;
                    }
                    else 
                    if(self.costType == CostTypeIndex.GreenItem)
                    {
                        self.cost = redCost.Value;
                        self.Networkcost = redCost.Value;
                    }
                }

            };
        }

        private void initConfig()
        {
            greenCost = Config.Wrap(
            "Config",
            "greenCost",
            "Number of items needed to use the green item lunar cauldron.",
            2);

            redCost = Config.Wrap(
            "Config",
            "redCost",
            "Number of items needed to use the red item lunar cauldron.",
            3);
        }
    }
}
