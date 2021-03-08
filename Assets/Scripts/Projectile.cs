using UnityEngine;

namespace dmdspirit
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private new Renderer renderer;
        
        private HitPoints target;
        private float damage;
        private float speed;
        private bool isAlive;
        
        public void Initialize(HitPoints target, float damage, float speed, Color color)
        {
            this.target = target;
            this.damage = damage;
            this.speed = speed;
            renderer.material.SetColor("_Color", color);
            isAlive = true;
        }

        private void Update()
        {
            if (isAlive == false || target.IsAlive==false)
            {
                Destroy(gameObject);
                return;
            }
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime*speed);
        }

        private void OnCollisionEnter(Collision collision)
        {
            var collisionHP = collision.gameObject.GetComponent<HitPoints>();
            if (collisionHP != target) return;
            collisionHP.GetHit(damage);
            isAlive = false;
        }
    }
}
