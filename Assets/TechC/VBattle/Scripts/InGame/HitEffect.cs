using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace TechC
{
    public class HitEffect : MonoBehaviour
    {
        public VisualEffect visualEffect;
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void OnClick()
        {
            visualEffect.SendEvent("Hit");
        }
    }
}
