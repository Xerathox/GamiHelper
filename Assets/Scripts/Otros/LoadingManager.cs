using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoadingManager
{
    public static int nuevaEscena;

    public static void NextScene(int PrimeraEscena){
        nuevaEscena = PrimeraEscena;
        SceneManager.LoadScene(4);

    }
}
