using MCCS.Station.Core.ControllerManagers;
using MCCS.Station.Core.SignalManagers;

namespace MCCS.Station.Core.PseudoChannelManagers
{
    public class PseudoChannelManager : IPseudoChannelManager
    {
        private readonly IControllerManager _controllerManager; 
        private readonly List<PseudoChannel> _pseudoChannels = [];
        private readonly ISignalManager _signalManager;

        public PseudoChannelManager(IControllerManager controllerManager,
            ISignalManager signalManager)
        {
            _controllerManager = controllerManager;
            _signalManager = signalManager;
        }

        public void Initialization(IEnumerable<PseudoChannelConfiguration> configuations)
        {
            foreach (var configuration in configuations)
            {
                _pseudoChannels.Add(new PseudoChannel(configuration, _signalManager));
            }
        }

        public PseudoChannel GetPseudoChannelById(long pseudoChannelId)
        {
            var res= _pseudoChannels.FirstOrDefault(s => s.ChannelId == pseudoChannelId) ?? throw new ArgumentNullException("pseudoChannelId is null");
            return res;
        }

        public IEnumerable<PseudoChannel> GetPseudoChannels()
        {
            return _pseudoChannels;
        }
    }
}
