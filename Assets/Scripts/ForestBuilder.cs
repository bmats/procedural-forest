using UnityEngine;
using String = System.String;

public class ForestBuilder : MonoBehaviour {
    public float TileSize = 50f;
    public float GroundVertSpacing = 2f;
    public float TrunkRadiusMult = 0.3f;
    public float LeafDimpleMult = 0.7f;
    public float RockClusterMaxRadius = 3f;

    public Material GroundMaterial = null;
    public Material TreeMaterial = null;
    public GameObject Rock = null;
    public GameObject Animal = null;

    private float _groundXFreq, _groundZFreq;
    private float _groundXOffset, _groundZOffset;

    void Awake() {
        // Randomize ground
        _groundXFreq = Random.Range(0.05f, 0.15f);
        _groundZFreq = Random.Range(0.05f, 0.15f);
        _groundXOffset = Random.value * 100f;
        _groundZOffset = Random.value * 100f;
    }

    public GameObject Build(Vector3 position) {
        GameObject tile = new GameObject(String.Format("Tile {0} {1}", position.x, position.z));
        tile.transform.position = position;

        // Generate ground
        GameObject ground = MakeGround(position);
        ground.transform.parent = tile.transform;
        ground.transform.localPosition = Vector3.zero;

        // Place trees
        int treeCount = Random.Range(8, 20);
        for (int i = 0; i < treeCount; ++i) {
            GameObject tree = GrowTree();
            tree.transform.parent = tile.transform;

            PlaceObjectOnGround(tree.transform,
                position.x + Random.Range(0, TileSize),
                position.z + Random.Range(0, TileSize));
        }

        // Make rock clusters
        int rockCount = Random.Range(5, 15);
        for (int i = 0; i < rockCount; ++i) {
            GameObject rocks = MakeRocks();
            rocks.transform.parent = tile.transform;

            PlaceObjectOnGround(rocks.transform,
                position.x + Random.Range(0, TileSize),
                position.z + Random.Range(0, TileSize),
                0.5f);
        }

        // Maybe stick an animal or two in there too
        int animalCount = Random.Range(0, 2);
        for (int i = 0; i < animalCount; ++i) {
            GameObject animal = (GameObject) Instantiate(Animal, Vector3.zero, Quaternion.identity);
            animal.transform.parent = tile.transform;

            PlaceObjectOnGround(animal.transform,
                position.x + Random.Range(0, TileSize),
                position.z + Random.Range(0, TileSize),
                2f);
        }

        return tile;
    }

    /// Build a square mesh using the y values generated in GetGroundHeight().
    private GameObject MakeGround(Vector3 position) {
        int rows = (int)(TileSize / GroundVertSpacing);
        Vector3[] verts = new Vector3[(rows + 1) * (rows + 1)];
        int[] tris = new int[(rows + 1) * (rows + 1) * 2 * 3];

        int vertIndex = 0;
        int triIndex = 0;
        for (int x = 0; x <= rows; ++x) {
            for (int z = 0; z <= rows; ++z) {
                verts[vertIndex] = new Vector3(
                    x * GroundVertSpacing,
                    GetGroundHeight(position.x + x * GroundVertSpacing, position.z + z * GroundVertSpacing),
                    z * GroundVertSpacing);

                if (x < rows && z < rows) {
                    tris[triIndex    ] = vertIndex;
                    tris[triIndex + 1] = vertIndex + (rows + 1) + 1;
                    tris[triIndex + 2] = vertIndex + (rows + 1);
                    tris[triIndex + 3] = vertIndex;
                    tris[triIndex + 4] = vertIndex + 1;
                    tris[triIndex + 5] = vertIndex + (rows + 1) + 1;
                    triIndex += 6;
                }

                ++vertIndex;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        GameObject ground = new GameObject("Ground");
        MeshFilter filter = ground.AddComponent<MeshFilter>();
        filter.mesh = mesh;

        MeshRenderer renderer = ground.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = GroundMaterial;

        ground.AddComponent<MeshCollider>();

        return ground;
    }

    /// Build a randomized tree mesh.
    private GameObject GrowTree() {
        // Randomize some tree parameters
        int levels = Random.Range(2, 4);
        int leaves = Random.Range(3, 12);
        float radius = Random.Range(0.8f, 3f);
        float levelHeight = Random.Range(1.5f, 2.5f);

        // Preallocate the mesh arrays
        Vector3[] verts = new Vector3[1 + (levels + 1) * leaves * 4 + levels * leaves * 4 + leaves * 2];
        int[] tris = new int[(levels * 2 + 1) * leaves * 6 + leaves * 3];
        Vector2[] uvs = new Vector2[verts.Length];
        int vertIndex = 0;
        int triIndex = 0;

        // Bottom vertex
        verts[0] = Vector3.zero;
        uvs[0] = new Vector2(0.75f, 0.25f); // trunk
        vertIndex = 1;

        // Build the leaves on each level. Treat the trunk as another level with a radius of the trunk radius.
        for (int level = 0; level < levels + 1; ++level) {
            for (int leaf = 0; leaf < leaves; ++leaf) {
                float bottomRadius = (level == 0) ? radius * TrunkRadiusMult : radius; // trunk radius if at bottom
                float topRadius = (level == levels) ? 0 : radius * TrunkRadiusMult; // close to a point if at top

                // Add the quad verts for this leaf
                AddTreeVerts(verts, vertIndex    , leaves, leaf, level       * levelHeight, bottomRadius);
                AddTreeVerts(verts, vertIndex + 2, leaves, leaf, (level + 1) * levelHeight, topRadius);

                tris[triIndex    ] = vertIndex;
                tris[triIndex + 1] = vertIndex + 3;
                tris[triIndex + 2] = vertIndex + 1;
                tris[triIndex + 3] = vertIndex;
                tris[triIndex + 4] = vertIndex + 2;
                tris[triIndex + 5] = vertIndex + 3;
                triIndex += 6;

                // Set UVs to leaf or trunk color
                uvs[vertIndex    ] =
                uvs[vertIndex + 1] =
                uvs[vertIndex + 2] =
                uvs[vertIndex + 3] = (level > 0) ? new Vector2(0.25f, 0.75f) : new Vector2(0.75f, 0.25f);
                vertIndex += 4;

                if (level > 0) {
                    // Join with previous level
                    AddTreeVerts(verts, vertIndex    , leaves, leaf, level * levelHeight, bottomRadius);
                    AddTreeVerts(verts, vertIndex + 2, leaves, leaf, level * levelHeight, radius * TrunkRadiusMult);

                    tris[triIndex    ] = vertIndex + 2;
                    tris[triIndex + 1] = vertIndex + 1;
                    tris[triIndex + 2] = vertIndex + 3;
                    tris[triIndex + 3] = vertIndex + 2;
                    tris[triIndex + 4] = vertIndex;
                    tris[triIndex + 5] = vertIndex + 1;
                    triIndex += 6;

                    uvs[vertIndex    ] =
                    uvs[vertIndex + 1] =
                    uvs[vertIndex + 2] =
                    uvs[vertIndex + 3] = new Vector2(0.25f, 0.75f); // leaf
                    vertIndex += 4;
                } else {
                    // Create tree bottom
                    AddTreeVerts(verts, vertIndex, leaves, leaf, level * levelHeight, bottomRadius);

                    tris[triIndex    ] = vertIndex;
                    tris[triIndex + 1] = vertIndex + 1;
                    tris[triIndex + 2] = 0;
                    triIndex += 3;

                    uvs[vertIndex    ] =
                    uvs[vertIndex + 1] = new Vector2(0.75f, 0.25f); // trunk
                    vertIndex += 2;
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GameObject tree = new GameObject("Tree");
        tree.tag = "Tree";

        tree.AddComponent<MeshFilter>().mesh = mesh;
        tree.AddComponent<MeshRenderer>().sharedMaterial = TreeMaterial;

        var collider = tree.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(0, (levels + 1) * levelHeight * 0.5f, 0);
        collider.height = (levels + 1) * levelHeight;
        collider.direction = 1; // y axis
        collider.radius = radius;

        return tree;
    }

    private void AddTreeVerts(Vector3[] verts, int vertIndex, int leaves, int leaf, float y, float radius) {
        bool useDimples = leaves % 2 == 0 && leaves > 6;
        float angle = Mathf.PI * 2 / leaves;

        verts[vertIndex    ] = new Vector3(
            Mathf.Cos(angle * leaf) * radius * ((useDimples && leaf % 2 == 0) ? LeafDimpleMult : 1),
            y,
            Mathf.Sin(angle * leaf) * radius * ((useDimples && leaf % 2 == 0) ? LeafDimpleMult : 1));
        verts[vertIndex + 1] = new Vector3(
            Mathf.Cos(angle * (leaf + 1)) * radius * ((useDimples && leaf % 2 == 1) ? LeafDimpleMult : 1),
            y,
            Mathf.Sin(angle * (leaf + 1)) * radius * ((useDimples && leaf % 2 == 1) ? LeafDimpleMult : 1));
    }

    /// Place 1-3 randomly scaled rocks close together.
    private GameObject MakeRocks() {
        GameObject rocks = new GameObject("Rocks");

        int rockCount = Random.Range(1, 3);
        for (int i = 0; i < rockCount; ++i) {
            GameObject rock = (GameObject) Instantiate(Rock, Vector3.zero, Quaternion.identity);
            rock.transform.parent = rocks.transform;

            rock.transform.position = new Vector3(
                Random.Range(-RockClusterMaxRadius, RockClusterMaxRadius),
                0,
                Random.Range(-RockClusterMaxRadius, RockClusterMaxRadius));

            rock.transform.rotation = Random.rotation;

            float scale = Random.Range(1f, 3f);
            rock.transform.localScale = new Vector3(scale, scale, scale);
        }

        return rocks;
    }

    /// Move the transform y to the ground height at the specific position.
    private void PlaceObjectOnGround(Transform transform, float x, float z, float above = 0) {
        Vector3 position = new Vector3(x, 0, z);
        position.y = GetGroundHeight(x, z) + above;
        transform.position = position;

        transform.rotation = Quaternion.FromToRotation(Vector3.up, GetGroundNormal(x, z));
    }

    /// Use noise to generate a ground with bumps and hills.
    public float GetGroundHeight(float x, float z) {
        return
            // Uneven ground
            Mathf.PerlinNoise(
                x * _groundXFreq + _groundXOffset,
                z * _groundZFreq + _groundZOffset) * 2 +
            // Big hills
            Mathf.PerlinNoise(
                x * 0.01f + _groundXOffset,
                z * 0.01f + _groundZOffset) * 15;
    }

    /// Use the ground slope at a point to get its normal.
    public Vector3 GetGroundNormal(float x, float z) {
        float xDiff = GetGroundHeight(x + 0.1f, z) - GetGroundHeight(x, z);
        float zDiff = GetGroundHeight(x, z + 0.1f) - GetGroundHeight(x, z);
        return Vector3.Cross(new Vector3(0, zDiff, 0.1f), new Vector3(0.1f, xDiff, 0));
    }
}
