using UnityEngine;

namespace SawTapes.Behaviours
{
    public class SawTape : PhysicsProp
    {
        public bool isGameEnded = false;
        public AudioSource sawTheme;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (ConfigManager.isSawTheme.Value && buttonDown && playerHeldBy != null && !isGameEnded)
            {
                GameObject audioObject = Instantiate(SawTapes.sawTheme, playerHeldBy.transform.position, Quaternion.identity);
                sawTheme = audioObject.GetComponent<AudioSource>();
                sawTheme.Play();
                audioObject.transform.SetParent(playerHeldBy.transform);
            }
        }
    }
}
