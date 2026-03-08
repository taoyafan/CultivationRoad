using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CultivationRoad.Common
{
    public class HoverShowObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Tooltip("鼠标悬停时要显示的 GameObject")]
        public GameObject targetObject;

        [Tooltip("是否在开始时隐藏目标对象")]
        public bool hideOnStart = true;

        private void Start()
        {
            if (targetObject != null && hideOnStart)
            {
                targetObject.SetActive(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (targetObject != null)
            {
                targetObject.SetActive(false);
            }
        }
    }
}
