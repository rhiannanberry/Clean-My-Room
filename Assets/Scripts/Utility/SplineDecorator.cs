﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplineDecorator : MonoBehaviour {

	public BezierSpline spline;
	public float radius;
	public int loopCount, cVertCount, decorationFrequency;
	public Transform decoration, hook;
	public int frequency;

	public GameObject debugTextPrefab;

	public bool lookForward = false;

	public Transform endPoint;

	public Transform[] items;

	private Queue<float> freePointTs;
	private List<Vector3> vertList;
	private List<int> triList;
	private List<Vector2> uvList;
	private List<Transform> hookList;
	private List<Transform> decorationList;

	private void Awake () {
		/*
		vertList = new List<Vector3>();
		//size is radiusPointNum * frequency
		freePointTs = new Queue<float>();
		Vector3[] verts = new Vector3[radiusPointNum * frequency];
		Vector3[] norms = new Vector3[radiusPointNum * frequency];
		Vector2[] uvs = new Vector2[radiusPointNum * frequency];
		List<int> tris = new List<int>();
		DiscreteDecoration(); 
		
		float splineDelta = 1.0f/(frequency-1); //also for uv delta
		float uvDelta = 1.0f/(radiusPointNum);
		for (int x = 0; x < frequency; x++) {
			float t = splineDelta * x;
			t = FudgeT(t);
			List<Vector3> loop = GenerateLoop(spline.GetPoint(t) - transform.position, spline.GetDirection(t));

			int mult = x * loop.Count;
			for (int i  = 0; i < loop.Count; i++) {
				//GameObject txtObj = Instantiate(debugTextPrefab, loop[i] + transform.position, Quaternion.identity);
				//txtObj.GetComponent<TextMesh>().text = (mult + i).ToString();
				verts[mult + i] = loop[i];
				norms[mult + i] = loop[i];
				uvs[mult + i] = new Vector2(x * splineDelta, (i == 0 ? 0 : (i*1.0f)/(loop.Count - 1)));
			}

			int triCount = tris.ToArray().Length / 3;
			int[] triArr = tris.ToArray();

			Mesh mesh = new Mesh();
			mesh.vertices = verts;
			mesh.uv = uvs;
			//mesh.triangles = GenerateTris(verts);
			mesh.RecalculateBounds();
			gameObject.GetComponent<MeshFilter>().mesh = mesh;

		}*/
	}

	void Update() {

	}

	public void GenerateMesh(){
	

		triList = null;
		vertList = null;
		GenerateVertices();
		GenerateTris();
		DiscreteDecoration();

		Mesh mesh = new Mesh();
		mesh.vertices = vertList.ToArray();
		mesh.triangles = triList.ToArray();

		mesh.RecalculateBounds();
		gameObject.GetComponent<MeshFilter>().mesh = mesh;
	}

	private void GenerateVertices() {
		vertList = new List<Vector3>();
		List<int> freePoints = GetFreeControlPoints();
		int cpLen = freePoints.Count;
		int totalControlPtCount = spline.ControlPointCount;
		bool startHanging, endHanging;
		startHanging = freePoints[0] != 0;
		endHanging = freePoints[freePoints.Count-1] != spline.ControlPointCount-1;
		int totCnt = cpLen + (startHanging ? 1 : 0) + (endHanging ? 1 : 0);
		if (startHanging) {
			GenerateSection(0f, 1.0f*freePoints[0]/(totalControlPtCount - 1));
		}
 

		for (int i = 0; i < freePoints.Count; i++) {//number of sections
			float startT, endT, tRange, tDelta;
			startT = 1.0f*(freePoints[i])/(totalControlPtCount-1);
			if (i <= freePoints.Count-2) {
				endT = 1.0f*(freePoints[i+1])/(totalControlPtCount-1);
			} else {
				endT = 1.0f;
			}
			tRange = endT - startT;
			tDelta = tRange / (loopCount-1);
			bool isLastSection = ((i == cpLen -1 && endHanging) || (i == cpLen - 2 && !endHanging));
			GenerateSection(startT, endT, isLastSection);
		}
	}

	private void GenerateSection(float tStart, float tEnd, bool end = false) {
		float tRange = tEnd - tStart;
		float tDelta = tRange / (loopCount - 1);

		for (int i = 0; i < loopCount-1; i++) {//(end ? loopCount : loopCount - 1); i++) {
			List<Vector3> loop = GenerateLoop(spline.GetPoint(tStart + (tDelta * i)) - transform.position, spline.GetDirection(tStart + (tDelta * i)));
			foreach(Vector3 vert in loop) {
				vertList.Add(vert);
			}
		}
	}
		
		private void GenerateTris() {
			triList = new List<int>();
			Vector3[] verts = vertList.ToArray();

			int cnt = 0;
			while (cnt < (verts.Length - cVertCount)) { //12 - 8 = 4
		//	Debug.Log(cnt);						//0 1 2 3  4 5 6 7  8 9 10 11	
			int i = cnt % cVertCount;		//0 1 2 3, 0 1 2 3, 0 1 2  3 
			int current = cnt - i;				 	//0 0 0 0, 4 4 4 4, 8 8 8  8 
			int next = current + cVertCount; 	//4 4 4 4, 8 8 8 8, 12__ 

			int pa1 = current + i;						// 0 1 2 3  4 5  6  7	8  9  10 11
			int pa2 = current + (i + 1)%cVertCount;	// 1 2 3 0	5 6  7  4	9  10 11 8
			int pa3 = next +  (i + 1)%cVertCount;  	// 5 6 7 4	9 10 11 8	13 14 15 12

			int pb1 = current + i;						// 0 1 2 3	4 5  6  7	8  9  10 11
			int pb2 = next + i;							// 4 5 6 7  8 9  10 11	12 13 14 15
			int pb3 = next + ((i + 1)%cVertCount);	// 5 6 7 4	9 10 11 8	13 14 15 12

			triList.Add(pa1);
			triList.Add(pa2);
			triList.Add(pa3);


			triList.Add(pb3);
			triList.Add(pb2);
			triList.Add(pb1);
			cnt++; 
		}
	}

	List<Vector3> GenerateLoop(Vector3 position, Vector3 direction) {
		List<Vector3> loop = new List<Vector3>();
		Quaternion rotation  = Quaternion.LookRotation(direction);
		float theta = 0f;
		float thetaScale = 1.0f / cVertCount;
		for (int i = 0; i < cVertCount; i++) {
			theta += (2.0f * Mathf.PI * thetaScale);
			Vector3 radiusPoint = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0) * radius;
			loop.Add(position + rotation * radiusPoint);
		}
		return loop;
	}

	private void DiscreteDecoration() {
			if (transform.childCount == 0) {
			decorationList = new List<Transform>();
			hookList = new List<Transform>();
		} else if (decorationList == null || decorationList.Count != decorationFrequency || hookList.Count + decorationList.Count != transform.childCount) {
			while (transform.childCount != 0) {
				DestroyImmediate(transform.GetChild(0).gameObject);
				Debug.Log("Destrps");
			}
				decorationList = new List<Transform>();
				hookList = new List<Transform>();
		}
		float stepSize = 1.0f/(decorationFrequency+3);
		bool hasDecorations = decorationList != null && decorationList.Count > 0;
		int d = 0;
		for (int f = 0; f < decorationFrequency+2; f++) {
			if (f != 0 && f != decorationFrequency+1) {
				float t = f * stepSize;
				Vector3 position = spline.GetPoint(t);
				if (hasDecorations) {
					decorationList[d].position = position;
					d++;
				} else {
					Transform item = Instantiate(decoration) as Transform;
					item.transform.localPosition = position;
					item.transform.parent = transform;
					decorationList.Add(item);
				}

			}
		}

		
		int ctrlPtCnt = spline.ControlPointCount;//
		bool hasHooks = hookList != null && hookList.Count > 0;
		int j = 0; //index for existing hooks
		for (int i = 0; i < ctrlPtCnt; i++) {
			if (spline.GetControlPointMode(i) == BezierControlPointMode.Free) {
				if (i != 0) {
					i++; //gets to middle free ctrl pt
				}
				Vector3 position = spline.GetPoint(i/((ctrlPtCnt-1) * 1.0f));
				if (hasHooks) {
					hookList[j].position = position;
					j++;
				} else {
					Transform item = Instantiate(hook) as Transform;
					hookList.Add(item);
					item.transform.localPosition = position;
					item.transform.parent = transform;
				}
				i++; //skips right hand free ctrl pt
			}
		}
	}

	private List<int> GetFreeControlPoints() {
		List<int> freePoints = new List<int>();
		int ctrlPtCnt = spline.ControlPointCount;
		for(int i = 0; i < ctrlPtCnt; i++) {
			if (spline.GetControlPointMode(i) == BezierControlPointMode.Free) {
				if (i != 0) {
					i++;
				}
				freePoints.Add(i);
				i++;
			}
		}
		return freePoints;
	}
}