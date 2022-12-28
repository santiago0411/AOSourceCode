using System.Collections;
using UnityEngine;

namespace AOClient.Extras
{
    public sealed class PileOfBoots : MonoBehaviour
    {
        [Min(0.1f)]
        [SerializeField] 
        private float speed = 0.5f;

        private int activeBootsCount;
        private float lastActiveTime;
        
        private void Start()
        {
            DisableAll();
            StartCoroutine(ActivateAll());
        }

        private IEnumerator ActivateAll()
        {
            for (;;)
            {
                ActivateNext();
                yield return new WaitForSeconds(speed);
                if (activeBootsCount >= transform.childCount)
                    DisableAll();
            }
        }

        private void DisableAll()
        {
            foreach (Transform child in transform)
                child.gameObject.SetActive(false);

            activeBootsCount = 0;
        }

        private void ActivateNext()
        {
            transform.GetChild(activeBootsCount++).gameObject.SetActive(true);
        }
    }
}