using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainGen : MonoBehaviour
{
    public Material mountain_Material;
    // Start is called before the first frame update
    void Start()
    {
        Texture2D hMap = Resources.Load("Heightmap") as Texture2D;

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        //Bottom left section of the map, other sections are similar
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                if (hMap.GetPixel(i, j).grayscale != 0)
                    j = j;
                //Add each new vertex in the plane
                verts.Add(new Vector3(i, hMap.GetPixel(i, j).grayscale * 20, j));
                //Skip if a new square on the plane hasn't been formed
                if (i == 0 || j == 0) continue;
                //Adds the index of the three vertices in order to make up each of the two tris
                tris.Add(100 * i + j); //Top right
                tris.Add(100 * i + j - 1); //Bottom right
                tris.Add(100 * (i - 1) + j - 1); //Bottom left - First triangle
                tris.Add(100 * (i - 1) + j - 1); //Bottom left 
                tris.Add(100 * (i - 1) + j); //Top left
                tris.Add(100 * i + j); //Top right - Second triangle
            }
        }

        Vector2[] uvs = new Vector2[verts.Count];
        for (var i = 0; i < uvs.Length; i++) //Give UV coords X,Z world coords
            uvs[i] = new Vector2(verts[i].x, verts[i].z);

        GameObject plane = new GameObject("ProcPlane"); //Create GO and add necessary components
        plane.AddComponent<MeshFilter>();
        plane.AddComponent<MeshRenderer>();
        Mesh procMesh = new Mesh();
        procMesh.vertices = verts.ToArray(); //Assign verts, uvs, and tris to the mesh
        procMesh.uv = uvs;
        procMesh.triangles = tris.ToArray();
        procMesh.RecalculateNormals(); //Determines which way the triangles are facing
        plane.GetComponent<MeshFilter>().mesh = procMesh; //Assign Mesh object to MeshFilter
        plane.GetComponent<MeshRenderer>().material = mountain_Material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
