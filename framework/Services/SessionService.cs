using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Services
{
    public interface ISessionService
    {
        Guid PlayerId { get; set; }

        PlayerObject Player { get; set; }

        //Dictionary<int, SpellTemplateObject> SpellTemplates { get; }

        //SpellTemplateObject GetSpellTemplate(int spellId);


        Task LoadTemplatesAsync();

        //Task LoadSpellTemplatesAsync();
    }

    public sealed partial class SessionService : Node, ISessionService
    {
        public Guid PlayerId { get; set; }

        public PlayerObject Player { get; set; }

        //public Dictionary<int, SpellTemplateObject> SpellTemplates { get; private set; } = [];

        public override void _Ready()
        {
            base._Ready();

            ObjectAccessor.SessionService = this;
        }


        //public SpellTemplateObject GetSpellTemplate(int cardId)
        //{
        //    return SpellTemplates.GetValueOrDefault(cardId);
        //}

        public async Task LoadTemplatesAsync()
        {
            //await LoadSpellTemplatesAsync();
        }

        //public async Task LoadSpellTemplatesAsync()
        //{
        //    var templates = await ObjectAccessor.ApiService.GetSpellTemplatesAsync();
        //    SpellTemplates = templates.ToDictionary(i => i.CardId, n => n);

        //    GD.Print($"Loaded {templates.Count} spell templates.");
        //}
    }
}
