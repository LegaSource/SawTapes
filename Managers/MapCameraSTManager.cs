using SawTapes.Behaviours;

namespace SawTapes.Managers
{
    public class MapCameraSTManager
    {
        public static void UpdateMapCamera(ManualCameraRenderer mapScreen)
        {
            if (StartOfRound.Instance.shipHasLanded && mapScreen.targetedPlayer != null)
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
