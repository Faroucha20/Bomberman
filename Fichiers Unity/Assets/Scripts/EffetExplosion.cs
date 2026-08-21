using UnityEngine;

public class EffetExplosion : MonoBehaviour
{
    public float tempsAffichage = 0.5f;

    void Start()
    {
        // Détruit cet objet visuel d'explosion (le cube jaune/orange) peu après son apparition
        // pour libérer la mémoire et nettoyer la scène.
        Destroy(gameObject, tempsAffichage);
    }
}