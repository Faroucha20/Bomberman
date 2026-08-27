using UnityEngine;
using UnityEngine.UIElements;

public class MenuCarte : MonoBehaviour
{
    [Header("Le Générateur de la scène")]
    public GenerateurMap generateur;

    [Header("Les configurations (Fiches) de tes niveaux")]
    public ConfigNiveau configNiveau1; // Ex: Volcan
    public ConfigNiveau configNiveau2; // Ex: Glace
    
    private VisualElement root;

void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        Button boutonNiveau1 = root.Q<Button>("Niveau1");
        Button boutonNiveau2 = root.Q<Button>("Niveau2");

        // --- Niveau 1 ---
        if (boutonNiveau1 != null) 
        {
            boutonNiveau1.clicked += () => LancerNiveau(configNiveau1);
        }

        // --- Niveau 2 ---
        if (boutonNiveau2 != null) 
        {
            Debug.Log("SUPER : Le bouton Niveau 2 a été trouvé par le code !");
            boutonNiveau2.clicked += () => LancerNiveau(configNiveau2);
        }
        else
        {
            Debug.LogError("ERREUR : Le code ne trouve pas le bouton 'Niveau2'. Vérifie l'UI Builder !");
        }
    }

    private void LancerNiveau(ConfigNiveau configChoisie)
    {
        Debug.Log("Clic détecté ! Je lance le niveau...");

        if (generateur != null)
        {
            generateur.LancerNouveauNiveau(configChoisie);
        }
        
        root.style.display = DisplayStyle.None; 
    }

    // Plus tard, quand le joueur gagne, tu pourras appeler cette fonction 
    // pour réafficher la carte !
    public void ReafficherCarte()
    {
        root.style.display = DisplayStyle.Flex;
        generateur.NettoyerAncienneMap(); // On vide le plateau
    }
}