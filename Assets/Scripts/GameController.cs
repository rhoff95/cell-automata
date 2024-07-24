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

    [Header("Water Properties")] [Range(0f, 2f)]
    public float maxWater = 1f;

    [Range(0f, 1f)] public float maxCompression = 0.02f;
    [Range(0f, 1f)] public float minWater = 0.0001f;

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

        // UpdateWater();
    }

    private void Update()
    {
        #region Update Water Sprites

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

        #endregion
    }

    public void UpdateWater()
    {
        var new_mass = new List<List<float>>();

        #region Initialize Next Grid

        for (var y = 0; y < _gridWater.Count; y++)
        {
            new_mass.Add(new List<float>());

            for (var x = 0; x < _gridWater[y].Count; x++)
            {
                new_mass[y].Add(_gridWater[y][x]);
            }
        }

        #endregion

        // Take the mass of the current cell and the cell below it and figure out how much water the bottom cell should
        // contain. If it has less than that, remove the corresponding amount from the current cell and add it to the
        // bottom cell.

        // Check the cell to the left of this one. If it has less water, move over enough water to make both cells
        // contain the same amount.

        // Do the same thing for the right neighbour.

        // Do the same thing as in step 1., but for the cell above the current one.

        for (var y = 1; y < _gridWater.Count; y++)
        {
            for (var x = 1; x < _gridWater[y].Count; x++)
            {
                var isWall = _gridWalls[y][x];

                if (isWall)
                {
                    continue;
                }

                var flow = 0f;
                var minFlow = 1f;
                var maxSpeed = 1f;
                var remaining_mass = _gridWater[y][x];

                if (remaining_mass <= 0f)
                {
                    continue;
                }

                if (!_gridWalls[y -1][x])
                {
                    flow = get_stable_state_b(remaining_mass + _gridWater[y - 1][x]) - _gridWater[y - 1][x];

                    if (flow > minFlow)
                    {
                        flow *= 0.5f;
                    }

                    flow = Mathf.Clamp(flow, 0, Mathf.Min(maxSpeed, remaining_mass));

                    new_mass[y][x] -= flow;
                    new_mass[y - 1][x] += flow;

                    remaining_mass -= flow;
                }


                if (remaining_mass <= 0f)
                {
                    continue;
                }
            }
        }
        
        _gridWater = new_mass;
    }


    //Returns the amount of water that should be in the bottom cell.
    private float get_stable_state_b(float totalMass)
    {
        if (totalMass <= 1)
        {
            return 1;
        }
        else if (totalMass < 2 * maxWater + maxCompression)
        {
            return (maxWater * maxWater + totalMass * maxCompression) / (maxWater + maxCompression);
        }
        else
        {
            return (totalMass + maxCompression) / 2;
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