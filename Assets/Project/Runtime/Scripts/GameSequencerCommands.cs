using PixelCrushers.DialogueSystem.SequencerCommands;
using Project.Runtime.Scripts.AssetLoading;
using Location = Project.Runtime.Scripts.ScriptableObjects.Location;

namespace Project.Runtime.Scripts
{

    public class SequencerCommandTravel : SequencerCommand
    {
       

        public void Start()
        {
            string address = GetParameter(0);
            string travelMethod = GetParameter(1, "default");

            
            AddressableLoader.RequestLoad<Location>(address, loc =>
            {
                if (travelMethod.ToLower() == "immediate") loc.MoveHereImmediate();
                else loc.MoveHere();
            });
        }
    }

}