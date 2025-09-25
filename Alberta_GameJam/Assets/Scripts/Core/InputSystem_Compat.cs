using UnityEngine.InputSystem;

namespace Game.Core
{
    // Compatibility wrapper to adapt the generated InputSystem_Actions (global namespace)
    // to the names expected by our gameplay scripts.
    public class InputSystem_Actions
    {
        private global::InputSystem_Actions _generated;
        public PlayerProxy Player { get; private set; }

        public InputSystem_Actions()
        {
            _generated = new global::InputSystem_Actions();
            Player = new PlayerProxy(_generated);
        }

        public void Enable() => _generated.Enable();
        public void Disable() => _generated.Disable();

        public class PlayerProxy
        {
            private readonly global::InputSystem_Actions _a;
            public PlayerProxy(global::InputSystem_Actions a) { _a = a; }

            // Map expected actions
            public InputAction Move => _a.Player.Move;     // as-is
            public InputAction Aim => _a.Player.Look;      // Look -> Aim
            public InputAction Fire => _a.Player.Attack;   // Attack -> Fire
            public InputAction Reload => _a.Player.Interact; // Interact -> Reload (hold by default)
        }
    }
}