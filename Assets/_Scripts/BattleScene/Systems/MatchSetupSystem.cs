using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private HeroData heroData;
    [SerializeField] private PerkData perkData;
    [SerializeField] private List<EnemyData> enemyDatas;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<WinGameGA>(WinGameGAPerformer);
        ActionSystem.AttachPerformer<LoseGameGA>(LoseGameGAPerformer);
        ActionSystem.AttachPerformer<SetupMatchGA>(SetupMatchGAPerformer);
        ActionSystem.AttachPerformer<SceneChangeGA>(SceneChangeGAPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<WinGameGA>();
        ActionSystem.DetachPerformer<LoseGameGA>();
        ActionSystem.DetachPerformer<SetupMatchGA>();
        ActionSystem.DetachPerformer<SceneChangeGA>();
    }

    private void Start()
    {
        DOTween.Clear(true);
        TimeSystem.Instance.AddAction(new SetupMatchGA(), 0f);
    }

    public void SetupMatch()
    {
        HeroSystem.Instance.Setup(heroData);
        EnemySystem.Instance.Setup(enemyDatas);
        CardSystem.Instance.Setup(heroData.Deck);
        ManaSystem.Instance.Setup();

        TimeSystem.Instance.AddAction(new DrawCardsGA(10), 0f);
    }

    private IEnumerator WinGameGAPerformer(WinGameGA winGameGA)
    {
        TimeSystem.Instance.ClearAllActions();
        CardSystem.Instance.ResetCards();
        EnemySystem.Instance.ResetEnemies();
        Interactions.Instance.Reset();
        ManaSystem.Instance.Reset();

        TimeSystem.Instance.AddAction(new ResumeTimeGA(), 0f);
        TimeSystem.Instance.AddAction(new SceneChangeGA("GameVictoryScene"), 0.1f);

        yield return null;
    }

    private IEnumerator LoseGameGAPerformer(LoseGameGA loseGameGA)
    {
        TimeSystem.Instance.ClearAllActions();
        CardSystem.Instance.ResetCards();
        EnemySystem.Instance.ResetEnemies();
        Interactions.Instance.Reset();
        ManaSystem.Instance.Reset();

        TimeSystem.Instance.AddAction(new ResumeTimeGA(), 0f);
        TimeSystem.Instance.AddAction(new SceneChangeGA("GameDefeatScene"), 0.1f);

        yield return null;
    }

    private IEnumerator SceneChangeGAPerformer(SceneChangeGA sceneChangeGA)
    {
        yield return new WaitForSeconds(0.3f);
        DOTween.KillAll();
        SceneManager.LoadScene(sceneChangeGA.SceneName);
    }

    private IEnumerator SetupMatchGAPerformer(SetupMatchGA setupMatchGA)
    {
        SetupMatch();
        yield return null;
    }
}
