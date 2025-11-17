using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface
{
    public sealed partial class PlayerSpellBarControl : Control
    {
        [Export]
        public Control ActionListContainer { get; set; }

        public void TriggerGlobalCooldown()
        {
            var slots = ActionListContainer.GetChildren().OfType<SpellActionBarSlot>().ToList();
            foreach (var slot in slots)
            {
                // #TODO: Check if cooldown is lower than global (< 1s).

                slot.SetCooldown(1.0f);
            }
        }
    }
}
