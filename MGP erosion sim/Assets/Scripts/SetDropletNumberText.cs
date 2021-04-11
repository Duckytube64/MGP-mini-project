using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MiniProject
{
    public class SetDropletNumberText : MonoBehaviour
    {
        Text textObj;

        // Start is called before the first frame update
        void Start()
        {
            textObj = transform.GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            textObj.text = Vars.currentDroplets.ToString();
        }
    }
}