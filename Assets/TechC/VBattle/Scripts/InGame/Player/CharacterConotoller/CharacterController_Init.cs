using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TechC.CommentSystem;

namespace TechC.Player
{
    /// <summary>
    /// CharacterController_Init.cs
    /// 初期化関連の分離クラス
    /// </summary>
    public partial class CharacterController
    {
        private void Awake()
        {
            var attackManager = new AttackManager();
            characterState = new CharacterState(playerInputManager, this, attackManager, anim, commandHistory);
            attackManager?.Initialize(weakAttack, strongAttack, appealBase, playerInputManager, this);

            anim.speed = defaultAnimSpeed;

            currentGuardPower = characterData.GuardPower;
            defaultSize = new Vector3(hitCollider.radius, hitCollider.height, 0f);
            defaultCenter = hitCollider.center;

            if (outlineMat != null && renderers != null)
            {
                outlineMat = Instantiate(outlineMat);

                foreach (var smr in renderers)
                {
                    if (smr == null) continue;
                    var mats = smr.materials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        if (mats[i] != null && mats[i].name.Contains("Outline"))
                        {
                            mats[i] = outlineMat;
                        }
                    }
                    smr.materials = mats;
                }
            }

            foreach (var e in Enum.GetValues(typeof(BuffType)))
            {
                multiplierEntries.Add((BuffType)e, new Dictionary<int, float>());
            }
        }

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            opponentController = BattleJudge.I.GetOtherPlayerObjects(PlayerID)[0].GetComponent<Player.CharacterController>();

            if (isClonePlayer) return;

            if (hpPresenter != null)
            {
                hpPresenter.OnDeath += Des;
            }
            else
            {
                Debug.LogError($"Player {playerID}: HPPresenterが見つかりません。");
            }
        }

        private void FindPresenters()
        {
            string presenterTag = $"Presenter_P{playerID}";
            GameObject preObj = GameObject.FindWithTag(presenterTag);

            gaugePresenter = preObj.GetComponent<GaugePresenter>();
            hpPresenter = preObj.GetComponent<HPPresenter>();
        }

        public void SetClonePlayerID(int id) => playerID = id;

        public void SetPlayerID(int id, InputDevice inputDevice)
        {
            playerID = id;
            if (id == 1)
            {
                if (outlineMat.HasProperty("_OutlineColor"))
                {
                    outlineMat.SetColor("_OutlineColor", outlineColor1);
                }
                transform.rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
            }
            else if (id == 2)
            {
                if (outlineMat.HasProperty("_OutlineColor"))
                {
                    outlineMat.SetColor("_OutlineColor", outlineColor2);
                }
                transform.rotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
            }

            this.inputDevice = inputDevice;
            FindPresenters();
        }
    }
}