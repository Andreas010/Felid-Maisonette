using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Entity : NetworkBehaviour
{

    [SyncVar]
    public bool takingKnockback;
    // [System.NonSerialized]

    [Header("Assignables")]
    [SerializeField] GameObject deathParticles;
    [SerializeField] GameObject hitParticles;
    [SerializeField] Material normalMat;
    [SerializeField] Material hitFlashMat;
    [SerializeField] PhysicsMaterial2D[] physicsMaterials; // 0 = no friction, 1 = high friction

    Collider2D collider;
    [SerializeField] Rigidbody2D rig;
    [SerializeField] GameObject visualsObj; // Object that holds all the objects with SpriteRenderers
    SpriteRenderer[] srs;
    NetworkTransform networkTransform;

    [SyncVar]
    public float health;

    [Header("Stats")]
    [SerializeField] float knockbackTolerance = 1; // knockbackForce * this =  outcome knockback. ex value: .5f (this entity will take half as much knockback as normal)
    public float maxHealth;
    float currentKnockbackTime;

    [SerializeField] bool destroyOnDeath;
    bool isServerEntity;

    private void Start()
    {
        srs = visualsObj.GetComponentsInChildren<SpriteRenderer>();
        networkTransform = GetComponent<NetworkTransform>();
        collider = GetComponent<Collider2D>();

        health = maxHealth;
        tag = "Entity";
        isServerEntity = !networkTransform.clientAuthority;
    }

    private void Update()
    {
        if (currentKnockbackTime > 0)
        {
            ChangePhysicsMat(0);
            currentKnockbackTime -= Time.deltaTime;
            takingKnockback = true;
        }
        else
        {
            takingKnockback = false;
        }
    }

    [Server]
    public void TakeDamage(float t_damage, Vector2 t_knockback, float t_knockbackTime)
    {
        if (health - t_damage <= 0)
        {
            RPC_SpawnParticles("Death");
            if (isServerEntity) Die();
        }
        else
        {
            RPC_SpawnParticles("Hit");
        }

        if(!isServerEntity)
        {
            RPC_TakeDamage(t_damage, t_knockback, t_knockbackTime);
        }
        else
        {
            health -= t_damage;

            takingKnockback = true;
            rig.AddForce(t_knockback * knockbackTolerance, ForceMode2D.Impulse);

            currentKnockbackTime = t_knockbackTime;
        }

    }
    [ClientRpc]
    public void RPC_TakeDamage(float t_damage, Vector2 t_knockback, float t_knockbackTime)
    {
        health -= t_damage;
        if (health <= 0) Die();

        takingKnockback = true;
        rig.AddForce(t_knockback * knockbackTolerance, ForceMode2D.Impulse);

        currentKnockbackTime = t_knockbackTime;
    }

    public void Die()
    {
        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
        else
        {
            health = maxHealth;
            transform.position = Vector3.zero;
        }
    }

    [ClientRpc]
    public void RPC_HitFlash()
    {
        foreach(SpriteRenderer t_sr in srs) t_sr.material = hitFlashMat;
        Invoke("StopHitFlash", .1f );
    }
    //[ClientRpc]
    public void StopHitFlash()
    {
        foreach (SpriteRenderer t_sr in srs) t_sr.material = normalMat;
    }

    [ClientRpc]
    void RPC_SpawnParticles(string t_type)
    {
        GameObject t_particles = null;
        if (t_type == "Hit") t_particles = Instantiate(hitParticles, transform.position, Quaternion.identity);
        else if (t_type == "Death") t_particles = Instantiate(deathParticles, transform.position, Quaternion.identity);
        if (!isServerEntity) t_particles.transform.position = new Vector3(t_particles.transform.position.x, t_particles.transform.position.y + 3, 0);
        if(t_particles) Destroy(t_particles, 2);
    }

    public void ChangePhysicsMat(int t_indexOfNewMat)
    {
        collider.sharedMaterial = physicsMaterials[t_indexOfNewMat];
    }

}
