using static BuzzControllerClassLibraryCore.BuzzController;

namespace BuzzControllerClassLibraryCore
{
    public class BuzzControllerEventArgs : EventArgs
    {
        public BuzzControllerNumber Controller { get; set; }
        public BuzzControllerButton Button { get; set; }
        public BuzzButtonAction Action { get; set; }

        public BuzzControllerEventArgs(BuzzControllerNumber controller, BuzzControllerButton button, BuzzButtonAction action)
        {
            Controller = controller;
            Button = button;
            Action = action;
        }

        public BuzzControllerEventArgs(int controllerNumber, int buttonNumber, BuzzButtonAction action) :
            this((BuzzControllerNumber)controllerNumber, ButtonFromBit(buttonNumber), action)
        { }

        public override string ToString()
        {
            return $"BuzzControllerEventArgs {{ Controller:{Controller}, Button:{Button}, Action:{Action}  }}";
        }
        private static BuzzControllerButton ButtonFromBit(int bit)
        {
            switch (bit)
            {
                case 0: return BuzzControllerButton.Red;
                case 1: return BuzzControllerButton.Yellow;
                case 2: return BuzzControllerButton.Green;
                case 3: return BuzzControllerButton.Orange;
                case 4: return BuzzControllerButton.Blue;
            }
            throw new ArgumentException($"Invalid bit value:{bit}");
        }
    }
}