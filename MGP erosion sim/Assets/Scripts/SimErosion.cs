using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniProject
{
    public static class Vars
    {
        public static float pCap, pDis, pInert = 0.5f, pRadius;
        public static float initDropletWater;
        public static float initDropletvelocity;
        public static float[] heights;
        public static int imgRes;
        public static int erosionRadius = 2;
    }

    public class SimErosion
    {
        public int nrDroplets = 100000;
        public int nrIterations = 10;
        // Start is called before the first frame update
        public SimErosion(float[] Heights, int ImgRes)
        {
            Vars.heights = Heights;
            Vars.imgRes = ImgRes;
        }

        // Update is called once per frame
        public void Update()
        {            
            for (int i = 0; i < nrDroplets; i++)
            {
                Droplet d = new Droplet();
                for (int j = 0; j < nrIterations; j++)
                {
                    // get droplet height by bilinear interpolation of the enclosing quad
                    float dHeight = 0;
                    Vector2 gradient = new Vector2(0, 0);
                    computeGradientHeight(Vars.heights, Vars.imgRes, ref d, ref dHeight, ref gradient);

                    // get new direction and normalize
                    d.dir.x = (d.dir.x * Vars.pInert - gradient.x * (1 - Vars.pInert));
                    d.dir.y = (d.dir.y * Vars.pInert - gradient.y * (1 - Vars.pInert));
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
                    computeGradientHeight(Vars.heights, Vars.imgRes, ref d, ref newHeight, ref newGradient);

                    float heightDiff = newHeight - dHeight;

                    // droplet is moving uphill
                    if (heightDiff > 0)
                    {
                        // Deposit sediment

                    } else
                    {
                        // Erode all points inside the radius
                        // get the upper left corner of the enclosing quad
                        applyErosion(d);


                    }
                }
            }
        }

        public float[] getUpdatedHeights()
        {
            return Vars.heights;
        }

        void computeGradientHeight(float[] mapHeights, int mapSize, ref Droplet d, ref float dropletHeight, ref Vector2 gradient)
        {
            // get the upper left corner of the enclosing quad
            int xGrid = (int)Math.Floor(d.x), yGrid = (int)Math.Floor(d.y);
            
            // get droplet position inside the quad
            d.u = d.x - xGrid;
            d.v = d.y - yGrid;

            // get the heights of the quad corners
            float xy = mapHeights[xGrid * mapSize + yGrid];
            float x1y = mapHeights[(xGrid + 1) * mapSize + yGrid];
            float xy1 = mapHeights[xGrid * mapSize + yGrid + 1];
            float x1y1 = mapHeights[(xGrid + 1) * mapSize + yGrid + 1];

            // get droplet height by bilinear interpolation of the enclosing quad corners
            dropletHeight = (x1y1 * d.u + xy1 * (1 - d.u)) * d.v + (xy1 * d.u + xy * (1 - d.v)) * (1 - d.v);
            gradient = new Vector2(
                (x1y - xy) * (1 - d.v) + (x1y1 - xy1) * d.v,
                (xy1 - xy) * (1 - d.u) + (x1y1 - x1y) * d.u);
        }

        void applyErosion(Droplet d, int mapSize)
        {
            int xGrid = (int)Math.Floor(d.x), yGrid = (int)Math.Floor(d.y);
            float[] weights = new float[4 * Vars.erosionRadius * Vars.erosionRadius];
            int index = 0;
            for (int x = -radius; x <= Vars.erosionRadius; ++x)
            {
                for (int y = -radius; y <= Vars.erosionRadius; ++y)
                {
                    int coordX = xGrid + x;
                    int coordY = yGrid + y;

                    if (coordX > 0 && coordX < mapSize && coordY > 0 && coordY < mapSize)
                    {
                        weights[index++] = Math.Max(0, Vars.erosionRadius - Math.Sqrt(x * x + y * y));
                    }
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
