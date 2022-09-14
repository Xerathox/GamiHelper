using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CargarPantalla : MonoBehaviour
{
    //[SerializeField] TextMeshProUGUI textoProgreso;
    //[SerializeField] Slider sliderProgreso;

    private void Start() {        
        StartCoroutine(Carga());        
    }

    private IEnumerator Carga() {
        //sliderProgreso.gameObject.SetActive(true);

        //textoProgreso.text = null;        
        yield return new WaitForSeconds(1.5f);
        AsyncOperation operacionCarga = SceneManager.LoadSceneAsync(LoadingManager.nuevaEscena);        

        while(operacionCarga.isDone == false) {             
            yield return null;
        }
    }
}
