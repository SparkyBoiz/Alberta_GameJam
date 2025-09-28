using UnityEngine.InputSystem;

namespace Game.Core
{
    public class InputSystem_Actions
    {
        private readonly global::InputSystem_Actions _generated;
        public PlayerProxy Player { get; }
        public UIProxy UI { get; }

        public InputSystem_Actions()
        {
            _generated = new global::InputSystem_Actions();
            Player = new PlayerProxy(_generated);
            UI = new UIProxy(_generated);
        }

        public void Enable() => _generated.Enable();
        public void Disable() => _generated.Disable();

        public class PlayerProxy
        {
            private readonly global::InputSystem_Actions _a;
            public PlayerProxy(global::InputSystem_Actions a) { _a = a; }

            public InputAction Move => _a.Player.Move;     // as-is
            public InputAction Aim => _a.Player.Look;      // Look -> Aim
            public InputAction Fire => _a.Player.Attack;   // Attack -> Fire
            public InputAction Reload => _a.Player.Interact; // Interact -> Reload (hold by default)

            public void Enable() => _a.Player.Enable();
            public void Disable() => _a.Player.Disable();
        }

        public class UIProxy
        {
            private readonly global::InputSystem_Actions _a;
            public UIProxy(global::InputSystem_Actions a) { _a = a; }

            public InputAction Pause => _a.UI.Pause;

            public void Enable() => _a.UI.Enable();
            public void Disable() => _a.UI.Disable();
        }
    }
}
