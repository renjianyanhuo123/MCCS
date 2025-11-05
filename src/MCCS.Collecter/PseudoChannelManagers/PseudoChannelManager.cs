using MCCS.Collecter.ControllerManagers;

namespace MCCS.Collecter.PseudoChannelManagers
{
    public class PseudoChannelManager : IPseudoChannelManager
    {
        private readonly IControllerManager _controllerManager;

        public PseudoChannelManager(IControllerManager controllerManager)
        {
            _controllerManager = controllerManager;
        }

    }
}
