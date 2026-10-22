using UnityEngine;
using UnityEngine.UIElements;

public class MenuAccueil : MonoBehaviour
{
    [Header("Écran Suivant")]
    public GameObject ecranMode;

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
            PasserPageMode();
        }
    }

    private void SurClicEcran(PointerDownEvent evt)
    {
        PasserPageMode();
    }

    private void PasserPageMode()
    {
        // 1. On allume le GameObject de la Map
        if (ecranMode != null)
        {
            ecranMode.SetActive(true);
        }

        // 2. On éteint complètement cet écran d'accueil
        gameObject.SetActive(false);
    }
}