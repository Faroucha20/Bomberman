using UnityEngine;
using UnityEngine.UIElements;

public class MenuPause : MonoBehaviour
{
    [Header("Écrans à gérer")]
    public GameObject ecranJeu;
    public GameObject ecranMap;

    [Header("Le Générateur (pour nettoyer)")]
    public GenerateurMap generateur;

    private Button boutonContinuer;
    private Button boutonAbandonner;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        // ⚠️ Vérifie que ces noms correspondent exactement à ton UI Builder !
        boutonContinuer = root.Q<Button>("Continuer");
        boutonAbandonner = root.Q<Button>("Abandonner");

        if (boutonContinuer != null) boutonContinuer.clicked += SurClicContinuer;
        if (boutonAbandonner != null) boutonAbandonner.clicked += SurClicAbandonner;
    }

    void OnDisable()
    {
        if (boutonContinuer != null) boutonContinuer.clicked -= SurClicContinuer;
        if (boutonAbandonner != null) boutonAbandonner.clicked -= SurClicAbandonner;
    }

    private void SurClicContinuer()
    {
        // 1. On remet le temps en route
        Time.timeScale = 1f;

        // 2. On éteint l'écran de pause (le Jeu en dessous redevient actif)
        gameObject.SetActive(false);
    }

    private void SurClicAbandonner()
    {
        // 1. TRÈS IMPORTANT : On remet le temps normal, sinon le jeu restera figé pour le prochain niveau !
        Time.timeScale = 1f;

        // 2. On nettoie complètement le plateau 3D
        if (generateur != null)
        {
            generateur.NettoyerPlateau();
        }

        // 3. On allume l'écran de la Carte
        if (ecranMap != null)
        {
            ecranMap.SetActive(true);
        }

        // 4. On éteint l'interface de Jeu
        if (ecranJeu != null)
        {
            ecranJeu.SetActive(false);
        }

        // 5. On éteint cet écran de Pause
        gameObject.SetActive(false);
    }
}