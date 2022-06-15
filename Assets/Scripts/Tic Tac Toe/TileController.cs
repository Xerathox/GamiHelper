using UnityEngine;
using UnityEngine.UI;

public class TileController : MonoBehaviour
{
    [Header("Component References")]
    public GameStateController gameController;                       // Reference to the gamecontroller
    public Button interactiveButton;                                 // Reference to this button
    public Text internalText;                                        // Reference to this Text

    // Cada vez que pulsaoms un botón, se actualiza el estado de esta baldosa.
    //El seguimiento interno de la posición de quien (el componente de texto) y desactivar el botón
    
    public void UpdateTile()
    {
        internalText.text = gameController.GetPlayersTurn();
        interactiveButton.image.sprite = gameController.GetPlayerSprite();
        interactiveButton.interactable = false;
        gameController.EndTurn();
    }

    // Resetea el componente de los textos y las imagenes de los botones 
    public void ResetTile() {
        internalText.text = "";
        interactiveButton.image.sprite = gameController.tileEmpty;
    }
}