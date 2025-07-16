using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TechC.Player;
using UnityEditor.Animations;

namespace TechC
{
    public class AnimationPreviewEditorWindow : EditorWindow
    {
        private List<CharacterData> characterDataList;
        private CharacterType selectedType;
        private CharacterData selectedData;

        private GameObject spawnedObject;
        private Animator animator;
        private AnimationClip currentClip;
        private float currentTime = 0f;
        private bool isPlaying = false;
        private float playbackSpeed = 1.0f;
        private Vector2 playSpeedRange = new Vector2(0.1f, 5f);
        private double lastUpdateTime = 0f;
        private bool isLooping = false;

        private AttackSet selectedAttackSet;
        private List<AttackData> availableAttacks = new();
        private int selectedAttackIndex = 0;
        private AttackData selectedAttackData;
        private bool hasDrawnHitbox = false;

        private string animatorControllerName = "PreviewController";
        private AnimatorController basePreviewController;

        [MenuItem("Tools/Animation Preview Tool")]
        public static void ShowWindow()
        {
            GetWindow<AnimationPreviewEditorWindow>("Animation Preview");
        }

        private void OnEnable()
        {
            string[] guids = AssetDatabase.FindAssets("t:CharacterData");
            characterDataList = guids
                .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList();

            EditorApplication.update += OnEditorUpdate;

            LoadBasePreviewControllerByName(animatorControllerName);

        }

        private void OnGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AnimatorController名", GUILayout.Width(140));
            string newName = EditorGUILayout.TextField(animatorControllerName);
            EditorGUILayout.EndHorizontal();

            if (newName != animatorControllerName)
            {
                animatorControllerName = newName;
                LoadBasePreviewControllerByName(animatorControllerName);
            }

            selectedType = (CharacterType)EditorGUILayout.EnumPopup("キャラクタータイプ", selectedType);
            selectedData = characterDataList.FirstOrDefault(d => d.type == selectedType);

            if (selectedData == null)
            {
                EditorGUILayout.HelpBox("該当するキャラデータが見つかりません", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("キャラクター生成"))
            {
                SpawnCharacter();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("攻撃データの選択", EditorStyles.boldLabel);

            selectedAttackSet = (AttackSet)EditorGUILayout.ObjectField("Attack Set", selectedAttackSet, typeof(AttackSet), false);

            if (selectedAttackSet != null)
            {
                availableAttacks = GetAllAttackData(selectedAttackSet);

                string[] attackNames = availableAttacks
                    .Select(ad => string.IsNullOrEmpty(ad.attackName) ? "(No Name)" : ad.attackName)
                    .ToArray();

                if (attackNames.Length > 0)
                {
                    int newAttackIndex = EditorGUILayout.Popup("AttackData", selectedAttackIndex, attackNames);
                    if (newAttackIndex != selectedAttackIndex)
                    {
                        selectedAttackIndex = newAttackIndex;
                        selectedAttackData = availableAttacks[selectedAttackIndex];
                        currentClip = selectedAttackData?.clip;

                        ApplyOverrideClip();
                        currentTime = 0f;
                        hasDrawnHitbox = false;
                    }
                    if (selectedAttackData?.clip != null)
                    {
                        currentClip = selectedAttackData.clip;

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("▶ 再生"))
                        {
                            isPlaying = true;
                            lastUpdateTime = EditorApplication.timeSinceStartup;
                            hasDrawnHitbox = false;
                        }

                        if (GUILayout.Button("⏸ 停止"))
                        {
                            isPlaying = false;
                        }
                        EditorGUILayout.EndHorizontal();

                        isLooping = EditorGUILayout.Toggle("ループ", isLooping);
                        playbackSpeed = EditorGUILayout.Slider("再生速度", playbackSpeed, playSpeedRange.x, playSpeedRange.y);
                        float newTime = EditorGUILayout.Slider("キーフレーム位置", currentTime, 0f, currentClip.length);
                        if (!Mathf.Approximately(newTime, currentTime))
                        {
                            currentTime = newTime;
                            PlayClipAtTime(currentClip, currentTime);

                            // ★ 停止中でもヒットボックス描画
                            if (selectedAttackData != null)
                            {
                                if (currentTime >= selectedAttackData.hitTiming && !hasDrawnHitbox)
                                {
                                    Vector3 hitPos = spawnedObject.transform.position + selectedAttackData.hitboxOffset;
                                    float radius = selectedAttackData.radius;
                                    HitboxSceneDrawer.ShowHitbox(hitPos, radius, new Color(1f, 0f, 0f, 0.5f));
                                    hasDrawnHitbox = true;
                                }
                                else if (currentTime < selectedAttackData.hitTiming)
                                {
                                    hasDrawnHitbox = false; // 戻したら再表示できるようにする
                                }
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("このAttackDataにAnimationClipが設定されていません。", MessageType.Info);
                        currentClip = null;
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("AttackData が見つかりません", MessageType.Warning);
                    selectedAttackData = null;
                    currentClip = null;
                }
            }
        }

        private void SpawnCharacter()
        {
            if (spawnedObject != null)
                GameObject.DestroyImmediate(spawnedObject);

            spawnedObject = (GameObject)PrefabUtility.InstantiatePrefab(selectedData.prefab);
            spawnedObject.transform.position = Vector3.zero;

            Transform target = spawnedObject.transform.GetChild(1);
            animator = target.GetComponent<Animator>() ?? target.gameObject.AddComponent<Animator>();

            currentTime = 0f;
            isPlaying = false;

            ApplyOverrideClip();
        }

        private void LoadBasePreviewControllerByName(string controllerName)
        {
            string[] guids = AssetDatabase.FindAssets(controllerName + " t:AnimatorController");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                basePreviewController = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                Debug.Log($"AnimatorController '{controllerName}' を読み込みました。");
            }
            else
            {
                basePreviewController = null;
                Debug.LogWarning($"AnimatorController '{controllerName}' が見つかりません。");
            }
        }

        private void ApplyOverrideClip()
        {
            if (animator == null || basePreviewController == null || selectedAttackData?.clip == null)
                return;

            var idleClip = GetStateMotionClip(basePreviewController, "Idle");
            if (idleClip == null)
            {
                Debug.LogWarning("Idle ステートのモーションクリップが見つかりません");
                return;
            }

            var overrideController = new AnimatorOverrideController(basePreviewController);
            overrideController[idleClip] = selectedAttackData.clip;
            animator.runtimeAnimatorController = overrideController;
        }

        private AnimationClip GetStateMotionClip(AnimatorController controller, string stateName)
        {
            foreach (var layer in controller.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    if (state.state.name == stateName)
                        return state.state.motion as AnimationClip;
                }
            }
            return null;
        }

        private List<AttackData> GetAllAttackData(AttackSet set)
        {
            var list = new List<AttackData>();
            var fields = typeof(AttackSet).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(AttackData))
                {
                    var value = field.GetValue(set) as AttackData;
                    if (value != null)
                        list.Add(value);
                }
            }

            return list;
        }

        private void PlayClipAtTime(AnimationClip clip, float time)
        {
            if (animator == null || clip == null) return;

            float normalizedTime = time / clip.length;
            animator.Play("Idle", 0, normalizedTime);
            animator.Update(0f);
            animator.speed = 0;
        }

        private void OnEditorUpdate()
        {
            if (!isPlaying || animator == null || currentClip == null)
                return;

            double now = EditorApplication.timeSinceStartup;
            double deltaTime = now - lastUpdateTime;
            lastUpdateTime = now;

            currentTime += (float)(deltaTime * playbackSpeed);
            if (isLooping && currentTime > currentClip.length)
            {
                currentTime = 0f;
                hasDrawnHitbox = false;
            }

            currentTime = Mathf.Clamp(currentTime, 0f, currentClip.length);
            PlayClipAtTime(currentClip, currentTime);

            if (selectedAttackData != null && !hasDrawnHitbox)
            {
                if (currentTime >= selectedAttackData.hitTiming)
                {
                    Vector3 hitPos = spawnedObject.transform.position + selectedAttackData.hitboxOffset;
                    float radius = selectedAttackData.radius;
                    HitboxSceneDrawer.ShowHitbox(hitPos, radius, new Color(1f, 0f, 0f, 0.5f));
                    hasDrawnHitbox = true;
                }
            }

            Repaint();
        }

        private void OnDisable()
        {
            if (spawnedObject != null)
                GameObject.DestroyImmediate(spawnedObject);

            EditorApplication.update -= OnEditorUpdate;
        }
    }
}