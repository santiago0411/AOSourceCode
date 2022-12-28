using System.Collections;
using TMPro;
using UnityEngine;
using AOClient.Core;
using AOClient.Core.Utils;

namespace AOClient.Npcs
{
    public class Npc : MonoBehaviour, IPoolObject
    {
        public int InstanceId { get; private set; }
        public NpcInfo Info { get; private set; }
        public bool IsBeingUsed { get; private set; }
        
        private string description = string.Empty;

        private Map currentMap;
        private TextMeshProUGUI chatBubble;
        private SpriteRenderer headRenderer;
        private SpriteRenderer bodyRenderer;
        private Animator animator;
        private Heading heading = Heading.South;

        public void Initialize(int instanceId, NpcInfo info, short mapNumber, Vector3 pos)
        {
            InstanceId = instanceId;
            Info = info;
            description = info.Description;
            currentMap = GameManager.Instance.GetWorldMap(mapNumber);
            if (!string.IsNullOrEmpty(description)) 
                chatBubble = GetComponentInChildren<TextMeshProUGUI>();
            
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
                headRenderer = spriteRenderers[0];
                bodyRenderer = spriteRenderers[1];
            }

            if (info.HeadId != 0)
            {
                headRenderer.enabled = true;
                headRenderer.sprite = GameManager.Instance.GetHeadSprites(info.HeadId)[0];
                headRenderer.gameObject.transform.localPosition =
                    info.IsShort ? Constants.ShortHeadPosition : Constants.TallHeadPosition;
            }
            else
            {
                headRenderer.enabled = false;
            }

            if (info.IsStatic)
            {
                animator.enabled = false;         
                bodyRenderer.sprite = GameManager.Instance.GetNpcBody(info.Animation);
            }
            else
            {
                animator.enabled = true;
                animator.runtimeAnimatorController = GameManager.Instance.GetNpcAnimation(info.Animation);
            }

            transform.parent = currentMap.transform;
            transform.position = pos;
            headRenderer.sortingOrder = (int)(currentMap.Boundaries.max.y * 2 - transform.position.y * 2);
            bodyRenderer.sortingOrder = (int)(currentMap.Boundaries.max.y * 2 - transform.position.y * 2);

            if (Info.TurnInableQuests.Count > 0)
            {
                // TODO set quest '?' icon, check if the quest is already completed to change colors
            }
            else if (Info.AvailableQuests.Count > 0)
            {
                // TODO set quest '!' icon   
            }

            IsBeingUsed = true;
            gameObject.SetActive(true);
        }

        public void ResetPoolObject()
        {
            if (!Info.IsStatic)
            {
                IsBeingUsed = false;
                gameObject.SetActive(false);
            }
        }

        public void CheckFacing(Vector2 difference)
        {
            if (difference.normalized == Vector2.up)
                heading = Heading.North;
            else if (difference.normalized == Vector2.right)
                heading = Heading.East;
            else if (difference.normalized == Vector2.left)
                heading = Heading.West;
            else
                heading = Heading.South;
        }

        /// <summary>Plays the walking animation according to the facing direction.</summary>
        public void PlayAnimationOnce(bool isMoving)
        {
            animator.SetInteger(Constants.FacingHash, (int)heading);
            animator.SetBool(Constants.IsMovingHash, isMoving);

            if (isMoving)
            {
                var position = transform.position;
                headRenderer.sortingOrder = (int)(currentMap.Boundaries.max.y * 2 - position.y * 2);
                bodyRenderer.sortingOrder = (int)(currentMap.Boundaries.max.y * 2 - position.y * 2);

                switch (heading)
                {
                    case Heading.North:
                        animator.Play(Constants.BodyUpHash);
                        break;
                    case Heading.South:
                        animator.Play(Constants.BodyDownHash);
                        break;
                    case Heading.West:
                        animator.Play(Constants.BodyLeftHash);
                        break;
                    case Heading.East:
                        animator.Play(Constants.BodyRightHash);
                        break;
                }
            }
        }

        public void ShowDescription()
        {
            if (chatBubble != null)
            {
                chatBubble.text = description;
                StopAllCoroutines();
                StartCoroutine(ClearChatBubble());
            }
        }

        private IEnumerator ClearChatBubble()
        {
            yield return new WaitForSeconds(5);
            chatBubble.text = string.Empty;
        }
        
        #if AO_DEBUG
        [SerializeField] private bool drawPath = true;
        private Vector2[] path;

        public void UpdatePath(Network.Packet packet)
        {
            path = new Vector2[packet.ReadByte()];
            for (var i = 0; i < path.Length; i++)
                path[i] = packet.ReadVector2();
        }

        private void OnDrawGizmos()
        {
            if (path == null || !drawPath)
                return;
            
            Gizmos.color = Color.black;
            for (var i = 0; i < path.Length; i++)
            {
                if (i != path.Length - 1)
                    Gizmos.DrawLine(path[i], path[i+1]);
                
                Gizmos.DrawSphere(path[i], 0.1f);
            }
            
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere((Vector2)transform.position, 0.2f);
        }
        #endif
    }
}
