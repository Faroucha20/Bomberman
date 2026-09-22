using UnityEngine;
using UnityEngine.UIElements;

public class MenuGameOver : MonoBehaviour
{
    [Header("Redirection")]
    [Tooltip("Glissez ici le GameObject qui contient l'UIDocument de la Map")]
    public UIDocument ecranMap;
    
    // --- NOUVEAU : On ajoute une case pour fermer l'écran de Jeu ---
    [Tooltip("Glissez ici le GameObject de l'écran de Jeu")]
    public GameObject ecranJeu; 

    public GenerateurMap generateur;

    private UIDocument monEcranGameOver;
    private Button boutonRessayer;
    private Button boutonQuitter;

    void OnEnable()
    {
        monEcranGameOver = GetComponent<UIDocument>();
        var root = monEcranGameOver.rootVisualElement;

        // On récupère les boutons. 
        // ⚠️ ATTENTION : Les noms entre guillemets doivent être EXACTEMENT ceux de ton UI Builder !
        boutonRessayer = root.Q<Button>("Ressayer");
        boutonQuitter = root.Q<Button>("Quitter");

        // On abonne les boutons aux actions
        if (boutonRessayer != null) boutonRessayer.clicked += SurClicRessayer;
        if (boutonQuitter != null) boutonQuitter.clicked += SurClicQuitter;
    }

    void OnDisable()
    {
        // On désabonne les boutons pour éviter les erreurs de mémoire
        if (boutonRessayer != null) boutonRessayer.clicked -= SurClicRessayer;
        if (boutonQuitter != null) boutonQuitter.clicked -= SurClicQuitter;
    }

    private void SurClicRessayer()
    {
        // --- NOUVEAU 1 : LA PUNITION ---
        // On remet l'inventaire à zéro car le joueur est mort !
        MemoireInventaire.PerdreBonusActifs();

        // --- NOUVEAU 2 : SÉCURITÉ ---
        // On s'assure que le jeu n'est pas resté figé (si le joueur est mort pendant une pause)
        Time.timeScale = 1f;

        // On nettoie le plateau 3D
        if (generateur != null)
        {
            generateur.NettoyerPlateau();
        }

        // --- NOUVEAU 3 : On éteint l'interface du jeu ---
        if (ecranJeu != null)
        {
            ecranJeu.SetActive(false);
        }

        // 2. On allume l'écran de la carte (Map)
        if (ecranMap != null)
        {
            ecranMap.gameObject.SetActive(true);
        }

        // 3. On éteint l'écran de Game Over
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
}