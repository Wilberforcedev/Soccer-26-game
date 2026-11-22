using System;
using System.Collections.Generic;
using System.Linq;
using FC2026.World;
using UnityEngine;

namespace FC2026.Core
{
    /// <summary>
    /// Offline career hub for the procedural world. Players browse six
    /// confederations, pick any national top division and club, then run a
    /// season. Only the managed league table is saved; clubs, squads and
    /// fixtures come from the deterministic world database.
    /// </summary>
    public sealed class CareerHubManager : MonoBehaviour
    {
        private static readonly string[] Confederations = { "UEFA", "CONMEBOL", "CONCACAF", "CAF", "AFC", "OFC" };
        private const int InitialBudget = 15000000;
        private const float DisabledRowAlpha = 0.4f;

        private CareerSaveData save;
        private GUIStyle titleStyle;
        private GUIStyle matchTeamStyle;
        private GUIStyle matchVsStyle;
        private GUIStyle headingStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;
        private GUIStyle noticeStyle;
        private GUIStyle buttonStyle;
        private GUIStyle primaryButtonStyle;
        private GUIStyle accentButtonStyle;
        private GUIStyle dangerButtonStyle;
        private GUIStyle highlightStyle;
        private GUIStyle rowStyle;
        private GUIStyle selectedRowStyle;
        private GUIStyle panelStyle;
        private GUIStyle cardStyle;
        private GUIStyle heroCardStyle;
        private GUIStyle hitboxStyle;
        private GUIStyle backStyle;
        private GUIStyle iconStyle;
        private GUIStyle cardTitleStyle;
        private GUIStyle cardValueStyle;
        private GUIStyle cardCaptionStyle;
        private GUIStyle dashboardCaptionStyle;
        private GUIStyle tileGlyphStyle;
        private GUIStyle tileLabelStyle;
        private GUIStyle tileCaptionStyle;
        private GUIStyle ovrStyle;
        private GUIStyle statNameStyle;
        private GUIStyle statValueStyle;
        private GUIStyle pitchNameStyle;
        private GUIStyle pitchOvrStyle;
        private GUIStyle pitchPosStyle;
        private GUIStyle subNameStyle;
        private GUIStyle transferOvrStyle;
        private GUIStyle transferNameStyle;
        private GUIStyle pitchStyle;
        private GUIStyle pitchCardStyle;
        private GUIStyle tableHeaderStyle;
        private GUIStyle tableHeaderNameStyle;
        private GUIStyle tableNameStyle;
        private Texture2D tableHeaderTexture;
        private Texture2D tableRowTexture;
        private Texture2D tableAltRowTexture;
        private Texture2D tableHighlightTexture;
        private string notice = "Browse the world and choose a club to begin.";
        private bool showLeagueTable;
        private bool showCupCampaign;
        private bool showHonours;
        private bool showSquadManagement;
        private bool showTransfers;
        private bool transfersShowSquad;
        private bool transfersShowScout;
        private TransferTarget[] marketCache;
        private TransferTarget[] scoutCache;
        private string scoutCacheClubId = string.Empty;
        private string scoutConfederation = "UEFA";
        private string scoutCountryId = string.Empty;
        private string scoutClubId = string.Empty;
        private Vector2 scoutScroll;
        private string selectedTransferPlayerId = string.Empty;
        private string pendingTransferPlayerId = string.Empty;
        private string pendingTransferAction = string.Empty;
        private bool showHomeScreen = true;
        private bool showSettingsScreen;
        private bool showClubBrowser;
        private bool confirmNewCareer;
        private bool showPlayerDetails;
        private bool showKitRoom;
        private bool showTournamentHub;
        private bool showMatchPreview;
        private bool squadListView;
        private WorldPlayer selectedPlayer;
        private int selectedKitSlot;
        private string browserConfederation = "UEFA";
        private string browserCountryId = string.Empty;
        private string pendingClubId = string.Empty;
        private Vector2 squadScroll;
        private Vector2 transferScroll;
        private Vector2 countryScroll;
        private Vector2 clubScroll;
        private Vector2 leagueTableScroll;
        private Vector2 substitutesScroll;
        private Vector2 honoursScroll;
        private Vector2 dashboardScroll;
        private Vector2 playerStatsScroll;
        private Vector2 competitionScroll;
        private int styleScreenWidth = -1;
        private int styleScreenHeight = -1;

        private void Awake()
        {
            save = CareerSaveSystem.Load();
            if (save != null)
                notice = "Career loaded. Resume or browse for a new club.";
        }

        private void OnGUI()
        {
            if (save != null && MatchIsRunning())
                return;

            InitStyles();
            HubTheme.DrawScreenBackground();

            var panel = HubTheme.CenteredPanel(820f, 680f, 16f);
            GUI.Box(panel, string.Empty, panelStyle);

            if (showHomeScreen)
            {
                DrawHomeScreen(panel);
                return;
            }
            if (confirmNewCareer)
            {
                DrawNewCareerConfirmation(panel);
                return;
            }
            if (showLeagueTable)
            {
                DrawLeagueTable(panel);
                return;
            }
            if (showCupCampaign)
            {
                DrawCupCampaign(panel);
                return;
            }
            if (showHonours)
            {
                DrawHonours(panel);
                return;
            }
            if (showSquadManagement)
            {
                DrawSquadManagement(panel);
                return;
            }
            if (showTransfers)
            {
                DrawTransfers(panel);
                return;
            }
            if (showPlayerDetails)
            {
                DrawPlayerDetails(panel);
                return;
            }
            if (showKitRoom)
            {
                DrawKitRoom(panel);
                return;
            }
            if (showTournamentHub)
            {
                DrawTournamentHub(panel);
                return;
            }
            if (showMatchPreview)
            {
                DrawMatchPreview(panel);
                return;
            }
            if (showSettingsScreen)
            {
                DrawSettingsScreen(panel);
                return;
            }
            if (showClubBrowser || save == null)
            {
                DrawClubBrowser(panel);
                return;
            }

            DrawDashboard(panel);
        }

        private void InitStyles()
        {
            if (styleScreenWidth != Screen.width || styleScreenHeight != Screen.height)
            {
                styleScreenWidth = Screen.width;
                styleScreenHeight = Screen.height;
                titleStyle = headingStyle = bodyStyle = smallStyle = noticeStyle = null;
                matchTeamStyle = matchVsStyle = null;
                buttonStyle = primaryButtonStyle = accentButtonStyle = dangerButtonStyle = null;
                highlightStyle = rowStyle = selectedRowStyle = panelStyle = null;
                cardStyle = heroCardStyle = hitboxStyle = backStyle = iconStyle = null;
                cardTitleStyle = cardValueStyle = cardCaptionStyle = dashboardCaptionStyle = null;
                tileGlyphStyle = tileLabelStyle = tileCaptionStyle = ovrStyle = null;
                statNameStyle = statValueStyle = pitchNameStyle = null;
                pitchOvrStyle = pitchPosStyle = subNameStyle = pitchStyle = pitchCardStyle = null;
                transferOvrStyle = transferNameStyle = null;
                tableHeaderStyle = tableHeaderNameStyle = tableNameStyle = null;
            }

            titleStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(24f, 24, 40), HubTheme.TextColor, TextAnchor.MiddleLeft, true, true);
            headingStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(30f, 19, 30), HubTheme.TextColor, TextAnchor.MiddleLeft, true, true);
            bodyStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(42f, 14, 22), HubTheme.SecondaryTextColor, TextAnchor.MiddleLeft, false, true);
            smallStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(56f, 11, 17), HubTheme.SecondaryTextColor, TextAnchor.MiddleLeft, false, true);
            noticeStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(58f, 11, 16), HubTheme.SecondaryTextColor, TextAnchor.MiddleCenter, false, true);
            matchTeamStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(34f, 16, 28), HubTheme.TextColor, TextAnchor.MiddleCenter, true, true);
            matchVsStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(46f, 16, 24), HubTheme.GoldTextColor, TextAnchor.MiddleCenter, true);
            var buttonFontSize = HubTheme.ResponsiveFontSize(44f, 14, 22);
            buttonStyle ??= HubTheme.SecondaryButton(buttonFontSize);
            primaryButtonStyle ??= HubTheme.PrimaryButton(buttonFontSize);
            accentButtonStyle ??= HubTheme.AccentButton(buttonFontSize);
            dangerButtonStyle ??= HubTheme.DangerButton(buttonFontSize);
            highlightStyle ??= new GUIStyle(smallStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = HubTheme.GoldTextColor }
            };
            tableHeaderStyle ??= new GUIStyle(smallStyle)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = HubTheme.TextColor }
            };
            tableNameStyle ??= new GUIStyle(smallStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(12, 6, 0, 0)
            };
            tableHeaderNameStyle ??= new GUIStyle(tableNameStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = HubTheme.TextColor }
            };
            var rowFontSize = HubTheme.ResponsiveFontSize(58f, 11, 18);
            rowStyle ??= HubTheme.Row(rowFontSize, false);
            selectedRowStyle ??= HubTheme.Row(rowFontSize, true);
            panelStyle ??= HubTheme.Panel();
            cardStyle ??= HubTheme.Card();
            heroCardStyle ??= HubTheme.HeroCard();
            hitboxStyle ??= HubTheme.Hitbox();
            backStyle ??= HubTheme.IconButton(HubTheme.ResponsiveFontSize(34f, 17, 26));
            iconStyle ??= HubTheme.IconButton(HubTheme.ResponsiveFontSize(42f, 13, 20));
            cardTitleStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(52f, 12, 18), HubTheme.SecondaryTextColor, TextAnchor.MiddleLeft, true);
            cardValueStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(34f, 18, 28), HubTheme.TextColor, TextAnchor.MiddleLeft, true);
            cardCaptionStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(62f, 10, 15), HubTheme.SecondaryTextColor, TextAnchor.MiddleLeft, false);
            dashboardCaptionStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(94f, 9, 11), HubTheme.SecondaryTextColor, TextAnchor.MiddleLeft, false);
            tileGlyphStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(44f, 14, 22), HubTheme.AccentTextColor, TextAnchor.MiddleCenter, true);
            tileLabelStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(52f, 12, 16), HubTheme.TextColor, TextAnchor.MiddleLeft, true);
            tileCaptionStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(64f, 10, 15), HubTheme.SecondaryTextColor, TextAnchor.MiddleLeft, false);
            ovrStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(44f, 16, 30), HubTheme.TextColor, TextAnchor.MiddleCenter, true);
            statNameStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(56f, 11, 17), HubTheme.SecondaryTextColor, TextAnchor.MiddleLeft, false);
            statValueStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(52f, 12, 19), HubTheme.TextColor, TextAnchor.MiddleRight, true);
            pitchNameStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(82f, 8, 11), HubTheme.TextColor, TextAnchor.MiddleCenter, true);
            pitchNameStyle.clipping = TextClipping.Clip;
            pitchOvrStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(88f, 8, 11), HubTheme.AccentTextColor, TextAnchor.MiddleLeft, true);
            pitchPosStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(92f, 7, 10), HubTheme.SecondaryTextColor, TextAnchor.MiddleRight, true);
            subNameStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(62f, 10, 15), HubTheme.TextColor, TextAnchor.MiddleLeft, true);
            pitchStyle ??= HubTheme.Pitch();
            pitchCardStyle ??= HubTheme.PitchCard();
            transferOvrStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(38f, 20, 30), HubTheme.TextColor, TextAnchor.MiddleLeft, true);
            transferNameStyle ??= HubTheme.Label(
                HubTheme.ResponsiveFontSize(62f, 11, 15), HubTheme.TextColor, TextAnchor.MiddleLeft, true);
            transferNameStyle.clipping = TextClipping.Clip;
            tableHeaderTexture ??= HubTheme.SolidTexture(HubTheme.TableHeaderColor);
            tableRowTexture ??= HubTheme.SolidTexture(HubTheme.TableRowColor);
            tableAltRowTexture ??= HubTheme.SolidTexture(HubTheme.TableAltRowColor);
            tableHighlightTexture ??= HubTheme.SolidTexture(HubTheme.HighlightColor);
        }

        private void DrawHomeScreen(Rect panel)
        {
            if (panel.height < 470f || panel.width < 600f)
            {
                DrawCompactHome(panel);
                return;
            }

            var x = panel.x + 20f;
            var width = panel.width - 40f;
            var club = save == null ? null : CareerLeagueSystem.GetManagedClub(save);
            var league = save == null ? null : CareerLeagueSystem.GetManagedLeague(save);
            var country = league == null ? null : WorldDatabase.GetCountry(league.CountryId);

            GUI.Label(new Rect(x, panel.y + 14f, width * 0.62f, 46f), "SOCCER 27", titleStyle);
            GUI.Label(new Rect(x, panel.y + 58f, width * 0.62f, 20f), "YOUR CLUB. YOUR STORY.", tileCaptionStyle);

            var chipFont = HubTheme.ResponsiveFontSize(62f, 10, 14);
            var moneyChipWidth = Mathf.Clamp(width * 0.17f, 120f, 150f);
            var seasonChipWidth = Mathf.Clamp(width * 0.25f, 152f, 208f);
            var chipHeight = 32f;
            var chipY = panel.y + 26f;
            if (save != null)
            {
                HubTheme.Chip(new Rect(x + width - moneyChipWidth - seasonChipWidth - 10f, chipY,
                    moneyChipWidth, chipHeight),
                    CareerTransferSystem.FormatMoney(save.transferBudget), HubTheme.GoldTextColor, chipFont);
                HubTheme.Chip(new Rect(x + width - seasonChipWidth, chipY, seasonChipWidth, chipHeight),
                    $"SEASON {save.season}  •  WEEK {save.matchweek}", HubTheme.AccentTextColor, chipFont);
            }
            else
            {
                HubTheme.Chip(new Rect(x + width - seasonChipWidth, chipY, seasonChipWidth, chipHeight),
                    "NEW CAREER", HubTheme.AccentTextColor, chipFont);
            }

            HubTheme.Divider(new Rect(x, panel.y + 92f, width, 1f), new Color32(0x1B, 0x4D, 0x70, 0xFF));

            var railWidth = Mathf.Clamp(width * 0.52f, 300f, 420f);
            var railX = x;
            var y = panel.y + 108f;
            var heroHeight = Mathf.Clamp(panel.height * 0.115f, 64f, 88f);
            if (DrawMenuTile(new Rect(railX, y, railWidth, heroHeight), "P", "PLAY MATCH", null, true, true))
            {
                if (save == null)
                    StartQuickMatch();
                else
                {
                    showHomeScreen = false;
                    showMatchPreview = true;
                }
            }

            y += heroHeight + 10f;
            var careerHeight = Mathf.Clamp(panel.height * 0.095f, 52f, 72f);
            if (DrawMenuTile(new Rect(railX, y, railWidth, careerHeight), "C", "CAREER  •  BUILD YOUR CLUB", null, false))
            {
                showHomeScreen = false;
                if (save == null)
                    showClubBrowser = true;
            }

            y += careerHeight + 8f;
            var halfWidth = (railWidth - 8f) * 0.5f;
            var tileHeight = Mathf.Clamp(panel.height * 0.075f, 44f, 60f);
            if (DrawMenuTile(new Rect(railX, y, halfWidth, tileHeight), "S", "SQUAD", null, false))
                OpenCareerPage(() => showSquadManagement = true, "squad management");
            if (DrawMenuTile(new Rect(railX + halfWidth + 8f, y, halfWidth, tileHeight), "T", "TOURNAMENTS", null, false))
                OpenCareerPage(() => showTournamentHub = true, "tournaments");

            y += tileHeight + 8f;
            if (DrawMenuTile(new Rect(railX, y, halfWidth, tileHeight), "K", "KIT ROOM", null, false))
            {
                selectedKitSlot = 0;
                OpenCareerPage(() => showKitRoom = true, "the kit room");
            }
            if (DrawMenuTile(new Rect(railX + halfWidth + 8f, y, halfWidth, tileHeight), "M", "PLAYER MARKET", null, false))
                OpenCareerPage(OpenTransfers, "the player market");

            var settingsHeight = Mathf.Clamp(panel.height * 0.06f, 38f, 50f);
            var settingsY = panel.yMax - settingsHeight - 18f;
            if (DrawMenuTile(new Rect(railX, settingsY, railWidth, settingsHeight), "G", "SETTINGS", null, false))
            {
                showHomeScreen = false;
                showSettingsScreen = true;
            }

            var rightX = x + railWidth + 14f;
            var rightWidth = width - railWidth - 14f;
            var rightTop = panel.y + 108f;
            var rightBottom = panel.yMax - 18f;
            var next = save == null ? null : CareerLeagueSystem.GetNextFixture(save);
            var hasFixture = next != null && next.IsValid;
            var heroCardHeight = hasFixture
                ? (rightBottom - rightTop) * 0.60f
                : rightBottom - rightTop;
            DrawHomeClubCard(new Rect(rightX, rightTop, rightWidth, heroCardHeight), club, league, country);
            if (hasFixture)
            {
                DrawNextFixtureCard(new Rect(rightX, rightTop + heroCardHeight + 10f, rightWidth,
                    rightBottom - rightTop - heroCardHeight - 10f), next);
            }
        }

        private void DrawCompactHome(Rect panel)
        {
            var x = panel.x + 24f;
            var width = panel.width - 48f;
            var compact = panel.height < 440f;
            var brandY = panel.y + (compact ? 12f : 24f);
            var brandHeight = compact ? 40f : 54f;
            GUI.Label(new Rect(x, brandY, width, brandHeight), "SOCCER 27", titleStyle);
            GUI.Label(new Rect(x, brandY + brandHeight, width, 22f), "YOUR CLUB. YOUR STORY.", tileCaptionStyle);

            var gap = compact ? 7f : 9f;
            var y = panel.y + (compact ? 80f : 110f);
            var heroHeight = compact ? 42f : 58f;
            if (GUI.Button(new Rect(x, y, width, heroHeight), "PLAY MATCH", primaryButtonStyle))
            {
                if (save == null)
                    StartQuickMatch();
                else
                {
                    showHomeScreen = false;
                    showMatchPreview = true;
                }
            }

            y += heroHeight + gap;
            if (GUI.Button(new Rect(x, y, width, heroHeight), "CAREER  •  BUILD YOUR CLUB", accentButtonStyle))
            {
                showHomeScreen = false;
                if (save == null)
                    showClubBrowser = true;
            }

            y += heroHeight + gap + 2f;
            var cardGap = 7f;
            var cardWidth = (width - cardGap) * 0.5f;
            var cardHeight = compact ? 36f : 46f;
            if (GUI.Button(new Rect(x, y, cardWidth, cardHeight), "SQUAD", buttonStyle))
                OpenCareerPage(() => showSquadManagement = true, "squad management");
            if (GUI.Button(new Rect(x + cardWidth + cardGap, y, cardWidth, cardHeight), "TOURNAMENTS", buttonStyle))
                OpenCareerPage(() => showTournamentHub = true, "tournaments");

            y += cardHeight + cardGap;
            if (GUI.Button(new Rect(x, y, cardWidth, cardHeight), "KIT ROOM", buttonStyle))
            {
                selectedKitSlot = 0;
                OpenCareerPage(() => showKitRoom = true, "the kit room");
            }
            if (GUI.Button(new Rect(x + cardWidth + cardGap, y, cardWidth, cardHeight), "PLAYER MARKET", buttonStyle))
                OpenCareerPage(OpenTransfers, "the player market");

            var settingsHeight = compact ? 32f : 40f;
            var settingsY = panel.yMax - settingsHeight - (compact ? 12f : 18f);
            if (GUI.Button(new Rect(x, settingsY, width, settingsHeight), "SETTINGS", buttonStyle))
            {
                showHomeScreen = false;
                showSettingsScreen = true;
            }
        }

        private void DrawHomeClubCard(Rect card, WorldClub club, WorldLeague league, WorldCountry country)
        {
            GUI.Box(card, GUIContent.none, heroCardStyle);
            var pad = 16f;
            var crestSize = Mathf.Clamp(card.width * 0.22f, 52f, 84f);
            var crestRect = new Rect(card.x + pad, card.y + pad, crestSize, crestSize);
            var crestFont = HubTheme.ResponsiveFontSize(58f, 11, 18);
            if (club != null)
            {
                HubTheme.Crest(crestRect, ToColor32(club.PrimaryColor), ToColor32(club.SecondaryColor),
                    club.ShortName, crestFont);
            }
            else
            {
                HubTheme.Crest(crestRect, new Color32(0x1D, 0x5A, 0x84, 0xFF),
                    new Color32(0x4F, 0xDB, 0xFF, 0xFF), "?", crestFont);
            }

            var textX = crestRect.xMax + 12f;
            var textWidth = Mathf.Max(20f, card.xMax - pad - textX);
            var name = club != null ? club.Name : "START A CAREER";
            GUI.Label(new Rect(textX, card.y + pad, textWidth, 34f), name, cardValueStyle);
            var subtitle = club == null
                ? "Choose from 96 national top divisions"
                : $"{league?.Name ?? string.Empty}  •  {country?.Name ?? club.CountryId}";
            GUI.Label(new Rect(textX, card.y + pad + 32f, textWidth, 22f), subtitle, cardCaptionStyle);

            var statsY = card.y + pad + crestSize + 12f;
            HubTheme.Divider(new Rect(card.x + pad, statsY, card.width - pad * 2f, 1f),
                new Color32(0x2A, 0x6C, 0x94, 0xFF));
            statsY += 10f;

            var squad = club == null ? Array.Empty<WorldPlayer>() : CareerTransferSystem.GetSquad(save, club.Id);
            var rating = SquadRating(squad);
            GUI.Label(new Rect(card.x + pad, statsY, card.width * 0.45f, 20f), "SQUAD RATING", cardTitleStyle);
            GUI.Label(new Rect(card.x + pad, statsY + 18f, card.width * 0.45f, 52f),
                rating.ToString(), ovrStyle);

            var record = save == null
                ? $"SQUAD {squad.Length}/{CareerTransferSystem.MaxSquadSize}"
                : $"W {save.wins}   D {save.draws}   L {save.losses}";
            GUI.Label(new Rect(card.x + pad + card.width * 0.45f, statsY, card.width * 0.55f - pad, 20f),
                "CAREER RECORD", cardTitleStyle);
            GUI.Label(new Rect(card.x + pad + card.width * 0.45f, statsY + 18f,
                card.width * 0.55f - pad, 26f), record, tileLabelStyle);
            var detail = save == null
                ? "Browse the world to begin."
                : $"Budget {CareerTransferSystem.FormatMoney(save.transferBudget)}";
            GUI.Label(new Rect(card.x + pad + card.width * 0.45f, statsY + 44f,
                card.width * 0.55f - pad, 22f), detail, cardCaptionStyle);
        }

        private void DrawNextFixtureCard(Rect card, CareerNextFixture next)
        {
            GUI.Box(card, GUIContent.none, cardStyle);
            var pad = 12f;
            var opponent = WorldDatabase.GetClub(next.OpponentClubId);
            var competition = next.Kind == CareerFixtureKind.Cup
                ? CareerCupSystem.CompetitionName(save)
                : CareerLeagueSystem.GetManagedLeague(save)?.Name ?? "League";
            GUI.Label(new Rect(card.x + pad, card.y + pad, card.width - pad * 2f, 20f),
                "NEXT MATCH", cardTitleStyle);

            var crestSize = Mathf.Clamp(card.height * 0.34f, 26f, 42f);
            var crestRect = new Rect(card.x + pad, card.y + pad + 24f, crestSize, crestSize);
            if (opponent != null)
            {
                HubTheme.Crest(crestRect, ToColor32(opponent.PrimaryColor), ToColor32(opponent.SecondaryColor),
                    opponent.ShortName, HubTheme.ResponsiveFontSize(70f, 8, 13));
            }

            var textX = crestRect.xMax + 10f;
            var textWidth = Mathf.Max(20f, card.xMax - pad - textX);
            GUI.Label(new Rect(textX, card.y + pad + 24f, textWidth, 24f),
                $"{(next.UserIsHome ? "vs" : "at")} {opponent?.Name ?? "TBC"}", tileLabelStyle);
            GUI.Label(new Rect(textX, card.y + pad + 48f, textWidth, 18f),
                $"{competition}  •  WEEK {Mathf.Clamp(save.matchweek, 1, WorldDatabase.MatchweeksPerSeason)}",
                cardCaptionStyle);

            var buttonHeight = Mathf.Clamp(card.height * 0.24f, 30f, 42f);
            var playLabel = next.Kind == CareerFixtureKind.Cup ? "PLAY CUP TIE" : "PLAY NEXT MATCH";
            if (GUI.Button(new Rect(card.x + pad, card.yMax - buttonHeight - pad,
                card.width - pad * 2f, buttonHeight), playLabel, primaryButtonStyle))
                showMatchPreview = true;
        }

        /// <summary>Card-styled menu entry with an icon tile; returns true when clicked.</summary>
        private bool DrawMenuTile(Rect rect, string glyph, string label, string caption, bool hero, bool playIcon = false)
        {
            GUI.Box(rect, GUIContent.none, hero ? heroCardStyle : cardStyle);
            var pad = Mathf.Clamp(rect.height * 0.16f, 7f, 14f);
            var iconSize = Mathf.Clamp(rect.height - pad * 2f, 22f, 52f);
            var iconRect = new Rect(rect.x + pad, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
            GUI.Box(iconRect, GUIContent.none, iconStyle);

            if (playIcon)
            {
                var previous = GUI.color;
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(iconRect.x + iconSize * 0.33f, iconRect.y + iconSize * 0.26f,
                    iconSize * 0.38f, iconSize * 0.48f), HubTheme.PlayTexture());
                GUI.color = previous;
            }
            else
            {
                GUI.Label(iconRect, glyph, tileGlyphStyle);
            }

            var textX = iconRect.xMax + pad;
            var textWidth = Mathf.Max(10f, rect.xMax - textX - pad);
            var labelHeight = Mathf.Clamp(rect.height * 0.44f, 16f, 26f);
            var captionHeight = Mathf.Clamp(rect.height * 0.3f, 10f, 18f);
            var hasCaption = !string.IsNullOrEmpty(caption);
            var stackHeight = labelHeight + (hasCaption ? captionHeight + 2f : 0f);
            var textY = rect.y + (rect.height - stackHeight) * 0.5f;
            GUI.Label(new Rect(textX, textY, textWidth, labelHeight), label,
                hero ? cardValueStyle : tileLabelStyle);
            if (hasCaption)
                GUI.Label(new Rect(textX, textY + labelHeight + 2f, textWidth, captionHeight), caption, tileCaptionStyle);
            return GUI.Button(rect, GUIContent.none, hitboxStyle);
        }

        /// <summary>Top bar used by every career screen; returns the first free y below it.</summary>
        private float DrawHeader(Rect panel, string title, string subtitle, bool showBack, Action back,
            float rightReserve = 0f, GUIStyle subtitleStyle = null)
        {
            var pad = 16f;
            var top = panel.y + 12f;
            var backSize = Mathf.Clamp(panel.height * 0.075f, 34f, 46f);
            var textX = panel.x + pad;
            if (showBack)
            {
                if (GUI.Button(new Rect(panel.x + pad, top, backSize, backSize), "‹", backStyle) && back != null)
                    back();
                textX += backSize + 10f;
            }

            var titleHeight = Mathf.Clamp(panel.height * 0.055f, 24f, 36f);
            var textWidth = Mathf.Max(20f, panel.width - (textX - panel.x) - pad - rightReserve);
            GUI.Label(new Rect(textX, top - 2f, textWidth, titleHeight), title, headingStyle);
            if (!string.IsNullOrEmpty(subtitle))
                GUI.Label(new Rect(textX, top + titleHeight - 5f, textWidth, 20f), subtitle,
                    subtitleStyle ?? cardCaptionStyle);

            var lineY = top + Mathf.Max(backSize, titleHeight + (string.IsNullOrEmpty(subtitle) ? 0f : 18f)) + 6f;
            HubTheme.Divider(new Rect(panel.x + pad, lineY, panel.width - pad * 2f, 1f),
                new Color32(0x1B, 0x4D, 0x70, 0xFF));
            return lineY + 10f;
        }

        private static int SquadRating(WorldPlayer[] squad)
        {
            if (squad == null || squad.Length == 0)
                return 0;
            var selected = squad.Where(player => player != null)
                .OrderByDescending(player => player.Overall)
                .Take(11)
                .ToArray();
            if (selected.Length == 0)
                return 0;
            var total = 0;
            foreach (var player in selected)
                total += player.Overall;
            return Mathf.Clamp(Mathf.RoundToInt(total / (float)selected.Length), 1, 99);
        }

        private static Color32 ToColor32(WorldColor color)
        {
            return new Color32(
                (byte)Mathf.Clamp(color.R * 255f, 0f, 255f),
                (byte)Mathf.Clamp(color.G * 255f, 0f, 255f),
                (byte)Mathf.Clamp(color.B * 255f, 0f, 255f),
                0xFF);
        }

        private void OpenCareerPage(Action openPage, string pageName)
        {
            showHomeScreen = false;
            if (save == null)
            {
                showClubBrowser = true;
                notice = "Choose a club before opening " + pageName + ".";
                return;
            }

            openPage();
        }

        private void StartQuickMatch()
        {
            showHomeScreen = false;
            enabled = false;
            if (GetComponent<MatchManager3D>() == null)
                gameObject.AddComponent<MatchManager3D>();
        }

        private void DrawSettingsScreen(Rect panel)
        {
            var x = panel.x + 18f;
            var width = panel.width - 36f;
            var y = DrawHeader(panel, "SETTINGS", null, true, () =>
            {
                showSettingsScreen = false;
                showHomeScreen = true;
            });

            var rowHeight = Mathf.Clamp(panel.height * 0.12f, 40f, 70f);
            var gap = 8f;
            var settings = new[]
            {
                "MATCH CONTROLS",
                "Mobile: left stick to move, right buttons to pass, shoot, tackle and sprint.",
                "Desktop: WASD or arrows to move; Shift to sprint; Space to shoot; E to pass; X to tackle.",
                "MATCH DISPLAY",
                "The match automatically uses landscape orientation on supported mobile devices."
            };
            for (var i = 0; i < settings.Length; i++)
            {
                var emphasis = i == 0 || i == 3;
                var height = emphasis ? rowHeight * 0.72f : Mathf.Max(38f, rowHeight - 10f);
                var rect = new Rect(x, y, width, height);
                GUI.Box(rect, GUIContent.none, emphasis ? heroCardStyle : cardStyle);
                GUI.Label(new Rect(rect.x + 14f, rect.y, rect.width - 28f, rect.height), settings[i],
                    emphasis ? tileLabelStyle : bodyStyle);
                y += height + gap;
            }

            var footerY = panel.yMax - 54f;
            if (GUI.Button(new Rect(x, footerY, width, 44f), "BACK TO SOCCER 27", primaryButtonStyle))
            {
                showSettingsScreen = false;
                showHomeScreen = true;
            }
        }

        private void DrawDashboard(Rect panel)
        {
            var x = panel.x + 24f;
            var width = panel.width - 48f;
            var y = panel.y + 10f;

            var club = CareerLeagueSystem.GetManagedClub(save);
            var league = CareerLeagueSystem.GetManagedLeague(save);
            var country = league == null ? null : WorldDatabase.GetCountry(league.CountryId);
            if (club == null || league == null)
            {
                notice = "This save points at a club that no longer exists.";
                showClubBrowser = true;
                return;
            }

            var ranked = CareerLeagueSystem.GetRankedStandings(save);
            var position = 0;
            for (var i = 0; i < ranked.Length; i++)
            {
                if (ranked[i].clubId == save.managedClubId)
                {
                    position = i + 1;
                    break;
                }
            }
            var next = CareerLeagueSystem.GetNextFixture(save);
            var opponent = next.IsValid ? WorldDatabase.GetClub(next.OpponentClubId) : null;
            var competition = next.Kind == CareerFixtureKind.Cup
                ? $"{CareerCupSystem.CompetitionName(save)} {CareerCupSystem.ShortRoundName(next.Round)}"
                : "League";
            if (panel.height < 460f || width < 470f)
            {
                DrawCompactDashboard(panel, x, width, club, league, country, position, ranked.Length, next, opponent, competition);
                return;
            }

            var headerY = DrawHeader(panel,
                club.Name.ToUpperInvariant(),
                $"{league.Name}  •  {country?.Name ?? club.CountryId}  •  SEASON {save.season}  •  WEEK {save.matchweek}/{WorldDatabase.MatchweeksPerSeason}  •  POSITION {position}/{ranked.Length}",
                true, () => showHomeScreen = true, 352f, dashboardCaptionStyle);

            var chipFont = HubTheme.ResponsiveFontSize(62f, 10, 15);
            var chipWidth = Mathf.Clamp(width * 0.21f, 132f, 168f);
            HubTheme.Chip(new Rect(x + width - chipWidth * 2f - 8f, panel.y + 18f, chipWidth, 30f),
                CareerTransferSystem.FormatMoney(save.transferBudget), HubTheme.GoldTextColor, chipFont);
            HubTheme.Chip(new Rect(x + width - chipWidth, panel.y + 18f, chipWidth, 30f),
                $"W {save.wins}   D {save.draws}   L {save.losses}", HubTheme.AccentTextColor, chipFont);

            var tabGap = 6f;
            var tabHeight = Mathf.Clamp(panel.height * 0.06f, 28f, 38f);
            var tabWidth = (width - tabGap * 3f) / 4f;
            if (GUI.Button(new Rect(x, headerY, tabWidth, tabHeight), "OVERVIEW", accentButtonStyle))
            {
            }
            if (GUI.Button(new Rect(x + tabWidth + tabGap, headerY, tabWidth, tabHeight), "STANDINGS", buttonStyle))
                showLeagueTable = true;
            if (GUI.Button(new Rect(x + (tabWidth + tabGap) * 2f, headerY, tabWidth, tabHeight), "TRANSFERS", buttonStyle))
                OpenTransfers();
            if (GUI.Button(new Rect(x + (tabWidth + tabGap) * 3f, headerY, tabWidth, tabHeight), "SQUAD", buttonStyle))
                showSquadManagement = true;

            y = headerY + tabHeight + 8f;
            var gap = 10f;
            var cardHeight = Mathf.Clamp(panel.height * 0.31f, 116f, 196f);
            var half = (width - gap) / 2f;
            DrawSeasonCard(new Rect(x, y, half, cardHeight), club, league, position, ranked.Length);
            DrawNextFixtureCard(new Rect(x + half + gap, y, half, cardHeight), next);

            y += cardHeight + 10f;
            var footerHeight = 30f;
            var buttonsBottom = panel.yMax - footerHeight - 12f;
            var buttonHeight = Mathf.Max(38f, (buttonsBottom - y - 8f) / 2f);
            var cardWidth = (width - gap * 2f) / 3f;
            if (DrawMenuTile(new Rect(x, y, cardWidth, buttonHeight), "S", "SQUAD & TACTICS", null, false))
                showSquadManagement = true;
            if (DrawMenuTile(new Rect(x + cardWidth + gap, y, cardWidth, buttonHeight), "M", "PLAYER MARKET", null, false))
                OpenTransfers();
            if (DrawMenuTile(new Rect(x + (cardWidth + gap) * 2f, y, cardWidth, buttonHeight), "T", "TOURNAMENTS", null, false))
                showTournamentHub = true;
            var secondRowY = y + buttonHeight + 8f;
            if (DrawMenuTile(new Rect(x, secondRowY, cardWidth, buttonHeight), "K", "KIT ROOM", null, false))
            {
                selectedKitSlot = 0;
                showKitRoom = true;
            }
            if (DrawMenuTile(new Rect(x + cardWidth + gap, secondRowY, cardWidth, buttonHeight), "H", "MY CLUB", null, false))
            {
                honoursScroll = Vector2.zero;
                showHonours = true;
            }
            if (DrawMenuTile(new Rect(x + (cardWidth + gap) * 2f, secondRowY, cardWidth, buttonHeight), "W", "BROWSE WORLD", null, false))
            {
                browserCountryId = string.Empty;
                showClubBrowser = true;
            }

            if (!string.IsNullOrEmpty(save.lastTransferSummary) && panel.height >= 600f)
                GUI.Label(new Rect(x, panel.yMax - 46f, width, 18f), save.lastTransferSummary, cardCaptionStyle);
            GUI.Label(new Rect(x, panel.yMax - 28f, width, 20f), notice, noticeStyle);
        }

        private void DrawSeasonCard(Rect card, WorldClub club, WorldLeague league, int position, int teamCount)
        {
            GUI.Box(card, GUIContent.none, cardStyle);
            var pad = 14f;
            GUI.Label(new Rect(card.x + pad, card.y + pad, card.width - pad * 2f, 20f),
                $"SEASON {save.season}", cardTitleStyle);

            var crestSize = Mathf.Clamp(card.height * 0.24f, 34f, 52f);
            var crestRect = new Rect(card.x + pad, card.y + pad + 26f, crestSize, crestSize);
            HubTheme.Crest(crestRect, ToColor32(club.PrimaryColor), ToColor32(club.SecondaryColor),
                club.ShortName, HubTheme.ResponsiveFontSize(70f, 8, 13));
            GUI.Label(new Rect(crestRect.xMax + 10f, card.y + pad + 24f,
                card.width - pad * 2f - crestSize - 10f, 24f), league?.Name ?? "League", tileLabelStyle);
            GUI.Label(new Rect(crestRect.xMax + 10f, card.y + pad + 48f,
                card.width - pad * 2f - crestSize - 10f, 20f),
                $"{Ordinal(position)} of {teamCount}  •  {save.formationId}", cardCaptionStyle);

            var statsY = card.y + pad + 26f + crestSize + 8f;
            HubTheme.Divider(new Rect(card.x + pad, statsY, card.width - pad * 2f, 1f),
                new Color32(0x2A, 0x6C, 0x94, 0xFF));
            statsY += 8f;
            var statWidth = (card.width - pad * 2f) / 3f;
            DrawCardStat(new Rect(card.x + pad, statsY, statWidth, 44f), "BUDGET",
                CareerTransferSystem.FormatMoney(save.transferBudget));
            DrawCardStat(new Rect(card.x + pad + statWidth, statsY, statWidth, 44f), "GOAL DIFF",
                GoalDifference().ToString("+0;-0;0"));
            DrawCardStat(new Rect(card.x + pad + statWidth * 2f, statsY, statWidth, 44f), "POINTS",
                LeaguePoints().ToString());
        }

        private void DrawCardStat(Rect rect, string label, string value)
        {
            GUI.Label(new Rect(rect.x, rect.y, rect.width, 18f), label, cardCaptionStyle);
            GUI.Label(new Rect(rect.x, rect.y + 16f, rect.width, 26f), value, tileLabelStyle);
        }

        private int GoalDifference()
        {
            foreach (var standing in CareerLeagueSystem.GetRankedStandings(save))
            {
                if (standing.clubId == save.managedClubId)
                    return standing.GoalDifference;
            }
            return 0;
        }

        private int LeaguePoints()
        {
            var ranked = CareerLeagueSystem.GetRankedStandings(save);
            foreach (var standing in ranked)
            {
                if (standing.clubId == save.managedClubId)
                    return standing.Points;
            }
            return save.wins * 3 + save.draws;
        }

        private static string Ordinal(int value)
        {
            var suffix = value % 100;
            if (suffix >= 11 && suffix <= 13)
                return value + "th";
            switch (value % 10)
            {
                case 1: return value + "st";
                case 2: return value + "nd";
                case 3: return value + "rd";
                default: return value + "th";
            }
        }

        private void DrawCompactDashboard(Rect panel, float x, float width, WorldClub club, WorldLeague league,
            WorldCountry country, int position, int teamCount, CareerNextFixture next, WorldClub opponent, string competition)
        {
            var headerY = DrawHeader(panel, club.Name.ToUpperInvariant(),
                $"{league.Name}  •  {country?.Name ?? club.CountryId}  •  {position}/{teamCount}  •  W {save.wins} D {save.draws} L {save.losses}",
                true, () => showHomeScreen = true);
            var viewport = new Rect(x, headerY, width, panel.yMax - headerY - 46f);
            var contentWidth = width - 18f;
            var gap = 8f;
            var cardWidth = (contentWidth - gap) / 2f;
            var actionHeight = 52f;
            var fixtureHeight = 74f;
            var contentHeight = fixtureHeight + 10f + 3f * (actionHeight + gap);
            dashboardScroll = GUI.BeginScrollView(viewport, dashboardScroll,
                new Rect(0f, 0f, contentWidth, contentHeight));
            var fixtureText = opponent == null ? "No fixture scheduled" :
                $"{competition}  •  {(next.UserIsHome ? "vs" : "at")} {opponent.Name}";
            var playText = next.Kind == CareerFixtureKind.Cup && next.IsValid ? "PLAY CUP TIE" : "PLAY NEXT MATCH";
            if (GUI.Button(new Rect(0f, 0f, contentWidth, fixtureHeight), $"{playText}\n{fixtureText}", primaryButtonStyle))
                showMatchPreview = true;
            var cardY = fixtureHeight + 10f;
            if (GUI.Button(new Rect(0f, cardY, cardWidth, actionHeight), "SQUAD & TACTICS", buttonStyle)) showSquadManagement = true;
            if (GUI.Button(new Rect(cardWidth + gap, cardY, cardWidth, actionHeight), "PLAYER MARKET", buttonStyle)) OpenTransfers();
            cardY += actionHeight + gap;
            if (GUI.Button(new Rect(0f, cardY, cardWidth, actionHeight), "COMPETITIONS", buttonStyle)) showTournamentHub = true;
            if (GUI.Button(new Rect(cardWidth + gap, cardY, cardWidth, actionHeight), "KIT ROOM", buttonStyle))
            {
                selectedKitSlot = 0;
                showKitRoom = true;
            }
            cardY += actionHeight + gap;
            if (GUI.Button(new Rect(0f, cardY, cardWidth, actionHeight), "MY CLUB", buttonStyle))
            {
                honoursScroll = Vector2.zero;
                showHonours = true;
            }
            if (GUI.Button(new Rect(cardWidth + gap, cardY, cardWidth, actionHeight), "BROWSE WORLD", buttonStyle))
            {
                browserCountryId = string.Empty;
                showClubBrowser = true;
            }
            GUI.EndScrollView();
            GUI.Label(new Rect(x, panel.yMax - 34f, width, 22f), notice, noticeStyle);
        }

        private void DrawPlayerDetails(Rect panel)
        {
            var x = panel.x + 24f;
            var width = panel.width - 48f;
            if (selectedPlayer == null)
            {
                showPlayerDetails = false;
                showSquadManagement = true;
                return;
            }
            var contentTop = DrawHeader(panel, "PLAYER PROFILE", null, true, () =>
            {
                showPlayerDetails = false;
                showSquadManagement = true;
            });
            var footerY = panel.yMax - 58f;
            var contentHeight = Mathf.Max(120f, footerY - contentTop - 8f);
            var leftWidth = Mathf.Clamp(width * 0.36f, 210f, 300f);
            var profileCard = new Rect(x, contentTop, leftWidth, contentHeight);
            DrawPlayerProfileCard(profileCard);

            var rightX = profileCard.xMax + 12f;
            var rightWidth = Mathf.Max(60f, panel.xMax - 24f - rightX);
            GUI.Label(new Rect(rightX, contentTop + 2f, rightWidth, 32f),
                selectedPlayer.Name.ToUpperInvariant(), cardValueStyle);
            GUI.Label(new Rect(rightX, contentTop + 34f, rightWidth, 22f),
                $"{PositionLabel(selectedPlayer.Position)}   •   #{selectedPlayer.ShirtNumber}   •   AGE {selectedPlayer.Age}   •   {selectedPlayer.HeightCm} CM   •   {(selectedPlayer.LeftFooted ? "LEFT" : "RIGHT")} FOOT",
                cardCaptionStyle);
            HubTheme.Divider(new Rect(rightX, contentTop + 62f, rightWidth, 1f),
                new Color32(0x1B, 0x4D, 0x70, 0xFF));

            var stats = new[]
            {
                ("PACE", selectedPlayer.Pace),
                ("SHOOTING", selectedPlayer.Shooting),
                ("PASSING", selectedPlayer.Passing),
                ("DEFENDING", selectedPlayer.Defending),
                ("PHYSICAL", selectedPlayer.Physical)
            };
            var statsTop = contentTop + 72f;
            var rowHeight = Mathf.Clamp(panel.height * 0.085f, 38f, 58f);
            var viewportHeight = Mathf.Max(40f, footerY - statsTop - 6f);
            playerStatsScroll = GUI.BeginScrollView(new Rect(rightX, statsTop, rightWidth, viewportHeight),
                playerStatsScroll, new Rect(0f, 0f, rightWidth - 16f, stats.Length * rowHeight));
            for (var i = 0; i < stats.Length; i++)
            {
                var rect = new Rect(0f, i * rowHeight, rightWidth - 16f, rowHeight - 6f);
                GUI.Box(rect, GUIContent.none, cardStyle);
                GUI.Label(new Rect(rect.x + 12f, rect.y + 4f, rect.width * 0.6f, 20f), stats[i].Item1, cardTitleStyle);
                GUI.Label(new Rect(rect.xMax - 70f, rect.y + 2f, 58f, 24f), stats[i].Item2.ToString(), statValueStyle);
                var bar = new Rect(rect.x + 12f, rect.yMax - 16f, rect.width - 24f, 6f);
                HubTheme.Bar(bar, stats[i].Item2 / 99f, HubTheme.AccentColor);
            }
            GUI.EndScrollView();

            if (GUI.Button(new Rect(x, footerY, width, 46f), "BACK TO SQUAD", buttonStyle))
            {
                showPlayerDetails = false;
                showSquadManagement = true;
            }
        }

        private void DrawPlayerProfileCard(Rect card)
        {
            GUI.Box(card, GUIContent.none, heroCardStyle);
            var pad = 16f;
            var club = WorldDatabase.GetClub(selectedPlayer.ClubId);
            if (club == null && save != null)
                club = CareerLeagueSystem.GetManagedClub(save);

            GUI.Label(new Rect(card.x + pad, card.y + pad, card.width * 0.5f, 44f),
                selectedPlayer.Overall.ToString(), HubTheme.Label(
                    HubTheme.ResponsiveFontSize(22f, 30, 52), HubTheme.TextColor, TextAnchor.MiddleLeft, true));
            GUI.Label(new Rect(card.x + pad, card.y + pad + 42f, card.width * 0.5f, 18f), "OVR", cardTitleStyle);

            if (club != null)
            {
                var crestSize = Mathf.Clamp(card.width * 0.26f, 36f, 64f);
                HubTheme.Crest(new Rect(card.xMax - pad - crestSize, card.y + pad, crestSize, crestSize),
                    ToColor32(club.PrimaryColor), ToColor32(club.SecondaryColor), club.ShortName,
                    HubTheme.ResponsiveFontSize(66f, 8, 13));
            }

            var nameY = card.y + pad + 70f;
            GUI.Label(new Rect(card.x + pad, nameY, card.width - pad * 2f, 30f),
                selectedPlayer.Name, cardValueStyle);
            GUI.Label(new Rect(card.x + pad, nameY + 30f, card.width - pad * 2f, 20f),
                $"{PositionLabel(selectedPlayer.Position)}  •  {club?.Name ?? "Free agent"}", cardCaptionStyle);

            var dividerY = nameY + 58f;
            HubTheme.Divider(new Rect(card.x + pad, dividerY, card.width - pad * 2f, 1f),
                new Color32(0x2A, 0x6C, 0x94, 0xFF));

            var rows = new[]
            {
                ("AGE", selectedPlayer.Age.ToString()),
                ("HEIGHT", selectedPlayer.HeightCm + " CM"),
                ("FOOT", selectedPlayer.LeftFooted ? "LEFT" : "RIGHT"),
                ("SHIRT", "#" + selectedPlayer.ShirtNumber)
            };
            var rowY = dividerY + 10f;
            var rowHeight = Mathf.Clamp(card.height * 0.085f, 24f, 38f);
            for (var i = 0; i < rows.Length && rowY + rowHeight <= card.yMax - 6f; i++)
            {
                GUI.Label(new Rect(card.x + pad, rowY, card.width * 0.5f, rowHeight), rows[i].Item1, cardTitleStyle);
                GUI.Label(new Rect(card.xMax - pad - card.width * 0.42f, rowY, card.width * 0.42f, rowHeight),
                    rows[i].Item2, statValueStyle);
                rowY += rowHeight;
            }
        }

        private void DrawKitRoom(Rect panel)
        {
            var x = panel.x + 18f;
            var width = panel.width - 36f;
            var club = CareerLeagueSystem.GetManagedClub(save);
            var headerY = DrawHeader(panel, "KIT ROOM",
                club.Name + "  •  preview and save club palettes", true, () => showKitRoom = false);
            var gap = 8f;
            var slotW = (width - gap * 3f) / 4f;
            var tabHeight = Mathf.Clamp(panel.height * 0.062f, 30f, 40f);
            for (var i = 0; i < CareerKitSystem.SlotNames.Length; i++)
            {
                if (GUI.Button(new Rect(x + i * (slotW + gap), headerY, slotW, tabHeight),
                    CareerKitSystem.SlotNames[i], HubTheme.Tab(
                        HubTheme.ResponsiveFontSize(50f, 11, 17), selectedKitSlot == i)))
                {
                    selectedKitSlot = i;
                    if (i < 3)
                        CareerKitSystem.SetActiveMatchSlot(club.Id, i);
                }
            }

            var footerY = panel.yMax - 56f;
            var presetHeight = Mathf.Clamp(panel.height * 0.11f, 50f, 74f);
            var presetsY = footerY - presetHeight - 10f;
            var previewY = headerY + tabHeight + 10f;
            var previewHeight = Mathf.Max(90f, presetsY - previewY - 26f);
            var preview = new Rect(x, previewY, width, previewHeight);
            GUI.Box(preview, GUIContent.none, heroCardStyle);

            var presetIndex = CareerKitSystem.GetPreset(club.Id, selectedKitSlot);
            var primary = CareerKitSystem.PreviewPrimary(presetIndex, ToColor(club.PrimaryColor));
            var secondary = CareerKitSystem.PreviewSecondary(presetIndex, ToColor(club.SecondaryColor));
            DrawKitShirt(preview, primary, secondary);

            var infoX = preview.x + preview.width * 0.58f;
            var infoWidth = preview.width * 0.40f;
            GUI.Label(new Rect(infoX, preview.y + 12f, infoWidth, 30f),
                CareerKitSystem.PresetNames[presetIndex], cardValueStyle);
            GUI.Label(new Rect(infoX, preview.y + 44f,
                infoWidth, 20f), CareerKitSystem.SlotNames[selectedKitSlot] + " KIT", cardTitleStyle);

            var swatchY = preview.y + Mathf.Max(72f, preview.height * 0.52f);
            var swatchSize = Mathf.Clamp(preview.height * 0.2f, 22f, 34f);
            var swatchGap = Mathf.Clamp(swatchSize * 1.8f, 44f, 64f);
            GUI.Label(new Rect(infoX, swatchY - 20f, infoWidth, 18f), "PRIMARY", cardCaptionStyle);
            GUI.color = primary;
            GUI.DrawTexture(new Rect(infoX, swatchY, swatchSize, swatchSize), HubTheme.RoundedShape(8));
            GUI.Label(new Rect(infoX + swatchSize + swatchGap, swatchY - 20f,
                infoWidth - swatchSize - swatchGap, 18f), "SECONDARY", cardCaptionStyle);
            GUI.color = secondary;
            GUI.DrawTexture(new Rect(infoX + swatchSize + swatchGap, swatchY, swatchSize, swatchSize),
                HubTheme.RoundedShape(8));
            GUI.color = Color.white;

            var activeKit = CareerKitSystem.GetActiveMatchSlot(club.Id);
            GUI.Label(new Rect(x, preview.yMax + 4f, width, 18f),
                $"MATCH KIT: {CareerKitSystem.SlotNames[activeKit]}  •  KEEPERS USE THEIR OWN PALETTE", cardCaptionStyle);

            var totalGap = 6f;
            var swatchWidth = (width - totalGap * (CareerKitSystem.PresetNames.Length - 1)) /
                              CareerKitSystem.PresetNames.Length;
            for (var i = 0; i < CareerKitSystem.PresetNames.Length; i++)
            {
                var rect = new Rect(x + i * (swatchWidth + totalGap), presetsY, swatchWidth, presetHeight);
                var selected = presetIndex == i;
                GUI.Box(rect, GUIContent.none, selected ? heroCardStyle : cardStyle);
                var primaryPreview = CareerKitSystem.PreviewPrimary(i, ToColor(club.PrimaryColor));
                var secondaryPreview = CareerKitSystem.PreviewSecondary(i, ToColor(club.SecondaryColor));
                var blockWidth = (rect.width - 16f) * 0.5f;
                var blockHeight = Mathf.Clamp(rect.height * 0.44f, 14f, 26f);
                GUI.color = primaryPreview;
                GUI.DrawTexture(new Rect(rect.x + 6f, rect.y + 7f, blockWidth - 3f, blockHeight),
                    HubTheme.RoundedShape(6));
                GUI.color = secondaryPreview;
                GUI.DrawTexture(new Rect(rect.x + 6f + blockWidth + 2f, rect.y + 7f,
                    blockWidth - 3f, blockHeight), HubTheme.RoundedShape(6));
                GUI.color = Color.white;
                GUI.Label(new Rect(rect.x + 4f, rect.y + blockHeight + 10f, rect.width - 8f, rect.height - blockHeight - 12f),
                    CareerKitSystem.PresetNames[i], tileCaptionStyle);
                if (GUI.Button(rect, GUIContent.none, hitboxStyle))
                {
                    CareerKitSystem.SetPreset(club.Id, selectedKitSlot, i);
                    notice = CareerKitSystem.SlotNames[selectedKitSlot] + " kit saved.";
                }
            }

            if (GUI.Button(new Rect(x, footerY, width, 44f), "BACK TO CAREER", buttonStyle))
                showKitRoom = false;
        }

        private void DrawKitShirt(Rect preview, Color primary, Color secondary)
        {
            var shirtHeight = Mathf.Clamp(preview.height * 0.72f, 70f, 170f);
            var shirtWidth = shirtHeight * 0.86f;
            var cx = preview.x + preview.width * 0.30f;
            var cy = preview.y + (preview.height - shirtHeight) * 0.5f;
            var body = new Rect(cx - shirtWidth * 0.32f, cy + shirtHeight * 0.14f,
                shirtWidth * 0.64f, shirtHeight * 0.86f);
            var previous = GUI.color;
            GUI.color = primary;
            GUI.DrawTexture(body, HubTheme.RoundedShape(12));
            GUI.DrawTexture(new Rect(body.x - shirtWidth * 0.24f, body.y,
                shirtWidth * 0.26f, shirtHeight * 0.34f), HubTheme.RoundedShape(10));
            GUI.DrawTexture(new Rect(body.xMax - shirtWidth * 0.02f, body.y,
                shirtWidth * 0.26f, shirtHeight * 0.34f), HubTheme.RoundedShape(10));
            GUI.color = secondary;
            GUI.DrawTexture(new Rect(body.x + body.width * 0.34f, body.y - 2f, body.width * 0.32f, 14f),
                HubTheme.RoundedShape(6));
            GUI.DrawTexture(new Rect(body.x, body.y + body.height * 0.62f, body.width, 6f),
                HubTheme.SolidTexture(Color.white));
            GUI.color = Color.white;
            GUI.Label(new Rect(body.x, body.y + body.height * 0.2f, body.width, body.height * 0.4f), "9",
                HubTheme.Label(HubTheme.ResponsiveFontSize(26f, 24, 46), secondary,
                    TextAnchor.MiddleCenter, true));
            GUI.color = previous;
        }

        private void DrawTournamentHub(Rect panel)
        {
            var x = panel.x + 18f;
            var width = panel.width - 36f;
            var headerY = DrawHeader(panel, "COMPETITIONS", "Your career competitions and club record",
                true, () => showTournamentHub = false);
            var gap = 8f;
            var footerY = panel.yMax - 58f;
            var viewHeight = Mathf.Max(40f, footerY - headerY - 6f);
            var rowHeight = Mathf.Clamp(panel.height * 0.115f, 52f, 82f);
            competitionScroll = GUI.BeginScrollView(new Rect(x, headerY, width, viewHeight), competitionScroll,
                new Rect(0f, 0f, width - 18f, 3f * (rowHeight + gap)));
            DrawCompetitionRow(new Rect(0f, 0f, width - 18f, rowHeight), "L",
                CareerLeagueSystem.GetManagedLeague(save)?.Name ?? "LEAGUE",
                "View standings and the next scheduled fixture",
                () => showLeagueTable = true);
            DrawCompetitionRow(new Rect(0f, rowHeight + gap, width - 18f, rowHeight), "C",
                CareerCupSystem.CompetitionName(save),
                save.cupRound > 0 ? CareerCupSystem.RoundName(save.cupRound) : "Season cup campaign",
                () => showCupCampaign = true);
            DrawCompetitionRow(new Rect(0f, (rowHeight + gap) * 2f, width - 18f, rowHeight), "H",
                $"CLUB HONOURS  •  {(save.honours == null ? 0 : save.honours.Length)} TROPHIES",
                "Review titles won in this save",
                () =>
                {
                    honoursScroll = Vector2.zero;
                    showHonours = true;
                });
            GUI.EndScrollView();
            if (GUI.Button(new Rect(x, footerY, width, 44f), "BACK TO CAREER", buttonStyle))
                showTournamentHub = false;
        }

        private void DrawCompetitionRow(Rect rect, string glyph, string title, string subtitle, Action open)
        {
            GUI.Box(rect, GUIContent.none, cardStyle);
            var iconSize = Mathf.Clamp(rect.height * 0.62f, 26f, 48f);
            var iconRect = new Rect(rect.x + 10f, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
            GUI.Box(iconRect, GUIContent.none, iconStyle);
            GUI.Label(iconRect, glyph, tileGlyphStyle);
            GUI.Label(new Rect(iconRect.xMax + 12f, rect.y + rect.height * 0.18f,
                rect.width - iconSize - 90f, 24f), title, tileLabelStyle);
            GUI.Label(new Rect(iconRect.xMax + 12f, rect.y + rect.height * 0.18f + 24f,
                rect.width - iconSize - 90f, 20f), subtitle, cardCaptionStyle);
            Gear(rect, "›");
            if (GUI.Button(rect, GUIContent.none, hitboxStyle))
                open();
        }

        private void Gear(Rect rect, string glyph)
        {
            var size = Mathf.Clamp(rect.height * 0.6f, 22f, 36f);
            GUI.Label(new Rect(rect.xMax - size - 10f, rect.y + (rect.height - size) * 0.5f, size, size),
                glyph, tileGlyphStyle);
        }

        private void DrawMatchPreview(Rect panel)
        {
            var x = panel.x + 24f;
            var width = panel.width - 48f;
            var next = CareerLeagueSystem.GetNextFixture(save);
            var userClub = CareerLeagueSystem.GetManagedClub(save);
            var opponent = WorldDatabase.GetClub(next.OpponentClubId);
            var compact = panel.height < 440f || width < 520f;
            var headerY = DrawHeader(panel, "MATCH PREVIEW", null, true, () => showMatchPreview = false);

            var footerY = panel.yMax - (compact ? 56f : 66f);
            var heroHeight = Mathf.Max(120f, footerY - headerY - (compact ? 46f : 64f));
            var hero = new Rect(x, headerY, width, heroHeight);
            GUI.Box(hero, GUIContent.none, heroCardStyle);

            var sideWidth = hero.width * 0.36f;
            var crestSize = Mathf.Clamp(hero.height * 0.30f, 40f, 74f);
            var leftCrest = new Rect(hero.x + sideWidth * 0.5f - crestSize * 0.5f,
                hero.y + hero.height * 0.12f, crestSize, crestSize);
            if (userClub != null)
            {
                HubTheme.Crest(leftCrest, ToColor32(userClub.PrimaryColor), ToColor32(userClub.SecondaryColor),
                    userClub.ShortName, HubTheme.ResponsiveFontSize(64f, 9, 14));
            }
            GUI.Label(new Rect(hero.x + 6f, leftCrest.yMax + 4f, sideWidth - 12f,
                Mathf.Clamp(hero.height * 0.22f, 28f, 46f)), userClub?.Name ?? "YOUR CLUB", matchTeamStyle);
            GUI.Label(new Rect(hero.x + 6f, leftCrest.yMax + 4f + Mathf.Clamp(hero.height * 0.22f, 28f, 46f),
                sideWidth - 12f, 18f), $"{save.formationId}  •  {save.tacticalStyle}", cardCaptionStyle);

            var rightCrest = new Rect(hero.xMax - sideWidth * 0.5f - crestSize * 0.5f, leftCrest.y, crestSize, crestSize);
            if (opponent != null)
            {
                HubTheme.Crest(rightCrest, ToColor32(opponent.PrimaryColor), ToColor32(opponent.SecondaryColor),
                    opponent.ShortName, HubTheme.ResponsiveFontSize(64f, 9, 14));
            }
            GUI.Label(new Rect(hero.xMax - sideWidth + 6f, rightCrest.yMax + 4f, sideWidth - 12f,
                Mathf.Clamp(hero.height * 0.22f, 28f, 46f)), opponent?.Name ?? "OPPONENT", matchTeamStyle);
            GUI.Label(new Rect(hero.xMax - sideWidth + 6f,
                rightCrest.yMax + 4f + Mathf.Clamp(hero.height * 0.22f, 28f, 46f), sideWidth - 12f, 18f),
                opponent == null ? string.Empty : $"STRENGTH {opponent.Strength}", cardCaptionStyle);

            var vsRect = new Rect(hero.x + sideWidth, leftCrest.y, hero.width - sideWidth * 2f, crestSize);
            GUI.Label(vsRect, "VS", matchVsStyle);

            var competition = next.Kind == CareerFixtureKind.Cup
                ? CareerCupSystem.CompetitionName(save)
                : CareerLeagueSystem.GetManagedLeague(save)?.Name;
            var stadiumClub = next.UserIsHome ? userClub : opponent;
            var stadiumName = stadiumClub == null ? string.Empty : stadiumClub.StadiumName;
            HubTheme.Divider(new Rect(hero.x + 16f, hero.yMax - 44f, hero.width - 32f, 1f),
                new Color32(0x2A, 0x6C, 0x94, 0xFF));
            GUI.Label(new Rect(hero.x + 16f, hero.yMax - 38f, hero.width - 32f, 20f),
                $"{competition}  •  FIXTURE VENUE: {(next.UserIsHome ? "HOME" : "AWAY")}  •  {stadiumName}",
                noticeStyle);

            var chipY = hero.yMax + 8f;
            var kitSlot = CareerKitSystem.GetActiveMatchSlot(save.managedClubId);
            var chipFont = HubTheme.ResponsiveFontSize(64f, 10, 14);
            var chipGap = 8f;
            var chipHeight = Mathf.Clamp(footerY - chipY - 8f, 24f, 34f);
            var chipWidth = (width - chipGap) * 0.5f;
            HubTheme.Chip(new Rect(x, chipY, chipWidth, chipHeight),
                $"FORMATION  {save.formationId}  •  {save.tacticalStyle}", HubTheme.AccentTextColor, chipFont);
            HubTheme.Chip(new Rect(x + chipWidth + chipGap, chipY, chipWidth, chipHeight),
                $"KIT  {CareerKitSystem.SlotNames[kitSlot]} / {CareerKitSystem.PresetNames[CareerKitSystem.GetPreset(save.managedClubId, kitSlot)]}",
                HubTheme.GoldTextColor, chipFont);

            var gap = 10f;
            var half = (width - gap) / 2f;
            var buttonHeight = compact ? 44f : 50f;
            if (GUI.Button(new Rect(x, footerY, half, buttonHeight), "BACK", buttonStyle))
                showMatchPreview = false;
            if (GUI.Button(new Rect(x + half + gap, footerY, half, buttonHeight), "KICK OFF", primaryButtonStyle))
            {
                showMatchPreview = false;
                enabled = false;
                if (GetComponent<MatchManager3D>() == null) gameObject.AddComponent<MatchManager3D>();
            }
        }

        private static Color ToColor(WorldColor color)
        {
            return new Color(color.R, color.G, color.B);
        }

        private void DrawClubBrowser(Rect panel)
        {
            var x = panel.x + 18f;
            var width = panel.width - 36f;
            var country = WorldDatabase.GetCountry(browserCountryId);
            var league = country == null ? null : WorldDatabase.GetLeagueForCountry(country.Id);
            var y = DrawHeader(panel, "BROWSE WORLD",
                league == null ? "Choose a confederation and country" : $"{country.Name}  •  {league.Name}",
                true, () =>
                {
                    if (save == null)
                    {
                        showClubBrowser = false;
                        showHomeScreen = true;
                    }
                    else
                    {
                        showClubBrowser = false;
                        notice = "Returned to your career.";
                    }
                });
            var footerHeight = Mathf.Clamp(panel.height * 0.09f, 44f, 60f);
            var footerY = panel.y + panel.height - footerHeight - 14f;

            if (league == null)
            {
                DrawCountryPicker(panel, x, width, ref y, footerY, footerHeight);
                return;
            }
            y += 4f;

            var clubs = WorldDatabase.GetLeagueClubs(league.Id);
            var gap = 8f;
            var columns = 2;
            var buttonWidth = (width - gap) / columns;
            var buttonHeight = Mathf.Clamp(panel.height * 0.085f, 42f, 62f);
            var contentHeight = Mathf.Ceil(clubs.Length / (float)columns) * (buttonHeight + gap);
            var viewHeight = footerY - y - 8f;
            clubScroll = GUI.BeginScrollView(new Rect(x, y, width, viewHeight), clubScroll,
                new Rect(0f, 0f, width - 22f, contentHeight));
            for (var i = 0; i < clubs.Length; i++)
            {
                var column = i % columns;
                var row = i / columns;
                var rect = new Rect(column * (buttonWidth + gap), row * (buttonHeight + gap),
                    buttonWidth, buttonHeight);
                var label = $"{clubs[i].Name}   ·   STR {clubs[i].Strength}";
                if (GUI.Button(rect, label, buttonStyle))
                {
                    if (save == null)
                        StartCareer(clubs[i].Id);
                    else
                    {
                        pendingClubId = clubs[i].Id;
                        confirmNewCareer = true;
                    }
                }
            }
            GUI.EndScrollView();

            if (GUI.Button(new Rect(x, footerY, footerHeight * 2.4f, footerHeight), "BACK", buttonStyle))
            {
                browserCountryId = string.Empty;
                notice = "Choose another country.";
            }
            GUI.Label(new Rect(x + footerHeight * 2.6f, footerY, width - footerHeight * 2.6f, footerHeight),
                "Tap a club to manage it.", noticeStyle);
        }

        private void DrawCountryPicker(Rect panel, float x, float width, ref float y,
            float footerY, float footerHeight)
        {
            var tabGap = 6f;
            var tabWidth = (width - tabGap * (Confederations.Length - 1)) / Confederations.Length;
            var tabHeight = Mathf.Clamp(panel.height * 0.065f, 32f, 46f);
            var tabFont = HubTheme.ResponsiveFontSize(58f, 11, 16);
            for (var i = 0; i < Confederations.Length; i++)
            {
                var confederation = Confederations[i];
                if (GUI.Button(new Rect(x + i * (tabWidth + tabGap), y, tabWidth, tabHeight), confederation,
                    HubTheme.Tab(tabFont, browserConfederation == confederation)))
                {
                    browserConfederation = confederation;
                    countryScroll = Vector2.zero;
                }
            }
            y += tabHeight + 10f;

            var countries = WorldDatabase.GetCountries()
                .Where(item => item.Confederation == browserConfederation)
                .ToArray();
            var gap = 8f;
            var columns = 4;
            var buttonWidth = (width - gap * (columns - 1)) / columns;
            var buttonHeight = Mathf.Clamp(panel.height * 0.06f, 32f, 44f);
            var contentHeight = Mathf.Ceil(countries.Length / (float)columns) * (buttonHeight + gap);
            var viewHeight = footerY - y - 8f;
            var countryStyle = HubTheme.SecondaryButton(HubTheme.ResponsiveFontSize(70f, 9, 13));
            countryStyle.wordWrap = false;
            countryStyle.clipping = TextClipping.Clip;
            countryScroll = GUI.BeginScrollView(new Rect(x, y, width, viewHeight), countryScroll,
                new Rect(0f, 0f, width - 22f, contentHeight));
            for (var i = 0; i < countries.Length; i++)
            {
                var column = i % columns;
                var row = i / columns;
                var rect = new Rect(column * (buttonWidth + gap), row * (buttonHeight + gap),
                    buttonWidth, buttonHeight);
                if (GUI.Button(rect, countries[i].Name, countryStyle))
                {
                    browserCountryId = countries[i].Id;
                    clubScroll = Vector2.zero;
                    notice = "Choose a club from " + countries[i].Name + ".";
                }
            }
            GUI.EndScrollView();

            GUI.Label(new Rect(x, footerY, save != null ? width * 0.5f : width * 0.5f, footerHeight),
                "96 national top divisions", smallStyle);
            if (save == null && GUI.Button(new Rect(x + width * 0.5f, footerY, width * 0.5f, footerHeight),
                "BACK TO SOCCER 27", buttonStyle))
            {
                showClubBrowser = false;
                showHomeScreen = true;
            }
            if (save != null && GUI.Button(new Rect(x + width * 0.5f, footerY, width * 0.5f, footerHeight),
                "CANCEL & RESUME CAREER", buttonStyle))
            {
                showClubBrowser = false;
                notice = "Returned to your career.";
            }
        }

        private void DrawNewCareerConfirmation(Rect panel)
        {
            var club = WorldDatabase.GetClub(pendingClubId);
            if (club == null)
            {
                confirmNewCareer = false;
                return;
            }

            var x = panel.x + 18f;
            var width = panel.width - 36f;
            var y = DrawHeader(panel, "START NEW CAREER?", null, true, () =>
            {
                confirmNewCareer = false;
                pendingClubId = string.Empty;
            });

            var league = WorldDatabase.GetLeague(club.LeagueId);
            var country = league == null ? null : WorldDatabase.GetCountry(league.CountryId);
            var leagueLabel = league == null
                ? club.LeagueId
                : country == null ? league.Name : $"{league.Name}  •  {country.Name}";

            var cardHeight = Mathf.Clamp(panel.height * 0.46f, 170f, 280f);
            var card = new Rect(x, y + 8f, width, cardHeight);
            GUI.Box(card, GUIContent.none, heroCardStyle);
            var pad = 18f;
            var crestSize = Mathf.Clamp(card.height * 0.34f, 46f, 78f);
            HubTheme.Crest(new Rect(card.x + pad, card.y + pad, crestSize, crestSize),
                ToColor32(club.PrimaryColor), ToColor32(club.SecondaryColor), club.ShortName,
                HubTheme.ResponsiveFontSize(70f, 10, 16));
            var textX = card.x + pad + crestSize + 14f;
            GUI.Label(new Rect(textX, card.y + pad + 4f, card.xMax - pad - textX, 32f),
                club.Name, cardValueStyle);
            GUI.Label(new Rect(textX, card.y + pad + 36f, card.xMax - pad - textX, 20f),
                leagueLabel, cardCaptionStyle);
            HubTheme.Divider(new Rect(card.x + pad, card.y + pad + crestSize + 18f,
                card.width - pad * 2f, 1f), HubTheme.TableHeaderColor);
            GUI.Label(new Rect(card.x + pad, card.y + pad + crestSize + 30f,
                card.width - pad * 2f, card.yMax - pad - (card.y + pad + crestSize + 30f)),
                "This replaces your current career save.", bodyStyle);

            var height = Mathf.Clamp(panel.height * 0.1f, 48f, 66f);
            var gap = 10f;
            var footerY = panel.yMax - height - 16f;
            var half = (width - gap) / 2f;
            if (GUI.Button(new Rect(x, footerY, half, height), "REPLACE CAREER", dangerButtonStyle))
            {
                confirmNewCareer = false;
                StartCareer(club.Id);
            }
            if (GUI.Button(new Rect(x + half + gap, footerY, half, height), "CANCEL", buttonStyle))
            {
                confirmNewCareer = false;
                pendingClubId = string.Empty;
            }
        }

        private void StartCareer(string clubId)
        {
            var club = WorldDatabase.GetClub(clubId);
            if (club == null)
            {
                notice = "That club could not be loaded.";
                return;
            }

            var previous = save;
            save = new CareerSaveData
            {
                worldSeed = WorldDatabase.DefaultWorldSeed,
                leagueId = club.LeagueId,
                managedClubId = club.Id,
                season = 1,
                matchweek = 1,
                transferBudget = InitialBudget,
                formationId = CareerFormationSystem.FormationIds[0],
                tacticalStyle = CareerFormationSystem.TacticalStyles[0]
            };

            if (!CareerSaveSystem.Save(save))
            {
                save = previous;
                notice = "Could not save the new career. Please try again.";
                return;
            }

            pendingClubId = string.Empty;
            showHomeScreen = false;
            showClubBrowser = false;
            showSettingsScreen = false;
            confirmNewCareer = false;
            showTransfers = false;
            transfersShowScout = false;
            marketCache = null;
            scoutCache = null;
            scoutCacheClubId = string.Empty;
            scoutCountryId = string.Empty;
            scoutClubId = string.Empty;
            pendingTransferPlayerId = string.Empty;
            pendingTransferAction = string.Empty;
            selectedTransferPlayerId = string.Empty;
            notice = "Career started: " + club.Name + ".";
        }

        private void DrawSquadManagement(Rect panel)
        {
            var x = panel.x + 18f;
            var width = panel.width - 36f;
            var club = CareerLeagueSystem.GetManagedClub(save);
            var squad = CareerTransferSystem.GetSquad(save, save.managedClubId);
            var formation = CareerFormationSystem.NormalizeFormation(save.formationId);
            var mentality = CareerFormationSystem.NormalizeTacticalStyle(save.tacticalStyle);

            var chipWidth = Mathf.Clamp(width * 0.17f, 92f, 132f);
            var y = DrawHeader(panel, "SQUAD & TACTICS", $"{formation}  •  {mentality}", true,
                () => showSquadManagement = false, chipWidth + 8f);
            HubTheme.Chip(new Rect(x + width - chipWidth, panel.y + 12f, chipWidth,
                    Mathf.Clamp(panel.height * 0.062f, 30f, 42f)),
                "OVR " + SquadRating(squad), HubTheme.AccentTextColor, HubTheme.ResponsiveFontSize(60f, 11, 16));

            var gap = 6f;
            var footerHeight = Mathf.Clamp(panel.height * 0.085f, 40f, 56f);
            var footerY = panel.yMax - footerHeight - 12f;
            var tabHeight = Mathf.Clamp(panel.height * 0.06f, 30f, 40f);
            var tabFont = HubTheme.ResponsiveFontSize(58f, 11, 16);
            var labelWidth = Mathf.Clamp(width * 0.18f, 90f, 132f);

            GUI.Label(new Rect(x, y, labelWidth - 4f, tabHeight), "FORMATION", cardTitleStyle);
            var formations = CareerFormationSystem.FormationIds;
            var tabLeft = x + labelWidth;
            var tabWidth = (x + width - tabLeft - gap * (formations.Length - 1)) / formations.Length;
            for (var i = 0; i < formations.Length; i++)
            {
                var id = formations[i];
                if (GUI.Button(new Rect(tabLeft + i * (tabWidth + gap), y, tabWidth, tabHeight), id,
                    HubTheme.Tab(tabFont, save.formationId == id)))
                {
                    save.formationId = id;
                    CareerSaveSystem.Save(save);
                    notice = "Formation saved: " + id;
                }
            }
            y += tabHeight + 6f;

            GUI.Label(new Rect(x, y, labelWidth - 4f, tabHeight), "MENTALITY", cardTitleStyle);
            var styles = CareerFormationSystem.TacticalStyles;
            var styleLeft = x + labelWidth;
            var styleWidth = (x + width - styleLeft - gap * (styles.Length - 1)) / styles.Length;
            for (var i = 0; i < styles.Length; i++)
            {
                var style = styles[i];
                if (GUI.Button(new Rect(styleLeft + i * (styleWidth + gap), y, styleWidth, tabHeight), style,
                    HubTheme.Tab(tabFont, save.tacticalStyle == style)))
                {
                    save.tacticalStyle = style;
                    CareerSaveSystem.Save(save);
                    notice = "Team mentality saved: " + style;
                }
            }
            y += tabHeight + 10f;

            var contentHeight = Mathf.Max(80f, footerY - y - 8f);
            var canPitch = width >= 560f && contentHeight >= 250f;
            if (canPitch && !squadListView)
                DrawSquadPitch(new Rect(x, y, width, contentHeight), squad, formation, mentality);
            else
                DrawSquadList(new Rect(x, y, width, contentHeight), squad);

            var backWidth = (width - gap * 2f) / 3f;
            if (GUI.Button(new Rect(x, footerY, backWidth, footerHeight), "BACK", buttonStyle))
                showSquadManagement = false;
            if (GUI.Button(new Rect(x + backWidth + gap, footerY, backWidth, footerHeight),
                squadListView ? "PITCH VIEW" : "LIST VIEW", buttonStyle))
                squadListView = !squadListView;
            if (GUI.Button(new Rect(x + (backWidth + gap) * 2f, footerY, backWidth, footerHeight),
                "VIEW LEAGUE TABLE", buttonStyle))
            {
                showSquadManagement = false;
                showLeagueTable = true;
            }
        }

        private void DrawSquadList(Rect area, WorldPlayer[] squad)
        {
            var rowHeight = Mathf.Clamp(area.height * 0.055f, 20f, 30f);
            var innerWidth = area.width - 22f;
            var columns = new[] { 0f, 0.07f, 0.55f, 0.65f, 0.73f, 0.81f, 0.9f, 1f };
            var headers = new[] { "#", "NAME", "POS", "OVR", "PAC", "SHO", "PAS" };
            GUI.DrawTexture(new Rect(area.x, area.y, innerWidth, rowHeight), tableHeaderTexture);
            for (var i = 0; i < headers.Length; i++)
            {
                GUI.Label(new Rect(area.x + innerWidth * columns[i], area.y,
                    innerWidth * (columns[i + 1] - columns[i]), rowHeight), headers[i],
                    i == 1 ? tableHeaderNameStyle : tableHeaderStyle);
            }

            var view = new Rect(area.x, area.y + rowHeight + 2f, area.width, area.height - rowHeight - 2f);
            squadScroll = GUI.BeginScrollView(view, squadScroll,
                new Rect(0f, 0f, innerWidth, squad.Length * rowHeight + 4f));
            var cell = new GUIStyle(smallStyle) { alignment = TextAnchor.MiddleLeft };
            for (var i = 0; i < squad.Length; i++)
            {
                var player = squad[i];
                GUI.DrawTexture(new Rect(0f, i * rowHeight, innerWidth, rowHeight),
                    i % 2 == 1 ? tableAltRowTexture : tableRowTexture);
                if (GUI.Button(new Rect(0f, i * rowHeight, innerWidth, rowHeight), GUIContent.none, rowStyle))
                {
                    selectedPlayer = player;
                    showSquadManagement = false;
                    showPlayerDetails = true;
                }
                var values = new[]
                {
                    player.ShirtNumber.ToString(), player.Name, PositionLabel(player.Position),
                    player.Overall.ToString(), player.Pace.ToString(), player.Shooting.ToString(),
                    player.Passing.ToString()
                };
                for (var column = 0; column < values.Length; column++)
                    GUI.Label(new Rect(innerWidth * columns[column], i * rowHeight,
                        innerWidth * (columns[column + 1] - columns[column]), rowHeight), values[column],
                        column == 1 ? tableNameStyle : cell);
            }
            GUI.EndScrollView();
        }

        private void DrawSquadPitch(Rect area, WorldPlayer[] squad, string formation, string mentality)
        {
            var subsWidth = Mathf.Clamp(area.width * 0.3f, 180f, 260f);
            var gap = 8f;
            var pitch = new Rect(area.x, area.y, area.width - subsWidth - gap, area.height);
            var subs = new Rect(pitch.xMax + gap, area.y, subsWidth, area.height);

            GUI.Box(pitch, GUIContent.none, pitchStyle);
            HubTheme.PitchLines(pitch);

            var lineup = BuildLineup(squad, formation);
            var positions = CareerFormationSystem.GetPositions(formation, mentality);
            var primary = (Color32)CareerKitSystem.GetPrimary(save.managedClubId,
                CareerKitSystem.GetActiveMatchSlot(save.managedClubId), Color.white);

            var cardWidth = Mathf.Clamp(pitch.width * 0.13f, 40f, 70f);
            var cardHeight = Mathf.Clamp(pitch.height * 0.19f, 44f, 58f);
            var insetX = cardWidth * 0.55f;
            var insetY = cardHeight * 0.6f;
            for (var i = 0; i < positions.Length && i < lineup.Length; i++)
            {
                var point = positions[i];
                var nx = Mathf.Clamp01((point.x + 30f) / 60f);
                var ny = Mathf.Clamp01((point.z + 50f) / 62f);
                var cx = pitch.x + insetX + nx * (pitch.width - insetX * 2f);
                var cy = pitch.yMax - insetY - ny * (pitch.height - insetY * 2f);
                DrawPitchPlayer(new Rect(cx - cardWidth * 0.5f, cy - cardHeight * 0.5f, cardWidth, cardHeight),
                    lineup[i], primary);
            }

            GUI.Box(subs, GUIContent.none, cardStyle);
            var pad = Mathf.Clamp(area.width * 0.012f, 8f, 12f);
            var titleHeight = Mathf.Clamp(area.height * 0.06f, 18f, 26f);
            var bench = BenchPlayers(squad, lineup);
            GUI.Label(new Rect(subs.x + pad, subs.y + pad * 0.6f, subs.width - pad * 2f, titleHeight),
                $"SUBSTITUTES  •  {bench.Length}", cardTitleStyle);
            HubTheme.Divider(new Rect(subs.x + pad, subs.y + pad * 0.6f + titleHeight,
                subs.width - pad * 2f, 1f), HubTheme.TableHeaderColor);

            var listTop = subs.y + pad * 0.6f + titleHeight + 6f;
            var listHeight = Mathf.Max(30f, subs.yMax - listTop - pad * 0.6f);
            var rowHeight = Mathf.Clamp(listHeight * 0.16f, 34f, 54f);
            var listWidth = subs.width - pad * 1.2f;
            substitutesScroll = GUI.BeginScrollView(
                new Rect(subs.x + pad * 0.6f, listTop, listWidth, listHeight), substitutesScroll,
                new Rect(0f, 0f, listWidth - 18f, bench.Length * rowHeight + 2f));
            for (var i = 0; i < bench.Length; i++)
                DrawBenchRow(new Rect(0f, i * rowHeight, listWidth - 18f, rowHeight - 4f), bench[i]);
            GUI.EndScrollView();
        }

        private void DrawPitchPlayer(Rect rect, WorldPlayer player, Color32 primary)
        {
            GUI.Box(rect, GUIContent.none, pitchCardStyle);
            var pad = Mathf.Clamp(rect.width * 0.08f, 3f, 6f);
            var badgeHeight = Mathf.Clamp(rect.height * 0.24f, 11f, 15f);
            if (player == null)
            {
                GUI.Label(new Rect(rect.x, rect.y + rect.height * 0.4f, rect.width, badgeHeight), "—",
                    pitchPosStyle);
                return;
            }

            GUI.Label(new Rect(rect.x + pad, rect.y + pad * 0.5f, rect.width * 0.42f, badgeHeight),
                player.Overall.ToString(), pitchOvrStyle);
            GUI.Label(new Rect(rect.xMax - pad - rect.width * 0.42f, rect.y + pad * 0.5f,
                rect.width * 0.42f, badgeHeight), PositionLabel(player.Position), pitchPosStyle);

            var jerseySize = Mathf.Clamp(rect.width * 0.5f, 16f, 34f);
            var jersey = new Rect(rect.x + (rect.width - jerseySize) * 0.5f,
                rect.y + (rect.height - jerseySize) * 0.52f, jerseySize, jerseySize);
            var previous = GUI.color;
            GUI.color = primary;
            GUI.DrawTexture(jersey, HubTheme.RoundedShape(8));
            GUI.color = previous;

            var nameHeight = Mathf.Clamp(rect.height * 0.3f, 13f, 18f);
            GUI.Label(new Rect(rect.x + pad * 0.5f, rect.yMax - nameHeight - 1f, rect.width - pad, nameHeight),
                ShortPlayerName(player.Name), pitchNameStyle);

            if (GUI.Button(rect, GUIContent.none, hitboxStyle))
            {
                selectedPlayer = player;
                showSquadManagement = false;
                showPlayerDetails = true;
            }
        }

        private void DrawBenchRow(Rect rect, WorldPlayer player)
        {
            GUI.Box(rect, GUIContent.none, pitchCardStyle);
            var pad = Mathf.Clamp(rect.height * 0.16f, 4f, 8f);
            var ovrWidth = Mathf.Clamp(rect.width * 0.2f, 26f, 40f);
            GUI.Label(new Rect(rect.x + pad, rect.y, ovrWidth, rect.height), player.Overall.ToString(),
                statValueStyle);
            GUI.Label(new Rect(rect.x + pad + ovrWidth + 6f, rect.y, rect.width - ovrWidth - pad * 2f - 40f,
                rect.height), ShortPlayerName(player.Name), subNameStyle);
            GUI.Label(new Rect(rect.xMax - pad - 34f, rect.y, 34f, rect.height),
                PositionLabel(player.Position), pitchPosStyle);
            if (GUI.Button(rect, GUIContent.none, hitboxStyle))
            {
                selectedPlayer = player;
                showSquadManagement = false;
                showPlayerDetails = true;
            }
        }

        private static WorldPlayer[] BuildLineup(WorldPlayer[] squad, string formation)
        {
            var lineup = new WorldPlayer[11];
            if (squad == null || squad.Length == 0)
                return lineup;

            var keepers = squad.Where(item => item != null && item.Position == PlayerPosition.Goalkeeper)
                .OrderByDescending(item => item.Overall).ToArray();
            var defenders = squad.Where(item => item != null && item.Position == PlayerPosition.Defender)
                .OrderByDescending(item => item.Overall).ToArray();
            var midfielders = squad.Where(item => item != null && item.Position == PlayerPosition.Midfielder)
                .OrderByDescending(item => item.Overall).ToArray();
            var forwards = squad.Where(item => item != null && item.Position == PlayerPosition.Forward)
                .OrderByDescending(item => item.Overall).ToArray();

            var keeperIndex = 0;
            var defenderIndex = 0;
            var midfielderIndex = 0;
            var forwardIndex = 0;
            for (var i = 0; i < lineup.Length; i++)
            {
                switch (RoleForFormationIndex(i, formation))
                {
                    case PlayerPosition.Goalkeeper:
                        lineup[i] = NextLineupPlayer(keepers, ref keeperIndex);
                        break;
                    case PlayerPosition.Defender:
                        lineup[i] = NextLineupPlayer(defenders, ref defenderIndex);
                        break;
                    case PlayerPosition.Midfielder:
                        lineup[i] = NextLineupPlayer(midfielders, ref midfielderIndex);
                        break;
                    default:
                        lineup[i] = NextLineupPlayer(forwards, ref forwardIndex);
                        break;
                }
            }
            return lineup;
        }

        private static WorldPlayer NextLineupPlayer(WorldPlayer[] pool, ref int index)
        {
            return index < pool.Length ? pool[index++] : null;
        }

        private static PlayerPosition RoleForFormationIndex(int index, string formation)
        {
            if (index == 0)
                return PlayerPosition.Goalkeeper;

            switch (formation)
            {
                case "4-2-3-1":
                    return index <= 4 ? PlayerPosition.Defender
                        : index <= 9 ? PlayerPosition.Midfielder : PlayerPosition.Forward;
                case "4-4-2":
                    return index <= 4 ? PlayerPosition.Defender
                        : index <= 8 ? PlayerPosition.Midfielder : PlayerPosition.Forward;
                case "3-5-2":
                    return index <= 3 ? PlayerPosition.Defender
                        : index <= 8 ? PlayerPosition.Midfielder : PlayerPosition.Forward;
                default:
                    return index <= 4 ? PlayerPosition.Defender
                        : index <= 7 ? PlayerPosition.Midfielder : PlayerPosition.Forward;
            }
        }

        private static WorldPlayer[] BenchPlayers(WorldPlayer[] squad, WorldPlayer[] lineup)
        {
            if (squad == null)
                return Array.Empty<WorldPlayer>();
            var starters = new HashSet<string>();
            if (lineup != null)
            {
                foreach (var player in lineup)
                {
                    if (player != null)
                        starters.Add(player.Id);
                }
            }

            return squad.Where(player => player != null && !starters.Contains(player.Id)).ToArray();
        }

        private static string ShortPlayerName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            var space = name.LastIndexOf(' ');
            return space > 0 && space < name.Length - 1 ? name.Substring(space + 1) : name;
        }

        private void OpenTransfers()
        {
            showTransfers = true;
            transfersShowSquad = false;
            transfersShowScout = false;
            marketCache = null;
            scoutCache = null;
            scoutCacheClubId = string.Empty;
            scoutCountryId = string.Empty;
            scoutClubId = string.Empty;
            scoutScroll = Vector2.zero;
            selectedTransferPlayerId = string.Empty;
            pendingTransferPlayerId = string.Empty;
            pendingTransferAction = string.Empty;
            transferScroll = Vector2.zero;
            notice = "Tap a player to sign, then tap again to confirm. SCOUT signs from other countries.";
        }

        private void DrawTransfers(Rect panel)
        {
            var x = panel.x + 18f;
            var width = panel.width - 36f;
            var club = CareerLeagueSystem.GetManagedClub(save);
            var squad = CareerTransferSystem.GetSquad(save, save.managedClubId);

            var y = DrawHeader(panel, "TRANSFERS",
                $"Budget {CareerTransferSystem.FormatMoney(save.transferBudget)}  •  Squad {squad.Length}/{CareerTransferSystem.MaxSquadSize}",
                true, () =>
                {
                    showTransfers = false;
                    pendingTransferPlayerId = string.Empty;
                    pendingTransferAction = string.Empty;
                });
            var lineHeight = Mathf.Clamp(panel.height * 0.034f, 18f, 24f);
            GUI.Label(new Rect(x, y, width, lineHeight),
                $"Wages {CareerTransferSystem.FormatMoney(CareerTransferSystem.WageBill(save))}/{CareerTransferSystem.FormatMoney(CareerTransferSystem.WageBudget(club))} per week",
                smallStyle);

            var tabY = y + lineHeight + 6f;
            var tabHeight = Mathf.Clamp(panel.height * 0.07f, 34f, 44f);
            var gap = 8f;
            var tabWidth = (width - gap * 2f) / 3f;
            var tabFont = HubTheme.ResponsiveFontSize(58f, 11, 16);
            if (GUI.Button(new Rect(x, tabY, tabWidth, tabHeight), "MARKET",
                HubTheme.Tab(tabFont, !transfersShowSquad && !transfersShowScout)))
            {
                transfersShowSquad = false;
                transfersShowScout = false;
                selectedTransferPlayerId = string.Empty;
                pendingTransferPlayerId = string.Empty;
                pendingTransferAction = string.Empty;
                transferScroll = Vector2.zero;
            }
            if (GUI.Button(new Rect(x + tabWidth + gap, tabY, tabWidth, tabHeight), "SCOUT",
                HubTheme.Tab(tabFont, transfersShowScout)))
            {
                transfersShowSquad = false;
                transfersShowScout = true;
                selectedTransferPlayerId = string.Empty;
                pendingTransferPlayerId = string.Empty;
                pendingTransferAction = string.Empty;
                scoutScroll = Vector2.zero;
                notice = "Scout any country, then choose a club and a player. Foreign deals add a premium.";
            }
            if (GUI.Button(new Rect(x + (tabWidth + gap) * 2f, tabY, tabWidth, tabHeight), "MY SQUAD",
                HubTheme.Tab(tabFont, transfersShowSquad)))
            {
                transfersShowSquad = true;
                transfersShowScout = false;
                pendingTransferPlayerId = string.Empty;
                pendingTransferAction = string.Empty;
                transferScroll = Vector2.zero;
            }

            var listTop = tabY + tabHeight + 8f;
            var footerHeight = Mathf.Clamp(panel.height * 0.09f, 40f, 58f);
            var footerY = panel.yMax - footerHeight - 12f;
            var noticeHeight = Mathf.Clamp(panel.height * 0.045f, 20f, 28f);
            var actionHeight = transfersShowSquad ? Mathf.Clamp(panel.height * 0.085f, 36f, 50f) : 0f;
            var actionY = footerY - noticeHeight - actionHeight - (actionHeight > 0f ? 6f : 0f);
            var viewHeight = Mathf.Max(40f, actionY - listTop - 6f);

            if (transfersShowScout)
            {
                DrawScoutContent(x, width, listTop, viewHeight);
            }
            else
            {
                marketCache ??= CareerTransferSystem.GetMarket(save);
                DrawTransferGrid(new Rect(x, listTop, width, viewHeight),
                    transfersShowSquad ? Array.Empty<TransferTarget>() : marketCache,
                    transfersShowSquad ? squad : null, transfersShowSquad);
            }

            if (transfersShowSquad)
                DrawSquadTransferActions(x, width, actionY, actionHeight, squad);

            GUI.Label(new Rect(x, footerY - noticeHeight, width, noticeHeight), notice, smallStyle);
            if (GUI.Button(new Rect(x, footerY, width, footerHeight), "BACK TO CAREER", buttonStyle))
            {
                showTransfers = false;
                pendingTransferPlayerId = string.Empty;
                pendingTransferAction = string.Empty;
            }
        }

        private void DrawTransferGrid(Rect area, TransferTarget[] targets, WorldPlayer[] squad, bool squadView)
        {
            var count = squadView ? squad?.Length ?? 0 : targets?.Length ?? 0;
            var columns = area.width >= 620f ? 4 : 3;
            var gap = 8f;
            var usable = Mathf.Max(60f, area.width - 20f);
            var cardWidth = (usable - gap * (columns - 1)) / columns;
            var cardHeight = Mathf.Clamp(cardWidth * 0.82f, 92f, 150f);
            var rows = Mathf.Max(1, Mathf.CeilToInt(count / (float)columns));

            var squadFull = CareerTransferSystem.GetSquad(save, save.managedClubId).Length >=
                            CareerTransferSystem.MaxSquadSize;
            var wageBill = CareerTransferSystem.WageBill(save);
            var wageCeiling = CareerTransferSystem.WageBudget(CareerLeagueSystem.GetManagedClub(save));
            var ownKit = (Color32)CareerKitSystem.GetPrimary(save.managedClubId,
                CareerKitSystem.GetActiveMatchSlot(save.managedClubId), Color.white);

            transferScroll = GUI.BeginScrollView(area, transferScroll,
                new Rect(0f, 0f, usable, rows * (cardHeight + gap)));
            for (var i = 0; i < count; i++)
            {
                var rect = new Rect((i % columns) * (cardWidth + gap), (i / columns) * (cardHeight + gap),
                    cardWidth, cardHeight);
                if (squadView)
                {
                    var player = squad[i];
                    var expiry = CareerTransferSystem.ContractExpiry(save, player.Id);
                    var clicked = DrawTransferCard(rect, player, ownKit, $"CONTRACT S{expiry}",
                        CareerTransferSystem.FormatMoney(CareerTransferSystem.MarketValue(player)),
                        false, false, player.Id == selectedTransferPlayerId);
                    if (clicked)
                    {
                        selectedTransferPlayerId = player.Id;
                        pendingTransferPlayerId = string.Empty;
                        pendingTransferAction = string.Empty;
                    }
                }
                else
                {
                    var target = targets[i];
                    var armed = target.Player.Id == pendingTransferPlayerId && pendingTransferAction == "SIGN";
                    var blocked = squadFull
                                  || target.Price > save.transferBudget
                                  || wageBill + target.Wage > wageCeiling;
                    var targetClub = string.IsNullOrEmpty(target.ClubId)
                        ? null : WorldDatabase.GetClub(target.ClubId);
                    var clubLabel = string.IsNullOrEmpty(target.ClubId)
                        ? "FREE AGENT"
                        : targetClub?.ShortName ?? "—";
                    var premium = CareerTransferSystem.PremiumPercent(save, target.Player.Id);
                    var detailLine = $"{clubLabel}  •  {CareerTransferSystem.FormatMoney(target.Wage)}/wk";
                    var priceLine = premium > 0
                        ? $"{CareerTransferSystem.FormatMoney(target.Price)} (+{premium}%)"
                        : CareerTransferSystem.FormatMoney(target.Price);
                    var clicked = DrawTransferCard(rect, target.Player,
                        targetClub == null ? new Color32(0x3F, 0x57, 0x6B, 0xFF) : ToColor32(targetClub.PrimaryColor),
                        detailLine, priceLine, armed, blocked, false);
                    if (!clicked)
                        continue;
                    if (armed || blocked)
                    {
                        ExecuteTransfer("SIGN", target.Player.Id);
                    }
                    else
                    {
                        pendingTransferPlayerId = target.Player.Id;
                        pendingTransferAction = "SIGN";
                        notice = "Tap again to confirm the signing of " + target.Player.Name + ".";
                    }
                }
            }
            GUI.EndScrollView();
        }

        private bool DrawTransferCard(Rect rect, WorldPlayer player, Color32 kit, string line1, string line2,
            bool armed, bool blocked, bool selected)
        {
            var previous = GUI.color;
            var alpha = blocked ? DisabledRowAlpha : 1f;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Box(rect, GUIContent.none, armed || selected ? heroCardStyle : cardStyle);

            var pad = Mathf.Clamp(rect.width * 0.07f, 8f, 14f);
            var ovrHeight = Mathf.Clamp(rect.height * 0.2f, 20f, 32f);
            GUI.Label(new Rect(rect.x + pad, rect.y + pad - 2f, rect.width * 0.4f, ovrHeight),
                player.Overall.ToString(), transferOvrStyle);
            GUI.Label(new Rect(rect.x + pad, rect.y + pad + ovrHeight - 4f, rect.width * 0.45f, 16f),
                PositionLabel(player.Position), cardCaptionStyle);

            var jersey = Mathf.Clamp(rect.width * 0.3f, 24f, 46f);
            GUI.color = new Color(kit.r / 255f, kit.g / 255f, kit.b / 255f, alpha);
            GUI.DrawTexture(new Rect(rect.xMax - pad - jersey, rect.y + pad, jersey, jersey),
                HubTheme.RoundedShape(10));
            GUI.color = new Color(1f, 1f, 1f, alpha);

            var nameHeight = Mathf.Clamp(rect.height * 0.15f, 16f, 22f);
            var lineHeight = Mathf.Clamp(rect.height * 0.12f, 13f, 18f);
            var line2Y = rect.yMax - pad - lineHeight;
            var line1Y = line2Y - lineHeight;
            var nameY = line1Y - nameHeight;
            GUI.Label(new Rect(rect.x + pad, nameY, rect.width - pad * 2f, nameHeight), player.Name,
                transferNameStyle);
            GUI.Label(new Rect(rect.x + pad, line1Y, rect.width - pad * 2f, lineHeight), line1,
                cardCaptionStyle);
            GUI.Label(new Rect(rect.x + pad, line2Y, rect.width - pad * 2f, lineHeight),
                armed ? "CONFIRM SIGN" : line2, armed ? highlightStyle : statValueStyle);

            GUI.color = previous;
            return GUI.Button(rect, GUIContent.none, hitboxStyle);
        }

        private void DrawScoutContent(float x, float width, float top, float height)
        {
            var barHeight = Mathf.Clamp(height * 0.1f, 28f, 40f);
            var gap = 6f;
            var backWidth = Mathf.Clamp(width * 0.22f, 70f, 120f);
            var rowHeight = Mathf.Clamp(height * 0.075f, 34f, 48f);
            var country = WorldDatabase.GetCountry(scoutCountryId);
            var league = country == null ? null : WorldDatabase.GetLeagueForCountry(country.Id);
            var club = WorldDatabase.GetClub(scoutClubId);

            var crumb = club != null
                ? $"{country?.Name ?? string.Empty}  •  {club.Name}"
                : league != null
                    ? $"{country?.Name ?? string.Empty}  •  {league.Name}"
                    : "Choose a confederation, then a country and club.";

            GUI.Label(new Rect(x, top, width - backWidth - gap, barHeight), crumb, smallStyle);
            if (!string.IsNullOrEmpty(scoutCountryId) &&
                GUI.Button(new Rect(x + width - backWidth, top, backWidth, barHeight), "BACK", buttonStyle))
            {
                if (!string.IsNullOrEmpty(scoutClubId))
                {
                    scoutClubId = string.Empty;
                    scoutCache = null;
                    scoutCacheClubId = string.Empty;
                }
                else
                {
                    scoutCountryId = string.Empty;
                }
                scoutScroll = Vector2.zero;
                pendingTransferPlayerId = string.Empty;
                pendingTransferAction = string.Empty;
            }

            var contentTop = top + barHeight + 6f;
            var contentHeight = Mathf.Max(40f, height - barHeight - 6f);
            if (league == null)
            {
                scoutCountryId = string.Empty;
                DrawScoutCountries(x, width, contentTop, contentHeight);
                return;
            }

            if (string.IsNullOrEmpty(scoutClubId) || club == null)
            {
                if (club == null)
                {
                    scoutClubId = string.Empty;
                    scoutCache = null;
                    scoutCacheClubId = string.Empty;
                }
                DrawScoutClubs(x, width, contentTop, contentHeight, league, rowHeight);
                return;
            }

            DrawScoutSquad(x, width, contentTop, contentHeight, club);
        }

        private void DrawScoutCountries(float x, float width, float top, float height)
        {
            var tabGap = 6f;
            var tabWidth = (width - tabGap * (Confederations.Length - 1)) / Confederations.Length;
            var tabHeight = Mathf.Clamp(height * 0.14f, 28f, 44f);
            var tabFont = HubTheme.ResponsiveFontSize(58f, 11, 16);
            for (var i = 0; i < Confederations.Length; i++)
            {
                var confederation = Confederations[i];
                if (GUI.Button(new Rect(x + i * (tabWidth + tabGap), top, tabWidth, tabHeight), confederation,
                    HubTheme.Tab(tabFont, scoutConfederation == confederation)))
                {
                    scoutConfederation = confederation;
                    scoutScroll = Vector2.zero;
                }
            }

            var countries = WorldDatabase.GetCountries()
                .Where(item => item.Confederation == scoutConfederation)
                .ToArray();
            var gridTop = top + tabHeight + 8f;
            var gap = 8f;
            var columns = 4;
            var buttonWidth = (width - gap * (columns - 1)) / columns;
            var buttonHeight = Mathf.Clamp(height * 0.11f, 28f, 40f);
            var contentHeight = Mathf.Ceil(countries.Length / (float)columns) * (buttonHeight + gap);
            var viewHeight = Mathf.Max(30f, height - tabHeight - 8f);
            var countryStyle = HubTheme.SecondaryButton(HubTheme.ResponsiveFontSize(70f, 9, 13));
            countryStyle.wordWrap = false;
            countryStyle.clipping = TextClipping.Clip;
            scoutScroll = GUI.BeginScrollView(new Rect(x, gridTop, width, viewHeight), scoutScroll,
                new Rect(0f, 0f, width - 22f, contentHeight));
            for (var i = 0; i < countries.Length; i++)
            {
                var column = i % columns;
                var row = i / columns;
                var rect = new Rect(column * (buttonWidth + gap), row * (buttonHeight + gap),
                    buttonWidth, buttonHeight);
                if (GUI.Button(rect, countries[i].Name, countryStyle))
                {
                    scoutCountryId = countries[i].Id;
                    scoutScroll = Vector2.zero;
                    notice = "Scouting " + countries[i].Name + ": choose a club.";
                }
            }
            GUI.EndScrollView();
        }

        private void DrawScoutClubs(float x, float width, float top, float height, WorldLeague league,
            float rowHeight)
        {
            var clubs = WorldDatabase.GetLeagueClubs(league.Id);
            var contentHeight = clubs.Length * rowHeight + 4f;
            scoutScroll = GUI.BeginScrollView(new Rect(x, top, width, height), scoutScroll,
                new Rect(0f, 0f, width - 22f, contentHeight));
            for (var i = 0; i < clubs.Length; i++)
            {
                var club = clubs[i];
                var rect = new Rect(0f, i * rowHeight, width - 22f, rowHeight - 2f);
                var label = $"{club.Name}   ·   STR {club.Strength}   ·   {club.City}";
                if (GUI.Button(rect, label, rowStyle))
                {
                    scoutClubId = club.Id;
                    scoutCache = null;
                    scoutCacheClubId = string.Empty;
                    scoutScroll = Vector2.zero;
                    pendingTransferPlayerId = string.Empty;
                    pendingTransferAction = string.Empty;
                    notice = "Tap a player from " + club.ShortName + " to sign, then tap again to confirm.";
                }
            }
            GUI.EndScrollView();
        }

        private void DrawScoutSquad(float x, float width, float top, float height, WorldClub club)
        {
            if (scoutCache == null || scoutCacheClubId != club.Id)
            {
                scoutCache = CareerTransferSystem.GetScoutSquad(save, club.Id);
                scoutCacheClubId = club.Id;
            }

            DrawTransferGrid(new Rect(x, top, width, height), scoutCache, null, false);
        }

        private void DrawSquadTransferActions(float x, float width, float y, float height, WorldPlayer[] squad)
        {
            var player = FindSquadPlayer(squad, selectedTransferPlayerId);
            if (player == null)
            {
                GUI.Label(new Rect(x, y, width, height), "Select a player to manage his contract.", smallStyle);
                return;
            }

            var gap = 6f;
            var buttonWidth = (width - gap * 2f) / 3f;
            var sellArmed = pendingTransferPlayerId == player.Id && pendingTransferAction == "SELL";
            var releaseArmed = pendingTransferPlayerId == player.Id && pendingTransferAction == "RELEASE";

            if (GUI.Button(new Rect(x, y, buttonWidth, height), "RENEW", buttonStyle))
                ExecuteTransfer("RENEW", player.Id);
            if (GUI.Button(new Rect(x + buttonWidth + gap, y, buttonWidth, height),
                sellArmed ? "CONFIRM SELL" : "SELL", sellArmed ? dangerButtonStyle : buttonStyle))
            {
                if (sellArmed)
                    ExecuteTransfer("SELL", player.Id);
                else
                    ArmTransfer("SELL", player.Id, "Tap again to sell " + player.Name + ".");
            }
            if (GUI.Button(new Rect(x + (buttonWidth + gap) * 2f, y, buttonWidth, height),
                releaseArmed ? "CONFIRM FREE" : "RELEASE", releaseArmed ? dangerButtonStyle : buttonStyle))
            {
                if (releaseArmed)
                    ExecuteTransfer("RELEASE", player.Id);
                else
                    ArmTransfer("RELEASE", player.Id, "Tap again to release " + player.Name + " on a free.");
            }
        }

        private void ArmTransfer(string action, string playerId, string message)
        {
            pendingTransferPlayerId = playerId;
            pendingTransferAction = action;
            notice = message;
        }

        private void ExecuteTransfer(string action, string playerId)
        {
            string message;
            bool success;
            switch (action)
            {
                case "SIGN":
                    success = CareerTransferSystem.SignPlayer(save, playerId, out message);
                    break;
                case "SELL":
                    success = CareerTransferSystem.SellPlayer(save, playerId, out message);
                    break;
                case "RELEASE":
                    success = CareerTransferSystem.ReleasePlayer(save, playerId, out message);
                    break;
                case "RENEW":
                    success = CareerTransferSystem.RenewContract(save, playerId, out message);
                    break;
                default:
                    message = "Unknown transfer action.";
                    success = false;
                    break;
            }

            pendingTransferPlayerId = string.Empty;
            pendingTransferAction = string.Empty;
            if (success)
            {
                marketCache = null;
                scoutCache = null;
                scoutCacheClubId = string.Empty;
                if (!CareerSaveSystem.Save(save))
                    message += " The change could not be saved.";
                else if (action == "SELL" || action == "RELEASE")
                    selectedTransferPlayerId = string.Empty;
            }

            notice = message;
        }

        private static WorldPlayer FindSquadPlayer(WorldPlayer[] squad, string playerId)
        {
            if (squad == null || string.IsNullOrEmpty(playerId))
                return null;
            foreach (var player in squad)
            {
                if (player != null && player.Id == playerId)
                    return player;
            }
            return null;
        }

        private void DrawLeagueTable(Rect panel)
        {
            var x = panel.x + 18f;
            var width = panel.width - 36f;
            var league = CareerLeagueSystem.GetManagedLeague(save);
            var y = DrawHeader(panel, $"SEASON {save.season}", league?.Name ?? "LEAGUE", true,
                () => showLeagueTable = false);

            var buttonHeight = Mathf.Clamp(panel.height * 0.09f, 40f, 58f);
            var footerY = panel.yMax - buttonHeight - 12f;
            var card = new Rect(x, y + 4f, width, footerY - y - 18f);
            GUI.Box(card, GUIContent.none, cardStyle);

            var ranked = CareerLeagueSystem.GetRankedStandings(save);
            var inset = Mathf.Clamp(width * 0.014f, 8f, 14f);
            // Fit all rows inside the card instead of scrolling a half-visible last row.
            var rowHeight = Mathf.Clamp((card.height - inset * 2f - 10f) / (ranked.Length + 1f), 18f, 29f);
            var tableX = card.x + inset;
            var tableWidth = card.width - inset * 2f;
            var innerWidth = tableWidth - 20f;
            var columns = new[] { 0f, 0.07f, 0.48f, 0.57f, 0.66f, 0.75f, 0.84f, 0.92f, 1f };
            var headers = new[] { "#", "CLUB", "P", "W", "D", "L", "GD", "PTS" };
            var headerY = card.y + inset;
            GUI.DrawTexture(new Rect(tableX, headerY, innerWidth, rowHeight + 4f), tableHeaderTexture);
            for (var i = 0; i < headers.Length; i++)
            {
                var left = innerWidth * columns[i];
                var right = innerWidth * columns[i + 1];
                GUI.Label(new Rect(tableX + left, headerY, right - left, rowHeight + 4f), headers[i],
                    i == 1 ? tableHeaderNameStyle : tableHeaderStyle);
            }

            var viewY = headerY + rowHeight + 6f;
            var viewHeight = Mathf.Max(30f, card.yMax - inset - viewY);
            var contentHeight = ranked.Length * rowHeight;

            leagueTableScroll = GUI.BeginScrollView(new Rect(tableX, viewY, tableWidth, viewHeight),
                leagueTableScroll, new Rect(0f, 0f, innerWidth, contentHeight));
            for (var i = 0; i < ranked.Length; i++)
            {
                var standing = ranked[i];
                var isManaged = standing.clubId == save.managedClubId;
                GUI.DrawTexture(new Rect(0f, i * rowHeight, innerWidth, rowHeight),
                    isManaged ? tableHighlightTexture : i % 2 == 1 ? tableAltRowTexture : tableRowTexture);
                var rowStyle = isManaged ? highlightStyle : smallStyle;
                var values = new[]
                {
                    (i + 1).ToString(),
                    (isManaged ? "> " : string.Empty) + CareerLeagueSystem.GetClubName(standing.clubId),
                    standing.played.ToString(), standing.wins.ToString(),
                    standing.draws.ToString(), standing.losses.ToString(),
                    standing.GoalDifference.ToString("+0;-0;0"), standing.Points.ToString()
                };
                for (var column = 0; column < values.Length; column++)
                {
                    var left = innerWidth * columns[column];
                    var right = innerWidth * columns[column + 1];
                    GUI.Label(new Rect(left, i * rowHeight, right - left, rowHeight), values[column],
                        column == 1 ? tableNameStyle : rowStyle);
                }
            }
            GUI.EndScrollView();

            if (GUI.Button(new Rect(x, footerY, width, buttonHeight), "BACK TO CAREER", buttonStyle))
                showLeagueTable = false;
        }

        private void DrawCupCampaign(Rect panel)
        {
            var x = panel.x + 18f;
            var width = panel.width - 36f;
            var played = CareerCupSystem.PlayedRounds(save);
            var tie = CareerCupSystem.GetUserTie(save);
            var status = save.cupChampionId == save.managedClubId
                ? "CHAMPIONS"
                : save.cupRound > 0
                    ? $"NEXT: {CareerCupSystem.RoundName(save.cupRound)}  •  MATCHWEEK {CareerCupSystem.WeekForRound(save.cupRound)}"
                    : played > 0 ? "CUP RUN COMPLETE" : "NOT QUALIFIED FOR THIS SEASON";
            var y = DrawHeader(panel, CareerCupSystem.CompetitionName(save).ToUpperInvariant(), status, true,
                () => showCupCampaign = false);

            var footerHeight = Mathf.Clamp(panel.height * 0.09f, 42f, 60f);
            var footerY = panel.yMax - footerHeight - 12f;
            var rowHeight = Mathf.Clamp(panel.height * 0.085f, 40f, 74f);
            var gap = 8f;
            var pad = 14f;
            for (var round = 1; round <= CareerCupSystem.Rounds; round++)
            {
                var index = round - 1;
                var rowY = y + 4f + index * (rowHeight + gap);
                if (rowY + rowHeight > footerY - 4f)
                    break;

                var opponentId = save.cupOpponentIds != null && index < save.cupOpponentIds.Length
                    ? save.cupOpponentIds[index] : string.Empty;
                var opponent = WorldDatabase.GetClub(opponentId);
                string detail;
                if (opponent != null)
                {
                    var score = index < save.cupUserGoals.Length && index < save.cupOpponentGoals.Length
                        ? $"{save.cupUserGoals[index]}  -  {save.cupOpponentGoals[index]}" : "Result unavailable";
                    var penalties = save.cupUserPenalties != null && save.cupOpponentPenalties != null &&
                                    index < save.cupUserPenalties.Length && index < save.cupOpponentPenalties.Length &&
                                    (save.cupUserPenalties[index] > 0 || save.cupOpponentPenalties[index] > 0)
                        ? $"  (PEN {save.cupUserPenalties[index]} - {save.cupOpponentPenalties[index]})" : string.Empty;
                    detail = $"{opponent.Name}   •   {score}{penalties}";
                }
                else if (tie != null && tie.Round == round)
                {
                    var nextOpponent = WorldDatabase.GetClub(tie.OpponentFor(save.managedClubId));
                    detail = nextOpponent == null ? "Fixture pending" : "vs " + nextOpponent.Name;
                }
                else if (save.cupRound == 0 && played > 0 && round > played)
                    detail = "Eliminated before this round";
                else
                    detail = "Not played yet";

                var rect = new Rect(x, rowY, width, rowHeight);
                GUI.Box(rect, GUIContent.none, round <= played ? heroCardStyle : cardStyle);
                GUI.Label(new Rect(rect.x + pad, rect.y, rect.width * 0.32f, rect.height),
                    CareerCupSystem.RoundName(round), cardTitleStyle);
                GUI.Label(new Rect(rect.x + rect.width * 0.34f, rect.y,
                    rect.width * 0.62f - pad, rect.height), detail, tileLabelStyle);
            }

            if (GUI.Button(new Rect(x, footerY, width, footerHeight), "BACK TO CAREER", buttonStyle))
                showCupCampaign = false;
        }

        private void DrawHonours(Rect panel)
        {
            var x = panel.x + 18f;
            var width = panel.width - 36f;
            var honours = save.honours ?? Array.Empty<string>();
            var y = DrawHeader(panel, "CLUB HONOURS",
                honours.Length == 1 ? "1 TROPHY" : $"{honours.Length} TROPHIES", true,
                () => showHonours = false);
            var footerHeight = Mathf.Clamp(panel.height * 0.09f, 42f, 60f);
            var footerY = panel.yMax - footerHeight - 12f;
            var rowHeight = Mathf.Clamp(panel.height * 0.075f, 34f, 54f);
            var innerWidth = width - 20f;
            var gap = 8f;
            var viewY = y + 4f;
            var viewHeight = Mathf.Max(40f, footerY - viewY - 6f);

            if (honours.Length == 0)
            {
                var empty = new Rect(x, viewY, width, rowHeight * 2f);
                GUI.Box(empty, GUIContent.none, cardStyle);
                GUI.Label(new Rect(empty.x + 14f, empty.y, empty.width - 28f, empty.height),
                    "No trophies recorded yet. Keep playing to build your club history.", bodyStyle);
            }
            else
            {
                honoursScroll = GUI.BeginScrollView(new Rect(x, viewY, width, viewHeight), honoursScroll,
                    new Rect(0f, 0f, innerWidth, honours.Length * (rowHeight + gap)));
                for (var i = 0; i < honours.Length; i++)
                {
                    var rect = new Rect(0f, i * (rowHeight + gap), innerWidth, rowHeight);
                    GUI.Box(rect, GUIContent.none, i == 0 ? heroCardStyle : cardStyle);
                    GUI.Label(new Rect(rect.x + 14f, rect.y, rect.width - 28f, rect.height),
                        $"{i + 1:00}   {honours[i]}", i == 0 ? cardValueStyle : tileLabelStyle);
                }
                GUI.EndScrollView();
            }

            if (GUI.Button(new Rect(x, footerY, width, footerHeight), "BACK TO CAREER", buttonStyle))
                showHonours = false;
        }

        public void RefreshCareer(bool matchResultSaved = true)
        {
            save = CareerSaveSystem.Load();
            showHomeScreen = true;
            showSettingsScreen = false;
            showClubBrowser = false;
            showLeagueTable = false;
            showCupCampaign = false;
            showHonours = false;
            showSquadManagement = false;
            showTransfers = false;
            confirmNewCareer = false;
            marketCache = null;
            pendingTransferPlayerId = string.Empty;
            pendingTransferAction = string.Empty;
            selectedTransferPlayerId = string.Empty;
            notice = save == null
                ? "Match ended. Browse the world to start a career."
                : !matchResultSaved
                    ? "Match abandoned. No result was recorded."
                    : save.matchweek == 1 && save.season > 1 && !string.IsNullOrEmpty(save.lastSeasonSummary)
                    ? save.lastSeasonSummary
                    : "Match result saved. Career updated.";
        }

        private bool MatchIsRunning()
        {
            return GetComponent<MatchManager3D>() != null && !enabled;
        }

        private static string PositionLabel(PlayerPosition position)
        {
            switch (position)
            {
                case PlayerPosition.Goalkeeper:
                    return "GK";
                case PlayerPosition.Defender:
                    return "DEF";
                case PlayerPosition.Midfielder:
                    return "MID";
                default:
                    return "FWD";
            }
        }
    }
}
