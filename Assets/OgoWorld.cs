using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class OgoWorld : MonoBehaviour
{
    #region Constants

    //The size of the tiles
    protected const float _tileWidth = 1.5f;                //Width of the WHOLE tile
    protected const float _tileHeight = 1.5f;               //Height of the WHOLE tile
    public float TileWidth { get { return _tileWidth; } }
    public float TileHeight { get { return _tileHeight; } }
    protected const int _tileVerticesTop = 8;

    //"Bumpiness" between tiles
    protected const float _bumpRange = 0.3f;

    //Ratio of inner-tile to outer-tile
    protected const float _tileRatio = 0.4f;

    //Heightmap constants
    private const int _blank = -1;

    //Invalid value
    private const int _invalid = int.MaxValue;

    #endregion

    /// <summary>
    /// The individual chunks
    /// </summary>
    protected List<OgoChunk> Chunks;

    /// <summary>
    /// The current state of Unity's Randomizer
    /// </summary>
    Random.State curState;

    /// <summary>
    /// World Generation Function
    /// </summary>
    public abstract void Generate(int seed);

    /// <summary>
    /// Initialization for all Worlds
    /// </summary>
    private void Awake()
    {
        //Setup the Chunk List
        Chunks = new List<OgoChunk>();
    }

    /// <summary>
    /// Renders the map
    /// </summary>
    protected virtual void Render()
    {
        StartCoroutine(_Render());
    }
    IEnumerator _Render()
    {
        //Iterate through all chunks
        for(int k = 0; k < Chunks.Count; k++)
        {
            //Create a gameobject for the chunk's mesh
            GameObject chunkObj = new GameObject();
            chunkObj.name = k.ToString();

            //Parent it to this world
            chunkObj.transform.parent = transform;

            //Give it a MeshFilter and MeshRenderer
            MeshFilter filter = chunkObj.AddComponent<MeshFilter>();
            Mesh mesh = filter.mesh;
            MeshRenderer r = chunkObj.AddComponent<MeshRenderer>();
            r.material = Instantiate(Resources.Load<Material>("aaa"));

            //Cache the Chunk
            OgoChunk chunk = Chunks[k];

            //Move to it's anchor
            Vector3 anchor = chunk.Anchor;
            chunkObj.transform.localPosition = anchor;

            //Setup the vertices/triangles
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            yield return null;

            //Create each tile
            int[][] heightMap = chunk.HeightMap;
            int chunkHeight = chunk.Height;
            int chunkWidth = chunk.Width;
            for (int y = 0; y < chunkHeight; y++)
            {
                for(int x = 0; x < chunkWidth; x++)
                {
                    //Determine if there is a tile here
                    if(heightMap[y][x] != _blank)
                    {
                        #region Create the Vertices

                        float innerWDist = (_tileWidth / 2) * _tileRatio;
                        float innerHDist = (_tileHeight / 2) * _tileRatio;

                        //Upper Left
                        vertices.Add(new Vector3(
                            (_tileWidth * x),
                            0,
                            (_tileHeight * y)
                            ));

                        //Bottom Left
                        vertices.Add(new Vector3(
                            (_tileWidth * x),
                            0,
                            (_tileHeight * (y + 1))
                            ));

                        //Upper Right
                        vertices.Add(new Vector3(
                            (_tileWidth * (x+1)),
                            0,
                            (_tileHeight * y)
                            ));

                        //Bottom Right
                        vertices.Add(new Vector3(
                            (_tileWidth * (x + 1)),
                            0,
                            (_tileHeight * (y+1))
                            ));

                        //IBR Bump Amount
                        float iba = 0;//Random.Range(-_bumpRange / 2, 0);

                        //Inner UL
                        vertices.Add(new Vector3(
                            (_tileWidth * x) + innerWDist,
                            0 + iba,
                            (_tileHeight * y) + innerHDist
                            ));

                        //Inner UR
                        vertices.Add(new Vector3(
                            (_tileWidth * (x+1)) - innerWDist,
                            0,
                            (_tileHeight * y) + innerHDist
                            ));


                        //Inner BR
                        vertices.Add(new Vector3(
                            (_tileWidth * (x+1)) - innerWDist,
                            0 + iba,
                            (_tileHeight * (y+1)) - innerHDist
                            ));

                        //Inner BL
                        vertices.Add(new Vector3(
                            (_tileWidth * x) + innerWDist,
                            0,
                            (_tileHeight * (y+1)) - innerHDist
                            ));

                        #endregion

                        #region Create Triangles

                        int bottomLeft_i = vertices.Count - 1;
                        int bottomRight_i = bottomLeft_i - 1;
                        int upperRight_i = bottomRight_i - 1;
                        int upperLeft_i = upperRight_i - 1;

                        int bottomRight = upperLeft_i - 1;
                        int upperRight = bottomRight - 1;
                        int bottomLeft = upperRight - 1;
                        int upperLeft = bottomLeft - 1;

                        //Side A ("Left")
                        triangles.Add(upperLeft);
                        triangles.Add(bottomLeft);
                        triangles.Add(bottomLeft_i);
                        triangles.Add(upperLeft);
                        triangles.Add(bottomLeft_i);
                        triangles.Add(upperLeft_i);

                        //Side B ("Right")
                        triangles.Add(upperRight);
                        triangles.Add(upperRight_i);
                        triangles.Add(bottomRight);
                        triangles.Add(bottomRight_i);
                        triangles.Add(bottomRight);
                        triangles.Add(upperRight_i);

                        //Side C ("Top")
                        triangles.Add(upperLeft_i);
                        triangles.Add(upperRight);
                        triangles.Add(upperLeft);
                        triangles.Add(upperRight_i);
                        triangles.Add(upperRight);
                        triangles.Add(upperLeft_i);

                        //Side D ("Bot")
                        triangles.Add(bottomRight_i);
                        triangles.Add(bottomLeft_i);
                        triangles.Add(bottomLeft);
                        triangles.Add(bottomRight_i);
                        triangles.Add(bottomLeft);
                        triangles.Add(bottomRight);

                        //Randomize Inner Square Angle
                        if (Random.Range(0, 2) == 0)
                        {
                            //Inner Square - "A"
                            triangles.Add(bottomRight_i);
                            triangles.Add(upperRight_i);
                            triangles.Add(upperLeft_i);
                            triangles.Add(bottomLeft_i);
                            triangles.Add(bottomRight_i);
                            triangles.Add(upperLeft_i);
                        }
                        else
                        {
                            //Inner Square - "B"
                            triangles.Add(bottomLeft_i);
                            triangles.Add(upperRight_i);
                            triangles.Add(upperLeft_i);
                            triangles.Add(bottomLeft_i);
                            triangles.Add(bottomRight_i);
                            triangles.Add(upperRight_i);
                        }

                        /*
                        triangles.Add(bottomLeft_i);
                        triangles.Add(bottomRight_i);
                        triangles.Add(upperLeft_i);*/
                        /*
                        triangles.Add(bottomRight_i);
                        triangles.Add(upperRight_i);
                        triangles.Add(upperLeft_i);*/

                        #endregion
                    }
                }
            }

            #region Vertex Compression
            
            Debug.Log("Before Combine VCount: " + vertices.Count);

            //Group Vertices by their position
            var sets = vertices.Select((p, i) => new { Index = i, Pos = p })
                .GroupBy(v => v.Pos).ToList();

            //Remove duplicates
            vertices = vertices.Distinct().ToList<Vector3>();

            yield return null;

            //Make the VertexMap (allows you to find a position's index in the list quickly-ish)
            Dictionary<Vector3, int> VertexMap = vertices.Select((p, i) => new { Index = i, Pos = p })
                .ToDictionary(i => i.Pos, j => j.Index);

            //The final Map we'll use for updating the triangles
            Dictionary<int, int> FinalMap = new Dictionary<int, int>();

            //Iterate through the vertices to create the map
            foreach(var set in sets)
            {
                //Grab the new position
                int newIndex = VertexMap[set.Key];

                //Add to the final map
                foreach(var old in set)
                {
                    FinalMap.Add(old.Index, newIndex);
                }
            }
            yield return null;

            //Finally, remap everything
            for(int t = 0; t < triangles.Count; t++)
            {
                triangles[t] = FinalMap[triangles[t]];
            }

            yield return null;
            Debug.Log("After Combine VCount: " + vertices.Count);

            #endregion

            #region Apply "Low Poly Bumps"

            for(int v = 0; v < vertices.Count; v++)
            {
                //Grab Vector
                Vector3 pos = vertices[v];

                //Is this an "outer vertex"?
                if((pos.x % _tileWidth) == 0)
                {
                    //Apply bumps
                    vertices[v] = new Vector3(pos.x, pos.y + Random.Range(-_bumpRange, _bumpRange/2), pos.z);
                }
                else
                {
                    //Apply bumps
                    //vertices[v] = new Vector3(pos.x, pos.y + Random.Range(-_bumpRange, 0), pos.z);
                }
            }
            yield return null;

            #endregion

            //Render the chunk
            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            UnityEditor.MeshUtility.Optimize(mesh);
            //mesh.RecalculateNormals();
        }
    }

    /// <summary>
    /// Determines if there is a neighbor next to them
    /// </summary>
    /// <param name="heightMap">Heightmap</param>
    /// <param name="x">Tile X</param>
    /// <param name="y">Tile Y</param>
    /// <param name="r">Neighbor's X offset</param>
    /// <param name="c">Neighbor's Y offset</param>
    /// <param name="h">Height of Heightmap</param>
    /// <param name="w">Width of Heightmap</param>
    /// <returns>
    /// Returns the height difference between a neighbor and a tile
    /// If there is no neighbor on the next tile, it will return the maximum value
    /// for an integer, because idk
    /// </returns>
    private int HasNeighbor(int[][] heightMap, int x, int y, int r, int c, int w, int h)
    {
        //Get neighbor's position
        int xx = x + r;
        int yy = y + c;

        //Is this position valid?
        if((xx > 0) && (yy > 0) && (yy < h) && (xx < w))
        {
            //Is there a neighbor?
            if(heightMap[yy][xx] == _blank)
            { return _invalid; }
            else
            { return (heightMap[yy][xx] - heightMap[y][x]); }
        }
        else
        {
            return _invalid;
        }
    }

    #region Random Functions

    /// <summary>
    /// Starts the Random with a unique state
    /// </summary>
    protected void StartRandom(int seed)
    {
        //Cache the original state
        curState = Random.state;

        //Seed the generator
        Random.InitState(seed);
    }

    /// <summary>
    /// Ends the Random with it's original state
    /// </summary>
    protected void EndRandom()
    {
        //Restore the generator
        Random.state = curState;
    }

    #endregion
}