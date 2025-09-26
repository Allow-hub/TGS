using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    public class SetTrailMaterial : MonoBehaviour
    {
        [SerializeField] private Player.CharacterController characterController;
        [SerializeField] private ParticleSystem mainParticleSystem;
        [SerializeField] private TrailRenderer mainTrailRenderer;
        [SerializeField] private TrailRenderer smokeTrailRenderer;
        [SerializeField] private Material p1MainParticleMaterial;
        [SerializeField] private Material p2MainParticleMaterial;
        [SerializeField] private Material p1MainTrailMaterial;
        [SerializeField] private Material p2MainTrailMaterial;
        [SerializeField] private Material p1SmokeTrailMaterial;
        [SerializeField] private Material p2SmokeTrailMaterial;

        private void Start()
        {
            if (characterController == null) return;
            SetMaterials(characterController.PlayerID == 1);
        }

        public void SetMaterials(bool isPlayer1)
        {
            if (mainParticleSystem != null)
            {
                mainParticleSystem.GetComponent<Renderer>().material = isPlayer1 ? p1MainParticleMaterial : p2MainParticleMaterial;
            }

            if (mainTrailRenderer != null)
            {
                mainTrailRenderer.material = isPlayer1 ? p1MainTrailMaterial : p2MainTrailMaterial;
            }

            if (smokeTrailRenderer != null)
            {
                smokeTrailRenderer.material = isPlayer1 ? p1SmokeTrailMaterial : p2SmokeTrailMaterial;
            }
        }
    }
}
