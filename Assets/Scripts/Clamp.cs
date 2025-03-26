using DG.Tweening;
using TMPro;
using UnityEngine;

public class Clamp : MonoBehaviour
{
    public  ClampStatus clampStatus = ClampStatus.open;
    public enum ClampStatus
    {
        open,
        clamped,
        getWord,
        outScreen,
    }

    public string answer;
    public Cell cell;
    public CanvasGroup answerBoxCg;
    private TextMeshProUGUI answerBox = null;

    private void Start()
    {
        if (this.answerBoxCg != null)
        {
            this.answerBoxCg.transform.localScale = Vector3.zero;
            SetUI.SetScale(this.answerBoxCg, false);
            this.answerBox = this.answerBoxCg.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void resetClamp()
    {
        this.setAnswer(null);
        this.clampStatus = ClampStatus.open;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Word"))
        {
            var cell = other.GetComponent<Cell>();
            if (cell != null)
            {
                /// trigger player collide word;
                cell.setCellEnterColor(true, GameController.Instance.showCells);
                if (cell.isSelected && this.clampStatus == Clamp.ClampStatus.open)
                {
                    this.clampStatus = Clamp.ClampStatus.clamped;
                    this.setAnswer(cell);
                }
            }
        }
        else if (other.CompareTag("Wall"))
        {
            Debug.Log("Collided Wall");
            this.clampStatus = ClampStatus.outScreen;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Word"))
        {
            var cell = other.GetComponent<Cell>();
            if (cell != null)
            {
                cell.setCellEnterColor(false);
                if (cell.isSelected)
                {
                    LogController.Instance.debug("Player has exited the trigger!" + other.name);
                }
            }
        }
    }

    public void setAnswer(Cell word = null)
    {
        if (word == null)
        {
            SetUI.SetScale(this.answerBoxCg, false, 0f, 0f);
            this.cell = null;
            this.answer = "";
        }
        else
        {
            if (this.clampStatus == ClampStatus.clamped)
            {
                this.cell = word;
                var gridManager = GameController.Instance.gridManager;
                if (gridManager.isMCType)
                {
                    this.answer = word.content.text;
                }
                else
                {
                    this.answer += word.content.text;
                }
                SetUI.SetScale(this.answerBoxCg, true, 1f, 0f, Ease.OutElastic);
                word.SetTextStatus(false);
                this.clampStatus = ClampStatus.getWord;
                AudioController.Instance?.PlayAudio(9);
            }      
        }

        if (this.answerBox != null)
            this.answerBox.text = this.answer;
    }
}
