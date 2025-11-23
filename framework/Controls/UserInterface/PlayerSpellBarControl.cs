using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Controls.Entities;
using Range = Godot.Range;

namespace Leatha.WarOfTheElements.Godot.framework.Controls.UserInterface
{
    public sealed partial class PlayerSpellBarControl : Control
    {
        [Export]
        public Control ActionListContainer { get; set; }

        [Export]
        public CastBarControl CastBarControl { get; set; }

        [Export]
        public RichTextLabel CastRemainingTimeLabel { get; set; }

        public float GlobalCooldown { get; } = 1.0f;

        public void TriggerGlobalCooldown()
        {
            var slots = ActionListContainer
                .GetChildren()
                .OfType<SpellActionBarSlot>()
                .Where(i => i.SpellId > 0)
                .ToList();

            foreach (var slot in slots)
            {
                // #TODO: Check if cooldown is lower than global (< 1s).

                if (slot.GetRemainingCooldown() < GlobalCooldown)
                    slot.SetCooldown(GlobalCooldown, true);
            }
        }

        public void OnSpellCastStarted(SpellObject spell)
        {
            CastBarControl.OnSpellCastStarted(spell);
        }
    }
}
