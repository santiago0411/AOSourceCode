using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AO.Core;
using AO.Core.Utils;
using AO.World;
using Random = UnityEngine.Random;

namespace AO.Npcs.Utils
{
    public class NpcSpawner : MonoBehaviour
    {
        public enum RespawnType { TimedRespawn, TimeFrameRespawnOnce, TimeFrameConstantRespawn, NoRespawn }

        [SerializeField] private Spawner[] spawners;

        private Map map;
        private BoxCollider2D[] spawnAreas = Array.Empty<BoxCollider2D>();
        private readonly Dictionary<Npc, Spawner> spawnersSpawnedNpc = new();

        public IEnumerator Start()
        {
            while (!GameManager.GameMangerLoaded)
                yield return null;

            map = GetComponentInParent<Map>();
            spawnAreas = GetComponentsInChildren<BoxCollider2D>();

            foreach (var area in spawnAreas)
            {
                if (!Physics2D.OverlapBox(area.transform.position, area.bounds.size, 0f, CollisionManager.BackgroundLayerMask))
                {
                    spawnAreas = Array.Empty<BoxCollider2D>();
                    break;
                }
            }

            foreach (var spawner in spawners)
            {
                switch (spawner.RespawnType)
                {
                    case RespawnType.TimedRespawn:
                        for (int i = 0; i < spawner.Amount; i++)
                        {
                            SpawnNpc(spawner);
                            spawner.IncreaseCount();
                        }

                        spawner.LastSpawnTime = 0f;
                        break;
                    case RespawnType.TimeFrameRespawnOnce:
                        spawner.SetDateTimes();
                        break;
                    case RespawnType.TimeFrameConstantRespawn:
                        spawner.SetDateTimes();
                        spawner.LastSpawnTime = 0f;
                        break;
                    case RespawnType.NoRespawn:
                        for (int i = 0; i < spawner.Amount; i++)
                        {
                            SpawnNpc(spawner);
                            spawner.IncreaseCount();
                        }
                        break;
                }
            }

            InvokeRepeating(nameof(SpawnCoroutine), 1f, 1f);
        }

        private void SpawnCoroutine()
        {
            foreach (var spawner in spawners)
            {
                switch (spawner.RespawnType)
                {
                    case RespawnType.TimedRespawn:
                        TrySpawnTimed(spawner);
                        break;
                    case RespawnType.TimeFrameRespawnOnce:
                        TrySpawnTimeFrameOnce(spawner);
                        break;
                    case RespawnType.TimeFrameConstantRespawn:
                        TrySpawnTimeFrameConstant(spawner);
                        break;
                }
            }
        }

        private void TrySpawnTimed(Spawner spawner)
        {
            if (spawner.CurrentSpawnsCount < spawner.Amount)
            {
                if ((Time.realtimeSinceStartup - spawner.LastSpawnTime) >= spawner.NextSpawnTime)
                {
                    SpawnNpc(spawner);
                    spawner.ResetSpawnTimer();
                    spawner.IncreaseCount();
                }
            }
        }

        private void TrySpawnTimeFrameOnce(Spawner spawner)
        {
            if (spawner.Spawned) return;

            var dateNow = DateTime.Now;
            //If it's inside the time frame spawn it and set the spawned bool to true
            if (dateNow >= spawner.BeginRespawn && dateNow < spawner.EndRespawn)
            {
                SpawnNpc(spawner);
                spawner.Spawned = true;
                spawner.IncreaseCount();
            }
        }

        private void TrySpawnTimeFrameConstant(Spawner spawner)
        {
            var dateNow = DateTime.Now;
            //If it's inside the time frame try to spawn it
            if (dateNow >= spawner.BeginRespawn && dateNow < spawner.EndRespawn)
            {
                TrySpawnTimed(spawner);
            }
        }

        private Vector2 GetRandomSpawnPosition(NpcInfo npcInfo)
        {
            if (spawnAreas.Length > 0)
                return GetRandomSpawnPositionInAreas(npcInfo);

            Vector2 position;
            
            for (;;)
            {
                var bounds = map.Boundaries;
                var min = new Vector2Int(Mathf.CeilToInt(bounds.min.x), Mathf.CeilToInt(bounds.min.y));
                var max = new Vector2Int(Mathf.FloorToInt(bounds.max.x), Mathf.FloorToInt(bounds.max.y));

                int x = ExtensionMethods.RandomNumber(min.x, max.x);
                int y = ExtensionMethods.RandomNumber(min.y, max.y);
                position = new Vector2(x, y);

                if (!WorldMap.Tiles.TryGetValue(position, out Tile tile))
                    continue;

                if (tile.CanNpcBeInTile(npcInfo))
                    break;
            }

            return position;
        }

        private Vector2 GetRandomSpawnPositionInAreas(NpcInfo npcInfo)
        {
            BoxCollider2D area = spawnAreas[Random.Range(0, spawnAreas.Length)];
            Vector2 position;

            for (;;)
            {
                int x = ExtensionMethods.RandomNumber(Mathf.FloorToInt(area.bounds.min.x), Mathf.CeilToInt(area.bounds.max.x));
                int y = ExtensionMethods.RandomNumber(Mathf.FloorToInt(area.bounds.min.y), Mathf.CeilToInt(area.bounds.max.y));
                position = new Vector2(x, y);

                if (!WorldMap.Tiles.TryGetValue(position, out Tile tile))
                    continue;

                if (tile.CanNpcBeInTile(npcInfo))
                    break;
            }

            return position;
        }

        private void SpawnNpc(Spawner spawner)
        {
            var npcInfo = GameManager.Instance.GetNpcInfo((ushort)spawner.Npc);
            var npc = GameManager.Instance.SpawnNpc(npcInfo, map, GetRandomSpawnPosition(npcInfo));
            spawnersSpawnedNpc.Add(npc, spawner);
            npc.NpcDespawned += OnSpawnedNpcDespawned;
        }
        
        private void OnSpawnedNpcDespawned(Npc deadNpc)
        {
            spawnersSpawnedNpc.PopKey(deadNpc).DecreaseCount();
            deadNpc.NpcDespawned -= OnSpawnedNpcDespawned;
        }
    }
}
