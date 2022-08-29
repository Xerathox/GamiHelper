using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ficha : MonoBehaviour
{
    public int Id {get;set;}
    public GameController gameController;

    [SerializeField] private SpriteRenderer m_SpriteRenderer;
    [SerializeField] private Animator m_Animator;

    private void OnMouseDown() {
        if (!EventSystem.current.IsPointerOverGameObject())
            gameController.ProcesarClickEnFicha(this);
    }

    public void MostrarFrente() { //animacion de la ficha 
        m_Animator.Play("FichaDeAtrasAlFrente");
    }

    public void MostrarReverso() { //animacion de la ficha 
        m_Animator.Play("FichaDeFrenteAAtras");
    }

    public void SetearImagen(Sprite sprite) {
        m_SpriteRenderer.sprite = sprite;
    }
}
