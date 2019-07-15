using BepInEx;

namespace DisableItemDisplay
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.MagnusMagnuson.DisableItemDisplay", "DisableItemDisplay", "1.0.0")]
    public class DisableItemDisplay : BaseUnityPlugin
    {
        public void Awake()
        {
            On.RoR2.CharacterModel.EnableItemDisplay += (orig, self, itemIndex) =>
            {
                return;
            };

        }
    }
}
