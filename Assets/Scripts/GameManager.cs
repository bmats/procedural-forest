using UnityEngine;
using System.Collections.Generic;
using String = System.String;

public class GameManager : MonoBehaviour {
    const float AdjacentDistance = 150f;

    public static GameManager Instance { get; private set; }

    public Transform Controller = null;

    [HideInInspector]
    public ForestBuilder Builder;

    private HashSet<Vector3> _tiles = new HashSet<Vector3>();

    void Start() {
        Instance = this;

        Builder = GetComponent<ForestBuilder>();
        BuildAdjacentTiles();

        // Reposition controller above ground
        Vector3 startPos = Controller.transform.position;
        startPos.y = Builder.GetGroundHeight(0, 0) + 2f;
        Controller.transform.position = startPos;
    }

    void Update() {
        BuildAdjacentTiles();
    }

    /// Ensure that all the tiles within AdjacentDistance from the player exist.
    private void BuildAdjacentTiles() {
        for (float x = -AdjacentDistance; x < AdjacentDistance; x += Builder.TileSize) {
            for (float z = -AdjacentDistance; z < AdjacentDistance; z += Builder.TileSize) {
                Vector3 tilePos = new Vector3(
                    (int) ((Controller.position.x + x) / Builder.TileSize) * Builder.TileSize, // round down to closest tile origin
                    0,
                    (int) ((Controller.position.z + z) / Builder.TileSize) * Builder.TileSize);

                if (!_tiles.Contains(tilePos)) {
                    Builder.Build(tilePos);
                    _tiles.Add(tilePos);
                    Debug.Log(String.Format("Built tile {0} {1}", tilePos.x, tilePos.z));
                }
            }
        }
    }
}
