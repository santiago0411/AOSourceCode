using UnityEngine;

namespace AOClient.Utilities
{
    public class ThreeDTests : MonoBehaviour
    {
        private Terrain terrain;

        private void Start()
        {
            terrain = FindObjectOfType<Terrain>();
            transform.eulerAngles = new Vector3(90f, transform.eulerAngles.y, transform.eulerAngles.z);
            Camera.main.transform.localEulerAngles = new Vector3(340f, 0f, 0f);
            Camera.main.transform.localPosition = new Vector3(0.0f, -2.0f, -7.0f);

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
                FindTrees(0.5f);

            if (Input.GetKeyDown(KeyCode.F9))
                FindTrees(155f);
        }

        private void FindTrees(float alpha)
        {
            TerrainData data = terrain.terrainData;

            for (int i = 0; i < data.treeInstances.Length; i++)
            {
                var tree = data.treeInstances[i];
                Color fadeOutColor = tree.color;
                fadeOutColor.a = alpha;
                tree.color = fadeOutColor;
            }
        }
    }
}
