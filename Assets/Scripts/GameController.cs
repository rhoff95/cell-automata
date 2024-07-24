using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Vector2Int gridSize = Vector2Int.zero;

    public Vector2 startPosition = Vector2.zero;
    public Vector2 scale = Vector2.zero;

    [Serializable] public class BoolList
    {
        public bool[] values;
    }

    public BoolList[] walls;
    public BoolList[] water;

    public GameObject wallPrefab;
    public GameObject waterPrefab;

    void Start()
    {
        for (int r = 0; r < gridSize.x; r++)
        {
            for (int c = 0; c < gridSize.y; c++)
            {
                var x = startPosition.x + ((c / (float)gridSize.x) * scale.x);
                var y = startPosition.y + ((r / (float)gridSize.y) * scale.y);

                Debug.Log($"Checking [{r}][{c}] is within [][]");

                if (r > walls.Length - 1 && c > walls[r].values.Length - 1)
                {
                    var isWall = walls[r].values[c];

                    if (isWall)
                    {
                        Instantiate(wallPrefab, new Vector3(x, y, 0f), Quaternion.identity, transform);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnDrawGizmos()
    {
        var center = startPosition + (scale / 2);


        Gizmos.DrawWireCube(
            center,
            scale
        );
        Gizmos.color = Color.green;

        for (int r = 0; r <= gridSize.x; r++)
        {
            for (int c = 0; c <= gridSize.y; c++)
            {
                var x = startPosition.x + ((c / (float)gridSize.x) * scale.x);
                var y = startPosition.y + ((r / (float)gridSize.y) * scale.y);

                Gizmos.DrawWireSphere(new Vector3(x, y, 0f), 0.1f);
            }
        }
    }
}
