using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechC
{
    [CreateAssetMenu(fileName = "New Stage Data", menuName = "TechC/Stage Data", order = 1)]
    public class StageData:ScriptableObject
    {
        [Header("基本情報")]
        public string stageName = "New Stage";
        public Sprite stageSprite;

        [Header("カメラの設定")]
        public bool overrideCameraPosition = false;
        public Vector3 cameraOffset = Vector3.zero;
        public Vector2 cameraDeadZone = Vector2.one;

        [Header("Camera Constraints")]
        public bool constrainCamera = false;
        public Collider2D cameraConfiner;

        [Header("Zoom Settings")]
        public bool overrideZoomSettings = false;
        public float minFOV = 30f;
        public float maxFOV = 60f;
        public float minCameraDistance = 5f;
        public float maxCameraDistance = 20f;

        [Header("Stage Bounds")]
        public bool useCustomBounds = false;
        public Bounds customBounds = new Bounds(Vector3.zero, Vector3.one * 20f);
    }
}
