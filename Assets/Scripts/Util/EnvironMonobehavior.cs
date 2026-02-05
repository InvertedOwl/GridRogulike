using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Util
{
    public class EnvironMonobehavior : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        public Image titleImage;
        public Image descriptionImage;
        public Image border;
        public Image divider;

        public void SetEnviron(string titleText, string descriptionText, Color color)
        {
            this.title.text = titleText;
            this.description.text = descriptionText;
            this.titleImage.color = color;
            this.descriptionImage.color = color;
            this.border.color = color;
            
            Color darker = new Color(color.r * .6f, color.g * .6f, color.b * .6f);
            this.divider.color = darker;
        }
    }
}
