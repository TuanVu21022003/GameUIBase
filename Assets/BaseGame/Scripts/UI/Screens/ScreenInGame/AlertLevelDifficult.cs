using System;
using Core;
using Core.GamePlay;
using Core.Manager;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertLevelDifficult : MonoBehaviour
{
    [field: SerializeField] public TextMeshProUGUI TextDifficult { get; set; }
    [field: SerializeField] public Image ImageLineTop { get; set; }
    [field: SerializeField] public Image ImageLineBottom { get; set; }
    [field: SerializeField] public Image ImageWarningRight { get; set; }
    [field: SerializeField] public Image ImageWarningLeft { get; set; }
    [field: SerializeField] public Image ImageBody { get; set; }
    [field: SerializeField] public Transform Content { get; set; }
    [field: SerializeField] public Image BackGround { get; set; }
    [field: SerializeField] public Transform PosCurrent { get; set; }
    [field: SerializeField] public float alphaMaxBG { get; set; } = 0.76f;

    public AlertLevelDifficult OnInit(LevelDifficulty levelDifficulty)
    {
        LevelDifficultyData levelDifficultyData =
            ItemGlobalConfig.Instance.GetLevelDifficultyData(levelDifficulty);
        if (levelDifficultyData != null)
        {
            TextDifficult.text = levelDifficultyData.AlertLevelDifficultyData.LevelDifficultyName;
            TextDifficult.fontMaterial = levelDifficultyData.AlertLevelDifficultyData.LevelDifficultyColor;
            // TextDifficult.color = levelDifficultyData.AlertLevelDifficultyData.LevelDifficultyColor;
            ImageLineBottom.sprite = levelDifficultyData.AlertLevelDifficultyData.LineBottomAlertSprite;
            ImageLineTop.sprite = levelDifficultyData.AlertLevelDifficultyData.LineTopAlertSprite;
            ImageWarningLeft.sprite = levelDifficultyData.AlertLevelDifficultyData.WarningAlertSprite;
            ImageWarningRight.sprite = levelDifficultyData.AlertLevelDifficultyData.WarningAlertSprite;
            ImageBody.sprite = levelDifficultyData.AlertLevelDifficultyData.BodySprite;
        }

        return this;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public async UniTask TryStartMoveAnimation()
    {
        Vector3 posStart = Vector3.zero;
        Vector3 posEnd = Vector3.zero;
        if (PosCurrent != null)
        {
            posStart = PosCurrent.position + new Vector3(Screen.width, 0, 0);
            posEnd = PosCurrent.position - new Vector3(Screen.width, 0, 0);
        }

        Content.position = posStart;
        SetActive(true);
        // Đặt alpha background ban đầu về 0
        var bgColor = BackGround.color;
        bgColor.a = 0f;
        BackGround.color = bgColor;

        // 1️⃣ Fade in background + Move in
        var moveIn = LMotion.Create(0f, 1f, 0.5f)
            .WithEase(Ease.InOutQuad)
            .WithDelay(0.3f)
            .Bind(t => { Content.position = Vector3.Lerp(posStart, PosCurrent.position, t); })
            .AddTo(this)
            .ToUniTask();

        var fadeIn = LMotion.Create(0f, alphaMaxBG, 0.5f)
            .WithEase(Ease.InOutQuad)
            .Bind(a =>
            {
                var c = BackGround.color;
                c.a = a; // Giới hạn alpha tối đa, ví dụ 0.6
                BackGround.color = c;
            })
            .AddTo(this)
            .ToUniTask();

        await UniTask.WhenAll(moveIn, fadeIn);
        // AudioManager.Instance.PlaySoundFx(LevelManager.Instance.LevelConfig.LevelType == LevelDifficulty.Hard
        //     ? AudioKey.SfxUIHard
        //     : AudioKey.SfxUISuperHard);

        // 2️⃣ Dừng 2s
        await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: this.GetCancellationTokenOnDestroy());

        // 3️⃣ Fade out background + Move out
        var moveOut = LMotion.Create(0f, 1f, 0.5f)
            .WithEase(Ease.InOutQuad)
            .Bind(t => { Content.position = Vector3.Lerp(PosCurrent.position, posEnd, t); })
            .AddTo(this)
            .ToUniTask();

        var fadeOut = LMotion.Create(alphaMaxBG, 0f, 0.5f)
            .WithEase(Ease.InOutQuad)
            .WithDelay(0.3f)
            .Bind(a =>
            {
                var c = BackGround.color;
                c.a = a;
                BackGround.color = c;
            })
            .AddTo(this)
            .ToUniTask();

        await UniTask.WhenAll(moveOut, fadeOut);

        // 4️⃣ Ẩn object
        SetActive(false);
    }
}