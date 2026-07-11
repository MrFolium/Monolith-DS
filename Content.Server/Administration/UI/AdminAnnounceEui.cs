using Content.Server.Administration.Managers;
using Content.Server.Chat;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.EUI;
using Content.Shared.Administration;
using Content.Shared.Chat; // Corvax-TTS
using Content.Shared.Eui;
using Robust.Shared.Audio; // Frontier
using Robust.Shared.GameObjects; // Corvax-TTS
using Robust.Shared.Player; // Corvax-TTS

namespace Content.Server.Administration.UI
{
    public sealed partial class AdminAnnounceEui : BaseEui
    {
        [Dependency] private IAdminManager _adminManager = default!;
        [Dependency] private IChatManager _chatManager = default!;
        [Dependency] private IEntityManager _entityManager = default!; // Corvax-TTS
        private readonly ChatSystem _chatSystem;

        public AdminAnnounceEui()
        {
            IoCManager.InjectDependencies(this);
            _chatSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();
        }

        public override void Opened()
        {
            StateDirty();
        }

        public override EuiStateBase GetNewState()
        {
            return new AdminAnnounceEuiState();
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            switch (msg)
            {
                case AdminAnnounceEuiMsg.DoAnnounce doAnnounce:
                    if (!_adminManager.HasAdminFlag(Player, AdminFlags.Admin))
                    {
                        Close();
                        break;
                    }

                    switch (doAnnounce.AnnounceType)
                    {
                        case AdminAnnounceType.Server:
                            _chatManager.DispatchServerAnnouncement(doAnnounce.Announcement);
                            break;
                        // TODO: Per-station announcement support
                        case AdminAnnounceType.Station:
                            _chatSystem.DispatchGlobalAnnouncement(doAnnounce.Announcement, doAnnounce.Announcer, colorOverride: Color.Gold);
                            break;
                        case AdminAnnounceType.Antag: // Frontier
                            _chatSystem.DispatchGlobalAnnouncement(doAnnounce.Announcement, doAnnounce.Announcer, true, new SoundPathSpecifier("/Audio/Announcements/war.ogg"), colorOverride: Color.Red);
                            break;
                    }

// Corvax-TTS-start:
                    if (doAnnounce.EnableTTS && !string.IsNullOrWhiteSpace(doAnnounce.Voice))
                    {
                        _entityManager.EventBus.RaiseEvent(
                            EventSource.Local,
                            new AnnounceSpokeEvent(doAnnounce.Voice, doAnnounce.Announcement, Filter.Broadcast(), null));
                    }

// Corvax-TTS-end.
                    StateDirty();

                    if (doAnnounce.CloseAfter)
                        Close();

                    break;
            }
        }
    }
}
