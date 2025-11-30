using Leatha.WarOfTheElements.Godot.framework.Controls;
using Leatha.WarOfTheElements.Godot.framework.Services;

namespace Leatha.WarOfTheElements.Godot.framework.Extensions
{
    public static class ObjectAccessor
    {
        static ObjectAccessor()
        {
        }

        public static IApiService ApiService
        {
            get => _apiService;
            set
            {
                if (_apiService != null)
                    return;

                _apiService = value;
            }
        }

        public static IGameHubService GameHubService
        {
            get => _gameHubService;
            set
            {
                if (_gameHubService != null)
                    return;

                _gameHubService = value;
            }
        }

        public static ISessionService SessionService
        {
            get => _sessionService;
            set
            {
                if (_sessionService != null)
                    return;

                _sessionService = value;
            }
        }

        public static IPlayerInputService PlayerInputService
        {
            get => _playerInputService;
            set
            {
                if (_playerInputService != null)
                    return;

                _playerInputService = value;
            }
        }

        public static ICharacterService CharacterService
        {
            get => _characterService;
            set
            {
                if (_characterService != null)
                    return;

                _characterService = value;
            }
        }

        public static IGameObjectService GameObjectService
        {
            get => _gameObjectService;
            set
            {
                if (_gameObjectService != null)
                    return;

                _gameObjectService = value;
            }
        }

        public static ITemplateService TemplateService
        {
            get => _templateService;
            set
            {
                if (_templateService != null)
                    return;

                _templateService = value;
            }
        }
        
        public static MainThreadDispatcher MainThreadDispatcher
        {
            get => _mainThreadDispatcher;
            set
            {
                if (_mainThreadDispatcher != null)
                    return;

                _mainThreadDispatcher = value;
            }
        }

        private static IApiService _apiService;
        private static IGameHubService _gameHubService;
        private static ISessionService _sessionService;
        private static IPlayerInputService _playerInputService;
        private static ICharacterService _characterService;
        private static IGameObjectService _gameObjectService;
        private static ITemplateService _templateService;
        private static MainThreadDispatcher _mainThreadDispatcher;
    }
}
