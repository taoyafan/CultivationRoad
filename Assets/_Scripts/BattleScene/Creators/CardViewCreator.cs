using DG.Tweening;
using UnityEngine;

public class CardViewCreator : Singleton<CardViewCreator>
{
    [SerializeField] private CardView _cardViewPrefab;

    public CardView CreateCardView(Card card, Vector3 pos, Quaternion rot, float scale)
    {
        if (_cardViewPrefab == null) return null;
        CardView cardView = Instantiate(_cardViewPrefab, pos, rot);
        cardView.transform.localScale = Vector3.zero;
        cardView.transform.DOScale(Vector3.one * scale, 0.15f);
        cardView.SetCard(card);
        card.CardView = cardView;
        cardView.DisableOutline();
        cardView.HideCastingEffect();
        return cardView;
    }

    public void DestroyCardView(CardView cardView)
    {
        if (cardView != null)
        {
            Destroy(cardView.gameObject);
        }
    }
}
