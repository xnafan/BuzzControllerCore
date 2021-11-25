using BuzzControllerClassLibraryCore;

namespace BuzzControllerWinFormsTestApp
{
    public partial class Form1 : Form
    {

        BuzzController _controller = new BuzzController();
        public Form1()
        {
            InitializeComponent();
            _controller.ButtonStateChanged += _controller_ButtonStateChanged;
        }

        private void _controller_ButtonStateChanged(object? sender, BuzzControllerEventArgs e)
        {
            if(InvokeRequired)
            {
                Invoke(new Action(() => HandleButtonPress(e)));
            }
            
        }

        private void HandleButtonPress(BuzzControllerEventArgs e)
        {
            Text = "Event: " + e;
            _controller.ToggleBuzzerLight(e.Controller);
            int controller = (int)e.Controller;
            int button = (int)e.Button;
            Button btn = (Button)tableLayoutPanel1.Controls[19 - (button * 4 + controller)];
            btn.Text = e.Action == BuzzController.BuzzButtonAction.Pressed ? "Pressed" : "Released";
            SwapForeAndBackColor(btn);
        }

        private static void SwapForeAndBackColor(Button btn)
        {
            Color tempColor = btn.ForeColor;
            btn.ForeColor = btn.BackColor;
            btn.BackColor = tempColor;
        }
    }
}