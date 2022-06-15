using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmisorDeParticulas : MonoBehaviour {
    [SerializeField] GameObject m_Particulas;
    [SerializeField] float m_TiempoDeVida = 2.0f;

    public void EmitirParticulasDeAcierto(Transform transform) {
        if (m_Particulas == null) {
            return;
        }

        GameObject particulas = Instantiate(m_Particulas, transform.position, m_Particulas.transform.rotation);
        particulas.transform.SetParent(this.gameObject.transform);
        Destroy(particulas, m_TiempoDeVida);
    }

   
}
