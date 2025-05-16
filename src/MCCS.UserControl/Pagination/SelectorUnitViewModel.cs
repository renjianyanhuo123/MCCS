namespace MCCS.UserControl.Pagination
{
    public enum UnitTypeEnum
    {
        Image = 1,
        TextBlock = 2
    }

    public class SelectorUnitViewModel:BindingBase
    {
        #region private field
        private bool _isSelected = false;
        private int _num = 1;
        private UnitTypeEnum _type = UnitTypeEnum.Image;
        #endregion

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        
        public int Num
        {
            get => _num;
            set => SetProperty(ref _num, value);
        }

        public UnitTypeEnum Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
    }
}
