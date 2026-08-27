using UnityEngine;

// Cette ligne permet de créer ces "fiches" directement depuis le menu clic droit d'Unity
[CreateAssetMenu(fileName = "NouveauNiveau", menuName = "Bomberman/Configuration Niveau")]
public class ConfigNiveau : ScriptableObject
{
    [Header("Habillage du niveau")]
    public GameObject prefabSol;
    public GameObject prefabMurIndestructible;
    public GameObject prefabMurDestructible;

    [Header("Paramètres")]
    public int nbMursDestructibles = 30;

    [Header("Monstres spécifiques à cette zone")]
    public ConfigMonstre[] monstresAGenerer;
}