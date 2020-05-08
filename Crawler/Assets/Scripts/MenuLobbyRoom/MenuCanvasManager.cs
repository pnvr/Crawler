﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuCanvasManager : MonoBehaviour {

    Action goLobby;
    Action goRoom;

    public static MenuCanvasManager Instance;

    public LobbyCanvas lobbyCanvas;

    public CurrentRoomCanvas currentRoomCanvas;

    public RectTransform roomInteractables;

    public RectTransform lobbyInteractables;

    private void Start() {
        goLobby += InLobby;
        goRoom += InRoom;
    }

    public void JoinedRoom() {
        LeanTween.move(lobbyInteractables, Vector3.right * 2500, .5f).setEaseInExpo().setOnComplete(goRoom);
    }

    public void JoinedLobby() {
        LeanTween.move(roomInteractables, Vector3.right * 2500, .5f).setEaseInExpo().setOnComplete(goLobby);
    }

    void InLobby() {
        MenuCanvasManager.Instance.lobbyCanvas.transform.SetAsLastSibling();
        LeanTween.move(lobbyInteractables, Vector3.zero, .5f).setEaseOutBack();
    }

    void InRoom() {
        MenuCanvasManager.Instance.currentRoomCanvas.transform.SetAsLastSibling();
        LeanTween.move(roomInteractables, Vector3.zero, .5f).setEaseOutBack();
    }


    private void Awake() {
        Instance = this;
    }
}
