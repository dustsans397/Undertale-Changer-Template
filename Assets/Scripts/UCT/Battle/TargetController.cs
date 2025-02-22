using DG.Tweening;
using TMPro;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;
using DataHandlerService = UCT.Service.DataHandlerService;

namespace UCT.Battle
{
    /// <summary>
    ///     控制Target
    /// </summary>
    public class TargetController : MonoBehaviour
    {
        [Header("攻击造成的伤害")] public int hitDamage;

        public GameObject hpBar;

        [Header("父级传入")] public int select;

        [Header("父级传入 要击打的怪物")] public EnemiesController hitMonster;

        private Animator _anim;
        private GameObject _bar;

        private TextMeshPro _hitUI, _hitUIb;
        private bool _pressZ;

        private void Start()
        {
            _anim = GetComponent<Animator>();
            _hitUIb = transform.Find("Move/HitTextB").GetComponent<TextMeshPro>();
            _hitUI = _hitUIb.transform.GetChild(0).GetComponent<TextMeshPro>();
            _bar = transform.Find("Bar").gameObject;
            hpBar = transform.Find("Move/EnemiesHp/EnemiesHpOn").gameObject;
        }

        private void Update()
        {
            if (!_pressZ)
            {
                if (InputService.GetKeyDown(KeyCode.Z))
                {
                    _pressZ = true;
                    _anim.SetBool("Hit", true);
                    _anim.SetFloat("MoveSpeed", 0);
                    AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipBattle);
                    Hit();
                }
            }
        }

        private void OnEnable()
        {
            if (_anim == null)
            {
                _anim = GetComponent<Animator>();
            }

            //anim.enabled = true;
            _anim.SetBool("Hit", false);
            _anim.SetFloat("MoveSpeed", 1);
            _pressZ = true;
        }

        /// <summary>
        ///     攻击敌人时进行的计算
        /// </summary>
        private void Hit()
        {
            if (Mathf.Abs(_bar.transform.localPosition.x) > 0.8f)
            {
                hitDamage = (int)
                    (2.2f / 13.2f * (14 - Mathf.Abs(_bar.transform.localPosition.x)) //准确度系数
                                  * (MainControl.Instance.playerControl.atk + DataHandlerService.GetItemFormDataName(MainControl.Instance.playerControl.wearWeapon).Data.Value
                                      - MainControl.Instance.BattleControl.enemiesDef[select] + Random.Range(0, 2)));
            }
            else
            {
                hitDamage = (int)
                    (2.2f / 13.2f * (14 - 0.8f) //准确度系数
                                  * (MainControl.Instance.playerControl.atk + DataHandlerService.GetItemFormDataName(MainControl.Instance.playerControl.wearWeapon).Data.Value
                                      - MainControl.Instance.BattleControl.enemiesDef[select] + Random.Range(0, 2)));
            }

            if (hitDamage <= 0)
            {
                hitDamage = 0;

                _hitUI.text = "<color=grey>MISS";
                _hitUIb.text = "MISS";
            }
            else
            {
                _hitUI.text = "<color=red>" + hitDamage;
                _hitUIb.text = hitDamage.ToString();
            }
        }

        //以下皆用于anim
        private void HitAnim()
        {
            hitMonster.anim.SetBool("Hit", true);
            hpBar.transform.localScale =
                new Vector3(
                    (float)MainControl.Instance.BattleControl.enemiesHp[select * 2] /
                    MainControl.Instance.BattleControl.enemiesHp[select * 2 + 1], 1);
            MainControl.Instance.BattleControl.enemiesHp[select * 2] -= hitDamage;
            DOTween.To(() => hpBar.transform.localScale, x => hpBar.transform.localScale = x,
                new Vector3(
                    (float)MainControl.Instance.BattleControl.enemiesHp[select * 2] /
                    MainControl.Instance.BattleControl.enemiesHp[select * 2 + 1], 1),
                0.75f).SetEase(Ease.OutSine);
        }

        private void OpenPressZ()
        {
            _pressZ = false;
        }

        private void ClosePressZ()
        {
            _pressZ = true;
        }

        private void NotActive()
        {
            //anim.enabled = false;
            gameObject.SetActive(false);
        }
    }
}