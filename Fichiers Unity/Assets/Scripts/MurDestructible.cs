using UnityEngine;

public class MurDestructible : MonoBehaviour
{
    [Header("Paramètres des Bonus")]
    // Le tableau qui contiendra tes modèles 3D de bonus
    public GameObject[] bonusDisponibles; 
    
    // La chance de faire apparaître un bonus (sur 100)
    [Range(0, 100)]
    public int chanceDeDrop = 80;

    public void Detruire()
    {
        // 1. On tire un nombre au hasard entre 0 et 99
        int tirage = Random.Range(0, 100);

        // 2. On vérifie si le tirage est inférieur à notre chance de drop (80%)
        if (tirage < chanceDeDrop)
        {
            // Sécurité : on vérifie que tu as bien mis des bonus dans l'Inspector
            if (bonusDisponibles.Length > 0)
            {
                // 3. On choisit un bonus au hasard dans notre liste
                int indexAleatoire = Random.Range(0, bonusDisponibles.Length);
                GameObject bonusChoisi = bonusDisponibles[indexAleatoire];

                Vector3 spawnPosition = transform.position;

                Quaternion spawnRotation = Quaternion.Euler(-90f, -0.75f, 0f);

                // 4. On fait apparaître le bonus sélectionné à la position actuelle du mur
                // Quaternion.identity signifie "aucune rotation par défaut"
                Instantiate(bonusChoisi, spawnPosition, spawnRotation);
            }
        }

        // 5. On détruit le mur une fois que le bonus (éventuel) est apparu
        Destroy(gameObject);
    }
}