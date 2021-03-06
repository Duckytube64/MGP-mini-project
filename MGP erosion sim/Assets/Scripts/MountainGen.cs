using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniProject
{
    public class MountainGen : MonoBehaviour
    {
        public Material mountain_Material;
        Texture2D hMap;
        int imgRes;
        float[] heights;
        SimErosion simErosion;
        Mesh procMesh;

        // Start is called before the first frame update
        void Start()
        {
            procMesh = new Mesh();
            string a = "Heightmaps/HM_" + Vars.fileIndex;
            hMap = (Texture2D)Resources.Load(a);
            imgRes = hMap.width;
            heights = new float[imgRes * imgRes];

            //Bottom left section of the map, other sections are similar
            for (int i = 0; i < imgRes; i++)            
                for (int j = 0; j < imgRes; j++)
                {
                    //Copy heightmap to heigh array
                    float height = hMap.GetPixel(i, j).grayscale;
                    heights[i * imgRes + j] = height;
                }

            //Create our terrain GO and add relevant components
            GameObject plane = new GameObject("ProcPlane"); //Create GO and add necessary components
            plane.AddComponent<MeshFilter>();
            plane.AddComponent<MeshRenderer>();
            plane.GetComponent<MeshRenderer>().material = mountain_Material;

            updateMesh(heights);
            Vars.heightMap = hMap;

            simErosion = new SimErosion(heights, imgRes);
        }

        void updateMesh(float[] newHeights)
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            for (int i = 0; i < imgRes; i++)            
                for (int j = 0; j < imgRes; j++)
                {
                    //Add each new vertex in the plane
                    float height = newHeights[i * imgRes + j] * 80;
                    verts.Add(new Vector3(i - imgRes / 2, height, j - imgRes / 2));
                    //Skip if a new square on the plane hasn't been formed
                    if (i == 0 || j == 0) continue;
                    //Adds the index of the three vertices in order to make up each of the two tris
                    tris.Add(imgRes * i + j); //Top right
                    tris.Add(imgRes * i + j - 1); //Bottom right
                    tris.Add(imgRes * (i - 1) + j - 1); //Bottom left - First triangle
                    tris.Add(imgRes * (i - 1) + j - 1); //Bottom left 
                    tris.Add(imgRes * (i - 1) + j); //Top left
                    tris.Add(imgRes * i + j); //Top right - Second triangle
                }
            
            Vector2[] uvs = new Vector2[verts.Count];
            for (var i = 0; i < uvs.Length; i++) //Give UV coords X,Z world coords
                uvs[i] = new Vector2(verts[i].x, verts[i].z);

            //Create and assign mesh to terrain GO
            GameObject plane = GameObject.Find("ProcPlane");
            procMesh.vertices = verts.ToArray(); //Assign verts, uvs, and tris to the mesh
            procMesh.uv = uvs;
            procMesh.triangles = tris.ToArray();
            procMesh.RecalculateNormals(); //Determines which way the triangles are facing
            plane.GetComponent<MeshFilter>().mesh = procMesh; //Assign Mesh object to MeshFilter          
        }

        int updatedDroplets = 0;
        // Update is called once per frame
        void Update()
        {
            if (Vars.reset)
            {
                for (int i = 0; i < imgRes; i++)
                    for (int j = 0; j < imgRes; j++)
                    {
                        //Copy heightmap to heigh array
                        float height = Vars.heightMap.GetPixel(i, j).grayscale;
                        heights[i * imgRes + j] = height;
                    }
                updateMesh(heights);
                simErosion = new SimErosion(heights, imgRes);
                Vars.reset = false;
                Vars.pause = true;
                Vars.currentDroplets = 0;
                updatedDroplets = 0;
            }
            float[] map = Vars.view ? simErosion.getUpdatedHeights() : heights;
            if (!Vars.pause)
            {
                if (updatedDroplets + 100 > Vars.dropletsPerUpdate)
                {
                    simErosion.Update(updatedDroplets - Vars.dropletsPerUpdate, true);
                    updateMesh(map);
                    updatedDroplets = 0;
                }
                else if (Vars.currentDroplets + 100 > Vars.totalDroplets)
                {
                    simErosion.Update(Vars.currentDroplets - Vars.totalDroplets, true);
                    updateMesh(map);
                    updatedDroplets = 0;
                }
                else
                {
                    simErosion.Update();
                    updatedDroplets += 100;
                }
            } else
            {
                updateMesh(map);
            }
        }

    }
}
