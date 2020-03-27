﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Photon.MonoBehaviour {
	int keys = 0;
	Canvas canvas;
	float counter;
	public bool useUIBoxes;
	GameObject[] playerUIBoxes;
	public GameObject playerUIBox;
	GameObject[] players;

	void Start() {
		//canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
	}

    private void Update() { } /*{
		if (useUIBoxes) {
			if (counter >= 1) {
				foreach (Transform child in canvas.transform.GetChild(0).transform) {
					Destroy(child.gameObject);
				}
				UpdatePlayerUIBoxes();
				counter = 0;
			} else {
				counter += Time.deltaTime;
			}
		}
	}
    */
	public bool UseKeyRPC() {
        Debug.Log(keys);

        if (keys > 0) {
            photonView.RPC("DecraseKeyAll", PhotonTargets.Others);
            keys -= 1;
            Debug.Log(keys);
            return true;
		}
        Debug.Log(keys);
        return false;
	}

    [PunRPC]
    public void DecraseKeyAll()
    {
        keys -= 1;
    }
	public void FoundKey() {
        keys += 1;
        Debug.Log(keys);
	}
	public bool UseKey() {
        return UseKeyRPC();
	}

	public void UpdatePlayerUIBoxes() {
		players = GameObject.FindGameObjectsWithTag("Player");
		for (int i = 0; i < players.Length; i++) {
			playerUIBoxes = new GameObject[players.Length]; // Taulukon koko on playerien määrä

			GameObject newUIBox = Instantiate(playerUIBox.gameObject); // Tee uusi UIBox
			newUIBox.GetComponent<PlayerUIBox>().player = players[i]; // Anna boxin playeriksi listan i:es pelaaja
			playerUIBoxes[i] = newUIBox;
			playerUIBoxes[i].transform.SetParent(canvas.transform.GetChild(0)); // laita boxi canvasin lapseksi
		}
	}
}
