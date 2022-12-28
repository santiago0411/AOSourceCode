using System.Collections.Generic;
using UnityEngine;

namespace AOClient.Core
{
    public class Door : MonoBehaviour
    {
        public static readonly Dictionary<Vector2, Door> Doors = new();

        private void Start()
        {
            Doors.Add(transform.parent.position, this);
            
            Transform root = transform.Find("Root");
            if (root != null)
                Destroy(root.gameObject);

            Transform child = transform.parent.Find("Extra");
            if (child != null)
                Destroy(child.gameObject);
        }

        private void OnDestroy()
        {
            Doors.Clear();
        }
    }
}
