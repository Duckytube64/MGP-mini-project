using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace MiniProject
{
    public class SettingsMenu : MonoBehaviour
    {
        string[] pars = new string[11] {"Inertia", "Gravity", "Evaporation", "Carry capacity", "Minimal slope",
            "Deposition speed", "Erosion", "Erosion radius", "Update rate", "Total droplets", "Droplet lifetime"};

        InputField[] inputfields = new InputField[12];
        float[] vals = new float[12];

        private void Start()
        {
            // Read default parameter settings
            vals[0] = Vars.pInertia;
            vals[1] = Vars.pGravity;
            vals[2] = Vars.pEvaporation;
            vals[3] = Vars.pCapacity;
            vals[4] = Vars.pMinSlope;
            vals[5] = Vars.pDeposition;
            vals[6] = Vars.pErosion;
            vals[7] = Vars.pErosionRadius;
            vals[8] = Vars.dropletsPerUpdate;
            vals[9] = Vars.totalDroplets;
            vals[10] = Vars.nrIterations;
            vals[11] = Vars.fileIndex;

            Transform parameters = transform.Find("Parameters");
            for (int i = 0; i < pars.Length; i++)
            {
                Transform par = parameters.Find(pars[i]);
                InputField inputfield = par.Find("InputField").GetComponent<InputField>();
                inputfields[i] = inputfield;
                inputfield.text = vals[i].ToString();
            }
            InputField fileNrInput = transform.Find("File Number").GetComponent<InputField>();
            inputfields[11] = fileNrInput;
            fileNrInput.text = vals[11].ToString();
        }

        public void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                setPause();
            }
        }

        public void setPause()
        {
            Vars.pause = !Vars.pause;
        }

        public void resetScene()
        {
            Vars.reset = true;
        }

        public void loadTerrain()
        {
            string a = "Heightmaps/HM_" + Vars.fileIndex;
            Texture2D texture = new Texture2D(2, 2);
            texture = (Texture2D)Resources.Load(a);
            if (texture != null)
            {
                Vars.heightMap = texture;
                Vars.reset = true;
            }
        }

        public void setFileNumber(string fileNumber)
        {
            if (fileNumber != "")
            {
                Vars.fileIndex = int.Parse(fileNumber);
                inputfields[11].text = fileNumber;
            }
            else
            {
                inputfields[11].text = Vars.fileIndex.ToString();
            }
        }

        public void toggleView()
        {
            Vars.view = !Vars.view;
        }

        // World settings
        public void setInertia(string inertia)
        {
            if (inertia != "")
            {
                float temp = Mathf.Clamp(float.Parse(inertia), 0, 1);
                Vars.pInertia = temp;
                inputfields[0].text = temp.ToString();
            }
            else
            {
                inputfields[0].text = Vars.pInertia.ToString();
            }
        }

        public void setGravity(string gravity)
        {
            if (gravity != "")
            {
                float temp = Mathf.Max(float.Parse(gravity), 0);
                Vars.pGravity = temp;
                inputfields[1].text = temp.ToString();
            }
            else
            {
                inputfields[1].text = Vars.pGravity.ToString();
            }
        }

        public void setEvaporation(string evaporation)
        {
            if (evaporation != "")
            {
                float temp = Mathf.Clamp(float.Parse(evaporation), 0, 1);
                Vars.pEvaporation = temp;
                inputfields[2].text = temp.ToString();
            }
            else
            {
                inputfields[2].text = Vars.pEvaporation.ToString();
            }
        }

        // Erosion settings
        public void setCarry(string carry)
        {
            if (carry != "")
            {
                float temp = Mathf.Max(float.Parse(carry), 0);
                Vars.pCapacity = temp;
                inputfields[3].text = temp.ToString();
            }
            else
            {
                inputfields[3].text = Vars.pCapacity.ToString();
            }
        }
        public void setMinSlope(string minSlope)
        {
            if (minSlope != "")
            {
                Vars.pMinSlope = float.Parse(minSlope);
                inputfields[4].text = minSlope;
            }
            else
            {
                inputfields[4].text = Vars.pMinSlope.ToString();
            }
        }

        public void setDeposition(string deposition)
        {
            if (deposition != "")
            {
                float temp = Mathf.Clamp(float.Parse(deposition), 0, 1);
                Vars.pDeposition = temp;
                inputfields[5].text = temp.ToString();
            }
            else
            {
                inputfields[5].text = Vars.pDeposition.ToString();
            }
        }

        public void setErosion(string erosion)
        {
            if (erosion != "")
            {
                float temp = Mathf.Clamp(float.Parse(erosion), 0, 1);
                Vars.pErosion = temp;
                inputfields[6].text = temp.ToString();
            }
            else
            {
                inputfields[6].text = Vars.pErosion.ToString();
            }
        }

        public void setRadius(string radius)
        {
            if (radius != "")
            {
                int temp = Mathf.Max(int.Parse(radius), 0);
                Vars.pErosionRadius = temp;
                inputfields[7].text = temp.ToString();
            }
            else
            {
                inputfields[7].text = Vars.pErosionRadius.ToString();
            }
        }

        // Simulation duration settings
        public void setDropletRate(string rate)
        {
            if (rate != "")
            {
                int temp = Mathf.Max(int.Parse(rate), 0);
                Vars.dropletsPerUpdate = temp;
                inputfields[8].text = temp.ToString();
            }
            else
            {
                inputfields[8].text = Vars.dropletsPerUpdate.ToString();
            }
        }

        public void setTotalDroplets(string total)
        {
            if (total != "")
            {
                int temp = Mathf.Max(int.Parse(total), 0);
                Vars.totalDroplets = temp;
                inputfields[9].text = temp.ToString();
            }
            else
            {
                inputfields[9].text = Vars.totalDroplets.ToString();
            }
        }

        public void setLifeTime(string time)
        {
            if (time != "")
            {
                int temp = Mathf.Max(int.Parse(time), 0);
                Vars.nrIterations = temp;
                inputfields[10].text = temp.ToString();
            }
            else
            {
                inputfields[10].text = Vars.nrIterations.ToString();
            }
        }
    }
}
