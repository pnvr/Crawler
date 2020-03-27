﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public EntityType spawningType;
    public float spawnInterval = 5f;
    float timer;
    public GameObject NetworkManager;

    void Update() {
        if(NetworkManager.GetComponent<NetworkManager>().joined == true) {
            //Debug.Log(NetworkManager.GetComponent<NetworkManager>().playersInGame);
            if (PhotonNetwork.isMasterClient)
            {

                if (timer < 0) {
                var enemyClone = PhotonNetwork.Instantiate("NetworkEnemy", transform.position, Quaternion.identity, 0);
                var c = enemyClone.GetComponent<Character>();
                c.characterType = spawningType;
                c.npc = true;
                timer = spawnInterval;
            }
            timer -= Time.deltaTime;
            }
        }
    }
}