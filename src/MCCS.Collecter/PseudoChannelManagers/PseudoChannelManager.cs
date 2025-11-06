using MCCS.Collecter.ControllerManagers;

namespace MCCS.Collecter.PseudoChannelManagers
{
    public class PseudoChannelManager : IPseudoChannelManager
    {
        private readonly IControllerManager _controllerManager;
        private readonly List<PseudoChannel> _pseudoChannels;

        public PseudoChannelManager(IControllerManager controllerManager)
        {
            _controllerManager = controllerManager;
        }

        public void Initial(IEnumerable<>)
        {

        }

    }
}
