using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CurrentTurnWheel : MonoBehaviour
{
    public CurrentTurnWheelSpoke SpokePF;

    public List<CurrentTurnWheelSpoke> Spokes { get; set; } = new List<CurrentTurnWheelSpoke>();

    public TurnOrderHolder TurnOrderHolder;

    private Coroutine turningAnimationCoroutine { get; set; }

    private float CurrentRotation { get; set; } = 0;
    public Transform RotationPivot;
    public float SpokeOffset = 5f;
    public float RotationSpeed = 100f;

    public void Awake()
    {
        this.TurnOrderHolder.OnTurnStarted += this.UpdateToNewTurn;
    }

    public void ResetGame()
    {
        for (int ii = this.RotationPivot.childCount - 1; ii >= 0; ii--)
        {
            Destroy(this.RotationPivot.GetChild(ii).gameObject);
        }
        this.Spokes.Clear();

        float baseRotation = -90f;
        for (int playerIndex = 0; playerIndex < this.TurnOrderHolder.PlayerCount; playerIndex++)
        {
            CurrentTurnWheelSpoke newSpoke = Instantiate(this.SpokePF, this.RotationPivot);
            this.Spokes.Add(newSpoke);

            float rotation = ((float)playerIndex / this.TurnOrderHolder.PlayerCount) * -360f;
            float rotationWithBase = rotation + baseRotation;
            newSpoke.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            newSpoke.transform.localPosition = new Vector3(Mathf.Cos(rotationWithBase * Mathf.Deg2Rad), Mathf.Sin(rotationWithBase * Mathf.Deg2Rad)) * SpokeOffset;

            newSpoke.SpokeImage.sprite = this.TurnOrderHolder.SpritesForTurns[playerIndex];

            string spokeName = this.TurnOrderHolder.PlayerNames[playerIndex];
            if (string.IsNullOrEmpty(spokeName))
            {
                newSpoke.NameLabel.gameObject.SetActive(false);
                newSpoke.NameLabel.text = "";
            }
            else
            {
                newSpoke.NameLabel.text = spokeName;
                // It'll be set to active when they're the current turn
                newSpoke.NameLabel.gameObject.SetActive(false);
            }
        }

        this.UpdateToNewTurn(this.TurnOrderHolder.GameConductor.CurrentGameState.CurrentPlayerIndex);
    }

    public void UpdateToNewTurn(int newTurn)
    {
        if (turningAnimationCoroutine != null)
        {
            StopCoroutine(this.turningAnimationCoroutine);
        }

        this.turningAnimationCoroutine = StartCoroutine(this.AnimateUpdateToNewTurn(newTurn));
    }

    IEnumerator AnimateUpdateToNewTurn(int newTurn)
    {
        float targetRotation = ((float)newTurn / this.TurnOrderHolder.PlayerCount) * 360f;
        float distanceRemaining;

        for (int ii = 0; ii < this.Spokes.Count; ii++)
        {
            this.Spokes[ii].NameLabel.gameObject.SetActive(ii == newTurn);
        }

        do
        {
            this.CurrentRotation = Mathf.MoveTowardsAngle(this.CurrentRotation, targetRotation, Time.deltaTime * this.RotationSpeed);
            this.RotationPivot.transform.localRotation = Quaternion.Euler(0, 0, this.CurrentRotation);

            distanceRemaining = Mathf.DeltaAngle(this.CurrentRotation, targetRotation);

            yield return new WaitForEndOfFrame();
        } while (distanceRemaining > 0);

        this.RotationPivot.transform.localRotation = Quaternion.Euler(0, 0, targetRotation);
    }

    public void KnockOutPlayer(int index)
    {
        ResetGame();
    }
}
