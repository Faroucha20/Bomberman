using UnityEngine;
using UnityEngine.UIElements;

public class MenuPause : MonoBehaviour
{
    [Header("Écrans à gérer")]
    public GameObject ecranJeu;
    public GameObject ecranMap;
    public GameObject ecranCommande;


    [Header("Le Générateur (pour nettoyer)")]
    public GenerateurMap generateur;

    private Button boutonContinuer;
    private Button boutonAbandonner;
    private Button boutonQuitter;
    private Button boutonCommande;


    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        // ⚠️ Vérifie que ces noms correspondent exactement à ton UI Builder !
        boutonContinuer = root.Q<Button>("Continuer");
        boutonAbandonner = root.Q<Button>("Abandonner");
        boutonQuitter = root.Q<Button>("Quitter");
        boutonCommande = root.Q<Button>("Commandes");



        if (boutonContinuer != null) boutonContinuer.clicked += SurClicContinuer;
        if (boutonAbandonner != null) boutonAbandonner.clicked += SurClicAbandonner;
        if (boutonQuitter != null) boutonQuitter.clicked += SurClicQuitter;
        if (boutonCommande != null) boutonCommande.clicked += SurClicCommande;

    }

    void OnDisable()
    {
        if (boutonContinuer != null) boutonContinuer.clicked -= SurClicContinuer;
        if (boutonAbandonner != null) boutonAbandonner.clicked -= SurClicAbandonner;
        if (boutonQuitter != null) boutonQuitter.clicked -= SurClicQuitter;
        if (boutonCommande != null) boutonCommande.clicked -= SurClicCommande;

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

    private void SurClicQuitter()
    {
        Debug.Log("Fermeture du jeu !");
        
        // Cette ligne fermera le vrai jeu une fois exporté
        Application.Quit();

        // Cette petite commande spéciale permet d'arrêter le mode "Play" directement dans l'éditeur Unity
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

        private void SurClicCommande()
    {
        // 2. On affiche la page de Commande
        ecranCommande.SetActive(true);
    }
}