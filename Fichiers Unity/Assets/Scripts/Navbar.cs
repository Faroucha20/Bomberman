using UnityEngine;
using UnityEngine.UIElements; // Indispensable pour utiliser UI Toolkit

public class GestionMenu : MonoBehaviour
{
    private UIDocument documentUI;

    // OnEnable est appelé quand l'objet s'active (parfait pour l'UI)
    void OnEnable()
    {
        // 1. Récupérer le document UI attaché à cet objet
        documentUI = GetComponent<UIDocument>();

        // 2. Récupérer la racine (le point de départ) de l'interface
        VisualElement racine = documentUI.rootVisualElement;


    }

    void QuitterLeJeu()
    {
        Debug.Log("Fermeture du jeu !");
        
        // Cette ligne fermera le vrai jeu une fois exporté
        Application.Quit();

        // Cette petite commande spéciale permet d'arrêter le mode "Play" directement dans l'éditeur Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}