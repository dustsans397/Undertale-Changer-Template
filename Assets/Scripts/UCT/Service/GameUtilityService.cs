﻿using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UCT.Battle;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Overworld;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace UCT.Service
{
    /// <summary>
    /// 提供一些较为全局的，与游戏内容相关的函数。
    /// </summary>
    public static class GameUtilityService
    {
        /// <summary>
        /// 设置16:9边框的Sprite
        /// </summary>
        /// <param name="framePic"></param>
        public static void SetCanvasFrameSprite(int framePic = 2)//一般为CanvasController.instance.framePic
        {
            var frame = CanvasController.Instance.frame;
            frame.sprite = framePic < 0 ? null : MainControl.Instance.OverworldControl.frames[framePic];
        }

        /// <summary>
        /// 分辨率设置
        /// </summary>
        public static void SetResolution(int resolution)
        {
            if (!MainControl.Instance.cameraMainInBattle)
            {
                if (!MainControl.Instance.cameraShake)
                    MainControl.Instance.cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
                else
                    MainControl.Instance.cameraMainInBattle = MainControl.Instance.cameraShake.GetComponent<Camera>();
            }

            if (!MainControl.Instance.OverworldControl.hdResolution)
            {
                if (MainControl.Instance.OverworldControl.resolutionLevel > 4)
                    MainControl.Instance.OverworldControl.resolutionLevel = 0;
            }
            else
            {
                if (MainControl.Instance.OverworldControl.resolutionLevel < 5)
                    MainControl.Instance.OverworldControl.resolutionLevel = 5;
            }

            if (!MainControl.Instance.OverworldControl.hdResolution)
            {
                MainControl.Instance.mainCamera.rect = new Rect(0, 0, 1, 1);
                if (MainControl.Instance.sceneState == MainControl.SceneState.InBattle)
                {
                    if (MainControl.Instance.cameraMainInBattle) MainControl.Instance.cameraMainInBattle.rect = new Rect(0, 0, 1, 1);
                }
                
                if (BackpackBehaviour.Instance)
                    BackpackBehaviour.Instance.SuitResolution();

                CanvasController.Instance.DOKill();
                CanvasController.Instance.fps.rectTransform.anchoredPosition = new Vector2();
                CanvasController.Instance.frame.color = new Color(1, 1, 1, 0);
                CanvasController.Instance.setting.transform.localScale = Vector3.one;
                CanvasController.Instance.setting.rectTransform.anchoredPosition = new Vector2(0, CanvasController.Instance.setting.rectTransform.anchoredPosition.y);
            }
            else
            {
                if (MainControl.Instance.mainCamera)
                    MainControl.Instance.mainCamera.rect = new Rect(0, 0.056f, 1, 0.888f);
                
                if (MainControl.Instance.sceneState == MainControl.SceneState.InBattle)
                    if (MainControl.Instance.cameraMainInBattle)
                        MainControl.Instance.cameraMainInBattle.rect = new Rect(0, 0.056f, 1, 0.888f);

                if (BackpackBehaviour.Instance)
                    BackpackBehaviour.Instance.SuitResolution();

                CanvasController.Instance.DOKill();

                if (CanvasController.Instance.framePic < 0)
                {
                    CanvasController.Instance.frame.color = Color.black;
                    CanvasController.Instance.fps.rectTransform.anchoredPosition = new Vector2(0, -30f);
                }
                else 
                    CanvasController.Instance.fps.rectTransform.anchoredPosition = new Vector2();

                CanvasController.Instance.frame.DOColor(new Color(1, 1, 1, 1) * Convert.ToInt32(CanvasController.Instance.framePic >= 0), 1f);
                CanvasController.Instance.setting.transform.localScale = Vector3.one * 0.89f;
                CanvasController.Instance.setting.rectTransform.anchoredPosition = new Vector2(142.5f, CanvasController.Instance.setting.rectTransform.anchoredPosition.y);
            }

            
            var resolutionHeights = new List<int> { 480, 768, 864, 960, 1080, 540, 1080 };
            var resolutionWidths = MathUtilityService.GetResolutionWidthsWithHeights(resolutionHeights, 5);

            var currentResolutionWidth = resolutionWidths[resolution];
            var currentResolutionHeight = resolutionHeights[resolution];
            
            Screen.SetResolution(currentResolutionWidth, currentResolutionHeight, MainControl.Instance.OverworldControl.fullScreen);
            MainControl.Instance.OverworldControl.resolution = new Vector2(currentResolutionWidth, currentResolutionHeight);
        }

        public static void SwitchScene(string sceneName, bool async = true)
        {
            SetCanvasFrameSprite();
            if (SceneManager.GetActiveScene().name != "Menu" && SceneManager.GetActiveScene().name != "Rename" && SceneManager.GetActiveScene().name != "Story" && SceneManager.GetActiveScene().name != "Start" && SceneManager.GetActiveScene().name != "Gameover")
                MainControl.Instance.PlayerControl.lastScene = SceneManager.GetActiveScene().name;
            if (async)
                SceneManager.LoadSceneAsync(sceneName);
            else SceneManager.LoadScene(sceneName);

            SetResolution(MainControl.Instance.OverworldControl.resolutionLevel);
            MainControl.Instance.isSceneSwitching = false;
        }

        public static void FadeOutToWhiteAndSwitchScene(string scene)
        {
            MainControl.Instance.isSceneSwitching = true;
            MainControl.Instance.inOutBlack.color = new Color(1, 1, 1, 0);
            AudioController.Instance.GetFx(6, MainControl.Instance.AudioControl.fxClipUI);
            MainControl.Instance.inOutBlack.DOColor(Color.white, 5.5f).SetEase(Ease.Linear).OnKill(() => SwitchScene(scene));
        }

        /// <summary>
        /// 淡出并切换场景。
        /// </summary>
        /// <param name="scene">要切换到的场景名称</param>
        /// <param name="fadeColor">淡出的颜色</param>
        /// <param name="isBgmMuted">是否静音背景音乐</param>
        /// <param name="fadeTime">淡出时间，默认为0.5秒</param>
        /// <param name="isAsync">是否异步切换场景，默认为true</param>
        public static void FadeOutAndSwitchScene(string scene, Color fadeColor, bool isBgmMuted = false, float fadeTime = 0.5f, bool isAsync = true)
        {
            MainControl.Instance.isSceneSwitching = true;
            if (isBgmMuted)
            {
                var bgm = AudioController.Instance.transform.GetComponent<AudioSource>();
                switch (fadeTime)
                {
                    case > 0:
                        DOTween.To(() => bgm.volume, x => bgm.volume = x, 0, fadeTime).SetEase(Ease.Linear);
                        break;
                    case 0:
                        bgm.volume = 0;
                        break;
                    default:
                        DOTween.To(() => bgm.volume, x => bgm.volume = x, 0, Mathf.Abs(fadeTime)).SetEase(Ease.Linear);
                        break;
                }
            }
            MainControl.Instance.OverworldControl.pause = true;
            switch (fadeTime)
            {
                case > 0:
                {
                    MainControl.Instance.inOutBlack.DOColor(fadeColor, fadeTime).SetEase(Ease.Linear).OnKill(() => GameUtilityService.SwitchScene(scene));
                    if (!MainControl.Instance.OverworldControl.hdResolution)
                        CanvasController.Instance.frame.color = new Color(1, 1, 1, 0);
                    break;
                }
                case 0:
                    MainControl.Instance.inOutBlack.color = fadeColor;
                    GameUtilityService.SwitchScene(scene, isAsync);
                    break;
                default:
                {
                    fadeTime = Mathf.Abs(fadeTime);
                    MainControl.Instance.inOutBlack.color = fadeColor;
                    MainControl.Instance.inOutBlack.DOColor(fadeColor, fadeTime).SetEase(Ease.Linear).OnKill(() => GameUtilityService.SwitchScene(scene));
                    if (!MainControl.Instance.OverworldControl.hdResolution)
                        CanvasController.Instance.frame.color = new Color(1, 1, 1, 0);
                    break;
                }
            }
        }

        /// <summary>
        /// 更改分辨率
        /// </summary>
        public static void UpdateResolutionSettings()
        {
            if (!MainControl.Instance.OverworldControl.hdResolution)
            {
                if (MainControl.Instance.OverworldControl.resolutionLevel is >= 0 and < 4)
                    MainControl.Instance.OverworldControl.resolutionLevel += 1;
                else
                    MainControl.Instance.OverworldControl.resolutionLevel = 0;
            }
            else
            {
                if (MainControl.Instance.OverworldControl.resolutionLevel is >= 5 and < 6)
                    MainControl.Instance.OverworldControl.resolutionLevel += 1;
                else
                    MainControl.Instance.OverworldControl.resolutionLevel = 5;
            }

            SetResolution(MainControl.Instance.OverworldControl.resolutionLevel);
        }

        /// <summary>
        /// 开/关 SFX
        /// </summary>
        public static void ToggleAllSfx(bool isClose)
        {
            foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(Light2D)))
            {
                var light2D = (Light2D)obj;
                light2D.enabled = !isClose;
            }
            
            MainControl.Instance.mainCamera.GetUniversalAdditionalCameraData().renderPostProcessing = !isClose;

            if (MainControl.Instance.sceneState != MainControl.SceneState.InBattle) return;
            
            if (!MainControl.Instance.cameraMainInBattle)
            {
                if (!MainControl.Instance.cameraShake)
                    MainControl.Instance.cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
                MainControl.Instance.cameraMainInBattle = MainControl.Instance.cameraShake.GetComponent<Camera>();
            }
            MainControl.Instance.  cameraMainInBattle.GetUniversalAdditionalCameraData().renderPostProcessing = !isClose;
        }

        /// <summary>
        /// 传入使用背包的哪个物体
        /// 然后就使用 打true会顺带把背包顺序整理下
        /// 然后再让打字机打个字
        /// plusText填0就自己计算
        /// </summary>
        public static void UseItem(TypeWritter typeWritter, TMP_Text tmpText, int sonSelect, int plusText = 0)
        {
            if (plusText == 0)
            {
                plusText = MainControl.Instance.PlayerControl.myItems[sonSelect - 1] switch
                {
                    >= 20000 => -20000 + MainControl.Instance.ItemControl.itemFoods.Count / 3 + MainControl.Instance.ItemControl.itemArms.Count / 2,
                    >= 10000 => -10000 + MainControl.Instance.ItemControl.itemFoods.Count / 3,
                    _ => 0
                };
            }

            switch (MainControl.Instance.PlayerControl.myItems[sonSelect - 1])
            {
                case >= 20000:// 防具
                    ProcessArmor(typeWritter, tmpText, sonSelect, plusText);
                    break;
                case >= 10000:// 武器
                    ProcessArm(typeWritter, tmpText, sonSelect, plusText);
                    break;
                // 食物
                default:
                {
                    ProcessFood(typeWritter, tmpText, sonSelect);
                    break;
                }
            }
        }

        private static void ProcessFood(TypeWritter typeWritter, TMP_Text tmpText, int sonSelect)
        {
            var plusHp = int.Parse(DataHandlerService.ItemIdGetName(MainControl.Instance.ItemControl, MainControl.Instance.PlayerControl.myItems[sonSelect - 1], "Auto", 2));
            if (MainControl.Instance.PlayerControl.wearArm == 10001)
                plusHp += 4;

            typeWritter.TypeOpen(MainControl.Instance.ItemControl.itemTextMaxItemSon[MainControl.Instance.PlayerControl.myItems[sonSelect - 1] * 5 - 3], false,
                plusHp, 0, tmpText);

            MainControl.Instance. PlayerControl.hp += plusHp;

            if (MainControl.Instance.PlayerControl.hp > MainControl.Instance.PlayerControl.hpMax)
                MainControl.Instance.PlayerControl.hp = MainControl.Instance.PlayerControl.hpMax;
            for (var i = 0; i < MainControl.Instance.ItemControl.itemFoods.Count; i++)
            {
                if (MainControl.Instance.ItemControl.itemTextMaxItemSon[MainControl.Instance.PlayerControl.myItems[sonSelect - 1] * 5 - 5] !=
                    MainControl.Instance.ItemControl.itemFoods[i]) continue;
                var text = MainControl.Instance.ItemControl.itemFoods[i + 1];
                text = text.Substring(1, text.Length - 1);
                MainControl.Instance. PlayerControl.myItems[sonSelect - 1] = DataHandlerService.ItemNameGetId(MainControl.Instance.ItemControl, text, "Foods");
                break;
            }
            AudioController.Instance.GetFx(2, MainControl.Instance.AudioControl.fxClipUI);
        }

        private static void ProcessArm(TypeWritter typeWritter, TMP_Text tmpText, int sonSelect, int plusText)
        {
            typeWritter.TypeOpen(MainControl.Instance.ItemControl.itemTextMaxItemSon[(MainControl.Instance.PlayerControl.myItems[sonSelect - 1] + plusText) * 5 - 3], false, 0, 0, tmpText);
            MainControl.Instance.PlayerControl.wearAtk = int.Parse(DataHandlerService.ItemIdGetName(MainControl.Instance.ItemControl, MainControl.Instance.PlayerControl.myItems[sonSelect - 1], "Auto", 1));
            (MainControl.Instance.PlayerControl.wearArm, MainControl.Instance.PlayerControl.myItems[sonSelect - 1]) = (MainControl.Instance.PlayerControl.myItems[sonSelect - 1],MainControl.Instance. PlayerControl.wearArm);

            AudioController.Instance.GetFx(3, MainControl.Instance.AudioControl.fxClipUI);
        }

        private static void ProcessArmor(TypeWritter typeWritter, TMP_Text tmpText, int sonSelect, int plusText)
        {
            typeWritter.TypeOpen(MainControl.Instance.ItemControl.itemTextMaxItemSon[(MainControl.Instance.PlayerControl.myItems[sonSelect - 1] + plusText) * 5 - 3], false, 0, 0, tmpText);
            MainControl.Instance.PlayerControl.wearDef = int.Parse(DataHandlerService.ItemIdGetName(MainControl.Instance.ItemControl, MainControl.Instance.PlayerControl.myItems[sonSelect - 1], "Auto", 1));
            (MainControl.Instance.PlayerControl.wearArmor, MainControl.Instance.PlayerControl.myItems[sonSelect - 1]) = (MainControl.Instance.PlayerControl.myItems[sonSelect - 1], MainControl.Instance.PlayerControl.wearArmor);

            AudioController.Instance.GetFx(3, MainControl.Instance.AudioControl.fxClipUI);
        }

        /// <summary>
        /// 传入默认KeyCode并转换为游戏内键位。
        /// mode:0按下 1持续 2抬起
        /// </summary>
        public static bool KeyArrowToControl(KeyCode key, int mode = 0)
        {
            var overworldControl = MainControl.Instance.OverworldControl;
            return mode switch
            {
                0 => key switch
                {
                    KeyCode.DownArrow => Input.GetKeyDown(overworldControl.keyCodes[0]) || Input.GetKeyDown(overworldControl.keyCodesBack1[0]) || Input.GetKeyDown(overworldControl.keyCodesBack2[0]),
                    KeyCode.RightArrow => Input.GetKeyDown(overworldControl.keyCodes[1]) || Input.GetKeyDown(overworldControl.keyCodesBack1[1]) || Input.GetKeyDown(overworldControl.keyCodesBack2[1]),
                    KeyCode.UpArrow => Input.GetKeyDown(overworldControl.keyCodes[2]) || Input.GetKeyDown(overworldControl.keyCodesBack1[2]) || Input.GetKeyDown(overworldControl.keyCodesBack2[2]),
                    KeyCode.LeftArrow => Input.GetKeyDown(overworldControl.keyCodes[3]) || Input.GetKeyDown(overworldControl.keyCodesBack1[3]) || Input.GetKeyDown(overworldControl.keyCodesBack2[3]),
                    KeyCode.Z => Input.GetKeyDown(overworldControl.keyCodes[4]) || Input.GetKeyDown(overworldControl.keyCodesBack1[4]) || Input.GetKeyDown(overworldControl.keyCodesBack2[4]),
                    KeyCode.X => Input.GetKeyDown(overworldControl.keyCodes[5]) || Input.GetKeyDown(overworldControl.keyCodesBack1[5]) || Input.GetKeyDown(overworldControl.keyCodesBack2[5]),
                    KeyCode.C => Input.GetKeyDown(overworldControl.keyCodes[6]) || Input.GetKeyDown(overworldControl.keyCodesBack1[6]) || Input.GetKeyDown(overworldControl.keyCodesBack2[6]),
                    KeyCode.V => Input.GetKeyDown(overworldControl.keyCodes[7]) || Input.GetKeyDown(overworldControl.keyCodesBack1[7]) || Input.GetKeyDown(overworldControl.keyCodesBack2[7]),
                    KeyCode.F4 => Input.GetKeyDown(overworldControl.keyCodes[8]) || Input.GetKeyDown(overworldControl.keyCodesBack1[8]) || Input.GetKeyDown(overworldControl.keyCodesBack2[8]),
                    KeyCode.Tab => Input.GetKeyDown(overworldControl.keyCodes[9]) || Input.GetKeyDown(overworldControl.keyCodesBack1[9]) || Input.GetKeyDown(overworldControl.keyCodesBack2[9]),
                    KeyCode.Semicolon => Input.GetKeyDown(overworldControl.keyCodes[10]) || Input.GetKeyDown(overworldControl.keyCodesBack1[10]) || Input.GetKeyDown(overworldControl.keyCodesBack2[10]),
                    KeyCode.Escape => Input.GetKeyDown(overworldControl.keyCodes[11]) || Input.GetKeyDown(overworldControl.keyCodesBack1[11]) || Input.GetKeyDown(overworldControl.keyCodesBack2[11]),
                    _ => false,
                },
                1 => key switch
                {
                    KeyCode.DownArrow => Input.GetKey(overworldControl.keyCodes[0]) || Input.GetKey(overworldControl.keyCodesBack1[0]) || Input.GetKey(overworldControl.keyCodesBack2[0]),
                    KeyCode.RightArrow => Input.GetKey(overworldControl.keyCodes[1]) || Input.GetKey(overworldControl.keyCodesBack1[1]) || Input.GetKey(overworldControl.keyCodesBack2[1]),
                    KeyCode.UpArrow => Input.GetKey(overworldControl.keyCodes[2]) || Input.GetKey(overworldControl.keyCodesBack1[2]) || Input.GetKey(overworldControl.keyCodesBack2[2]),
                    KeyCode.LeftArrow => Input.GetKey(overworldControl.keyCodes[3]) || Input.GetKey(overworldControl.keyCodesBack1[3]) || Input.GetKey(overworldControl.keyCodesBack2[3]),
                    KeyCode.Z => Input.GetKey(overworldControl.keyCodes[4]) || Input.GetKey(overworldControl.keyCodesBack1[4]) || Input.GetKey(overworldControl.keyCodesBack2[4]),
                    KeyCode.X => Input.GetKey(overworldControl.keyCodes[5]) || Input.GetKey(overworldControl.keyCodesBack1[5]) || Input.GetKey(overworldControl.keyCodesBack2[5]),
                    KeyCode.C => Input.GetKey(overworldControl.keyCodes[6]) || Input.GetKey(overworldControl.keyCodesBack1[6]) || Input.GetKey(overworldControl.keyCodesBack2[6]),
                    KeyCode.V => Input.GetKey(overworldControl.keyCodes[7]) || Input.GetKey(overworldControl.keyCodesBack1[7]) || Input.GetKey(overworldControl.keyCodesBack2[7]),
                    KeyCode.F4 => Input.GetKey(overworldControl.keyCodes[8]) || Input.GetKey(overworldControl.keyCodesBack1[8]) || Input.GetKey(overworldControl.keyCodesBack2[8]),
                    KeyCode.Tab => Input.GetKey(overworldControl.keyCodes[9]) || Input.GetKey(overworldControl.keyCodesBack1[9]) || Input.GetKey(overworldControl.keyCodesBack2[9]),
                    KeyCode.Semicolon => Input.GetKey(overworldControl.keyCodes[10]) || Input.GetKey(overworldControl.keyCodesBack1[10]) || Input.GetKey(overworldControl.keyCodesBack2[10]),
                    KeyCode.Escape => Input.GetKey(overworldControl.keyCodes[11]) || Input.GetKey(overworldControl.keyCodesBack1[11]) || Input.GetKey(overworldControl.keyCodesBack2[11]),
                    _ => false,
                },
                2 => key switch
                {
                    KeyCode.DownArrow => Input.GetKeyUp(overworldControl.keyCodes[0]) || Input.GetKeyUp(overworldControl.keyCodesBack1[0]) || Input.GetKeyUp(overworldControl.keyCodesBack2[0]),
                    KeyCode.RightArrow => Input.GetKeyUp(overworldControl.keyCodes[1]) || Input.GetKeyUp(overworldControl.keyCodesBack1[1]) || Input.GetKeyUp(overworldControl.keyCodesBack2[1]),
                    KeyCode.UpArrow => Input.GetKeyUp(overworldControl.keyCodes[2]) || Input.GetKeyUp(overworldControl.keyCodesBack1[2]) || Input.GetKeyUp(overworldControl.keyCodesBack2[2]),
                    KeyCode.LeftArrow => Input.GetKeyUp(overworldControl.keyCodes[3]) || Input.GetKeyUp(overworldControl.keyCodesBack1[3]) || Input.GetKeyUp(overworldControl.keyCodesBack2[3]),
                    KeyCode.Z => Input.GetKeyUp(overworldControl.keyCodes[4]) || Input.GetKeyUp(overworldControl.keyCodesBack1[4]) || Input.GetKeyUp(overworldControl.keyCodesBack2[4]),
                    KeyCode.X => Input.GetKeyUp(overworldControl.keyCodes[5]) || Input.GetKeyUp(overworldControl.keyCodesBack1[5]) || Input.GetKeyUp(overworldControl.keyCodesBack2[5]),
                    KeyCode.C => Input.GetKeyUp(overworldControl.keyCodes[6]) || Input.GetKeyUp(overworldControl.keyCodesBack1[6]) || Input.GetKeyUp(overworldControl.keyCodesBack2[6]),
                    KeyCode.V => Input.GetKeyUp(overworldControl.keyCodes[7]) || Input.GetKeyUp(overworldControl.keyCodesBack1[7]) || Input.GetKeyUp(overworldControl.keyCodesBack2[7]),
                    KeyCode.F4 => Input.GetKeyUp(overworldControl.keyCodes[8]) || Input.GetKeyUp(overworldControl.keyCodesBack1[8]) || Input.GetKeyUp(overworldControl.keyCodesBack2[8]),
                    KeyCode.Tab => Input.GetKeyUp(overworldControl.keyCodes[9]) || Input.GetKeyUp(overworldControl.keyCodesBack1[9]) || Input.GetKeyUp(overworldControl.keyCodesBack2[9]),
                    KeyCode.Semicolon => Input.GetKeyUp(overworldControl.keyCodes[10]) || Input.GetKeyUp(overworldControl.keyCodesBack1[10]) || Input.GetKeyUp(overworldControl.keyCodesBack2[10]),
                    KeyCode.Escape => Input.GetKeyUp(overworldControl.keyCodes[11]) || Input.GetKeyUp(overworldControl.keyCodesBack1[11]) || Input.GetKeyUp(overworldControl.keyCodesBack2[11]),
                    _ => false,
                },
                _ => false,
            };
        }

        /// <summary>
        /// 应用默认键位
        /// </summary>
        public static void ApplyDefaultControl()
        {
            var overworldControl = MainControl.Instance.OverworldControl;
            overworldControl.keyCodes = new List<KeyCode>
            {
                KeyCode.DownArrow,
                KeyCode.RightArrow,
                KeyCode.UpArrow,
                KeyCode.LeftArrow,
                KeyCode.Z,
                KeyCode.X,
                KeyCode.C,
                KeyCode.V,
                KeyCode.F4,
                KeyCode.None,
                KeyCode.None,
                KeyCode.Escape
            };

            overworldControl.keyCodesBack1 = new List<KeyCode>
            {
                KeyCode.S,
                KeyCode.D,
                KeyCode.W,
                KeyCode.A,
                KeyCode.Return,
                KeyCode.RightShift,
                KeyCode.RightControl,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None
            };
            overworldControl.keyCodesBack2 = new List<KeyCode>
            {
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.LeftShift,
                KeyCode.LeftControl,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None
            };
        }

        public static Color GetRandomColor()
        {
            return new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 1);
        }

        /// <summary>
        /// 控制节拍器
        /// </summary>
        public static void Metronome()
        {
            if (MainControl.Instance.beatTimes.Count <= 0) return;

            var firstIn = true;
            while (MainControl.Instance.currentBeatIndex < MainControl.Instance.beatTimes.Count && AudioController.Instance.audioSource.time >= MainControl.Instance.nextBeatTime)
            {
                if (firstIn)
                {
                    AudioController.Instance.GetFx(MainControl.Instance.currentBeatIndex % 4 == 0 ? 13 : 14, MainControl.Instance.AudioControl.fxClipUI);
                }
                MainControl.Instance. currentBeatIndex++;

                if (MainControl.Instance.currentBeatIndex < MainControl.Instance.beatTimes.Count)
                {
                    MainControl.Instance.nextBeatTime = MainControl.Instance.beatTimes[MainControl.Instance.currentBeatIndex];
                }
                firstIn = false;
            }

            if (MainControl.Instance.currentBeatIndex <MainControl.Instance. beatTimes.Count) return;
            MainControl.Instance. nextBeatTime =MainControl.Instance. beatTimes[0];
            MainControl.Instance.   currentBeatIndex = 0;
        }
    }
}