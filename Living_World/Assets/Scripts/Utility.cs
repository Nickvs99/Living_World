using UnityEngine;
using UnityEngine.Profiling;

public static class Utility {
    
    /// <summary>
    /// Initializes the random number generator state with a randomseed.
    /// </summary>
    public static void SetSeed() {
        SetSeed(Random.Range(0, 100000));
    }

    /// <summary>
    /// Initializes the random number generator state with the given seed.
    /// </summary>
    /// <param name="seed">int</param>
    public static void SetSeed(int seed) {

        Debug.Log($"Seed: {seed}");
        Random.InitState(seed);
    }


    /// <summary>
    /// Returns the distance between 2 vectors
    /// </summary>
    /// <param name="v1">Vector3</param>
    /// <param name="v2">Vector3</param>
    /// <returns>float: Distance between the two vectors</returns>
    public static float dist(Vector3 v1, Vector3 v2){

        return Mathf.Pow(Mathf.Pow(v1.x - v2.x, 2) + Mathf.Pow(v1.y - v2.y, 2) + Mathf.Pow(v1.z - v2.z, 2), 0.5f);
    }

    
    public static void Profile(System.Action func){
        Profiler.BeginSample(func.Method.Name);
        func();
        Profiler.EndSample();
    }
}