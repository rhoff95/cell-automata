using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public Vector2Int gridSize = Vector2Int.zero;

    public Vector2 startPosition = Vector2.zero;
    public Vector2 scale = Vector2.zero;

    private enum T
    {
        Empty,
        H20,
        Wall
    }

    private readonly T[][] _tiles =
    {
        new[]
        {
            T.Wall, T.Wall, T.Wall, T.Wall, T.Wall, T.Wall, T.Wall, T.Wall, T.Wall, T.Wall, T.Wall,
        },
        new[]
        {
            T.Wall, T.Empty, T.Empty, T.Empty, T.Wall, T.Wall, T.Wall, T.Empty, T.Empty, T.Empty, T.Wall
        },
        new[]
        {
            T.Wall, T.H20, T.Wall, T.Empty, T.Wall, T.Wall, T.Empty, T.Empty, T.Empty, T.Empty, T.Wall
        },
        new[]
        {
            T.Wall, T.H20, T.Wall, T.Empty, T.Wall, T.Empty, T.Empty, T.Empty, T.Empty, T.Empty, T.Wall
        },
        new[]
        {
            T.Wall, T.H20, T.Wall, T.Empty, T.Empty, T.Empty, T.Wall, T.Wall, T.Empty, T.Empty, T.Wall
        },
        new[]
        {
            T.Wall, T.H20, T.Wall, T.Empty, T.Empty, T.Empty, T.H20, T.Wall, T.Wall, T.Empty, T.Wall
        },
        new[]
        {
            T.Wall, T.H20, T.Wall, T.Empty, T.Empty, T.Empty, T.Empty, T.H20, T.Wall, T.Wall, T.Wall
        },
        new[]
        {
            T.Wall, T.Empty, T.Wall, T.Empty, T.Empty, T.Empty, T.Empty, T.Empty, T.H20, T.Wall, T.Wall
        },
    };

    public GameObject tilePrefab;
    public Color wallColor = Color.black;
    public Color waterColor = Color.cyan;
    public Color emptyColor = Color.clear;

    private List<List<SpriteRenderer>> _gridSpriteRenderers;
    private List<List<bool>> _gridWalls;
    private List<List<float>> _gridWater;

    private void Start()
    {
        _gridSpriteRenderers = new List<List<SpriteRenderer>>();
        _gridWalls = new List<List<bool>>();
        _gridWater = new List<List<float>>();

        for (var r = 0; r <= gridSize.x; r++)
        {
            _gridSpriteRenderers.Add(new List<SpriteRenderer>());
            _gridWalls.Add(new List<bool>());
            _gridWater.Add(new List<float>());

            for (var c = 0; c <= gridSize.y; c++)
            {
                var x = startPosition.x + c / (float)gridSize.x * scale.x;
                var y = startPosition.y + r / (float)gridSize.y * scale.y;

                var go = Instantiate(tilePrefab, new Vector3(x, y, 0f), Quaternion.identity, transform);
                go.transform.localScale = scale / gridSize * 2;
                var sr = go.GetComponent<SpriteRenderer>();

                var tile = (r < _tiles.Length && c < _tiles[r].Length)
                    ? _tiles[r][c]
                    : T.Empty;

                _gridSpriteRenderers[r].Add(sr);
                _gridWalls[r].Add(tile == T.Wall);
                _gridWater[r].Add(tile == T.H20 ? 1f : 0f);


                if (tile == T.Wall)
                {
                    sr.color = wallColor;
                }
                else if (tile == T.H20)
                {
                    sr.color = waterColor;
                }
            }
        }
    }

    private void Update()
    {
        for (var r = 0; r < _gridWater.Count; r++)
        {
            for (var c = 0; c < _gridWater[r].Count; c++)
            {
                var isWall = _gridWalls[r][c];

                if (isWall)
                {
                    continue;
                }

                var waterValue = _gridWater[r][c];
                var spriteRenderer = _gridSpriteRenderers[r][c];

                spriteRenderer.color =
                    Color.Lerp(emptyColor, waterColor, waterValue);
            }
        }
    }


    public void OnDrawGizmos()
    {
        var center = startPosition + (scale / 2);

        Gizmos.DrawWireCube(
            center,
            scale
        );
        Gizmos.color = Color.green;

        for (var r = 0; r <= gridSize.x; r++)
        {
            for (var c = 0; c <= gridSize.y; c++)
            {
                var x = startPosition.x + ((c / (float)gridSize.x) * scale.x);
                var y = startPosition.y + ((r / (float)gridSize.y) * scale.y);

                Gizmos.DrawWireSphere(new Vector3(x, y, 0f), 0.1f);
            }
        }
    }
}