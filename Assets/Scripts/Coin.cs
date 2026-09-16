using UnityEngine;

public class Coin : MonoBehaviour
{
    void Start()
    {
        string monedasGuardadas = PlayerPrefs.GetString("CollectedCoins", "");
        
        // Split: separa el string por comas y da un array con cada nombre suelto.
        string[] listaDeMonedas = monedasGuardadas.Split(',');  

        for (int i = 0; i < listaDeMonedas.Length; i++)
        {
            if (listaDeMonedas[i] == gameObject.name)
            {
                Destroy(gameObject); 
                break;
            }
        }
    }
}
