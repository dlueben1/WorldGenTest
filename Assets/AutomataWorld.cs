using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomataWorld : OgoWorld
{
    #region Settings

    /// <summary>
    /// The number of passes each chunk should make through the generator
    /// </summary>
    public int Passes;

    #endregion

    private void Start()
    {
        Generate(400);
    }

    /// <summary>
    /// Create one single chunk as a world
    /// </summary>
    public override void Generate(int seed)
    {
        //Seed the generator
        StartRandom(seed);

        //Create an Automata Chunk
        Chunks.Add(new AutomataChunk(new Vector3(0, 0, 0), 20, 20, Passes));

        Render();

        //And restore the generator!
        EndRandom();
    }
}
