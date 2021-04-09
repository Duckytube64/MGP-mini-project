using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MiniProject
{
    public class SettingsMenu : MonoBehaviour
    {
        public void setCarry(string carry)
        {
            Vars.pCapacity = float.Parse(carry);
        }

        public void setDeposition(string depos)
        {
            Vars.pDeposition = float.Parse(depos);
        }

        public void setErosion(string erosion)
        {
            Vars.pErosion = float.Parse(erosion);
        }

        public void setRadius(string radius)
        {
            Vars.pRadius = int.Parse(radius);
        }

        public void setInertia(string inertia)
        {
            Vars.pInertia = float.Parse(inertia);
        }

        public void setGravity(string gravity)
        {
            //Vars. = float.Parse(gravity);
        }

        public void setEvaporation(string carry)
        {
            Vars.pEvaporation = float.Parse(carry);
        }

        public void setMinSlope(string minSlope)
        {
            Vars.pMinSlope = float.Parse(minSlope);
        }

        public void setDropletRate(string rate)
        {
            Vars.dropletsPerUpdate = int.Parse(rate);
        }

        public void setTotalDroplets(string total)
        {
            Vars.totalDroplets = int.Parse(total);
        }

        public void setLifeTime(string time)
        {
            Vars.nrIterations = int.Parse(time);
        }
    }
}
