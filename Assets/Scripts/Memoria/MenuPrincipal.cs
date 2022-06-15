using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuPrincipal : MonoBehaviour{
    
    public void IniciarTic_Tac_Toe() {
        SceneManager.LoadScene(1);
    }

    public void IniciarMemoria() {
        SceneManager.LoadScene(2);
    }

    public void IniciarDamas() {
        SceneManager.LoadScene(3);
    }

    public void SalirDeAplicación() {
        Application.Quit();

    }
}


