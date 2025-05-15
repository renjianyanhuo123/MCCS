using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.UserControl.Pagination
{
    public class SelectorUnitViewModel:BindingBase
    {
        #region private field
        private bool _isSelected = false;
        private bool _isVisible = true;
        private int _num = 1;
        #endregion

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public int Num
        {
            get => _num;
            set => SetProperty(ref _num, value);
        }
    }
}
