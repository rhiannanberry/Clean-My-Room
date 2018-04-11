﻿using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[System.Serializable]
public class Obj: MonoBehaviour {
	public GameObject buttonReference;
	public bool util = false;
	private Renderer[] childMats;
	private Color baseColor;

	// Update is called once per frame
	void Awake() {
		childMats = GetComponentsInChildren<Renderer>();
		baseColor = childMats[0].materials[0].GetColor("_OutlineColor");
	}

	void Update () {
		if (SceneManager.GetActiveScene().buildIndex != 0 && SceneManager.GetActiveScene().name != "TimeMode"){
			if (!util && transform.position.y < -40) {
				buttonReference.GetComponent<ObjectButton>().DespawnObject(gameObject);
			}
			if (util && transform.localPosition.x <= -10) {
				Destroy(gameObject);
			}
		}
	}

	public void ObjectHover(bool hovering) {
		Color col = hovering ? Color.yellow : baseColor;
		UpdateShader(col);
	}

	private void UpdateShader(Color col) {
		foreach(Renderer rend in childMats) {
			foreach(Material mat in rend.materials) {
				try {
					mat.SetColor("_OutlineColor", col);
				} catch (Exception e) {
					Debug.Log(transform.name + ": " + e.ToString());
				}
			}
		}
	}
}
