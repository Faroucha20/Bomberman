using UnityEngine;
using UnityEngine.UIElements;

public class GestionnaireUI : MonoBehaviour
{
    [Header("Écrans du Jeu")]
    public UIDocument ecranJeu;
    public UIDocument ecranGameOver;

    void Start()
    {
        if (ecranJeu != null) 
        {
            ecranJeu.rootVisualElement.style.display = DisplayStyle.Flex;
        }
        
        if (ecranGameOver != null) 
        {
            ecranGameOver.rootVisualElement.style.display = DisplayStyle.None;
        }
    }

    public void AfficherGameOver()
    {
        Debug.Log("ÉTAPE A : Le GestionnaireUI a bien reçu l'ordre !");

        if (ecranJeu != null)
        {
            ecranJeu.rootVisualElement.style.display = DisplayStyle.None;
            Debug.Log("ÉTAPE B : L'écran de jeu a été masqué avec succès.");
        }
        else
        {
            Debug.LogWarning("⚠️ ATTENTION : La case 'Ecran Jeu' est vide dans l'Inspector !");
        }
        
        if (ecranGameOver != null)
        {
            ecranGameOver.rootVisualElement.style.display = DisplayStyle.Flex;
            Debug.Log("ÉTAPE C : L'écran Game Over a été affiché avec succès.");
        }
        else
        {
            Debug.LogWarning("⚠️ ATTENTION : La case 'Ecran Game Over' est vide dans l'Inspector !");
        }
    }
}