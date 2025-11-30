using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using Leatha.WarOfTheElements.Common.Communication.Transfer;
using Leatha.WarOfTheElements.Godot.framework.Extensions;

namespace Leatha.WarOfTheElements.Godot.framework.Services
{
    public interface ITemplateService
    {
        Dictionary<int, SpellInfoObject> SpellTemplates { get; }

        Dictionary<int, MapInfoObject> MapTemplates { get; }

        Dictionary<int, GameObjectInfoObject> GameObjectTemplates { get; }

        SpellInfoObject GetSpellInfo(int spellId);

        MapInfoObject GetMapInfo(int mapId);

        GameObjectInfoObject GetGameObjectTemplate(int gameObjectId);




        Task LoadTemplatesAsync();

        Task LoadSpellTemplatesAsync();

        Task LoadMapTemplatesAsync();

        Task LoadGameObjectTemplatesAsync();
    }

    public sealed partial class TemplateService : Node, ITemplateService
    {
        public Dictionary<int, SpellInfoObject> SpellTemplates { get; private set; } = [];

        public Dictionary<int, MapInfoObject> MapTemplates { get; private set; } = [];

        public Dictionary<int, GameObjectInfoObject> GameObjectTemplates { get; private set; } = [];

        public override void _Ready()
        {
            base._Ready();

            ObjectAccessor.TemplateService = this;
        }

        public SpellInfoObject GetSpellInfo(int spellId)
        {
            return SpellTemplates.GetValueOrDefault(spellId);
        }

        public MapInfoObject GetMapInfo(int mapId)
        {
            return MapTemplates.GetValueOrDefault(mapId);
        }

        public GameObjectInfoObject GetGameObjectTemplate(int gameObjectId)
        {
            return GameObjectTemplates.GetValueOrDefault(gameObjectId);
        }

        public async Task LoadTemplatesAsync()
        {
            await LoadSpellTemplatesAsync();
            await LoadMapTemplatesAsync();
            await LoadGameObjectTemplatesAsync();
        }

        public async Task LoadSpellTemplatesAsync()
        {
            var templates = await ObjectAccessor.ApiService.GetSpellTemplatesAsync();
            SpellTemplates = templates.ToDictionary(i => i.SpellId, n => n);

            GD.Print($"Loaded { templates.Count } spell templates.");
        }

        public async Task LoadMapTemplatesAsync()
        {
            var templates = await ObjectAccessor.ApiService.GetMapTemplatesAsync();
            MapTemplates = templates.ToDictionary(i => i.MapId, n => n);

            GD.Print($"Loaded { templates.Count } map templates.");
        }

        public async Task LoadGameObjectTemplatesAsync()
        {
            var templates = await ObjectAccessor.ApiService.GetGameObjectTemplatesAsync();

            GD.Print("JSON = " + JsonSerializer.Serialize(templates));

            GameObjectTemplates = templates.ToDictionary(i => i.GameObjectId, n => n);

            GD.Print($"Loaded {templates.Count} game object templates.");
        }
    }
}
