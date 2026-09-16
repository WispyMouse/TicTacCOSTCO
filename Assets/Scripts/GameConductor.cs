using UnityEngine;
using UnityEngine.InputSystem;

public class GameConductor : MonoBehaviour
{
    public Camera ClickCamera;

    public TurnOrderHolder TurnOrderHolder;
    public GridPainter GridPainter;

    public void Start()
    {
        this.GridPainter.Paint();
        this.TurnOrderHolder.SetTurnIndex(0);
    }

    public void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        RaycastHit2D raycast = Physics2D.Raycast(this.ClickCamera.ScreenToWorldPoint(Mouse.current.position.value), Vector2.zero);
        if (raycast.collider == null)
        {
            return;
        }

        Cell getCell = raycast.collider.gameObject.GetComponent<Cell>();

        if (getCell == null)
        {
            return;
        }

        if (getCell.AlreadyPlaced)
        {
            return;
        }

        getCell.SpriteRenderer.sprite = TurnOrderHolder.CurrentTurnIconHolder.sprite;
        TurnOrderHolder.NextPlayerIcon();
    }
}
