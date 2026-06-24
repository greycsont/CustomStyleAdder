using Cysharp.Threading.Tasks;

namespace CustomStyleAdder.UI;

public abstract class Overlay : AnimatedPanel
{
    public bool Visible { get; private set; }

    public override UniTask Show() { Visible = true;  return base.Show(); }
    public override UniTask Hide() { Visible = false; return base.Hide(); }

    public UniTask Toggle() => Visible ? Hide() : Show();
}