using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace BossRush
{
    interface IMode
    {
        void CreateTerminals(On.RoR2.MultiShopController.orig_CreateTerminals orig, MultiShopController self);

        void UpdateTerminals(MultiShopController self);
    }
}
