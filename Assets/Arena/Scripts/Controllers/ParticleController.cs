using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Arena.Scripts.Controllers
{
    public class ParticleController : MonoBehaviour
    {
        public static ParticleController instance;
        
        
        [SerializeField] private ParticleData[] particles;
        
        // Кешируем системы частиц для быстрого доступа
        private Dictionary<EParticleType, ParticleSystem> particleDictionary;

        private void Awake()
        {
            if (instance == null) instance = this;
            // Инициализируем словарь при старте
            particleDictionary = new Dictionary<EParticleType, ParticleSystem>();
            
            foreach (var data in particles)
            {
                if (!particleDictionary.ContainsKey(data.particleType))
                {
                    particleDictionary.Add(data.particleType, data.particleSystem);
                }
            }
        }

        public void ParticlePlay(Vector3 position, Vector3 forward, EParticleType particleType)
        {
            // Проверяем наличие системы частиц в словаре
            if (particleDictionary.TryGetValue(particleType, out var particleSystem))
            {
                // Устанавливаем позицию и направление
                particleSystem.transform.position = position;
                if (forward != Vector3.zero)
                {
                    particleSystem.transform.forward = forward;
                }

                // Воспроизводим систему частиц
                particleSystem.Play();
            }
            else
            {
                Debug.LogWarning($"Particle system of type {particleType} not found!");
            }
        }
    }

    [System.Serializable]
    public class ParticleData
    {
        public ParticleSystem particleSystem;
        public EParticleType particleType;
    }

    public enum EParticleType
    {
        Null,Hit, Blood, WallHit
    }
}