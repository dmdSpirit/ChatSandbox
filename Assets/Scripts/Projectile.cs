using UnityEngine;

namespace dmdspirit
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private new Renderer renderer;
        
        private HitPoints target;
        private float damage;
        private float speed;
        
        public void Initialize(HitPoints target, float damage, float speed, Color color)
        {
            this.target = target;
            this.damage = damage;
            this.speed = speed;
            renderer.material.SetColor("_Color", color);
        }

        private void Update()
        {
            if (target.IsAlive==false)
            {
                Destroy(gameObject);
                return;
            }
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime*speed);
            if (Vector3.Distance(transform.position, target.transform.position) > target.radius) return;
                target.GetHit(damage);
                Destroy(gameObject);
        }
    }
}
