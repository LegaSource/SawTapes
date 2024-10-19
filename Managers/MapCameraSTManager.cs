using SawTapes.Behaviours;

namespace SawTapes.Managers
{
    internal class MapCameraSTManager
    {
        public static void UpdateMapCamera(ref ManualCameraRenderer mapScreen)
        {
            if (mapScreen.targetedPlayer != null)
            {
                PlayerSTBehaviour playerBehaviour = StartOfRound.Instance.mapScreen.targetedPlayer.GetComponent<PlayerSTBehaviour>();
                if (playerBehaviour.isInGame)
                {
                    StartOfRound.Instance.mapScreenPlayerName.enabled = false;
                    StartOfRound.Instance.screenLevelDescription.enabled = true;
                    StartOfRound.Instance.screenLevelDescription.text = mapScreen.targetedPlayer.playerUsername + " is playing a game";
                }
                else
                {
                    StartOfRound.Instance.mapScreenPlayerName.enabled = true;
                    StartOfRound.Instance.screenLevelDescription.enabled = false;
                    StartOfRound.Instance.screenLevelDescription.text = "";
                }
            }
        }
    }
}
