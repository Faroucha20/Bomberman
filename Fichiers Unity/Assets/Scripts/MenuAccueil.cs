using UnityEngine;
using UnityEngine.UIElements;

public class MenuAccueil : MonoBehaviour
{
    [Header("Écran Suivant")]
    public GameObject ecranMap;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        // On écoute le clic de souris n'importe où sur l'interface
        if (root != null)
        {
            root.RegisterCallback<PointerDownEvent>(SurClicEcran);
        }
    }

    void Update()
    {
        // On vérifie si n'importe quelle touche du clavier ou de la souris est pressée
        if (Input.anyKeyDown)
        {
            PasserALaCarte();
        }
    }

    private void SurClicEcran(PointerDownEvent evt)
    {
        PasserALaCarte();
    }

    private void PasserALaCarte()
    {
        // 1. On allume le GameObject de la Map
        if (ecranMap != null)
        {
            ecranMap.SetActive(true);
        }

        // 2. On éteint complètement cet écran d'accueil
        gameObject.SetActive(false);
    }
}