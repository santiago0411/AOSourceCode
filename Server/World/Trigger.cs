using System.Collections;
using UnityEngine;
using AO.Core;
#if AO_DEBUG
using System.Collections.Generic;
using AO.Core.Logging;
#endif

namespace AO.World
{
    public class Trigger : MonoBehaviour
    {
        public TriggerType Type => type;
        public int ExploreAreaId => exploreAreaId;
        [SerializeField] private TriggerType type = TriggerType.SafeZone;
        [SerializeField] private int exploreAreaId;

        #if AO_DEBUG
        private static readonly HashSet<int> areaIds = new();
        private static readonly LoggerAdapter log = new(typeof(Trigger));
        #endif
        
        private IEnumerator Start()
        {
            #if AO_DEBUG
            if (type == TriggerType.QuestExploreArea)
            {
                if (areaIds.Contains(exploreAreaId))
                    log.Warn("There are more than one exploration areas containing the id {0}", exploreAreaId);

                areaIds.Add(exploreAreaId);
            }
            #endif
            
            if (type != TriggerType.InvalidNpcPosition)
                yield break;
            
            while (!GameManager.GameMangerLoaded)
                yield return new WaitForSeconds(1f);

            var collider = GetComponent<BoxCollider2D>();
            var bounds = new Bounds((Vector2) collider.transform.position + collider.offset, collider.size);
            
            var min = new Vector2Int(Mathf.CeilToInt(bounds.min.x), Mathf.CeilToInt(bounds.min.y));
            var max = new Vector2Int(Mathf.FloorToInt(bounds.max.x), Mathf.FloorToInt(bounds.max.y));

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    var position = new Vector2(x, y);

                    if (WorldMap.Tiles.TryGetValue(position, out Tile tile))
                        tile.InvalidNpcPosition = true;
                }
            }
        }
    }
}
