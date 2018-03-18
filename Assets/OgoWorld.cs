using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class OgoWorld : MonoBehaviour
{
    #region Constants

    //The size of the tiles
    protected const int _tileWidth = 3;   //Width of the WHOLE tile
    protected const int _tileHeight = 3;  //Height of the WHOLE tile
    public int TileWidth { get { return _tileWidth; } }
    public int TileHeight { get { return _tileHeight; } }
    protected const int _tileVerticesTop = 8;

    //Ratio of inner-tile to outer-tile
    protected const float _tileRatio = 0.5f;

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
    protected void Render()
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

            //Iterate through the chunk's heightmap
            Dictionary<Vector2, int[]> VMap = new Dictionary<Vector2, int[]>();  //Stores a map of what vertices go to which tile
            int[][] heightMap = chunk.HeightMap;
            int chunkWidth = chunk.Width;
            int chunkHeight = chunk.Height;
            for(int y = 0; y < chunkHeight; y++)
            {
                for(int x = 0; x < chunkWidth; x++)
                {
                    //Is this a tile?
                    if(heightMap[y][x] != _blank)
                    {
                        //Store it's vertices
                        int[] myVertices = new int[_tileVerticesTop];
                        for(int l = 0; l < myVertices.Length; l++)
                        { myVertices[l] = _blank; }

                        #region Create Vertex Data

                        ///This is where the UN begins

                        ///Neighbor Check

                        //Do I have a top neighbor?
                        if (HasNeighbor(heightMap, x, y, 0, -1, chunkWidth, chunkHeight) != _invalid)
                        {
                            //Grab my top neighbor's bottom vertices
                            int[] topVerts = VMap[new Vector2(x, y - 1)];
                            myVertices[1] = topVerts[0];
                            myVertices[4] = topVerts[7];
                        }
                        //Do I have a left neighbor?
                        if (HasNeighbor(heightMap, x, y, -1, 0, chunkWidth, chunkHeight) != _invalid)
                        {
                            //Grab my top neighbor's bottom vertices
                            int[] leftVerts = VMap[new Vector2(x - 1, y)];
                            myVertices[1] = leftVerts[4];
                            myVertices[0] = leftVerts[7];
                        }
                        /*
                        //Do I have a right neighbor?
                        if (HasNeighbor(heightMap, x, y, 1, 0, chunkWidth, chunkHeight) != _invalid)
                        {
                            //Grab my top neighbor's bottom vertices
                            int[] rightVerts = VMap[new Vector2(x + 1, y)];
                            myVertices[7] = rightVerts[0];
                            myVertices[4] = rightVerts[1];
                        }
                        //Do I have a bottom neighbor?
                        if (HasNeighbor(heightMap, x, y, 0, 1, chunkWidth, chunkHeight) != _invalid)
                        {
                            //Grab my top neighbor's bottom vertices
                            int[] botVerts = VMap[new Vector2(x, y + 1)];
                            myVertices[0] = botVerts[1];
                            myVertices[7] = botVerts[4];
                        }
                        */

                        ///Now for whatever vertices I DON'T have, create
                        
                        //Outer BL
                        if (myVertices[0] == _blank)
                        {
                            myVertices[0] = vertices.Count;
                            vertices.Add(
                                new Vector3(
                                    (_tileWidth * x),
                                    0,
                                    (_tileHeight * (y+1))
                                    ));
                        }

                        //Outer UL
                        if (myVertices[1] == _blank)
                        {
                            myVertices[1] = vertices.Count;
                            vertices.Add(
                                new Vector3(
                                    (_tileWidth * x),
                                    0,
                                    (_tileHeight * y)
                                    ));
                        }

                        //Inner BL
                        if (myVertices[2] == _blank)
                        {
                            myVertices[2] = vertices.Count;
                            vertices.Add(
                                new Vector3(
                                    (_tileWidth * x) + ((_tileWidth / 2) * _tileRatio),
                                    0,
                                    (_tileHeight * (y+1)) - ((_tileHeight / 2) * _tileRatio)
                                    ));
                        }

                        //Inner UL
                        if (myVertices[3] == _blank)
                        {
                            myVertices[3] = vertices.Count;
                            vertices.Add(
                                new Vector3(
                                    (_tileWidth * x) + ((_tileWidth/2) * _tileRatio),
                                    0,
                                    (_tileHeight * y) + ((_tileHeight/2) * _tileRatio)
                                    ));
                        }

                        //Outer UR
                        if (myVertices[4] == _blank)
                        {
                            myVertices[4] = vertices.Count;
                            vertices.Add(
                                new Vector3(
                                    (_tileWidth * (x + 1)),
                                    0,
                                    (_tileHeight * y)
                                    ));
                        }

                        //Inner UR
                        if (myVertices[5] == _blank)
                        {
                            myVertices[5] = vertices.Count;
                            vertices.Add(
                                new Vector3(
                                    (_tileWidth * (x+1)) - ((_tileWidth / 2) * _tileRatio),
                                    0,
                                    (_tileHeight * y) + ((_tileHeight / 2) * _tileRatio)
                                    ));
                        }

                        //Inner BR
                        if (myVertices[6] == _blank)
                        {
                            myVertices[6] = vertices.Count;
                            vertices.Add(
                                new Vector3(
                                    (_tileWidth * (x+1)) - ((_tileWidth / 2) * _tileRatio),
                                    0,
                                    (_tileHeight * (y+1)) - ((_tileHeight / 2) * _tileRatio)
                                    ));
                        }

                        //Outer BR
                        if (myVertices[7] == _blank)
                        {
                            myVertices[7] = vertices.Count;
                            vertices.Add(
                                new Vector3(
                                    (_tileWidth * (x+1)),
                                    0,
                                    (_tileHeight * (y+1))
                                    ));
                        }

                        ///Create Triangles
                        triangles.AddRange(new int[] { myVertices[0], myVertices[1], myVertices[2] }.OrderBy(item => -item));
                        triangles.AddRange(new int[] { myVertices[1], myVertices[2], myVertices[3] }.OrderBy(item => item));
                        triangles.AddRange(new int[] { myVertices[1], myVertices[3], myVertices[4] }.OrderBy(item => item));
                        triangles.AddRange(new int[] { myVertices[3], myVertices[4], myVertices[5] }.OrderBy(item => item));
                        triangles.AddRange(new int[] { myVertices[4], myVertices[5], myVertices[7] }.OrderBy(item => item));
                        triangles.AddRange(new int[] { myVertices[5], myVertices[6], myVertices[7] }.OrderBy(item => item));
                        triangles.AddRange(new int[] { myVertices[0], myVertices[6], myVertices[7] }.OrderBy(item => item));
                        triangles.AddRange(new int[] { myVertices[0], myVertices[2], myVertices[6] }.OrderBy(item => item));

                        //Add to dictionary
                        VMap.Add(new Vector2(x, y), myVertices);

                        #endregion
                    }
                }
            }

            //Render the chunk
            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            UnityEditor.MeshUtility.Optimize(mesh);
            mesh.RecalculateNormals();
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

/// When determining how a tile should look, here is the map of vertices:
/// Top of Tile:
/// 1 --- 4
/// \  /
///   3-5\ 
///  \2-6
///    /  \
/// 0 --- 7
/// 
/// So the 0th vertex in the dictionary corresponds to the bottom left corner, and so on, so forth