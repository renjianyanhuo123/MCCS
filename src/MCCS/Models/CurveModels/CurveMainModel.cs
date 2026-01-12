using MCCS.Interface.Components.Models;
using MCCS.Station.Core.PseudoChannelManagers; 

namespace MCCS.Models.CurveModels
{
    public class CurveMainModel : BindableBase
    {
        private readonly IPseudoChannelManager _pseudoChannelManager;

        private XyBindCollectionItem _selectedXBindItem;
        public XyBindCollectionItem SelectedXBindItem
        {
            get => _selectedXBindItem;
            set
            {
                if (SetProperty(ref _selectedXBindItem, value))
                {
                    UpdateTable();
                }
            }
        }

        private XyBindCollectionItem _selectedYBindItem;
        public XyBindCollectionItem SelectedYBindItem
        {
            get => _selectedYBindItem;
            set
            {
                if (SetProperty(ref _selectedYBindItem, value))
                {
                    UpdateTable();
                }
            }
        }

        private CurveShowModel _curve;
        public CurveShowModel Curve
        {
            get => _curve;
            set => SetProperty(ref _curve, value);
        }

        public CurveMainModel(XyBindCollectionItem xAxe, XyBindCollectionItem yAxe, IPseudoChannelManager pseudoChannelManager)
        {
            _pseudoChannelManager = pseudoChannelManager;
            _selectedXBindItem = xAxe ?? throw new ArgumentNullException(nameof(xAxe));
            _selectedYBindItem = yAxe ?? throw new ArgumentNullException(nameof(yAxe));
            _curve = new CurveShowModel(_selectedXBindItem, _selectedYBindItem, _pseudoChannelManager);
        }

        private void UpdateTable()
        {
            if (_selectedYBindItem == null || _selectedXBindItem == null) return;
            Curve?.Dispose();
            Curve = new CurveShowModel(_selectedXBindItem, _selectedYBindItem, _pseudoChannelManager);
        }
    }
}
