using UnityEngine;

namespace Arena.Scripts
{
    public abstract class AbstractDamagable: MonoBehaviour
    {
        [SerializeField] public float Health;
        public float currentHealth;
        [SerializeField] public float damage;
        public float currentDamage;
        public abstract void Damage(float damage,Vector3 hitPoint, Vector3 hitNormal);
        protected abstract void Dead(Vector3 directions,float damage);
    }
}
