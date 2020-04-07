﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerCharacter : Character {

	Rigidbody2D rb2D;
	CircleCollider2D col;
	SpriteRenderer spriteRenderer;
	GameObject projHead;
	GameObject[] players;
	GameObject MainCamera;
	int camNum = 0;
	bool potion;
	bool dashing = false;
	bool camFound = false;
	bool alive;
	float playerCamOffset = 0.002f;
	float dashCooldown = 3.0f;
	float dashTime = 0.15f;
	float dashTimer = 0.15f;
	float respawnTime = 10.0f;
	float respawnTimer = 10.0f;
	// Multiplier for base player speed when dashing
	float dashFactor = 4.0f;
	Vector2 dashVector;
	Vector2 lastDir;
	Vector3 TargetPosition;

	public Animator animator;
	public GameObject playerCam;
	public GameObject myUIBox;

	void Start() {
		alive = true;
		rb2D = GetComponent<Rigidbody2D>();
		col = GetComponent<CircleCollider2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		projHead = transform.Find("ProjectileHeading").gameObject;
		MainCamera = transform.Find("Main Camera").gameObject;
		dashCooldown = 0.0f;
		SetCharacterAttributes();
		if(!PhotonNetwork.isMasterClient)
			return;
		var photonView = GetComponent<PhotonView>();
		if(photonView != null)
			PlayerManager.Instance.ModifyHealth(photonView.owner, health);
		players = GameObject.FindGameObjectsWithTag("Player");
	}
	public void TakeDamage(int dmg, Character c) {
		PhotonView photonView = c.GetComponent<PhotonView>();
		if(photonView != null) {
			PlayerManager.Instance.ModifyHealth(photonView.owner, -dmg);
			//print(photonView.owner);
		}


		//if(npc) {
		//    health -= dmg;
		//} else {

		//    health -= dmg;

		//}
		// LOL@above! =)

		//print("Health after damage " + health);

	}
	[PunRPC]
	public void Die()
	{
		Debug.Log(gameObject.name + " died");
		rb2D.isKinematic = true;
		rb2D.velocity = Vector2.zero;
		movement.x = 0;
		movement.y = 0;
		// Disable collider
		col.enabled = false;
		spriteRenderer.enabled = false;
		// Enable ProjectileHeading child GameObject
		projHead.SetActive(false);
		dashCooldown = 5.0f;
		alive = false;

		// Try to find a remote camera
		if (players.Length > 1)
		{
			findCamera();
		}
		else
		{
			Debug.Log("Cant find camera, no alive players found");
		}
	}

	[PunRPC]
	void respawn()
	{
		Debug.Log(gameObject.name + " respawned");
		// Spawn at currently chosen remote cam/player position
		gameObject.transform.position = players[camNum].transform.position;
		// Fix camera position
		MainCamera.transform.position = gameObject.transform.position + new Vector3(0, 0, -11);
		// Reset character attributes
		SetCharacterAttributes(); 
		rb2D.isKinematic = false;
		// Enable collider
		col.enabled = true;
		spriteRenderer.enabled = true;
		// Enable ProjectileHeading child GameObject
		projHead.SetActive(true);
		alive = true;
	}

	// Try to find a camera to "follow".
	void findCamera()
	{
		bool cameraFound = false;
		int alivePlayers = 0;
		for (int i = 0; i < players.Length; i++)
		{
			if (players[i].GetComponent<PlayerCharacter>().alive)
			{
				alivePlayers++;
			}
		}
		//Debug.Log("Alive players" + alivePlayers);
		if(alivePlayers > 0)
		{
			while (!cameraFound)
			{
				if (camNum == (players.Length - 1))
				{

					camNum = 0;
				}
				else
				{
					camNum++;
				}
				if (players[camNum].GetPhotonView().viewID != photonView.viewID && players[camNum].gameObject.GetComponent<PlayerCharacter>().alive)
				{
					//Debug.Log("Found Cam num " + camNum);
					//Debug.Log("Camera found");
					//Debug.Log("Found camera ID: " + players[camNum].GetPhotonView().viewID);
					MainCamera.transform.position = players[camNum].transform.Find("Main Camera").transform.position;
					cameraFound = true;
					// Global camFound variable
					camFound = true;
				}
			}
		}
		else
		{
			Debug.Log("No alive players found");
		}
	}

	

	void Update() {

		if (photonView.isMine) {

			players = GameObject.FindGameObjectsWithTag("Player");

			// When the player is dead
			if (!alive)
			{
				respawnTimer -= Time.deltaTime;
				
				if(Input.GetMouseButtonDown(0))
				{
					if (players.Length > 1)
					{
						findCamera();
					}
					else
					{
						Debug.Log("Cant search for a remote camera, only 1 player in the game");
					}
				}
			}
			
			//// Death
			//if (health <= 0 && alive)
			//{
			//	die();
			//	photonView.RPC("die", PhotonTargets.Others);
			//}

			// Respawn
			if (respawnTimer <= 0)
			{
				respawnTimer = respawnTime;
				respawn();
				photonView.RPC("respawn", PhotonTargets.Others);
			}

			// When the player is alive
			if (alive)
			{
				attackTimer -= Time.deltaTime;
				// Health potion input
				if (Input.GetKeyDown(KeyCode.H))
				{
					UsePotion();
				}
				// Attack input
				if (attackTimer < 0 && Input.GetKey(KeyCode.Mouse0))
				{
					Attack();
				}
				// Movement input
				movement.x = Input.GetAxisRaw("Horizontal");
				movement.y = Input.GetAxisRaw("Vertical");


				if (movement.x != 0 || movement.y != 0)
				{
					lastDir = new Vector2(movement.x, movement.y);
				}

				// Camera movement
				Vector3 mousePos = Camera.main.WorldToScreenPoint(transform.position);
				playerCam.transform.position = new Vector3((Input.mousePosition.x - mousePos.x) * playerCamOffset, (Input.mousePosition.y - mousePos.y) * playerCamOffset, playerCam.transform.position.z) + transform.position;


				// Setting the correct animation/stance depending on the current mouse position and if moving or not
				Vector2 mouseVector = new Vector2(Input.mousePosition.x - mousePos.x, Input.mousePosition.y - mousePos.y);
				//Debug.Log(mouseVector);

				animator.SetFloat("Horizontal", mouseVector.x);
				animator.SetFloat("Vertical", mouseVector.y);
				animator.SetFloat("Magnitude", movement.magnitude);



				if (Input.GetKeyDown(KeyCode.Space) && dashCooldown <= 0)
				{
					Debug.Log("Dashing");
					dashing = true;
					dashCooldown = 5.0f;

					// Initial dash direction from mouse position
					//Vector3 position = Camera.main.WorldToScreenPoint(transform.position);
					//dashVector = new Vector2(Input.mousePosition.x - position.x, Input.mousePosition.y - position.y);

					// Dashing to the last movement direction from keyboard inputs
					dashVector = lastDir;
				}

				if (dashing)
				{
					dashTimer -= Time.deltaTime;
					// Updating dashing direction mid dash with mouse position.
					//Vector3 position = Camera.main.WorldToScreenPoint(transform.position);
					//dashVector = new Vector2(Input.mousePosition.x - position.x, Input.mousePosition.y - position.y);

					// Updating dashing direction mid dash with keyboard inputs. Comment to have static direction
					//dashVector = lastDir;
				}

				if (dashTimer <= 0)
				{
					dashing = false;
					dashTimer = dashTime;
				}

				if (!dashing && dashCooldown - Time.deltaTime > 0)
				{
					dashCooldown -= Time.deltaTime;
				}
				else if (!dashing && dashCooldown - Time.deltaTime < 0)
				{
					dashCooldown = 0f;
				}

			}
			//if (myUIBox != null)
			//	myUIBox.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 120); // Make my UIBox Bigger than others.
			
		} 
		else 
		{
			transform.position = Vector3.Lerp(transform.position, TargetPosition, 0.1f);
			rb2D.isKinematic = true;
		}
	}

	private void FixedUpdate() {

		// Move the PlayerCharacter of the correct player
		if (photonView.isMine) {

			// When the player is dead
			if(!alive)
			{
				// If remote camera is found follow a camera/player with current camNum.
				if (camFound)
				{
					//MainCamera.transform.position = players[camNum].transform.Find("Main Camera").transform.position;
					MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, players[camNum].transform.Find("Main Camera").transform.position, 0.1f);
				}
			}
			

			if (rb2D != null)
				if (dashing) {
					//rb2D.velocity = new Vector2(movement.x * speed, movement.y * speed).normalized * speed * dashFactor;
					rb2D.velocity = dashVector.normalized * speed * dashFactor;
				} else {
					rb2D.velocity = new Vector2(movement.x * speed, movement.y * speed).normalized * speed;
				}
			//Debug.Log(rb2D.velocity.magnitude);
		}
	}

	void upgradeWeapon() {
		if (characterType == EntityType.Hero0) {
			if (weaponLevel == 0) {
				projectilesPerAttack++;
			} else if (weaponLevel == 1) {
				attackInterval = 0.2f;
			}

		} else if (characterType == EntityType.Hero1) {
			if (weaponLevel == 0) {
				attackInterval = 1f;
			} else if (weaponLevel == 1) {
				projectileSpeed = 20f;
			}

		}
		weaponLevel++;
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.CompareTag("WeaponUpgrade")) {
			upgradeWeapon();
			Destroy(collision.gameObject);
		}
	}
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		if (stream.isWriting) {
			stream.SendNext(transform.position);
		} else {
			TargetPosition = (Vector3)stream.ReceiveNext();
		}
	}


	public void UsePotion() {
		if (potion) {
			health += 100;
			potion = false;
		}
	}

	public void GetPotion() {
		potion = true;
	}

	public void GetSpeed() {
		speed += 10;
	}

	// _____________________________Sort these out BELOW!!!____________________________________
	private void OnCollisionEnter2D(Collision2D collision) {
		// Check if collision is projectile and type of shooter
		if(collision.gameObject.CompareTag("Projectile")) {
			var projectile = collision.gameObject.GetComponent<Projectile>();
			if(npc != projectile.shotByNPC) {
				TakeDamage(projectile.damage, this);
				print("Damage goes to " + this.gameObject);
			}

			//photonView.RPC("TakeDamage", PhotonTargets.Others, projectile.damage);
		}
	}

	public void Attack() {
		if(ranged) {
			Shoot(projectilesPerAttack, damage);
			photonView.RPC("Shoot", PhotonTargets.Others, projectilesPerAttack, 0);
		} else {
			Melee();
			photonView.RPC("Melee", PhotonTargets.Others);
		}
		attackTimer = attackInterval;
	}



	[PunRPC]
	public void Shoot(int amount, int damage) {


		if(amount % 2 == 0) {
			float startOffset = 0.25f * (amount - 1);
			for(int i = 0; i < amount; i++) {
				GameObject projectileClone = Instantiate(projectilePrefab, projectileSpawn.transform.position, projectileSpawn.transform.rotation);
				projectileClone.transform.parent = projectileSpawn.transform;
				projectileClone.transform.localPosition = new Vector3(0f, startOffset - (i * 0.50f), 0f);
				projectileClone.transform.parent = null;
				Projectile projectile = projectileClone.GetComponent<Projectile>();
				projectile.LaunchProjectile(damage, attackRange, projectileSpeed, npc, (projectileSpawn.transform.position - transform.position).normalized);
			}
		} else {
			float leftOffset = 0.50f;
			float rightOffset = 0.50f;
			for(int i = 0; i < amount; i++) {
				if(i == 0) {
					GameObject projectileClone = Instantiate(projectilePrefab, projectileSpawn.transform.position, projectileSpawn.transform.rotation);
					projectileClone.transform.parent = projectileSpawn.transform;
					projectileClone.transform.localPosition = new Vector3(0f, 0f, 0f);
					projectileClone.transform.parent = null;
					Projectile projectile = projectileClone.GetComponent<Projectile>();
					projectile.LaunchProjectile(damage, attackRange, projectileSpeed, npc, (projectileSpawn.transform.position - transform.position).normalized);
				} else if(i % 2 == 0) {
					GameObject projectileClone = Instantiate(projectilePrefab, projectileSpawn.transform.position, projectileSpawn.transform.rotation);
					projectileClone.transform.parent = projectileSpawn.transform;
					projectileClone.transform.localPosition = new Vector3(0f, leftOffset, 0f);
					projectileClone.transform.parent = null;
					Projectile projectile = projectileClone.GetComponent<Projectile>();
					projectile.LaunchProjectile(damage, attackRange, projectileSpeed, npc, (projectileSpawn.transform.position - transform.position).normalized);
					leftOffset += 0.50f;
				} else {
					GameObject projectileClone = Instantiate(projectilePrefab, projectileSpawn.transform.position, projectileSpawn.transform.rotation);
					projectileClone.transform.parent = projectileSpawn.transform;
					projectileClone.transform.localPosition = new Vector3(0f, -rightOffset, 0f);
					projectileClone.transform.parent = null;
					Projectile projectile = projectileClone.GetComponent<Projectile>();
					projectile.LaunchProjectile(damage, attackRange, projectileSpeed, npc, (projectileSpawn.transform.position - transform.position).normalized);
					rightOffset += 0.50f;
				}
			}
		}
	}



	[PunRPC]
	public void Melee() {

		Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

		foreach(var hit in hits) {
			var ec = hit.gameObject.GetComponent<EnemyCharacter>();
			if(ec != null) {
				Debug.Log(hit.gameObject);
				ec.TakeDamage(damage);
			}
		}
	}
}