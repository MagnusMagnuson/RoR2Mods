using ReinDirectorCardLib;
using RoR2;
using RoR2.Navigation;
using System.Collections.Generic;
using UnityEngine;
using static ReinDirectorCardLib.AddedMonsterCard;

namespace BossRush
{
    class RandomBoss
    {
        internal static void AddAllBossesToDirector()
        {
            List<DirectorCard> bossMonsterDirectorCards = new List<DirectorCard>();
            bossMonsterDirectorCards.Add(CreateDirectorCard("TitanMaster", 600, MapNodeGroup.GraphType.Ground));
            bossMonsterDirectorCards.Add(CreateDirectorCard("BeetleQueenMaster", 600, MapNodeGroup.GraphType.Ground));
            bossMonsterDirectorCards.Add(CreateDirectorCard("VagrantMaster", 600, MapNodeGroup.GraphType.Air));
            bossMonsterDirectorCards.Add(CreateDirectorCard("ClayBossMaster", 600, MapNodeGroup.GraphType.Ground));
            bossMonsterDirectorCards.Add(CreateDirectorCard("MagmaWormMaster", 800, MapNodeGroup.GraphType.Ground));
            bossMonsterDirectorCards.Add(CreateDirectorCard("ImpBossMaster", 800, MapNodeGroup.GraphType.Ground));
            bossMonsterDirectorCards.Add(CreateDirectorCard("GravekeeperMaster", 800, MapNodeGroup.GraphType.Ground));
            bossMonsterDirectorCards.Add(CreateDirectorCard("ElectricWormMaster", 4000, MapNodeGroup.GraphType.Ground));

            foreach (var card in bossMonsterDirectorCards) AddMonsterDirectorCardToAllStages(card);
        }

        internal static DirectorCard CreateDirectorCard(string bossName, int cost, MapNodeGroup.GraphType nodeGraphType)
        {
            CharacterSpawnCard spawnCard = ScriptableObject.CreateInstance<CharacterSpawnCard>();
            spawnCard.noElites = false;
            spawnCard.prefab = Resources.Load<GameObject>("prefabs/charactermasters/" + bossName);
            spawnCard.forbiddenFlags = NodeFlags.NoCharacterSpawn;
            spawnCard.requiredFlags = NodeFlags.None;
            spawnCard.hullSize = HullClassification.Human;
            spawnCard.occupyPosition = false;
            spawnCard.sendOverNetwork = true;
            spawnCard.nodeGraphType = nodeGraphType;
            spawnCard.name = bossName;

            DirectorCard directorCard = new DirectorCard();
            directorCard.spawnCard = spawnCard;
            directorCard.cost = cost;
            directorCard.selectionWeight = 1;
            directorCard.allowAmbushSpawn = true;
            directorCard.forbiddenUnlockable = "";
            directorCard.minimumStageCompletions = 0;
            directorCard.preventOverhead = true;
            directorCard.spawnDistance = DirectorCore.MonsterSpawnDistance.Standard;

            return directorCard;
        }

        internal static void AddMonsterDirectorCardToAllStages(DirectorCard directorCard)
        {
            AddedMonsterCard card = new AddedMonsterCard(MonsterCategory.Champion, SpawnStages.AllStages, directorCard);
            ReinDirectorCardLib.ReinDirectorCardLib.AddedMonsterCards.Add(card);
        }

    }
}
