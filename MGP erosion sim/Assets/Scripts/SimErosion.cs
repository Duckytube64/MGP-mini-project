using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniProject
{
    public static class Vars
    {
        // World settings
        public static float pInertia = 0.125f, pGravity = 10, pEvaporation = 0.01f;
        // Erosion settings
        public static float pCapacity = 0.2f, pMinSlope = 0.1f, pDeposition = 0.01f, pErosion = 0.2f;
        public static int pErosionRadius = 3;
        // Simulation duration settings
        public static int dropletsPerUpdate = 1000, totalDroplets = 100000, nrIterations = 20, currentDroplets = 0;

        public static float initDropletWater = 1;
        public static float initDropletvelocity = 1;
        public static float[] heights;
        public static int imgRes;

        public static bool pause = true;
        public static bool reset = false;
        public static bool view = true;
        public static Texture2D heightMap;

        public static int fileIndex = 0;
    }

    public class SimErosion
    {        
        private float[] updatedHeights;
        bool[] updatedPixel;

        public SimErosion(float[] Heights, int ImgRes)
        {
            Vars.heights = Heights;
            Vars.imgRes = ImgRes;
            updatedHeights = (float[])Heights.Clone();
            updatedPixel = new bool[updatedHeights.Length];
        }

        
        public void Update(int miniIts = 100, bool ret = false)
        {
            for (int i = 0; i < miniIts; i++)
            {
                Droplet d = new Droplet();
                for (int j = 0; j < Vars.nrIterations; j++)
                {
                    Vector2 oldPos = new Vector2(d.x, d.y);
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
                    Console.WriteLine(d.dir);

                    // update position
                    d.x += d.dir.x;
                    d.y += d.dir.y;                   

                    // stop simulating droplet if it doesn't move or if it gets out of bounds
                    if ((d.dir.x == 0 && d.dir.y == 0) || d.x < 0 || d.x >= (Vars.imgRes - 1) || d.y < 0 || d.y >= (Vars.imgRes - 1))
                        break;

                    // compute new droplet height
                    float newHeight = 0;
                    Vector2 newGradient = new Vector2(0, 0);
                    computeGradientHeight(updatedHeights, ref d, ref newHeight, ref newGradient);

                    float heightDiff = newHeight - dHeight;
                    float c = Math.Max(-heightDiff, Vars.pMinSlope) * d.velocity * d.water * Vars.pCapacity;

                    // droplet is moving uphill or has more sediment than its capacity
                    if (d.sediment > c)
                    {
                        // Deposit sediment                        
                        float depositAmount = (d.sediment - c) * Vars.pDeposition;
                        applyDeposition(ref d, ref updatedHeights, ref updatedPixel, depositAmount, newHeight);
                    }
                    else if (heightDiff > 0)
                    {
                        float depositAmount = Math.Min(heightDiff, d.sediment);
                        applyDeposition(ref d, ref updatedHeights, ref updatedPixel, depositAmount, dHeight, true, oldPos.x, oldPos.y);
                    }
                    else
                    {
                        // Erode all points inside the radius
                        float erosionAmount = Math.Min((c - d.sediment) * Vars.pErosion, -heightDiff);
                        applyErosion(ref d, ref updatedHeights, ref updatedPixel, erosionAmount);
                    }

                    // update droplet velocity and water amount
                    d.velocity = (float)Math.Sqrt(Math.Max(d.velocity * d.velocity - heightDiff * Vars.pGravity, 0));
                    d.water = d.water * (1 - Vars.pEvaporation);
                }
                Vars.currentDroplets++;
            }

            if (ret)
            {
                updatedHeights = blurMap(updatedHeights, Vars.imgRes, updatedPixel);
                updatedPixel = new bool[updatedHeights.Length];
            }                
        }

        public float[] getUpdatedHeights()
        {
            return updatedHeights;
        }

        float[] blurMap(float[] map, int mapSize, bool[] applyBlur, float blurFactor = 0.1f)
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

        void applyErosion(ref Droplet d, ref float[] map, ref bool[] updated, float erosionAmount)
        {
            int radius = Vars.pErosionRadius;
            double[] weights = new double[(int)Math.Pow(2 * radius + 1, 2)];
            Vector2[] coords = new Vector2[(int)Math.Pow(2 * radius + 1, 2)];
            int numPoint = 0;

            double weighSum = 0;
            for (int x = -radius; x <= radius + 1; ++x)
                for (int y = -radius; y <= radius + 1; ++y)
                {
                    int coordX = (int)d.x + x;
                    int coordY = (int)d.y + y;

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

        void applyDeposition(ref Droplet d, ref float[] map, ref bool[] updated, float depositAmount, float currHeight, bool tooHigh = false, float oldX = 0, float oldY = 0)
        {
            float X = d.x, Y = d.y;
            // We want to deposit before the hill, not on the hill
            if (tooHigh)
            {
                X = oldX;
                Y = oldY;
            }
            
            int radius = Vars.pErosionRadius;
            List<double> distanceWeights = new List<double>();
            List<double> heightWeights = new List<double>();
            List<Vector2> coords = new List<Vector2>();
            int numPoint = 0;

            double dWeightSum = 0, hWeightSum = 0;
            for (int x = -radius; x <= radius + 1; ++x)
                for (int y = -radius; y <= radius + 1; ++y)
                {
                    int coordX = (int)X + x;
                    int coordY = (int)Y + y;

                    float diffX = coordX - X, diffY = coordY - Y;
                    float distanceSqrd = diffX * diffX + diffY * diffY;
                    // ignore points outside the circle
                    if (distanceSqrd > radius * radius)
                        continue;


                    if (coordX >= 0 && coordX < Vars.imgRes && coordY >= 0 && coordY < Vars.imgRes)
                    {
                        float hDiff = map[coordX * Vars.imgRes + coordY] - currHeight;

                        double distanceWeight = Math.Max(0, radius - Math.Sqrt(distanceSqrd));
                        distanceWeights.Add(distanceWeight);
                        // higher points recieve exponentially less deposition
                        double heightWeight = Math.Pow(1 - hDiff, 2);
                        heightWeights.Add(heightWeight);

                        coords.Add(new Vector2(coordX, coordY));
                        numPoint++;
                        dWeightSum += distanceWeight;
                        hWeightSum += heightWeight;
                    }
                }

            for (int i = 0; i < numPoint; ++i)
            {
                distanceWeights[i] /= dWeightSum;
                heightWeights[i] /= hWeightSum;
                double weight = (distanceWeights[i] + heightWeights[i]) / 2;
                float pointDeposition = (float)(depositAmount * weight);
                int pointIndex = (int)(coords[i].x * Vars.imgRes + coords[i].y);
                d.sediment -= pointDeposition;
                map[pointIndex] += pointDeposition;
				updated[pointIndex] = true;
            }
        }

        //void getWeights(int radius, float dX, float dY, ref double[] weights, ref Vector2[] coords, ref int numPoint)
        //{
        //    double weighSum = 0;
        //    for (int x = -radius; x <= radius + 1; ++x)            
        //        for (int y = -radius; y <= radius + 1; ++y)
        //        {
        //            int coordX = (int)dX + x;
        //            int coordY = (int)dY + y;

        //            float diffX = coordX - dX, diffY = coordY - dY;
        //            float distanceSqrd = x * x + y * y;
        //            // ignore points outside the circle
        //            if (distanceSqrd > radius * radius)
        //                continue;

        //            if (coordX >= 0 && coordX < Vars.imgRes && coordY >= 0 && coordY < Vars.imgRes)
        //            {
        //                double weight = Math.Max(0, radius - Math.Sqrt(distanceSqrd));
        //                weights[numPoint] = weight;
        //                coords[numPoint] = new Vector2(coordX, coordY);
        //                numPoint++;
        //                weighSum += weight;
        //            }
        //        }
        //    for (int i = 0; i < numPoint; ++i)            
        //        weights[i] /= weighSum;            
        //}
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
