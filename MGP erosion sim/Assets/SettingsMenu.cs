using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MiniProject
{
    public class SettingsMenu : MonoBehaviour
    {
        public void setRadius(string radius)
        {
            Vars.pRadius = int.Parse(radius);
        }
    }
}
