namespace FileFlow.ViewModels
{
    public class MoveBookmarkDownContextItem : MoveBookmarkUpContextItem
    {
        public override string Text => "Опустить закладку в списке";
        public override string IconPath => "Assets/Icons/pin_down.png";
        public override int Order => 4;
        protected override int Direction => -1;
    }
}
