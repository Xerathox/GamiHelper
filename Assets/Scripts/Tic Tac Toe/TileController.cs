using UnityEngine;
using UnityEngine.UI;

public class TileController : MonoBehaviour
{
    [Header("Component References")]
    public GameStateController gameController;
    public Button interactiveButton;
    public Text internalText;

    public void UpdateTile() {
        internalText.text = gameController.GetPlayersTurn();
        interactiveButton.image.sprite = gameController.GetPlayerSprite();
        interactiveButton.interactable = false;
        gameController.EndTurn();
    }

    public void ResetTile() {
        internalText.text = "";
        interactiveButton.image.sprite = gameController.tileEmpty;
    }
}