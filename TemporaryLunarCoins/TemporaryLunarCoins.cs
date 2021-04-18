using BepInEx;
using BepInEx.Configuration;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace TemporaryLunarCoins
{
    [BepInDependency("com.bepis.r2api")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin("com.MagnusMagnuson.TemporaryLunarCoins", "TemporaryLunarCoins", "0.3.1")]
    public class TemporaryLunarCoins : BaseUnityPlugin
    {
        bool AllAgree = false;
        static List<SteamPlayer> SteamPlayers = new List<SteamPlayer>();

        private static ConfigEntry<bool> ChangeDroprate;
        private static ConfigEntry<float> DropChance;
        private static ConfigEntry<float> DropMulti;
        private static ConfigEntry<bool> SkipOnSinglePlayer;
        private static ConfigEntry<bool> AlwaysRemoveCoinsInMultiplayer;


        //public void Update()
        //{
        //    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C)) 
        //    {
        //        PlayerCharacterMasterController.instances[0].networkUser.AwardLunarCoins(10);
        //    }
        //}

        public void Awake()
        {

            ChangeDroprate = Config.Bind("", "ChangeDroprate", true, new ConfigDescription("If this is set to false, it will ignore the other values and use vanilla settings."));
            DropChance = Config.Bind("", "DropChance", 1.5f, new ConfigDescription("The initial chance to drop coins. Vanilla is 1 (percent)"));
            DropMulti = Config.Bind("", "DropMulti", 0.5f, new ConfigDescription("The multiplier applied to the drop chance after a lunar coins has dropped. Results in diminishing returns if smaller than 1. Vanilla is 0.5 (halving chance every drop). Set to 1 if you want a consistent drop rate."));
            SkipOnSinglePlayer = Config.Bind("", "SkipOnSinglePlayer", false, new ConfigDescription("If this is set to true, Lunar Coins are automatically removed in single player."));
            AlwaysRemoveCoinsInMultiplayer = Config.Bind("", "AlwaysRemoveCoinsInMultiplayer", true, new ConfigDescription("If this is set to true, Lunar Coins are always automatically removed for you."));

            //RoR2.Run.onRunStartGlobal += Run_Start; //Apparently the proper way to do it, since the game natively has an event for it. But ProperSave is now il hooking TLC to deal with compatibility issues I believe, so just leaving as is, as ProperSave breaks otherwise.
            On.RoR2.Run.Start += Run_Start;

            On.RoR2.Chat.UserChatMessage.ConstructChatString += UserChatMessage_ConstructChatString;

            //Taken from LoonerCoins
            BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var initDelegate = typeof(PlayerCharacterMasterController).GetNestedTypes(allFlags)[0].GetMethodCached(name: "<Init>b__62_0");

            if (ChangeDroprate.Value)
            {
                MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Modify(initDelegate, (Action<ILContext>)coinDropHook);
                On.RoR2.PlayerCharacterMasterController.Awake += PlayerCharacterMasterController_Awake;
            }

        }

        private void PlayerCharacterMasterController_Awake(On.RoR2.PlayerCharacterMasterController.orig_Awake orig, PlayerCharacterMasterController self)
        {
            orig(self);
            self.SetFieldValue("lunarCoinChanceMultiplier", DropChance.Value);
        }


        private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);
            if(AlwaysRemoveCoinsInMultiplayer.Value && !RoR2Application.isInSinglePlayer)
            {
                StartCoroutine(DelayedAutomaticCoinRemovalLocal());
            }
            if (NetworkServer.active)
            { 
                if (Run.instance.stageClearCount == 0) // ProperSave already added a fix for this.
                {
                    if (RoR2Application.isInSinglePlayer && SkipOnSinglePlayer.Value)
                    {
                        StartCoroutine(DelayedAutomaticCoinRemoval());
                    }
                    else
                    {
                        AllAgree = false;
                        StartCoroutine(StartCoinRemovalAgreement());
                    }

                }
            }
        }

        private List<SteamPlayer> PopulateSteamPlayersList()
        {
            List<SteamPlayer> steamPlayers = new List<SteamPlayer>();
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                if(PlayerCharacterMasterController.instances[i].networkUser.lunarCoins != 0)
                {
                    steamPlayers.Add(new SteamPlayer(PlayerCharacterMasterController.instances[i]));
                }
                    
            }

            return steamPlayers;
        }

        private string UserChatMessage_ConstructChatString(On.RoR2.Chat.UserChatMessage.orig_ConstructChatString orig, Chat.UserChatMessage self)
        {
            if(!AllAgree && (self.text.ToLower().Trim().Equals("tlc_aye") || self.text.ToLower().Trim().Equals("tic_aye")))
            {
                NetworkUser networkUser = self.sender.GetComponent<NetworkUser>();
                if (networkUser)
                {
                    var steamID = networkUser.id.steamId;
                    for (int i = 0; i < SteamPlayers.Count; i++)
                    {
                        if (SteamPlayers[i].steamID == steamID)
                        {
                            SteamPlayers[i].isReady = true;
                            break;
                        }
                    }
                    bool checkAllReady = true;
                    for (int i = 0; i < SteamPlayers.Count; i++)
                    {
                        if (!SteamPlayers[i].isReady)
                        {
                            checkAllReady = false;
                            break;
                        }
                    }
                    if(checkAllReady)
                    {
                        //Time.timeScale = 1f;
                        AllAgree = true;
                        RemoveLunarCoins();
                        UnfreezePlayers();
                    }
                }
            }
            
            return orig(self);
        }


        // Taken from LoonerCoins by Paddywaan
        private void coinDropHook(ILContext il)
        {
            var c = new ILCursor(il);

            //This doesn't actually change the lunarCoinChanceMultiplier, but it does influence it
            //c.GotoNext(
            //    x => x.MatchLdcR4(1f),
            //    x => x.MatchLdloc(out _),
            //    x => x.MatchLdfld<PlayerCharacterMasterController>("lunarCoinChanceMultiplier"),
            //    x => x.MatchMul(),
            //    x => x.MatchLdcR4(0f)
            //);
            //c.Next.Operand = DropChance.Value;


            c.GotoNext(
                x => x.MatchDup(),
                x => x.MatchLdfld<PlayerCharacterMasterController>("lunarCoinChanceMultiplier"),
                x => x.MatchLdcR4(0.5f),
                x => x.MatchMul()
                );
            c.Index += 2;
            c.Next.Operand = DropMulti.Value;
            //Debug.Log(il);
        }


        private void UnfreezePlayers()
        {
            foreach (var p in SteamPlayers)
            {
                p.pcmc.master.GetBody().gameObject.GetComponent<SetStateOnHurt>().SetFrozen(0.1f);
                p.pcmc.master.GetBody().RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
                p.pcmc.master.GetBody().AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 3f);
            }
        }

        private void RemoveLunarCoins()
        {
            for(int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                PlayerCharacterMasterController.instances[i].networkUser.DeductLunarCoins(PlayerCharacterMasterController.instances[i].networkUser.lunarCoins);
            }
        }

        public IEnumerator StartCoinRemovalAgreement()
        {

            yield return new WaitForSeconds(7f);
            //Time.timeScale = 0f;
            SteamPlayers = PopulateSteamPlayersList();
            if (SteamPlayers.Count == 0)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = " -- Temporary Lunar Coins --" +
                    "\n<size=15px>Automatically removed lunar coins.</size>"
                });
                yield break;
            }
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = " -- Temporary Lunar Coins --" +
                "\n<size=15px>If you wish to participate in this run, you need to agree to have your lunar coins reset. Type <color=#00cc00>\"tlc_aye\"</color> in the chat if you agree. Otherwise leave the game and keep your coins. The game will resume once everyone present agreed.</size>"
            });
            foreach (var p in SteamPlayers)
            {
                var state = p.pcmc.master.GetBody().gameObject.GetComponent<SetStateOnHurt>();
                p.pcmc.master.GetBody().AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
            

            while (!AllAgree && Run.instance)
            { 
                foreach (var p in SteamPlayers)
                {
                    var state = p.pcmc.master.GetBody().gameObject.GetComponent<SetStateOnHurt>();
                    if (state.targetStateMachine.state.GetType().Equals(typeof(EntityStates.GenericCharacterMain))) {
                        state.SetFrozen(10000f);
                    }
                }
                yield return new WaitForSeconds(1f);
            }


        }

        public IEnumerator DelayedAutomaticCoinRemovalLocal()
        {
            yield return new WaitForSeconds(4f);
            foreach (var n in NetworkUser.readOnlyLocalPlayersList)
            {
                n.localUser.userProfile.coins = 0;
                n.CallCmdSetNetLunarCoins(n.localUser.userProfile.coins);
            }
        }

        public IEnumerator DelayedAutomaticCoinRemoval()
        {
            yield return new WaitForSeconds(4f);
            RemoveLunarCoins();
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = " -- Temporary Lunar Coins --" +
            "\n<size=15px>Automatically removed lunar coins.</size>"
            });
        }

    }

    internal class SteamPlayer
    {
        public PlayerCharacterMasterController pcmc;
        public CSteamID steamID;
        public bool isReady;
        public NetworkConnection networkConnection;
        public string playerName;

        public SteamPlayer(PlayerCharacterMasterController player)
        {
            pcmc = player;
            steamID = player.networkUser.id.steamId;
            isReady = false;
            networkConnection = player.master.GetComponent<NetworkIdentity>().clientAuthorityOwner;
            playerName = player.networkUser.userName;
        }
    }
}
