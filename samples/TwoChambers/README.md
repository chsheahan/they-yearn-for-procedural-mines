# Two Chambers sample

Open **this folder** with **Unity 6000.4.0f1**. Keep the whole repository together; the manifest references `../../../src/Core` and `../../../src/Unity`.

Open `Assets/Scenes/TwoChambers.unity` and press Play. The saved scene contains just the generator; geometry is generated at runtime. The Inspector's **Generate preview / regenerate** button creates a temporary preview.

WASD/arrows move, mouse looks, Shift increases speed, Escape releases the mouse, and R resets to the entrance. Click outside the settings panel to capture the mouse; release it before editing the seed/sliders.

Run collision/traversal checks with **Window → General → Test Runner → PlayMode → Run All**. The sample uses the Built-in Render Pipeline and legacy Input Manager, with no downloaded art.
