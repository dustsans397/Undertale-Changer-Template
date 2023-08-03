using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ��Ҫ����Overworld��������ͨ�û�������
/// </summary>
[CreateAssetMenu(fileName = "OverworldControl", menuName = "OverworldControl")]
public class OverworldControl : ScriptableObject
{
    //public int languagePack;
    public bool pause;//��������ʱ���ֹ��Ҳ������±���

    [Header("--UI--")]
    [Header("����洢")]
    public List<TMPro.TMP_FontAsset> tmpFonts;

    [Header("����ȫ���")]
    public bool textWidth;//����ȫ���
    [Header("�ֱ��ʵȼ�")]
    public int resolutionLevel;//�ֱ��ʵȼ�
    [Header("ȫ��")]
    public bool fullScreen;//ȫ������



    [Header("ȫ������")]
    public float mainVolume;//ȫ������
    [Header("����Ч")]
    public bool noSFX;//��Ч ������Ч��ʾ
    [Header("��ʾFPS")]
    public bool openFPS;//��ʾFPS
    [Header("�ֱ��ʣ���ʾ�ã�")]
    public Vector2 resolution;//�ֱ���

    [Header("�ı�����ȡ")]
    public string sceneTextsAsset;
    public List<string> sceneTextsSave;

    public string settingAsset;
    public List<string> settingSave;
    public bool isSetting;
    public List<KeyCode> keyCodes, keyCodesBack1, keyCodesBack2;//��������˳��



    [Header("��������")]
    public bool isDebug;
    [Header("--����ģʽ�趨--")]
    [Header("��Ѫ")]
    public bool invincible;


    //[Header("��Ϸ����Ҫ�浵������������д")]

}