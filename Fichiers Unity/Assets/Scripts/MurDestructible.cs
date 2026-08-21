using UnityEngine;

public class MurDestructible : MonoBehaviour
{
    public void Detruire()
    {
        // Plus tard, on pourra instancier des particules ou un bonus ici
        Destroy(gameObject);
    }
}