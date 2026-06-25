namespace CustomStyleAdder.UI;

public abstract class Overlay : AnimatedPanel
{
    public bool Visible { get; private set; }

    // Flip Visible at the START so rapid toggling always reflects the latest intent.
    public override void Show() { Visible = true;  base.Show(); }
    public override void Hide() { Visible = false; base.Hide(); }

    public void Toggle()
    {
        if (Visible) Hide();
        else Show();
    }
}
