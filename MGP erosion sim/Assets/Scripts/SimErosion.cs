using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniProject
{
    public static class Vars
    {
        public static float pCap, pDis, pInert = 0.5f;
        public static float[] heights;
        public static int imgRes;
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
                    int xGrid = (int)Math.Floor(d.x), yGrid = (int)Math.Floor(d.y);
                    float xdiff = d.x - xGrid, ydiff = d.y - yGrid;
                    float xy = Vars.heights[xGrid * Vars.imgRes + yGrid],
                        x1y = Vars.heights[(xGrid + 1) * Vars.imgRes + yGrid],
                        xy1 = Vars.heights[xGrid * Vars.imgRes + yGrid + 1],
                        x1y1 = Vars.heights[(xGrid + 1) * Vars.imgRes + yGrid + 1];
                    float dHeight = (x1y1 * xdiff + xy1 * (1 - xdiff)) * ydiff + (xy1 * xdiff + xy * (1 - xdiff)) * (1 - ydiff); // Bilinear interpolation for droplet height
                    Vector2 gxy = new Vector2(
                        (x1y - xy) * (1 - d.v) + (x1y1 - xy1) * d.v,
                        (xy1 - xy) * (1 - d.u) + (x1y1 - x1y) * d.u);
                }
            }
        }

        public float[] getUpdatedHeights()
        {
            return Vars.heights;
        }

    }

    public class Droplet
    {
        public float x, y, u = 0, v = 0;

        // Start is called before the first frame update
        public Droplet()
        {
            System.Random r = new System.Random();
            x = (float)r.NextDouble() * (Vars.imgRes - 1);
            y = (float)r.NextDouble() * (Vars.imgRes - 1);
        }
    }
}