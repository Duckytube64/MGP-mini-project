using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniProject
{
    public static class Vars
    {
        // World settings
        public static float pInertia = 0.0125f, pGravity = 4, pEvaporation = 0.01f;
        // Erosion settings
        public static float pCapacity = 0.02f, pMinSlope = 0.1f, pDeposition = 0.01f, pErosion = 0.1f;
        public static int pErosionRadius = 4;
        // Simulation duration settings
        public static int dropletsPerUpdate = 1000, totalDroplets = 1000000, nrIterations = 20, currentDroplets = 0;

        public static float initDropletWater = 1;
        public static float initDropletvelocity = 1;
        public static float[] heights;
        public static int imgRes;
        public static System.Random randomGen = new System.Random();

        public static bool pause = true;
    }

    public class SimErosion
    {        
        private float[] updatedHeights;
        // Start is called before the first frame update
        public SimErosion(float[] Heights, int ImgRes)
        {
            Vars.heights = Heights;
            Vars.imgRes = ImgRes;
            updatedHeights = (float [])Heights.Clone();
        }

        // Update is called once per frame
        public void Update()
        {
            bool[] updatedPixel = new bool[updatedHeights.Length];
            for (int i = 0; i < Vars.dropletsPerUpdate && Vars.currentDroplets < Vars.totalDroplets; i++)
            {
                Droplet d = new Droplet();
                for (int j = 0; j < Vars.nrIterations; j++)
                {
                    int xGrid = (int)Math.Floor(d.x), yGrid = (int)Math.Floor(d.y);
                    float offsetX = d.x - xGrid;
                    float offsetY = d.y - yGrid;
                    // get droplet height by bilinear interpolation of the enclosing quad
                    float dHeight = 0;
                    Vector2 gradient = new Vector2(0, 0);
                    computeGradientHeight(updatedHeights, ref d, ref dHeight, ref gradient);

                    // get new direction and normalize
                    d.dir.x = (d.dir.x * Vars.pInertia - gradient.x * (1 - Vars.pInertia));
                    d.dir.y = (d.dir.y * Vars.pInertia - gradient.y * (1 - Vars.pInertia));
                    d.dir.Normalize();

                    // update position
                    d.x += d.dir.x;
                    d.y += d.dir.y;

                   

                    // stop simulating droplet if it doesn't move or if it gets out of bounds
                    if ((d.dir.x == 0 && d.dir.y == 0) || (d.x < 0 || d.x >= (Vars.imgRes - 1) || d.y < 0 || d.y >= (Vars.imgRes - 1)))
                        break;

                    // compute new droplet height
                    float newHeight = 0;
                    Vector2 newGradient = new Vector2(0, 0);
                    computeGradientHeight(updatedHeights, ref d, ref newHeight, ref newGradient);

                    float heightDiff = newHeight - dHeight;
                    float c = Math.Max(Math.Max(-heightDiff, Vars.pMinSlope) * d.velocity * d.water * Vars.pCapacity, 0.01f);


                    // droplet is moving uphill or has more sediment than its capacity
                    if (d.sediment > c || heightDiff > 0)
                    {
                        // Deposit sediment                        
                        float depositAmount = heightDiff > 0 ? Math.Min(heightDiff, d.sediment) : (d.sediment - c) * Vars.pDeposition;
                        updatedHeights[xGrid * Vars.imgRes + yGrid] += depositAmount * (1 - offsetX) * (1 - offsetY);
                        updatedHeights[(xGrid + 1) * Vars.imgRes + yGrid] += depositAmount * offsetX * (1 - offsetY);
                        updatedHeights[xGrid * Vars.imgRes + yGrid + 1] += depositAmount * (1 - offsetX) * offsetY;
                        updatedHeights[(xGrid + 1) * Vars.imgRes + yGrid + 1] += depositAmount * offsetX * offsetY;
                        updatedPixel[xGrid * Vars.imgRes + yGrid] = true;
                        updatedPixel[(xGrid + 1) * Vars.imgRes + yGrid] = true;
                        updatedPixel[(xGrid + 1) * Vars.imgRes + yGrid + 1] = true;
                        updatedPixel[xGrid * Vars.imgRes + yGrid] = true;
                        d.sediment -= depositAmount;
                    }
                    else
                    {
                        // Erode all points inside the radius
                        float erosionAmount = Math.Min((c - d.sediment) * Vars.pErosion, -heightDiff);
                        applyErosion(ref d, ref updatedHeights, ref updatedPixel, erosionAmount, xGrid, yGrid);
                    }

                    // update droplet velocity and water amount
                    d.velocity = (float)Math.Sqrt(Math.Max(d.velocity * d.velocity - heightDiff * Vars.pGravity, 0));
                    d.water = d.water * (1 - Vars.pEvaporation);
                }
                Vars.currentDroplets++;
            }
            updatedHeights = blurMap(updatedHeights, Vars.imgRes, updatedPixel); ;
        }

        public float[] getUpdatedHeights()
        {
            return updatedHeights;
        }

        float[] blurMap(float[] map, int mapSize, bool[] applyBlur, float blurFactor = 0.005f)
        {
            float[] blurredMap = (float[])map.Clone();
            int[,] kernel = new int[,] { { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 } };

            for (int row = 0; row < mapSize; row++)
            {
                for (int col = 0; col < mapSize; col++)
                {
                    if (!applyBlur[row * mapSize + col])
                        continue;
                    float sum = 0;
                    float sumKernel = 0;
                    for (int j = -1; j <= 1; j++)
                    {
                        for (int i = -1; i <= 1; i++)
                        {
                            if ((row + j) >= 0 && (row + j) < mapSize && (col + i) >= 0 && (col + i) < mapSize)
                            {
                                float height = map[(row + j) * mapSize + (col + i)];
                                sum += height * kernel[i + 1, j + 1];
                                sumKernel += kernel[i + 1, j + 1];
                            }
                        }
                    }

                    blurredMap[row * mapSize + col] = blurFactor * (sum / sumKernel) + (1 - blurFactor) * map[row * mapSize + col];
                }
            }
            return blurredMap;
        }

        void computeGradientHeight(float[] mapHeights, ref Droplet d, ref float dropletHeight, ref Vector2 gradient)
        {
            // get the upper left corner of the enclosing quad
            int xGrid = (int)Math.Floor(d.x), yGrid = (int)Math.Floor(d.y);
            
            // get droplet position inside the quad
            d.u = d.x - xGrid;
            d.v = d.y - yGrid;

            // get the heights of the quad corners
            float xy = mapHeights[xGrid * Vars.imgRes + yGrid];
            float x1y = mapHeights[(xGrid + 1) * Vars.imgRes + yGrid];
            float xy1 = mapHeights[xGrid * Vars.imgRes + yGrid + 1];
            float x1y1 = mapHeights[(xGrid + 1) * Vars.imgRes + yGrid + 1];

            // get droplet height by bilinear interpolation of the enclosing quad corners
            dropletHeight = (x1y1 * d.u + x1y * (1 - d.u)) * d.v + (xy1 * d.u + xy * (1 - d.v)) * (1 - d.v);
            gradient = new Vector2(
                (x1y - xy) * (1 - d.v) + (x1y1 - xy1) * d.v,
                (xy1 - xy) * (1 - d.u) + (x1y1 - x1y) * d.u);
        }

        void applyErosion(ref Droplet d, ref float[] map, ref bool[] updated, float erosionAmount, int xGrid, int yGrid)
        {
            int radius = Vars.pErosionRadius;
            double[] weights = new double[(int)Math.Pow(2 * radius + 1, 2)];
            Vector2[] coords = new Vector2[(int)Math.Pow(2 * radius + 1, 2)];
            int numPoint = 0;
            double weighSum = 0;
            for (int x = -radius; x <= radius + 1; ++x)
            {
                for (int y = -radius; y <= radius + 1; ++y)
                {
                    int coordX = xGrid + x;
                    int coordY = yGrid + y;

                    float diffX = coordX - d.x, diffY = coordY - d.y;
                    float distanceSqrd = x * x + y * y;
                    // ignore points outside the circle
                    if (distanceSqrd > radius * radius)
                        continue;

                    if (coordX >= 0 && coordX < Vars.imgRes && coordY >= 0 && coordY < Vars.imgRes)
                    {
                        double weight = Math.Max(0, radius - Math.Sqrt(distanceSqrd));
                        weights[numPoint] = weight;
                        coords[numPoint] = new Vector2(coordX, coordY);
                        numPoint++;
                        weighSum += weight;
                    }
                }
            }

            float initsed = d.sediment;

            for (int i = 0; i < numPoint; ++i)
            {
                weights[i] /= weighSum;
                
                float pointErosion = (float)(erosionAmount * weights[i]);
                int pointIndex = (int)(coords[i].x * Vars.imgRes + coords[i].y);
                updated[pointIndex] = true;
                if (pointErosion > map[pointIndex])
                {
                    d.sediment += map[pointIndex];
                    map[pointIndex] = 0;
                } else
                {
                    d.sediment += pointErosion;
                    map[pointIndex] -= pointErosion;
                }
            }
        }
    }

    public class Droplet
    {
        public float x, y, u = 0, v = 0, water = Vars.initDropletWater, velocity = Vars.initDropletvelocity;
        public float sediment = 0;
        public Vector2 dir = new Vector2(0, 0);
        
        public Droplet()
        {
            //System.Random r = new System.Random();
            x = (float)Vars.randomGen.NextDouble() * (Vars.imgRes - 1);
            y = (float)Vars.randomGen.NextDouble() * (Vars.imgRes - 1);
        }
    }
}
