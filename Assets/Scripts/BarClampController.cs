using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BarClampController : MonoBehaviour
{
    public enum BarClampStatus
    {
        rotating,
        extending
    }
    public CharacterController characterController;
    private PlayerController playerController = null;
    public BarClampStatus barClampStatus = BarClampStatus.rotating;
    public Image bar;       
    public float clampOffset = -1500f;
    public Clamp clamp;
    public float fillSpeed = 0.5f;
    public float startingFill = 0.03f;
    private float targetFill = 0f;
    private bool isFilling = true;
    public float rotationSpeed = 50f;
    public float maxRotation = 50f;
    public float minRotation = -40f;
    private float currentRotation = 0f;
    private bool rotatingToMax = true;
    public BoxCollider2D barCollider;
    private AudioSource audioSource;

    public void Init(float _fillSpeed=0.5f, float _rotationSpeed=50f, Texture[] clampTextures=null)
    {
        this.fillSpeed = _fillSpeed;
        this.rotationSpeed = _rotationSpeed;
        if (this.clamp != null && clampTextures != null) 
            this.clamp.CongfigClampTexture(clampTextures);
        this.resetBarClamp();
        this.StartRotation();
        if (this.characterController != null)
        {
            this.characterController.OnPointerClickEvent += this.StartClamp;
        }
        this.audioSource = this.GetComponent<AudioSource>();
    }

    private void resetBarClamp()
    {
        this.barClampStatus = BarClampStatus.rotating;
        this.targetFill = this.startingFill;
        if(this.clamp != null) this.clamp.resetClamp();
    }

    void FixedUpdate()
    {
        if(this.bar == null || this.clamp == null)
            return;

        switch (this.barClampStatus)
        {
            case BarClampStatus.rotating:
                this.HandleRotatingState();
                break;
            case BarClampStatus.extending:
                if (this.clamp != null) this.clamp.ControlDottedLine(false);
                this.HandleExtendingState();
                break;
        }
    }

    private void HandleRotatingState()
    {
        this.StartRotation();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.barClampStatus = BarClampStatus.extending;
        }
    }

    private void HandleExtendingState()
    {
        if (this.isFilling)
        {
            this.UpdateFillAmount();
        }
        else
        {
            this.DecreaseFillAmount();
        }

        this.bar.fillAmount = targetFill;
        this.UpdateColliderSize();
        this.UpdateClampPosition();
    }

    private void UpdateFillAmount()
    {
        this.targetFill += this.fillSpeed * Time.deltaTime;
        if (this.targetFill >= 1f)
        {
            this.clamp.clampStatus = Clamp.ClampStatus.clamped;
            this.isFilling = false;
        }
        else if (this.clamp.clampStatus == Clamp.ClampStatus.outScreen ||
                 this.clamp.clampStatus == Clamp.ClampStatus.getWord ||
                 this.clamp.clampStatus == Clamp.ClampStatus.collidePlayer)
        {
            this.isFilling = false;
        }
    }

    private void DecreaseFillAmount()
    {
        this.targetFill -= this.fillSpeed * Time.deltaTime;
        if (this.targetFill <= this.startingFill)
        {
            ProcessClampReset();
        }
    }

    private void ProcessClampReset()
    {
        if (this.characterController == null)
            return;
        this.playerController = this.characterController.playerController;
        if (this.clamp.clampStatus == Clamp.ClampStatus.getWord)
        {
            GameController.Instance.checkAnswerResult(this.playerController.UserId, this.clamp);
        }
        this.SetClampExtendEffect(false);
        this.targetFill = this.startingFill;
        this.isFilling = true;
        this.clamp.resetClamp();
        this.barClampStatus = BarClampStatus.rotating;
        if (!this.playerController.IsTriggerToNextQuestion)
            this.characterController.TriggerActive(true);
    }

    public void StartClamp(BaseEventData data)
    {
        this.SetClampExtendEffect(true, true, 0.35f);
        this.barClampStatus = BarClampStatus.extending;
        this.characterController.TriggerActive(false);
    }

    public void SetClampExtendEffect(bool _play = false, bool loop = false, float volume = 1f)
    {
        if (!AudioController.Instance.audioStatus)
            return;

        if (!_play) { 
            this.audioSource.Stop();
            return;
        }

        this.audioSource.volume = volume;
        this.audioSource.Play();
        this.audioSource.loop = loop;
    }

    private void UpdateColliderSize()
    {
        if (this.barCollider != null && this.bar != null)
        {
            Vector2 barSize = this.bar.rectTransform.rect.size;
            // Calculate the new width based on the fill amount
            float newHeight = barSize.y * this.bar.fillAmount;
            this.barCollider.size = new Vector2(barSize.x, newHeight);
            this.barCollider.offset = new Vector2(0, -barSize.y / 2 + newHeight / 2);
        }
    }

    private void UpdateClampPosition()
    {
        RectTransform clampRect = this.clamp.GetComponent<RectTransform>();
        float barHeight = bar.rectTransform.rect.height;
        float clampHeight = clampRect.rect.height;
        float newY = this.bar.rectTransform.anchoredPosition.y + (barHeight * targetFill) - (clampHeight / 2) + this.clampOffset;
        clampRect.anchoredPosition = new Vector2(clampRect.anchoredPosition.x, newY);
    }

    private void StartRotation()
    {
        // Determine the target rotation based on current direction
        float targetRotation = rotatingToMax ? maxRotation : minRotation;

        // Rotate toward the target rotation
        currentRotation = Mathf.MoveTowards(currentRotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0, 0, currentRotation);

        // Check if we reached the target rotation to switch direction
        if (Mathf.Abs(currentRotation - targetRotation) < 0.01f)
        {
            rotatingToMax = !rotatingToMax; // Switch direction
        }
    }


}
