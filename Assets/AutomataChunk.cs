using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AutomataChunk : OgoChunk
{
    //Amount of tiles in 3x3 area to determine if a space is blank or not
    private const int _blankRequirement = 5;

    //Generation Constants
    private const int _blank = -1;
    private const int _valid = 0;

    /// <summary>
    /// Generates a chunk, using cellular automata's 4-5 rule
    /// </summary>
    /// <param name="_width">Width of the chunk</param>
    /// <param name="_height">Height of the chunk</param>
    /// <param name="_passes"># of passes to do, for the 4-5 rule</param>
    public AutomataChunk(Vector3 _anchor, int _width, int _height, int _passes) : base(_anchor, _width, _height)
    {
        //Generate noise
        for(int y = 0; y < _height; y++)
        {
            for(int x = 0; x < _width; x++)
            {
                heightMap[y][x] = Random.Range(-1, 1);
            }
        }

        //Now we run it through the 4-5 rule for X passes
        for(int c = 0; c < _passes; c++)
        {
            //Setup New Heightmap
            int[][] nextHeightMap = new int[_height][];
            for (int i = 0; i < _height; i++)
            {
                nextHeightMap[i] = new int[_width];
            }

            //Run through the original heightmap
            for(int y = 0; y < _height; y++)
            {
                for(int x = 0; x < _width; x++)
                {
                    //Calculate the anchors
                    int top = y - 1;
                    int bot = y + 1;
                    int left = x - 1;
                    int right = x + 1;

                    //Count the number of blanks around this cell
                    int validCount = 0;
                    for (int yy = top; yy <= bot; yy++)
                    {
                        for(int xx = left; xx <= right; xx++)
                        {
                            //Check if this is in-bounds
                            if((xx > 0) && (yy > 0) && (xx < _width) && (yy < _height))
                            {
                                //Determine if it is a valid space or not
                                if(heightMap[yy][xx] == (_valid))
                                {
                                    //If it's a valid space, increment the count
                                    validCount++;
                                }
                            }
                        }
                    }

                    //Place it's new symbol
                    nextHeightMap[y][x] = (validCount < _blankRequirement) ? (_valid) : (_blank);
                }
            }

            //Copy over the new heightmap
            heightMap = nextHeightMap;
            /*for(int y = 0; y < _height; y++)
            {
                for(int x = 0; x < _width; x++)
                {
                    heightMap[y][x] = nextHeightMap[y][x];
                }
            }*/
        }

        //Print
        using (StreamWriter sw = new StreamWriter("output.txt"))
        {
            for(int y = 0; y < _height; y++)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for(int x = 0; x < _width; x++)
                {
                    char toplace;
                    if(heightMap[y][x] == _blank)
                    {
                        toplace = ' ';
                    }
                    else
                    {
                        toplace = '#';
                    }
                    sb.Append(toplace);
                }
                sw.WriteLine(sb.ToString());
            }
        }
    }
}
