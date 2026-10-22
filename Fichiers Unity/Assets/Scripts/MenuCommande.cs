using UnityEngine;
using UnityEngine.UIElements;

public class MenuCommande : MonoBehaviour
{

    private Button boutonOK;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        // ⚠️ Vérifie que ces noms correspondent exactement à ton UI Builder !
        boutonOK = root.Q<Button>("OK");




        if (boutonOK != null) boutonOK.clicked += SurClicOK;


    }

    void OnDisable()
    {
        if (boutonOK != null) boutonOK.clicked -= SurClicOK;
    }


        private void SurClicOK()
    {
        // 1. On éteint l'écran de pause (le Jeu en dessous redevient actif)
        gameObject.SetActive(false);

    }
}