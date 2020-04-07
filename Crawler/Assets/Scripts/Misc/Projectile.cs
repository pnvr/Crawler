﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public int damage;
    private float range;
    Vector2 origPos;
    public bool shotByNPC;
    public bool explosive;
    public bool reflective;


    public void LaunchProjectile(int d, float r, float s, bool npc, Vector2 dir) {
        // Get rigidbody
        Rigidbody2D rb2D = GetComponent<Rigidbody2D>();
        // Set damage of the projectile
        damage = d;
        // Set shooter type
        shotByNPC = npc;
        // Set range
        range = r;
        rb2D.AddForce(dir * s, ForceMode2D.Impulse);
        origPos = transform.position;
    }

    void Update() {
        if(Vector2.Distance(transform.position, origPos) > range) {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if(!reflective)
            Destroy(gameObject);
        else {
            // Reflect from colliding object with proper angle
        }
    }
}