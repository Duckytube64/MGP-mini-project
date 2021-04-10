using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniProject
{
    public static class Vars
    {
        // World settings
        public static float pInertia = 0.1f, pGravity = 10, pEvaporation = 0.01f;
        // Erosion settings
        public static float pCapacity = 0.01f, pMinSlope = 0.01f, pDeposition = 0.01f, pErosion = 0.1f;
        public static int pErosionRadius = 1;
        // Simulation duration settings
        public static int dropletsPerUpdate = 1000, totalDroplets = 100000, nrIterations = 10;

        public static float initDropletWater = 1;
        public static float initDropletvelocity = 1;
        public static float[] heights;
        public static int imgRes;
    }

    public class SimErosion
    {        
        public int currentDroplets = 0;
        private float[] updatedHeights;
        // Start is called before the first frame update
        public SimErosion(float[] Heights, int ImgRes)
        {
            Vars.heights = Heights;
            Vars.imgRes = ImgRes;
            updatedHeights = (float[])Heights.Clone();
        }

        // Update is called once per frame
        public void Update()
        {
            for (int i = 0; i < Vars.dropletsPerUpdate && currentDroplets < Vars.totalDroplets; i++)
            {
                Droplet d = new Droplet();
                for (int j = 0; j < Vars.nrIterations; j++)
                {
                    int xGrid = (int)Math.Floor(d.x), yGrid = (int)Math.Floor(d.y);
                    // get droplet height by bilinear interpolation of the enclosing quad
                    float dHeight = 0;
                    Vector2 gradient = new Vector2(0, 0);
                    computeGradientHeight(Vars.heights, ref d, ref dHeight, ref gradient);

                    // get new direction and normalize
                    d.dir.x = (d.dir.x * Vars.pInertia - gradient.x * (1 - Vars.pInertia));
                    d.dir.y = (d.dir.y * Vars.pInertia - gradient.y * (1 - Vars.pInertia));
                    d.dir.Normalize();
                    Console.WriteLine(d.dir);

                    // update position
                    d.x += d.dir.x;
                    d.y += d.dir.y;

                   

                    // stop simulating droplet if it doesn't move or if it gets out of bounds
                    if ((d.dir.x == 0 && d.dir.y == 0) || (d.x < 0 || d.x >= (Vars.imgRes - 1) || d.y < 0 || d.y >= (Vars.imgRes - 1)))
                        break;

                    // compute new droplet height
                    float newHeight = 0;
                    Vector2 newGradient = new Vector2(0, 0);
                    computeGradientHeight(Vars.heights, ref d, ref newHeight, ref newGradient);

                    float heightDiff = newHeight - dHeight;
                    float c = Math.Max(-heightDiff * d.velocity * d.water * Vars.pCapacity, Vars.pMinSlope);

                    // droplet is moving uphill or has more sediment than its capacity
                    if (d.sediment > c || heightDiff > 0)
                    {
                        // Deposit sediment                        
                        float depositAmount = heightDiff > 0 ? Math.Min(heightDiff, d.sediment) : (d.sediment - c) * Vars.pDeposition;
                        updatedHeights[xGrid * Vars.imgRes + yGrid] += depositAmount * (1 - d.u) * (1 - d.v);
                        updatedHeights[(xGrid + 1) * Vars.imgRes + yGrid] += depositAmount * (1 - d.u) * d.v;
                        updatedHeights[xGrid * Vars.imgRes + yGrid + 1] += depositAmount * d.u * (1 - d.v);
                        updatedHeights[(xGrid + 1) * Vars.imgRes + yGrid + 1] += depositAmount * d.u * d.v;
                        d.sediment -= depositAmount;
                    }
                    else
                    {
                        // Erode all points inside the radius
                        float erosionAmount = Math.Min((c - d.sediment) * Vars.pErosion, -heightDiff);
                        d.sediment += erosionAmount;
                        applyErosion(ref d, Vars.pErosionRadius, ref updatedHeights, erosionAmount, xGrid, yGrid);
                    }

                    // update droplet velocity and water amount
                    d.velocity = (float)Math.Sqrt(Math.Max(d.velocity * d.velocity + heightDiff * Vars.pGravity, 0));
                    d.water = d.water * (1 - Vars.pEvaporation);
                }
                currentDroplets++;
            }
        }

        public float[] getUpdatedHeights()
        {
            return updatedHeights;
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

        void applyErosion(ref Droplet d, int radius, ref float[] map, float erosionAmount, int xGrid, int yGrid)
        {
            double[] weights = new double[(2 * radius + 1) * (2 * radius + 1)];
            Vector2[] coords = new Vector2[(2 * radius + 1) * (2 * radius + 1)];
            int numPoint = 0;
            double weighSum = 0;
            for (int x = -radius; x <= radius; ++x)
            {
                for (int y = -radius; y <= radius; ++y)
                {
                    // ignore points outside the circle
                    if (x * x + y * y > radius * radius)
                        continue;

                    int coordX = xGrid + x;
                    int coordY = yGrid + y;

                    if (coordX >= 0 && coordX < Vars.imgRes && coordY >= 0 && coordY < Vars.imgRes)
                    {
                        double weight = Math.Max(0, radius - Math.Sqrt(x * x + y * y));
                        weights[numPoint] = weight;
                        coords[numPoint] = new Vector2(coordX, coordY);
                        numPoint++;
                        weighSum += weight;
                    }
                }
            }

            for (int i = 0; i < numPoint; ++i)
            {
                weights[i] /= weighSum;
                
                float pointErosion = (float)(erosionAmount * weights[i]);

                int pointIndex = (int)(coords[i].x * Vars.imgRes + coords[i].y);
                if (map[pointIndex] < pointErosion)
                {
                    d.sediment += map[pointIndex];
                    map[pointIndex] = 0;
                } 
                else 
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
            System.Random r = new System.Random();
            x = (float)r.NextDouble() * (Vars.imgRes - 1);
            y = (float)r.NextDouble() * (Vars.imgRes - 1);
        }
    }
}
