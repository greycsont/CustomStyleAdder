using Cysharp.Threading.Tasks;

namespace CustomStyleAdder.UI;

public abstract class Overlay : AnimatedPanel
{
    public bool Visible { get; private set; }

    public async UniTask Toggle()
    {
        if (Visible)
            await Hide();
        else
            await Show();
    }
}