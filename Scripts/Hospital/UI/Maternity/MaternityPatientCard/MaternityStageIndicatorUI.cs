using UnityEngine;
using UnityEngine.UI;
using TMPro;
using I2.Loc;

namespace Maternity.UI
{
    public class MaternityStageIndicatorUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject checkmark = null;
        [SerializeField]
        private GameObject stageNo = null;

        [SerializeField]
        private Color highlightedColor = Color.white;
        [SerializeField]
        private Color downlightedColor = Color.white;
        
        [TermsPopup][SerializeField]
        private string highlightedFontKey = "";
        [TermsPopup][SerializeField]
        private string downlightedFontKey = "";

        [SerializeField]
        private Sprite highlightedIndicatorSprite = null;
        [SerializeField]
        private Sprite downlightedIndicatorSprite = null;

        [SerializeField]
        private Image stageIndicatorBg = null;

        [SerializeField]
        private Localize stageLabelLoc = null;

        [SerializeField]
        private TextMeshProUGUI stageLabel = null;

        public int stageID = 0;

		[SerializeField]
		private PulseAnimation pulse;

        public void SetStageIndicatorHighlighted(bool setHighlighted)
        {
            stageLabelLoc.SecondaryTerm = setHighlighted ? highlightedFontKey : downlightedFontKey;
            stageIndicatorBg.sprite = setHighlighted ? highlightedIndicatorSprite : downlightedIndicatorSprite;
            stageLabel.font = null;
            stageLabelLoc.OnLocalize();
            stageLabel.color = setHighlighted ? highlightedColor : downlightedColor;
			//pulse.enabled = true;
        }

        public void SetCheckmarkActive(bool setActive)
        {
            checkmark.SetActive(setActive);
            stageNo.SetActive(!setActive);
        }
    }
}
