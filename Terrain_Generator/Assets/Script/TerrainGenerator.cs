using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int terrainWidth = 512; // Width of the terrain in units
    public int terrainHeight = 512; // Height of the terrain in units
    public float terrainScale = 50f; // Scale of the terrain
    public float noiseScale = 50f; // Scale of the Perlin Noise
    public float maxHeight = 50f; // Maximum height of the terrain
    public float minHeight = -20f; // Minimum height of the terrain
    public int octaves = 8; // Number of octaves to use for Perlin noise
    public float persistence; // Persistence value for Perlin noise
    public float lacunarity = 2f; // Lacunarity value for Perlin noise
    public Vector2 offset = new Vector2(0, 0); // Offset for the Perlin noise

    private Terrain terrain; // Reference to the Terrain component

    private void Start()
    {
        terrain = GetComponent<Terrain>(); // Get the Terrain component on this object
        GenerateTerrain(); // Generate the terrain
    }

    private void GenerateTerrain()
    {
        TerrainData terrainData = new TerrainData(); // Create a new TerrainData object

        // Set the heightmap resolution and size of the terrain
        terrainData.heightmapResolution = terrainWidth + 1;
        terrainData.size = new Vector3(terrainWidth, maxHeight, terrainHeight);

        // Generate the heights for the terrain
        terrainData.SetHeights(0, 0, GenerateHeights());

        // Assign the TerrainData object to the terrain component
        terrain.terrainData = terrainData;
    }

    // This method generates the heights for the terrain using Perlin noise
    private float[,] GenerateHeights()
    {
        // Create a new 2D array to hold the heights
        float[,] heights = new float[terrainWidth, terrainHeight];

        // Loop through each x and z position and generate the height at that position
        for(int x = 0; x < terrainWidth; x++)
        {
            for(int z = 0; z < terrainHeight; z++)
            {
                // Initialize the height to zero
                float height = 0;

                // Initialize the frequency and amplitude to 1
                float frequency = 1;
                float amplitude = 1;

                // Loop through each octave and add its contribution to the height
                for(int i = 0; i < octaves; i++)
                {
                    // Calculate the sample position for this octave
                    float sampleX = (x + offset.x) / noiseScale * frequency;
                    float sampleZ = (z + offset.y) / noiseScale * frequency;

                    // Generate the noise for this octave
                    float noise = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
                    height += noise * amplitude;

                    // Increase the frequency and decrease the amplitude for the next octave
                    frequency *= lacunarity;
                    amplitude *= persistence;
                }

                // Clamp the height between the minimum and maximum values
                height = Mathf.Clamp(height, minHeight, maxHeight);

                // Store the height in the heights array
                heights[x, z] = height;
            }
        }

        // Add some noise to the terrain to create more varied heights and shapes
        for(int i = 0; i < 5; i++)
        {
            // Generate a random X coordinate for the center of the noise circle, between 25% and 75% of the terrain width
            int centerX = Random.Range(terrainWidth / 4, 3 * terrainWidth / 4);

            // Generate a random Z coordinate for the center of the noise circle, between 25% and 75% of the terrain width
            int centerZ = Random.Range(terrainHeight / 4, 3 * terrainHeight / 4);

            // Generate a random radius for the noise circle, between 1/16 and 1/8 of the terrain width
            float radius = Random.Range(terrainWidth / 16, terrainWidth / 8);

            // Generate a random height offset to apply to the terrain, between 2 and 5 units
            float heightOffset = Random.Range(2f, 5f);

            // It loops through every point in the terrain and calculates the distance between that point and a
            // randomly generated center point, using distance formula
            for(int x = 0; x < terrainWidth; x++)
            {
                for(int z = 0; z < terrainHeight; z++)
                {
                    // This line calculates the distance between a point with coordinates (x, z) and a center point with coordinates (centerX, centerZ)
                    // using the Pythagorean theroem in 2D space, The distance value is stored in the variable "distance"
                    float distance = Mathf.Sqrt(Mathf.Pow(x - centerX, 2) + Mathf.Pow(z - centerZ, 2));

                    // The weight is clamped between 0 and 1
                    float weight = Mathf.Clamp01((radius - distance) / radius);

                    // The weight is multiplied by a random height, offset value and added to the height of the current point in the terrain
                    heights[x, z] += weight * heightOffset;
                }
            }
        }

        // Normalize terrain heights to be bewtween 0 and 1
        // Calculate the minimum and maximum height values for normalization
        float minHeightNormalized = Mathf.InverseLerp(minHeight, maxHeight, 0);
        float maxHeightNormalized = Mathf.InverseLerp(minHeight, maxHeight, maxHeight);

        // Loop through each vertex and normalize its height value
        for(int x = 0; x < terrainWidth; x++)
        {
            for(int z = 0; z < terrainHeight; z++)
            {
                // Normalize the height value using the calculated minumum and maximum heights
                float heightNormilized = Mathf.InverseLerp(minHeight, maxHeight, heights[x, z]);

                // Rescale the height value to be between the normalized minumum and maximum heights
                heights[x, z] = Mathf.Lerp(minHeightNormalized, maxHeightNormalized, heightNormilized);
            }
        }
        // Return the normalized heights array
        return heights;
    }

    // This method will be called when the play button is pressed
    public void RegenerateTerrain()
    {
        GenerateTerrain();
    }
}
