using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Clamp : MonoBehaviour
{
    public  ClampStatus clampStatus = ClampStatus.open;
    public enum ClampStatus
    {
        open,
        clamped,
        getWord,
        collidePlayer,
        outScreen,
    }

    public string answer;
    public Cell cell;
    public CanvasGroup answerBoxCg;
    public CanvasGroup dottedLine;
    private TextMeshProUGUI answerBox = null;
    public Texture[] clampTextures;

    private void Start()
    {
        if (this.answerBoxCg != null)
        {
            this.answerBoxCg.transform.localScale = Vector3.zero;
            SetUI.SetScale(this.answerBoxCg, false);
            this.answerBox = this.answerBoxCg.GetComponentInChildren<TextMeshProUGUI>();
        }
        this.ControlDottedLine(true);
    }

    public void CongfigClampTexture(Texture[] clamps=null)
    {
        if(clamps != null)
        {
            this.clampTextures = clamps;
        }
    }

    public void ControlDottedLine(bool status)
    {
        if (this.dottedLine != null)
        {
            this.dottedLine.alpha = status ? 0.5f : 0f;
        }
    }
    private void FixedUpdate()
    {
        switch (this.clampStatus)
        {
            case ClampStatus.open:
                this.setClampSprite(0);
                break;
            case ClampStatus.clamped:
            case ClampStatus.getWord:
            case ClampStatus.outScreen:
                this.setClampSprite(1);
                break;
        }
    }

    public void resetClamp()
    {
        this.setAnswer(null);
        this.ControlDottedLine(true);
        this.clampStatus = ClampStatus.open;
    }

    void setClampSprite(int id)
    {
        var clampImg = this.transform.GetComponent<RawImage>();
        if(clampImg != null)
        {
            clampImg.texture = this.clampTextures[id];
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Word"))
        {
            var cell = other.GetComponent<Cell>();
            if (cell != null)
            {
                cell.setCellEnterColor(true, GameController.Instance.showCells);
                if (cell.isSelected && this.clampStatus == ClampStatus.open)
                {
                    this.clampStatus = ClampStatus.clamped;
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug log the collision information
        if (collision.gameObject.tag == "Clamp")
        {
           this.clampStatus = ClampStatus.collidePlayer;
           LogController.Instance.debug($"Collision with: {collision.gameObject.name}");
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
