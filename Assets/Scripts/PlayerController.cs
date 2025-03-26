using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : UserData
{
    public CharacterController characterButton;
    //public BloodController bloodController;
    public Scoring scoring;
    public string answer = string.Empty;
    public bool IsCorrect = false;
    public bool IsTriggerToNextQuestion = false;
    public bool IsCheckedAnswer = false;
    public Image answerBoxFrame;
    public float speed;
    [HideInInspector]
    public Transform characterTransform;
    [HideInInspector]
    public Vector3 startPosition = Vector3.zero;
    private CharacterAnimation characterAnimation = null;
    public List<Cell> collectedCell = new List<Cell>();
    public float countGetAnswerAtStartPoints = 2f;
    private float countAtStartPoints = 0f;

    public RectTransform rectTransform = null;
    public Vector3 playerCurrentPosition = Vector3.zero;
    public float reduceBaseFactor = 0.93f;
    public float resetCount = 5.0f;

    public void Init(CharacterSet characterSet = null, Sprite[] defaultAnswerBoxes = null, Vector3 startPos = default)
    {
        /*if(LoaderConfig.Instance.gameSetup.playersMovingSpeed > 0f)
        {
            this.moveSpeed = LoaderConfig.Instance.gameSetup.playersMovingSpeed;
        }

        if(LoaderConfig.Instance.gameSetup.playersRotationSpeed > 0f)
        {
            this.rotationSpeed = LoaderConfig.Instance.gameSetup.playersRotationSpeed;
        }*/
    
        this.countAtStartPoints = this.countGetAnswerAtStartPoints;
        this.updateRetryTimes(false);
        this.startPosition = startPos;
        this.characterTransform = this.transform;
        this.characterTransform.localPosition = this.startPosition;
        this.characterAnimation = this.GetComponent<CharacterAnimation>();
        this.characterAnimation.characterSet = characterSet;

        if (this.characterButton == null)
        {
            this.characterButton = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "-controller").GetComponent<CharacterController>();
            this.characterButton.playerController = this;
        }

        if (this.PlayerIcons[0] == null)
        {
            this.PlayerIcons[0] = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_Icon").GetComponent<PlayerIcon>();
        }

        if (this.scoring.scoreTxt == null)
        {
            this.scoring.scoreTxt = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_Score").GetComponent<TextMeshProUGUI>();
        }

        if (this.scoring.answeredEffectTxt == null)
        {
            this.scoring.answeredEffectTxt = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_AnswerScore").GetComponent<TextMeshProUGUI>();
        }

        if (this.scoring.resultScoreTxt == null)
        {
            this.scoring.resultScoreTxt = GameObject.FindGameObjectWithTag("P" + this.RealUserId + "_ResultScore").GetComponent<TextMeshProUGUI>();
        }

        this.scoring.init();
    }

    void updateRetryTimes(bool deduct = false)
    {
        if (deduct)
        {
            if (this.Retry > 0)
            {
                this.Retry--;
            }

            /*if (this.bloodController != null)
            {
                this.bloodController.setBloods(false);
            }*/
        }
        else
        {
            this.NumberOfRetry = LoaderConfig.Instance.gameSetup.retry_times;
            this.Retry = this.NumberOfRetry;
        }
    }

    public void updatePlayerIcon(bool _status = false, string _playerName = "", Sprite _icon = null)
    {
        for (int i = 0; i < this.PlayerIcons.Length; i++)
        {
            if (this.PlayerIcons[i] != null)
            {
                this.PlayerColor = this.characterAnimation.characterSet.playerColor;
                this.PlayerIcons[i].playerColor = this.characterAnimation.characterSet.playerColor;
                this.PlayerIcons[i].SetStatus(_status, _playerName, _icon);
            }
        }

    }


    string CapitalizeFirstLetter(string str)
    {
        if (string.IsNullOrEmpty(str)) return str; // Return if the string is empty or null
        return char.ToUpper(str[0]) + str.Substring(1).ToLower();
    }

    public void checkAnswer(int currentTime, Action onCompleted = null)
    {
        var currentQuestion = QuestionController.Instance?.currentQuestion;
        if(currentQuestion.answersChoics == null || currentQuestion.answersChoics.Length == 0) return;

        var getChoice = this.answer;
        var lowerQIDAns = currentQuestion.correctAnswer.ToLower();
        switch (getChoice)
        {
            case "A":
                this.answer = currentQuestion.answersChoics[0].ToLower();
                break;
            case "B":
                this.answer = currentQuestion.answersChoics[1].ToLower();
                break;
            case "C":
                this.answer = currentQuestion.answersChoics[2].ToLower();
                break;
            case "D":
                this.answer = currentQuestion.answersChoics[3].ToLower();
                break;
        }

        if (!this.IsCheckedAnswer)
        {
            this.IsCheckedAnswer = true;
            var loader = LoaderConfig.Instance;
            int eachQAScore = currentQuestion.qa.score.full == 0 ? 10 : currentQuestion.qa.score.full;
            int currentScore = this.Score;

            int resultScore = this.scoring.score(this.answer, currentScore, lowerQIDAns, eachQAScore);
            this.Score = resultScore;
            this.IsCorrect = this.scoring.correct;
            StartCoroutine(this.showAnswerResult(this.scoring.correct,()=>
            {
                if (this.UserId == 0 && loader != null && loader.apiManager.IsLogined) // For first player
                {
                    float currentQAPercent = 0f;
                    int correctId = 0;
                    float score = 0f;
                    float answeredPercentage;
                    int progress = (int)((float)currentQuestion.answeredQuestion / QuestionManager.Instance.totalItems * 100);

                    if (this.answer == lowerQIDAns)
                    {
                        if (this.CorrectedAnswerNumber < QuestionManager.Instance.totalItems)
                            this.CorrectedAnswerNumber += 1;

                        correctId = 2;
                        score = eachQAScore; // load from question settings score of each question

                        LogController.Instance?.debug("Each QA Score!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" + eachQAScore + "______answer" + this.answer);
                        currentQAPercent = 100f;
                    }
                    else
                    {
                        if (this.CorrectedAnswerNumber > 0)
                        {
                            this.CorrectedAnswerNumber -= 1;
                        }
                    }

                    if (this.CorrectedAnswerNumber < QuestionManager.Instance.totalItems)
                    {
                        answeredPercentage = this.AnsweredPercentage(QuestionManager.Instance.totalItems);
                    }
                    else
                    {
                        answeredPercentage = 100f;
                    }

                    loader.SubmitAnswer(
                               currentTime,
                               this.Score,
                               answeredPercentage,
                               progress,
                               correctId,
                               currentTime,
                               currentQuestion.qa.qid,
                               currentQuestion.correctAnswerId,
                               this.CapitalizeFirstLetter(this.answer),
                               currentQuestion.correctAnswer,
                               score,
                               currentQAPercent,
                               onCompleted
                               );
                }
                else
                {
                   onCompleted?.Invoke();
                }
            }, ()=>
            {
                this.IsCheckedAnswer = false;
                onCompleted?.Invoke();
            }));
        }
    }

    public void resetRetryTime()
    {
        this.scoring.resetText();
        this.updateRetryTimes(false);
       // this.bloodController.setBloods(true);
        this.IsTriggerToNextQuestion = false;
    }

    public IEnumerator showAnswerResult(bool correct, Action onCorrectCompleted = null, Action onFailureCompleted = null)
    {
        float delay = 2f;
        if (correct)
        {
            GameController.Instance?.PrepareNextQuestion();
            LogController.Instance?.debug("Add marks" + this.Score);
            GameController.Instance?.setGetScorePopup(true);
            AudioController.Instance?.PlayAudio(1);
            onCorrectCompleted?.Invoke();
            yield return new WaitForSeconds(delay);
            GameController.Instance?.setGetScorePopup(false);
            GameController.Instance?.UpdateNextQuestion();
        }
        else
        {
            GameController.Instance?.setWrongPopup(true);
            AudioController.Instance?.PlayAudio(2);
            this.updateRetryTimes(true);
            yield return new WaitForSeconds(delay);
            GameController.Instance?.setWrongPopup(false);
            if (this.Retry <= 0)
            {
                this.IsTriggerToNextQuestion = true;
            }
            onFailureCompleted?.Invoke();
        }
        this.scoring.correct = false;
    }

    public void characterReset(Vector3 newStartPostion)
    {
        this.startPosition = newStartPostion;
        this.characterTransform.localPosition = this.startPosition;
    }


    public void playerReset(Vector3 newStartPostion)
    {             
        this.answer = "";
        this.IsCheckedAnswer = false;
        this.IsCorrect = false;
        this.resetCount = 2.0f;
        this.collectedCell.Clear();
    }
    
    public void autoDeductAnswer()
    {
        if(this.collectedCell.Count > 0) {
            if (this.countAtStartPoints > 0f)
            {
                this.countAtStartPoints -= Time.deltaTime;
            }
            else
            {
                this.answer = "";
                this.countAtStartPoints = this.countGetAnswerAtStartPoints;
            }
        }
        else
        {
            this.countAtStartPoints = this.countGetAnswerAtStartPoints;
        }
    }

    
    /*private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the other collider has a specific tag, e.g., "Player"
        if (other.CompareTag("Word"))
        {
            var cell = other.GetComponent<Cell>();
            if (cell != null)
            {
                cell.setCellEnterColor(true, GameController.Instance.showCells);
                if (cell.isSelected && this.Retry > 0)
                {
                    //LogController.Instance.debug("Player has entered the trigger!" + other.name);
                    AudioController.Instance?.PlayAudio(9);

                    var gridManager = GameController.Instance.gridManager;
                    if (gridManager.isMCType){
                        if (this.collectedCell.Count > 0)
                        {
                            var latestCell = this.collectedCell[this.collectedCell.Count - 1];
                            //latestCell.SetTextStatus(true);
                            gridManager.updateNewWordPosition(latestCell);
                            this.collectedCell.RemoveAt(this.collectedCell.Count - 1);
                        }
                    }
                    this.setAnswer(cell.content.text);
                    this.collectedCell.Add(cell);
                    cell.SetTextStatus(false);
                    this.characterStatus = CharacterStatus.idling;
                    var gameTimer = GameController.Instance.gameTimer;
                    int currentTime = Mathf.FloorToInt(((gameTimer.gameDuration - gameTimer.currentTime) / gameTimer.gameDuration) * 100);
                    this.checkAnswer(currentTime);
                }
            }
        }
        else if (other.CompareTag("Wall"))
        {
            this.ReBornCharacter();
        }
    }

    void StopCharacter()
    {
        this.rb.velocity = Vector2.zero;
        this.rb.angularVelocity = 0f;
        if(this.characterStatus != CharacterStatus.born) this.characterStatus = CharacterStatus.rotating;
    }

    void HoldCharacter()
    {
        this.rb.velocity = Vector2.zero;
        this.rb.angularVelocity = 0f;
    }

    void ReBornCharacter()
    {
        if (this.GetComponent<CircleCollider2D>().enabled)
        {
            SetUI.SetScale(this.answerBoxCg, false);
            AudioController.Instance?.PlayAudio(11, false, 0.5f);
            this.deductAnswer();
            this.characterStatus = CharacterStatus.recover;
            this.transform.DOScale(0f, 1f);
            this.characterButton.TriggerActive(false);
            this.GetComponent<CircleCollider2D>().enabled = false;
        }
    }*/

  /* private void OnCollisionEnter2D(Collision2D collision)
    {
        if (this.characterStatus == CharacterStatus.moving)
        {
            if (this.gameObject.name != collision.gameObject.name)
            {
                Rigidbody2D rb = collision.rigidbody;
                Vector2 relativeVelocity = collision.relativeVelocity;

                // Calculate the distance between the two objects
                float distance = Vector2.Distance(this.playerCurrentPosition, collision.transform.localPosition);

                var distanceFactor = distance / 10000f;
                this.reducedFactor = this.reduceBaseFactor + distanceFactor;
                // Apply the reduced factor
                collision.gameObject.GetComponent<PlayerController>().reducedFactor = this.reducedFactor;
                rb.angularVelocity = 0f;

                // Debug log the collision information
                LogController.Instance.debug($"Collision with: {collision.gameObject.name},distanceFactor: {distanceFactor}, Reduced Factor: {reducedFactor}, Distance: {distance}");
            }
            AudioController.Instance?.PlayAudio(10); //blob
            this.characterStatus = CharacterStatus.idling;
        }
    }



    /*private void OnCollisionExit2D(Collision2D collision)
    {
        collision.collider.enabled = true;
    }*/
}
