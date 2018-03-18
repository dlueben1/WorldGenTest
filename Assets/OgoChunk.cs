using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OgoChunk
{
    #region Settings

    //The size of the chunk
    private int width;
    private int height;
    public int Width { get { return width; } }
    public int Height { get { return height; } }

    #endregion

    #region Data

    //The heightmap for this chunk, as an integer array
    protected int[][] heightMap;
    public int[][] HeightMap
    {
        get { return heightMap; }
    }

    //The anchor for this position
    public Vector3 Anchor;

    #endregion

    /// <summary>
    /// Constructor for OgoChunk
    /// </summary>
    /// <param name="_width">Width of the Chunk</param>
    /// <param name="_height">Height of the Chunk</param>
    public OgoChunk(Vector3 _anchor, int _width, int _height)
    {
        //Set width and height and anchor
        width = _width;
        Anchor = _anchor;
        height = _height;

        //Setup Heightmap
        heightMap = new int[height][];
        for(int i = 0; i < height; i++)
        {
            heightMap[i] = new int[width];
        }
    }
}
