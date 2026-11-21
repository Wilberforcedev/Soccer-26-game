import 'dart:async';
import 'dart:math' as math;

import 'package:flutter/material.dart';

import 'models/club_player.dart';
import 'theme/game_theme.dart';
import 'widgets/game_painters.dart';

void main() => runApp(const FC2026App());

class FC2026App extends StatelessWidget {
  const FC2026App({super.key});

  @override
  Widget build(BuildContext context) => MaterialApp(
        title: 'FC 2026',
        debugShowCheckedModeBanner: false,
        theme: buildGameTheme(),
        home: const GameShell(),
      );
}

class GameShell extends StatefulWidget {
  const GameShell({super.key});
  @override
  State<GameShell> createState() => _GameShellState();
}

class _GameShellState extends State<GameShell> {
  String screen = 'home';
  String previousScreen = 'home';
  final List<ClubPlayer> lineup = List<ClubPlayer>.of(startingPlayers);
  final List<ClubPlayer> bench = List<ClubPlayer>.of(reservePlayers);
  int selectedPlayer = 9;
  int selectedKit = 0;
  int equippedKit = 0;
  int careerTab = 0;
  int storeTab = 0;
  int matchMinute = 32;
  int matchSecond = 14;
  int homeGoals = 1;
  int awayGoals = 0;
  int coins = 245870;
  int gems = 1250;
  int transferBudget = 180000000;
  String formation = '4-3-3';
  String tacticalStyle = 'Balanced';
  String? captainName = 'Kylian Mbappé';
  String? enteredTournament;
  String? scoutedPlayer;
  bool musicEnabled = true;
  bool soundEnabled = true;
  bool vibrationEnabled = true;
  final List<ClubPlayer> collection = [];
  Offset controlledPlayer = const Offset(.54, .52);
  Offset ballPosition = const Offset(.57, .50);
  String matchMessage = '';
  Timer? matchClock;
  Timer? matchMessageTimer;

  @override
  void dispose() {
    matchClock?.cancel();
    matchMessageTimer?.cancel();
    super.dispose();
  }

  void go(String target) {
    if (target == 'pause' && screen == 'hud') {
      previousScreen = screen;
      matchClock?.cancel();
    }
    if (target == 'hud' && screen == 'prematch') {
      matchMinute = 0;
      matchSecond = 0;
      homeGoals = 0;
      awayGoals = 0;
      controlledPlayer = const Offset(.54, .52);
      ballPosition = const Offset(.57, .50);
    }
    setState(() => screen = target);
    if (target == 'hud') _startMatchClock();
  }

  void _startMatchClock() {
    matchClock?.cancel();
    matchClock = Timer.periodic(const Duration(seconds: 1), (_) {
      if (!mounted || screen != 'hud') return;
      if (matchMinute >= 90) {
        matchClock?.cancel();
        message('Full time  •  $homeGoals - $awayGoals');
        go('pause');
      } else {
        setState(() {
          matchSecond += 15;
          if (matchSecond >= 60) {
            matchMinute++;
            matchSecond -= 60;
          }
        });
      }
    });
  }

  Offset _clampFieldPoint(Offset point) => Offset(
        point.dx.clamp(.06, .94).toDouble(),
        point.dy.clamp(.08, .92).toDouble(),
      );

  void _movePlayer(DragUpdateDetails details) {
    _movePlayerBy(details.delta);
  }

  void _movePlayerBy(Offset delta) {
    setState(() {
      controlledPlayer = _clampFieldPoint(Offset(
        controlledPlayer.dx + delta.dx / 320,
        controlledPlayer.dy + delta.dy / 420,
      ));
      ballPosition = _clampFieldPoint(
          controlledPlayer + const Offset(.025, -.035));
    });
  }

  Future<void> _editFormation() async {
    final options = ['4-3-3', '4-4-2', '3-5-2'];
    final choice = await showModalBottomSheet<String>(
      context: context,
      backgroundColor: const Color(0xFF081B2D),
      builder: (context) => SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(mainAxisSize: MainAxisSize.min, children: [
            const Text('CHOOSE FORMATION',
                style: TextStyle(fontWeight: FontWeight.w900, letterSpacing: 1)),
            const SizedBox(height: 10),
            ...options.map((option) => ListTile(
                  leading: Icon(Icons.sports_soccer,
                      color: option == formation ? cyan : muted),
                  title: Text(option),
                  trailing: option == formation
                      ? const Icon(Icons.check_circle, color: cyan)
                      : null,
                  onTap: () => Navigator.pop(context, option),
                )),
          ]),
        ),
      ),
    );
    if (choice != null) setState(() => formation = choice);
  }

  Future<void> _editTactics() async {
    const options = ['Balanced', 'Possession', 'High press', 'Counter attack'];
    final choice = await showModalBottomSheet<String>(
      context: context,
      backgroundColor: const Color(0xFF081B2D),
      builder: (context) => SafeArea(
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          const Padding(
            padding: EdgeInsets.all(16),
            child: Text('TEAM TACTICS',
                style: TextStyle(fontWeight: FontWeight.w900, letterSpacing: 1)),
          ),
          ...options.map((option) => ListTile(
                title: Text(option),
                trailing: option == tacticalStyle
                    ? const Icon(Icons.check_circle, color: cyan)
                    : null,
                onTap: () => Navigator.pop(context, option),
              )),
        ]),
      ),
    );
    if (choice != null) setState(() => tacticalStyle = choice);
  }

  Future<void> _editRoles() async {
    final choice = await showModalBottomSheet<String>(
      context: context,
      backgroundColor: const Color(0xFF081B2D),
      builder: (context) => SafeArea(
        child: ListView(
          shrinkWrap: true,
          children: [
            const Padding(
              padding: EdgeInsets.all(16),
              child: Text('CHOOSE CAPTAIN',
                  style: TextStyle(fontWeight: FontWeight.w900, letterSpacing: 1)),
            ),
            ...lineup.map((player) => ListTile(
                  title: Text(player.name),
                  subtitle: Text('${player.role}  •  OVR ${player.rating}'),
                  trailing: captainName == player.name
                      ? const Icon(Icons.check_circle, color: cyan)
                      : null,
                  onTap: () => Navigator.pop(context, player.name),
                )),
          ],
        ),
      ),
    );
    if (choice != null) setState(() => captainName = choice);
  }

  void _autoSelect() {
    final available = [...lineup, ...bench];
    final keepers = available.where((player) => player.role == 'GK').toList()
      ..sort((a, b) => b.rating.compareTo(a.rating));
    final outfield = available.where((player) => player.role != 'GK').toList()
      ..sort((a, b) => b.rating.compareTo(a.rating));
    if (keepers.isEmpty || outfield.length < 10) return;
    setState(() {
      lineup
        ..clear()
        ..add(keepers.first)
        ..addAll(outfield.take(10));
      bench
        ..clear()
        ..addAll(available.where((player) => !lineup.contains(player)));
      selectedPlayer = lineup.indexWhere((player) => player.name == captainName);
      if (selectedPlayer < 0) {
        selectedPlayer = 9;
        captainName = lineup[9].name;
      }
    });
    message('Best available starting XI selected');
  }

  Future<void> _swapPlayer() async {
    if (bench.isEmpty) {
      message('There are no substitutes available');
      return;
    }
    final replacement = await showModalBottomSheet<ClubPlayer>(
      context: context,
      backgroundColor: const Color(0xFF081B2D),
      builder: (context) => SafeArea(
        child: ListView(
          shrinkWrap: true,
          children: [
            const Padding(
              padding: EdgeInsets.all(16),
              child: Text('SELECT A SUBSTITUTE',
                  style: TextStyle(fontWeight: FontWeight.w900, letterSpacing: 1)),
            ),
            ...bench.map((player) => ListTile(
                  leading: _avatar(player.role, cyan, 42),
                  title: Text(player.name),
                  subtitle: Text('${player.role}  •  OVR ${player.rating}'),
                  onTap: () => Navigator.pop(context, player),
                )),
          ],
        ),
      ),
    );
    if (replacement == null) return;
    final outgoing = lineup[selectedPlayer];
    setState(() {
      lineup[selectedPlayer] = replacement;
      bench
        ..remove(replacement)
        ..add(outgoing);
      if (captainName == outgoing.name) captainName = replacement.name;
    });
    message('${replacement.name} replaces ${outgoing.name}');
  }

  void _trainPlayer() {
    if (lineup[selectedPlayer].rating >= 99) {
      message('${lineup[selectedPlayer].name} has reached the training limit');
      return;
    }
    if (coins < 10000) {
      message('Training costs 10,000 coins');
      return;
    }
    setState(() {
      final player = lineup[selectedPlayer];
      lineup[selectedPlayer] = player.copyWith(
        rating: (player.rating + 1).clamp(1, 99).toInt(),
        fitness: 100,
        dribbling: (player.dribbling + 1).clamp(1, 99).toInt(),
      );
      coins -= 10000;
    });
    message('${lineup[selectedPlayer].name} improved  •  10,000 coins spent');
  }

  void _autoPass() {
    setState(() {
      ballPosition = _clampFieldPoint(
        Offset(controlledPlayer.dx + .18, controlledPlayer.dy - .12),
      );
      controlledPlayer = _clampFieldPoint(
        Offset(controlledPlayer.dx + .10, controlledPlayer.dy - .06),
      );
    });
    message('Pass played');
  }

  void _sprint() {
    setState(() {
      controlledPlayer = _clampFieldPoint(
        Offset(controlledPlayer.dx, controlledPlayer.dy - .10),
      );
      ballPosition = _clampFieldPoint(
        controlledPlayer + const Offset(.025, -.025),
      );
    });
    message('Sprint');
  }

  void _shoot() {
    final inRange = controlledPlayer.dy < .34 && ballPosition.dy < .38;
    setState(() {
      matchMinute = (matchMinute + 1).clamp(0, 90).toInt();
      matchSecond = 0;
      if (inRange) homeGoals++;
      ballPosition = _clampFieldPoint(
          Offset(controlledPlayer.dx, controlledPlayer.dy - .22));
    });
    message(inRange ? 'GOAL! Real Madrid scores' : 'Shot blocked  •  Move closer to goal');
  }

  void _equipKit() {
    setState(() => equippedKit = selectedKit);
    message('${_kitName(equippedKit)} kit equipped');
  }

  String _kitName(int index) =>
      ['Home', 'Away', 'Third', 'Goalkeeper', 'Retro', 'Training'][index % 6];

  void _startTrainingReport() => showDialog<void>(
        context: context,
        builder: (context) => AlertDialog(
          backgroundColor: const Color(0xFF081B2D),
          title: const Text('PLAYER REPORT'),
          content: Text(
              '${lineup[selectedPlayer].name}\n\n${lineup[selectedPlayer].role}  •  OVR ${lineup[selectedPlayer].rating}\nCaptain: ${captainName == lineup[selectedPlayer].name ? 'Yes' : 'No'}\nFitness: ${lineup[selectedPlayer].fitness}%\nTraining upgrades: ${math.max(0, lineup[selectedPlayer].rating - startingPlayers.firstWhere((p) => p.name == lineup[selectedPlayer].name, orElse: () => lineup[selectedPlayer]).rating)}'),
          actions: [TextButton(onPressed: () => Navigator.pop(context), child: const Text('CLOSE'))],
        ),
      );

  Future<void> _showSettings() async {
    await showDialog<void>(
      context: context,
      builder: (context) => StatefulBuilder(
        builder: (context, updateDialog) => AlertDialog(
          backgroundColor: const Color(0xFF081B2D),
          title: const Text('SETTINGS'),
          content: Column(mainAxisSize: MainAxisSize.min, children: [
            SwitchListTile.adaptive(
              contentPadding: EdgeInsets.zero,
              title: const Text('Music'),
              value: musicEnabled,
              onChanged: (value) => updateDialog(() => musicEnabled = value),
            ),
            SwitchListTile.adaptive(
              contentPadding: EdgeInsets.zero,
              title: const Text('Match sound'),
              value: soundEnabled,
              onChanged: (value) => updateDialog(() => soundEnabled = value),
            ),
            SwitchListTile.adaptive(
              contentPadding: EdgeInsets.zero,
              title: const Text('Vibration'),
              value: vibrationEnabled,
              onChanged: (value) => updateDialog(() => vibrationEnabled = value),
            ),
          ]),
          actions: [
            TextButton(
                onPressed: () => Navigator.pop(context),
                child: const Text('DONE')),
          ],
        ),
      ),
    );
    if (mounted) setState(() {});
  }

  void _buyPlayer(String name, int cost, ClubPlayer player) {
    if (lineup.any((member) => member.name == player.name) ||
        bench.any((member) => member.name == player.name)) {
      message('$name is already in your club');
      return;
    }
    if (coins < cost) {
      message('You need ${cost - coins} more coins to sign $name');
      return;
    }
    setState(() {
      coins -= cost;
      bench.add(player);
      collection.add(player);
    });
    message('$name added to your squad');
  }

  void _purchasePack(int cost, {required String title}) {
    if (gems < cost) {
      message('You need ${cost - gems} more gems for $title');
      return;
    }
    final pool = [
      ...startingPlayers.where((player) => player.rating >= 84),
      ...reservePlayers.where((player) => player.rating >= 82),
    ];
    final reward = pool[math.Random().nextInt(pool.length)];
    setState(() {
      gems -= cost;
      collection.add(reward);
      if (!lineup.any((player) => player.name == reward.name) &&
          !bench.any((player) => player.name == reward.name)) {
        bench.add(reward);
      }
    });
    showDialog<void>(
      context: context,
      builder: (context) => AlertDialog(
        backgroundColor: const Color(0xFF081B2D),
        title: Text('$title REWARD'),
        content: Row(children: [
          _avatar(reward.role, cyan, 54),
          const SizedBox(width: 12),
          Expanded(child: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text(reward.name, style: const TextStyle(fontWeight: FontWeight.w900)),
            Text('${reward.role}  •  OVR ${reward.rating}', style: const TextStyle(color: muted)),
          ])),
        ]),
        actions: [TextButton(onPressed: () => Navigator.pop(context), child: const Text('ADD TO SQUAD'))],
      ),
    );
  }

  void back() => go(screen == 'player' ? 'squad' : 'home');

  void message(String text) {
    setState(() => matchMessage = text);
    matchMessageTimer?.cancel();
    matchMessageTimer = Timer(const Duration(seconds: 2), () {
      if (mounted) setState(() => matchMessage = '');
    });
    if (screen != 'hud') {
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(
        content: Text(text),
        behavior: SnackBarBehavior.floating,
        backgroundColor: const Color(0xFF0E3550),
      ));
    }
  }

  @override
  Widget build(BuildContext context) {
    final isMatch = screen == 'hud' || screen == 'pause';
    return Scaffold(
      body: DecoratedBox(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [Color(0xFF071A30), ink, Color(0xFF020812)],
          ),
        ),
        child: Stack(children: [
          const Positioned.fill(child: CustomPaint(painter: StadiumGlowPainter())),
          SafeArea(
            child: Column(children: [
              if (!isMatch) _topBar(),
              Expanded(child: _screenBody()),
              if (!isMatch) _bottomNav(),
            ]),
          ),
        ]),
      ),
    );
  }

  Widget _screenBody() => switch (screen) {
        'home' => _home(),
        'squad' => _squad(),
        'player' => _playerDetails(),
        'kit' => _kitRoom(),
        'career' => _career(),
        'tournaments' => _tournaments(),
        'store' => _store(),
        'prematch' => _preMatch(),
        'hud' => _matchHud(),
        'pause' => _pauseMenu(),
        _ => _home(),
      };

  Widget _topBar() => Padding(
        padding: const EdgeInsets.fromLTRB(16, 8, 14, 7),
        child: Row(children: [
          if (screen != 'home') ...[
            _iconButton(Icons.arrow_back_rounded, back),
            const SizedBox(width: 8),
          ],
          Expanded(
            child: InkWell(
              onTap: () => go('home'),
              child: const Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('FC 2026',
                      style: TextStyle(
                          fontSize: 21,
                          height: 1,
                          fontWeight: FontWeight.w900,
                          fontStyle: FontStyle.italic,
                          letterSpacing: -1)),
                  SizedBox(height: 3),
                  Text('NEXT GEN FOOTBALL',
                      style: TextStyle(
                          color: cyan,
                          fontSize: 10,
                          fontWeight: FontWeight.w800,
                          letterSpacing: 1.2)),
                ],
              ),
            ),
          ),
          _currency(Icons.monetization_on, '$coins', gold),
          const SizedBox(width: 6),
          _currency(Icons.diamond, '$gems', cyan),
          const SizedBox(width: 6),
          _iconButton(Icons.settings_outlined, _showSettings),
        ]),
      );

  Widget _currency(IconData icon, String value, Color tint) => Container(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 7),
        decoration: BoxDecoration(
          color: const Color(0xE90A1D30),
          border: Border.all(color: line),
          borderRadius: BorderRadius.circular(11),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          Icon(icon, size: 14, color: tint),
          const SizedBox(width: 4),
          Text(value,
              style: const TextStyle(fontSize: 10, fontWeight: FontWeight.w900)),
        ]),
      );

  Widget _iconButton(IconData icon, VoidCallback onTap) => SizedBox(
        width: 42,
        height: 42,
        child: IconButton.filledTonal(
          onPressed: onTap,
          style: IconButton.styleFrom(
            backgroundColor: const Color(0xC90D2339),
            foregroundColor: Colors.white,
            side: const BorderSide(color: line),
          ),
          icon: Icon(icon, size: 19),
        ),
      );

  Widget _bottomNav() => Container(
        padding: const EdgeInsets.fromLTRB(6, 6, 6, 8),
        decoration: const BoxDecoration(
          color: Color(0xF5061220),
          border: Border(top: BorderSide(color: line)),
        ),
        child: Row(mainAxisAlignment: MainAxisAlignment.spaceAround, children: [
          _navItem(Icons.home_rounded, 'HOME', 'home'),
          _navItem(Icons.emoji_events_outlined, 'CAREER', 'career'),
          _navItem(Icons.groups_rounded, 'SQUAD', 'squad'),
          _navItem(Icons.checkroom_rounded, 'KIT ROOM', 'kit'),
          _navItem(Icons.storefront_outlined, 'STORE', 'store'),
        ]),
      );

  Widget _navItem(IconData icon, String label, String target) {
    final active = screen == target || (target == 'squad' && screen == 'player');
    return InkWell(
      onTap: () => go(target),
      borderRadius: BorderRadius.circular(12),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Icon(icon, size: 20, color: active ? cyan : muted),
          const SizedBox(height: 3),
          Text(label,
              style: TextStyle(
                  color: active ? cyan : muted,
                  fontSize: 10,
                  fontWeight: FontWeight.w900,
                  letterSpacing: .5)),
        ]),
      ),
    );
  }

  Widget _page({required String title, String? eyebrow, required List<Widget> children}) =>
      ListView(
        padding: const EdgeInsets.fromLTRB(16, 13, 16, 22),
        children: [
          if (eyebrow != null) _eyebrow(eyebrow),
          Text(title,
              style: const TextStyle(
                  fontSize: 24,
                  height: 1.05,
                  fontWeight: FontWeight.w900,
                  letterSpacing: -.5)),
          const SizedBox(height: 14),
          ...children,
        ],
      );

  Widget _eyebrow(String text) => Padding(
        padding: const EdgeInsets.only(bottom: 6),
        child: Text(text.toUpperCase(),
            style: const TextStyle(
                color: cyan,
                fontSize: 10,
                fontWeight: FontWeight.w900,
                letterSpacing: 1.5)),
      );

  Widget _home() => _page(
        eyebrow: 'MATCHDAY 32  •  LA LIGA',
        title: 'The next chapter\nstarts here.',
        children: [
          _homeFeature(),
          const SizedBox(height: 12),
          Row(children: [
            Expanded(child: _quickMode(Icons.sports_soccer, 'PLAY MATCH', 'Kick off now', 'prematch')),
            const SizedBox(width: 9),
            Expanded(child: _quickMode(Icons.emoji_events, 'CAREER', 'Season 2025/26', 'career')),
          ]),
          const SizedBox(height: 12),
          Row(children: [
            Expanded(child: _quickMode(Icons.emoji_events_outlined, 'TOURNAMENTS', '5 competitions', 'tournaments')),
            const SizedBox(width: 9),
            Expanded(child: _quickMode(Icons.groups_rounded, 'MY SQUAD', 'OVR ${_teamRating()}  •  $formation', 'squad')),
          ]),
          const SizedBox(height: 12),
          _sectionTitle('NEXT MATCH', 'VIEW FIXTURE', () => go('prematch')),
          const SizedBox(height: 8),
          _nextMatchCard(),
        ],
      );

  Widget _homeFeature() => Container(
        height: 202,
        clipBehavior: Clip.antiAlias,
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: const Color(0xFF1E7299)),
          gradient: const LinearGradient(
              begin: Alignment.topRight,
              end: Alignment.bottomLeft,
              colors: [Color(0xFF14517A), Color(0xFF09243B), Color(0xFF061322)]),
        ),
        child: Stack(children: [
          const Positioned.fill(child: CustomPaint(painter: StadiumGlowPainter())),
          Positioned(
            right: -4,
            bottom: 0,
            child: IgnorePointer(
              child: Image.asset(
                'assets/images/fictional_player.webp',
                width: 155,
                height: 194,
                fit: BoxFit.contain,
                alignment: Alignment.bottomCenter,
              ),
            ),
          ),
          Positioned.fill(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                _pill('ULTIMATE TEAM', cyan),
                const Spacer(),
                const Text('BUILD YOUR\nLEGACY',
                    style: TextStyle(
                        fontSize: 29,
                        height: .98,
                        letterSpacing: -.8,
                        fontWeight: FontWeight.w900)),
                const SizedBox(height: 7),
                const Text('Every match moves you forward.',
                    style: TextStyle(color: Color(0xFFC1D2E1), fontSize: 11)),
                const SizedBox(height: 11),
                _actionButton('PLAY MATCH', Icons.play_arrow_rounded, () => go('prematch')),
              ]),
            ),
          ),
        ]),
      );

  Widget _playerSilhouette({double size = 80, Color color = cyan, double opacity = .76}) =>
      SizedBox.square(
        dimension: size,
        child: CustomPaint(
          painter: PlayerSilhouettePainter(jerseyColor: color, opacity: opacity),
        ),
      );

  Widget _quickMode(IconData icon, String label, String sub, String target) =>
      InkWell(
        onTap: () => go(target),
        borderRadius: BorderRadius.circular(16),
        child: Container(
          height: 82,
          padding: const EdgeInsets.all(12),
          decoration: _boxDecoration(),
          child: Row(children: [
            _iconTile(icon, cyan, 39),
            const SizedBox(width: 10),
            Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, mainAxisAlignment: MainAxisAlignment.center, children: [
              Text(label, maxLines: 1, overflow: TextOverflow.ellipsis,
                  style: const TextStyle(fontSize: 11, fontWeight: FontWeight.w900)),
              const SizedBox(height: 4),
              Text(sub, maxLines: 1, overflow: TextOverflow.ellipsis,
                  style: const TextStyle(fontSize: 10, color: muted)),
            ])),
          ]),
        ),
      );

  Widget _nextMatchCard() => _panel(
        Row(children: [
          _crest('RMA', gold),
          const SizedBox(width: 9),
          const Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text('REAL MADRID', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 12)),
            SizedBox(height: 3),
            Text('SAT  •  20:00  •  SANTIAGO BERNABÉU', maxLines: 1,
                overflow: TextOverflow.ellipsis, style: TextStyle(fontSize: 10, color: muted)),
          ])),
          _crest('FCB', const Color(0xFFFF667A)),
        ]),
      );

  Widget _squad() => _page(
        eyebrow: 'REAL MADRID  •  FORMATION $formation  •  $tacticalStyle',
        title: 'My Squad',
        children: [
          Row(children: [
            const Expanded(child: Text('Starting XI', style: TextStyle(color: muted, fontSize: 12))),
            _pill('OVR ${_teamRating()}', cyan),
          ]),
          const SizedBox(height: 10),
          Container(
            height: 430,
            padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 8),
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(18),
              border: Border.all(color: const Color(0xFF21754B)),
              gradient: const LinearGradient(colors: [Color(0xFF0B653D), Color(0xFF08452D), Color(0xFF073A2B)]),
            ),
            child: Stack(children: [
              const Positioned.fill(child: CustomPaint(painter: PitchPainter())),
              ...List.generate(lineup.length, (i) {
                final points = _formationPoints();
                return Positioned(
                  left: points[i].dx * (MediaQuery.sizeOf(context).width - 48) - 29,
                  top: points[i].dy * 398 - 39,
                  child: _squadCard(i),
                );
              }),
            ]),
          ),
          const SizedBox(height: 12),
          Row(children: [
            Expanded(child: _outlineButton('TACTICS', Icons.tune, _editTactics)),
            const SizedBox(width: 8),
            Expanded(child: _outlineButton('ROLES', Icons.badge_outlined, _editRoles)),
            const SizedBox(width: 8),
            Expanded(child: _outlineButton('AUTO SELECT', Icons.auto_fix_high, _autoSelect)),
          ]),
          const SizedBox(height: 14),
          _sectionTitle('SUBSTITUTES', 'EDIT', () => message('Bench editor opened')),
          const SizedBox(height: 8),
          SizedBox(height: 102, child: ListView.separated(
            scrollDirection: Axis.horizontal,
            itemCount: bench.length,
            separatorBuilder: (_, __) => const SizedBox(width: 8),
            itemBuilder: (_, i) => _benchCard(bench[i]),
          )),
        ],
      );

  Widget _squadCard(int index) {
    final p = lineup[index];
    return InkWell(
      onTap: () { setState(() => selectedPlayer = index); go('player'); },
      borderRadius: BorderRadius.circular(12),
      child: Container(
        width: 58,
        height: 78,
        padding: const EdgeInsets.all(5),
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(12),
          gradient: const LinearGradient(begin: Alignment.topLeft, end: Alignment.bottomRight,
              colors: [Color(0xFF38DFFF), Color(0xFF0A5D82), Color(0xFF082D4C)]),
          border: Border.all(color: index == selectedPlayer ? Colors.white : const Color(0xFF64E6FF), width: index == selectedPlayer ? 2 : 1),
          boxShadow: [BoxShadow(color: cyan.withValues(alpha: .18), blurRadius: 8)],
        ),
        child: Column(children: [
          Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
            Text('${p.rating}', style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w900)),
            Text(p.role, style: const TextStyle(fontSize: 10, fontWeight: FontWeight.w900)),
          ]),
          Expanded(child: Icon(Icons.person, color: Colors.white.withValues(alpha: .86), size: 30)),
          Text(p.shortName, maxLines: 1, overflow: TextOverflow.ellipsis,
              style: const TextStyle(fontSize: 10, fontWeight: FontWeight.w900)),
        ]),
      ),
    );
  }

  Widget _benchCard(ClubPlayer player) => InkWell(
    onTap: () => showDialog<void>(
      context: context,
      builder: (context) => AlertDialog(
        backgroundColor: const Color(0xFF081B2D),
        title: Text(player.name),
        content: Text('${player.role}  •  OVR ${player.rating}\nFitness ${player.fitness}%'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(context), child: const Text('CLOSE')),
          FilledButton(onPressed: () { Navigator.pop(context); _swapBenchPlayer(player); }, child: const Text('SUB IN')),
        ],
      ),
    ),
    child: Container(width: 138, padding: const EdgeInsets.all(9), decoration: _boxDecoration(), child: Row(children: [
      _avatar(player.role, cyan, 42),
      const SizedBox(width: 8),
      Column(crossAxisAlignment: CrossAxisAlignment.start, mainAxisAlignment: MainAxisAlignment.center, children: [
        Text('${player.rating}  ${player.role}', style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 11)),
        SizedBox(width: 80, child: Text(player.shortName, maxLines: 1, overflow: TextOverflow.ellipsis, style: const TextStyle(fontSize: 10, color: muted))),
      ]),
    ])),
  );

  void _swapBenchPlayer(ClubPlayer replacement) {
    final outgoing = lineup[selectedPlayer];
    setState(() {
      lineup[selectedPlayer] = replacement;
      bench
        ..remove(replacement)
        ..add(outgoing);
    });
    message('${replacement.shortName} enters for ${outgoing.shortName}');
  }

  int _teamRating() => lineup.isEmpty
      ? 0
      : (lineup.fold<int>(0, (total, player) => total + player.rating) /
              lineup.length)
          .round();

  List<Offset> _formationPoints() => switch (formation) {
        '4-4-2' => const [
            Offset(.50,.90), Offset(.16,.70), Offset(.39,.73), Offset(.61,.73), Offset(.84,.70),
            Offset(.16,.48), Offset(.39,.51), Offset(.61,.51), Offset(.84,.48), Offset(.37,.23), Offset(.63,.23),
          ],
        '3-5-2' => const [
            Offset(.50,.90), Offset(.25,.70), Offset(.50,.73), Offset(.75,.70), Offset(.12,.49),
            Offset(.32,.51), Offset(.50,.47), Offset(.68,.51), Offset(.88,.49), Offset(.38,.23), Offset(.62,.23),
          ],
        _ => const [
            Offset(.50,.90), Offset(.16,.70), Offset(.39,.73), Offset(.61,.73), Offset(.84,.70),
            Offset(.31,.49), Offset(.51,.53), Offset(.70,.47), Offset(.18,.24), Offset(.50,.19), Offset(.82,.24),
          ],
      };

  Widget _playerDetails() {
    final p = lineup[selectedPlayer];
    return _page(
      eyebrow: 'PLAYER PROFILE  •  REAL MADRID',
      title: p.name,
      children: [
        Container(
          padding: const EdgeInsets.all(14),
          decoration: _boxDecoration(radius: 20),
          child: Row(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            Expanded(flex: 4, child: Container(
              constraints: const BoxConstraints(minHeight: 285),
              decoration: BoxDecoration(
                borderRadius: BorderRadius.circular(17),
                border: Border.all(color: cyan.withValues(alpha: .55)),
                gradient: const LinearGradient(begin: Alignment.topCenter, end: Alignment.bottomCenter,
                    colors: [Color(0xFF155880), Color(0xFF092744), Color(0xFF071626)]),
              ),
              child: Stack(children: [
                Positioned(right: 0, bottom: 0, child: _playerSilhouette(size: 175)),
                Padding(padding: const EdgeInsets.all(12), child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text('${p.rating}', style: const TextStyle(fontSize: 34, fontWeight: FontWeight.w900)),
                  Text(p.role, style: const TextStyle(color: cyan, fontWeight: FontWeight.w900)),
                  const Spacer(),
                  Text(captainName == p.name ? 'CAPTAIN  •  REAL MADRID' : 'REAL MADRID', style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 10)),
                  const SizedBox(height: 4),
                  Text('AGE     ${p.age}\nHEIGHT  ${p.heightCm} cm\nFOOT    ${p.preferredFoot.toUpperCase()}', style: const TextStyle(color: Color(0xFFD0DEEA), fontSize: 10, height: 1.7)),
                ])),
              ]),
            )),
            const SizedBox(width: 12),
            Expanded(flex: 5, child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              _statRow('PAC', p.pace),
              _statRow('SHO', p.shooting),
              _statRow('PAS', p.passing),
              _statRow('DRI', p.dribbling),
              _statRow('DEF', p.defending),
              _statRow('PHY', p.physical),
              const SizedBox(height: 12),
              const Text('FITNESS', style: TextStyle(fontSize: 10, color: muted, fontWeight: FontWeight.w900, letterSpacing: 1)),
              const SizedBox(height: 6),
              ClipRRect(borderRadius: BorderRadius.circular(8), child: LinearProgressIndicator(value: p.fitness / 100, minHeight: 10, color: const Color(0xFF18D888), backgroundColor: const Color(0xFF173447))),
              const SizedBox(height: 5),
              Text('${p.fitness}%', style: const TextStyle(color: Color(0xFF18D888), fontWeight: FontWeight.w900, fontSize: 10)),
            ])),
          ]),
        ),
        const SizedBox(height: 14),
        Row(children: [
          Expanded(child: _actionButton('SWAP', Icons.swap_horiz, _swapPlayer)),
          const SizedBox(width: 8),
          Expanded(child: _outlineButton('TRAIN · 10K', Icons.fitness_center, _trainPlayer)),
          const SizedBox(width: 8),
          Expanded(child: _outlineButton('DETAILS', Icons.info_outline, _startTrainingReport)),
        ]),
      ],
    );
  }

  Widget _statRow(String label, int value) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 7),
    child: Row(children: [
      SizedBox(width: 35, child: Text(label, style: const TextStyle(color: muted, fontSize: 10, fontWeight: FontWeight.w800))),
      Expanded(child: ClipRRect(borderRadius: BorderRadius.circular(5), child: LinearProgressIndicator(value: value / 99, minHeight: 6, color: cyan, backgroundColor: const Color(0xFF173447)))),
      const SizedBox(width: 8),
      Text('$value', style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 11)),
    ]),
  );

  Widget _kitRoom() => _page(
    eyebrow: 'REAL MADRID  •  6 / 6 KITS',
    title: 'Kit Room',
    children: [
      _segmented(['HOME', 'AWAY', 'THIRD', 'GK'], selectedKit % 4, (i) => setState(() => selectedKit = i)),
      const SizedBox(height: 12),
      Container(
        height: 335,
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(20),
          border: Border.all(color: line),
          gradient: const LinearGradient(begin: Alignment.topCenter, end: Alignment.bottomCenter,
            colors: [Color(0xFF113550), Color(0xFF061321), Color(0xFF091A2A)]),
        ),
        child: Stack(children: [
          const Positioned.fill(child: CustomPaint(painter: StadiumGlowPainter())),
          Align(alignment: Alignment.center, child: _jersey(selectedKit, 178)),
          Positioned(left: 6, top: 0, bottom: 0, child: Center(child: _iconButton(Icons.chevron_left, () => setState(() => selectedKit = (selectedKit + 5) % 6)))),
          Positioned(right: 6, top: 0, bottom: 0, child: Center(child: _iconButton(Icons.chevron_right, () => setState(() => selectedKit = (selectedKit + 1) % 6)))),
          Positioned(left: 15, top: 14, child: _pill('${_kitName(selectedKit).toUpperCase()} KIT', cyan)),
          if (equippedKit == selectedKit)
            Positioned(right: 14, top: 14, child: _pill('EQUIPPED', const Color(0xFF18D888))),
        ]),
      ),
      const SizedBox(height: 12),
      SizedBox(height: 86, child: ListView.separated(
        scrollDirection: Axis.horizontal,
        itemCount: 6,
        separatorBuilder: (_, __) => const SizedBox(width: 8),
        itemBuilder: (_, i) => InkWell(
          onTap: () => setState(() => selectedKit = i),
          child: Container(width: 73, decoration: BoxDecoration(color: const Color(0xD9091B2C), borderRadius: BorderRadius.circular(14), border: Border.all(color: i == selectedKit ? cyan : line, width: i == selectedKit ? 2 : 1)), child: Center(child: _jersey(i, 54))),
        ),
      )),
      const SizedBox(height: 12),
      Center(child: _actionButton(equippedKit == selectedKit ? 'EQUIPPED' : 'EQUIP KIT', Icons.checkroom, _equipKit)),
    ],
  );

  Widget _jersey(int kit, double size) => SizedBox.square(
        dimension: size,
        child: CustomPaint(
          painter: JerseyPainter(
            color: [Colors.white, const Color(0xFF101C2B), const Color(0xFF6546B5), const Color(0xFF00A898), const Color(0xFFB58B55), const Color(0xFF00A7D8)][kit],
            stripes: kit == 0 || kit == 2 || kit == 4,
          ),
        ),
      );

  Widget _career() => _page(
    eyebrow: 'SEASON 2025/26  •  MANAGER LEVEL 12',
    title: 'Career',
    children: [
      _segmented(['OVERVIEW', 'STANDINGS', 'FIXTURES', 'TRANSFERS'], careerTab, (i) => setState(() => careerTab = i)),
      const SizedBox(height: 12),
      if (careerTab == 0) ...[
        Row(children: [
          Expanded(child: _careerSummary()),
          const SizedBox(width: 9),
          Expanded(child: _fixtureSummary()),
        ]),
        const SizedBox(height: 11),
        _sectionTitle('SEASON SNAPSHOT', 'FULL TABLE', () => setState(() => careerTab = 1)),
        const SizedBox(height: 7),
        _standingsTable(compact: true),
        const SizedBox(height: 11),
        Row(children: [
          Expanded(child: _careerAction(Icons.groups, 'SQUAD', () => go('squad'))),
          const SizedBox(width: 8),
          Expanded(child: _careerAction(Icons.calendar_month, 'CALENDAR', () => setState(() => careerTab = 2))),
          const SizedBox(width: 8),
          Expanded(child: _careerAction(Icons.swap_horiz, 'TRANSFERS', () => setState(() => careerTab = 3))),
        ]),
      ] else if (careerTab == 1) _standingsTable()
      else if (careerTab == 2) _fixtures()
      else _transfers(),
    ],
  );

  Widget _careerSummary() => _panel(Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
    _eyebrow('SEASON 2025/26'),
    _crest('RMA', gold),
    const SizedBox(height: 7),
    const Text('Real Madrid', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 15)),
    const SizedBox(height: 3),
    const Text('1st  •  La Liga', style: TextStyle(color: muted, fontSize: 10)),
    const SizedBox(height: 10),
    const Text('TRANSFER BUDGET', style: TextStyle(color: muted, fontSize: 10, fontWeight: FontWeight.w800)),
    const SizedBox(height: 3),
    Text('€${_formatNumber(transferBudget)}', style: const TextStyle(color: gold, fontSize: 13, fontWeight: FontWeight.w900)),
  ]));

  Widget _fixtureSummary() => _panel(Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
    _eyebrow('NEXT FIXTURE'),
    const SizedBox(height: 7),
    const Text('REAL MADRID', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 12)),
    const Padding(padding: EdgeInsets.symmetric(vertical: 4), child: Text('VS', style: TextStyle(color: cyan, fontWeight: FontWeight.w900))),
    const Text('BARCELONA', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 12)),
    const SizedBox(height: 5),
    const Text('APR 26, 2026', style: TextStyle(color: muted, fontSize: 9)),
    const SizedBox(height: 8),
    SizedBox(width: double.infinity, child: _smallButton('CONTINUE', () => go('prematch'))),
  ]));

  Widget _standingsTable({bool compact = false}) => _panel(Column(children: [
    _tableRow('POS', 'CLUB', 'P', 'PTS', header: true),
    _tableRow('1', 'Real Madrid', '32', '78', highlight: true),
    _tableRow('2', 'Barcelona', '32', '74'),
    _tableRow('3', 'Atlético Madrid', '32', '67'),
    if (!compact) ...[
      _tableRow('4', 'Athletic Club', '32', '60'),
      _tableRow('5', 'Girona', '32', '56'),
      _tableRow('6', 'Real Sociedad', '32', '52'),
    ],
  ]));

  Widget _tableRow(String pos, String club, String played, String points, {bool header = false, bool highlight = false}) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 8),
    child: Row(children: [
      SizedBox(width: 36, child: Text(pos, style: TextStyle(color: header ? muted : highlight ? cyan : Colors.white, fontSize: header ? 8 : 10, fontWeight: FontWeight.w900))),
      Expanded(child: Text(club, style: TextStyle(color: header ? muted : Colors.white, fontSize: header ? 8 : 10, fontWeight: header || highlight ? FontWeight.w900 : FontWeight.w600))),
      SizedBox(width: 34, child: Text(played, textAlign: TextAlign.center, style: TextStyle(color: header ? muted : Colors.white, fontSize: header ? 8 : 10))),
      SizedBox(width: 34, child: Text(points, textAlign: TextAlign.right, style: TextStyle(color: header ? muted : highlight ? cyan : Colors.white, fontSize: header ? 8 : 10, fontWeight: FontWeight.w900))),
    ]),
  );

  Widget _fixtures() => Column(children: [
    _fixtureTile('APR 26  •  20:00', 'Real Madrid', 'Barcelona', 'EL CLÁSICO'),
    const SizedBox(height: 8),
    _fixtureTile('MAY 03  •  18:30', 'Valencia', 'Real Madrid', 'AWAY'),
    const SizedBox(height: 8),
    _fixtureTile('MAY 10  •  20:00', 'Real Madrid', 'Sevilla', 'HOME'),
  ]);

  Widget _fixtureTile(String date, String home, String away, String tag) => _panel(Row(children: [
    SizedBox(width: 83, child: Text(date, style: const TextStyle(color: muted, fontSize: 10, fontWeight: FontWeight.w800))),
    Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [Text(home, style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 11)), const Text('vs', style: TextStyle(color: cyan, fontSize: 9)), Text(away, style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 11))])),
    _pill(tag, cyan),
  ]));

  Widget _transfers() => Column(children: [
    _panel(Row(children: [const Expanded(child: Text('Transfer budget', style: TextStyle(color: muted))), Text('€${(transferBudget / 1000000).toStringAsFixed(1)}M', style: const TextStyle(color: gold, fontWeight: FontWeight.w900))])),
    const SizedBox(height: 8),
    _transferCard('Alphonso Davies', 'LB  •  Bayern Munich', 55000000,
        const ClubPlayer('Alphonso Davies', 'DAVIES', 'LB', 88, 95, 66, 81, 100)),
    const SizedBox(height: 8),
    _transferCard('Florian Wirtz', 'CAM  •  Leverkusen', 92000000,
        const ClubPlayer('Florian Wirtz', 'WIRTZ', 'CAM', 91, 82, 85, 91, 100)),
    const SizedBox(height: 8),
    _transferCard('Lautaro Martínez', 'ST  •  Inter', 78000000,
        const ClubPlayer('Lautaro Martínez', 'LAUTARO', 'ST', 90, 84, 91, 78, 100)),
  ]);

  Widget _transferCard(String name, String details, int fee, ClubPlayer player) => _panel(Row(children: [
    _avatar(name.substring(0, 2).toUpperCase(), cyan, 44), const SizedBox(width: 10),
    Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [Text(name, style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 12)), Text(details, style: const TextStyle(color: muted, fontSize: 10))])),
    Column(crossAxisAlignment: CrossAxisAlignment.end, children: [Text('${player.rating} OVR', style: const TextStyle(color: cyan, fontWeight: FontWeight.w900, fontSize: 10)), Text('€${(fee / 1000000).toStringAsFixed(0)}M', style: const TextStyle(color: gold, fontWeight: FontWeight.w900, fontSize: 11))]),
    const SizedBox(width: 8),
    _smallButton(scoutedPlayer == name ? 'SIGN' : 'SCOUT', () {
      if (scoutedPlayer != name) {
        setState(() => scoutedPlayer = name);
        message('Scouting report: $name is rated ${player.rating}');
      } else {
        _signTransfer(name, fee, player);
      }
    }),
  ]));

  void _signTransfer(String name, int fee, ClubPlayer player) {
    if (transferBudget < fee) {
      message('The transfer budget cannot cover $name');
      return;
    }
    setState(() {
      transferBudget -= fee;
      bench.add(player);
      collection.add(player);
      scoutedPlayer = null;
    });
    message('$name signed and added to the reserves');
  }

  String _formatNumber(int value) => value.toString().replaceAllMapped(
      RegExp(r'\B(?=(\d{3})+(?!\d))'), (match) => ',');

  Widget _careerAction(IconData icon, String text, VoidCallback onTap) => InkWell(
    onTap: onTap,
    borderRadius: BorderRadius.circular(15),
    child: Container(height: 72, decoration: _boxDecoration(radius: 15), child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [Icon(icon, color: cyan, size: 22), const SizedBox(height: 5), Text(text, style: const TextStyle(fontSize: 10, fontWeight: FontWeight.w900))])),
  );

  Widget _tournaments() => _page(
    eyebrow: 'COMPETE FOR GLORY',
    title: 'Tournaments',
    children: [
      _tournamentTile(Icons.emoji_events, 'UEFA Champions League', 'ROUND OF 16  •  OVR 85+', cyan, 'ENTER'),
      const SizedBox(height: 9),
      _tournamentTile(Icons.shield, 'Copa del Rey', 'QUARTER-FINAL  •  OVR 80+', gold, 'ENTER'),
      const SizedBox(height: 9),
      _tournamentTile(Icons.sports_soccer, 'Premier League', 'FEATURED CUP  •  OVR 82+', const Color(0xFFC4A0FF), 'ENTER'),
      const SizedBox(height: 9),
      _tournamentTile(Icons.public, 'Club World Cup', 'GROUP STAGE  •  OVR 84+', const Color(0xFFFF9C55), 'ENTER'),
      const SizedBox(height: 9),
      _tournamentTile(Icons.sports, 'Penalty Shootout', '5 SHOTS  •  ALL TEAMS', muted, 'PLAY'),
      const SizedBox(height: 12),
      Container(height: 130, decoration: BoxDecoration(borderRadius: BorderRadius.circular(18), border: Border.all(color: line), gradient: const LinearGradient(colors: [Color(0xFF0D2943), Color(0xFF081322)])), child: const Stack(children: [Positioned(right: 14, bottom: 4, child: Icon(Icons.emoji_events, size: 112, color: Color(0xFF91A9BE))), Positioned(left: 15, top: 20, child: Text('ONE CLUB.\nONE DREAM.', style: TextStyle(fontSize: 18, height: 1.05, fontWeight: FontWeight.w900))), Positioned(left: 15, bottom: 16, child: Text('THE ROAD TO THE FINAL', style: TextStyle(color: cyan, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 1.2)))])),
    ],
  );

  Widget _tournamentTile(IconData icon, String title, String details, Color color, String action) => InkWell(
    onTap: () { setState(() => enteredTournament = title); go('prematch'); },
    borderRadius: BorderRadius.circular(15),
    child: Container(padding: const EdgeInsets.all(12), decoration: _boxDecoration(radius: 15), child: Row(children: [
      _iconTile(icon, color, 44), const SizedBox(width: 11),
      Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [Text(title, style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 12)), const SizedBox(height: 4), Text(details, style: const TextStyle(color: muted, fontSize: 10, fontWeight: FontWeight.w700))])),
      _smallButton(enteredTournament == title ? 'CONTINUE' : action, () { setState(() => enteredTournament = title); go('prematch'); }),
    ])),
  );

  Widget _store() => _page(
    eyebrow: 'BUILD YOUR ULTIMATE TEAM  •  ${collection.length} NEW CARDS',
    title: 'Store',
    children: [
      _segmented(['PLAYERS', 'PACKS', 'COINS', 'SPECIAL'], storeTab, (i) => setState(() => storeTab = i)),
      const SizedBox(height: 12),
      if (storeTab == 0) _playerMarket()
      else if (storeTab == 1) _packs()
      else if (storeTab == 2) _coinPacks()
      else _specialOffers(),
    ],
  );

  Widget _playerMarket() => Column(children: [
    Row(children: [
      Expanded(child: _marketCard('MBAPPÉ', '91', 'ST  •  FRA', '2,500,000', 2500000, cyan,
          ClubPlayer('Kylian Mbappé', 'MBAPPÉ', 'ST', 91, 97, 96, 82, 100))),
      const SizedBox(width: 8), Expanded(child: _marketCard('SALAH', '89', 'RW  •  EGY', '1,800,000', 1800000, const Color(0xFFEF6C6C),
          ClubPlayer('Mohamed Salah', 'SALAH', 'RW', 89, 91, 89, 84, 100))),
    ]),
    const SizedBox(height: 8),
    Row(children: [
      Expanded(child: _marketCard('BELLINGHAM', '91', 'CM  •  ENG', '1,200,000', 1200000, gold,
          ClubPlayer('Jude Bellingham', 'BELLINGHAM', 'CM', 91, 80, 86, 88, 100))),
      const SizedBox(width: 8), Expanded(child: _marketCard('RÜDIGER', '87', 'CB  •  GER', '1,000,000', 1000000, const Color(0xFFAB8EFF),
          ClubPlayer('Antonio Rüdiger', 'RÜDIGER', 'CB', 87, 78, 54, 76, 95))),
    ]),
    const SizedBox(height: 12),
    _packBanner(),
  ]);

  Widget _marketCard(String name, String rating, String meta, String price, int cost, Color tint, ClubPlayer player) => InkWell(
    onTap: () => _buyPlayer(name, cost, player),
    borderRadius: BorderRadius.circular(15),
    child: Container(height: 185, padding: const EdgeInsets.all(10), decoration: BoxDecoration(borderRadius: BorderRadius.circular(15), border: Border.all(color: line), gradient: LinearGradient(begin: Alignment.topLeft, end: Alignment.bottomRight, colors: [Color.alphaBlend(tint.withValues(alpha: .20), const Color(0xFF0D2942)), const Color(0xFF071522)])), child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [Text(rating, style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900)), const Icon(Icons.star, color: gold, size: 15)]),
      Expanded(child: Center(child: _playerSilhouette(size: 73, color: tint))),
      Text(name, maxLines: 1, overflow: TextOverflow.ellipsis, style: const TextStyle(fontSize: 10, fontWeight: FontWeight.w900)),
      Text(meta, style: const TextStyle(color: muted, fontSize: 8)),
      const SizedBox(height: 5),
      Row(children: [const Icon(Icons.monetization_on, size: 13, color: gold), const SizedBox(width: 4), Text(price, style: const TextStyle(color: gold, fontWeight: FontWeight.w900, fontSize: 9))]),
    ])),
  );

  Widget _packBanner() => Container(padding: const EdgeInsets.all(14), decoration: BoxDecoration(borderRadius: BorderRadius.circular(16), border: Border.all(color: const Color(0xFF703BD5)), gradient: const LinearGradient(colors: [Color(0xFF562D9E), Color(0xFF201343), Color(0xFF10182D)])), child: Row(children: [const Icon(Icons.style, color: Color(0xFFE2CBFF), size: 50), const SizedBox(width: 12), const Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [Text('LEGENDARY PACK', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 12)), SizedBox(height: 4), Text('5 players  •  84+ OVR', style: TextStyle(color: Color(0xFFD0C2E9), fontSize: 10)), SizedBox(height: 5), Text('◆  2,500', style: TextStyle(color: gold, fontWeight: FontWeight.w900))])), _smallButton('OPEN', () => _purchasePack(2500, title: 'Legendary pack'))]));

  Widget _packs() => Column(children: [
    _packBanner(), const SizedBox(height: 9),
    _packOffer('GOLD PLAYER PACK', '3 players  •  80+ OVR', '750', 750, Icons.style),
    const SizedBox(height: 8), _packOffer('ELITE PACK', '3 players  •  86+ OVR', '1,500', 1500, Icons.auto_awesome),
    const SizedBox(height: 8), _packOffer('LEGENDARY PACK', '5 players  •  84+ OVR', '2,500', 2500, Icons.workspace_premium),
  ]);

  Widget _packOffer(String title, String desc, String costLabel, int cost, IconData icon) => _panel(Row(children: [_iconTile(icon, gold, 46), const SizedBox(width: 10), Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [Text(title, style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 11)), Text(desc, style: const TextStyle(color: muted, fontSize: 10))])), _smallButton('◆ $costLabel', () => _purchasePack(cost, title: title))]));

  Widget _coinPacks() => Column(children: [
    _walletCard(Icons.monetization_on, '250,000', '€2.99', gold), const SizedBox(height: 8),
    _walletCard(Icons.monetization_on, '1,000,000', '€9.99', gold), const SizedBox(height: 8),
    _walletCard(Icons.monetization_on, '3,000,000', '€24.99', gold),
  ]);

  Widget _walletCard(IconData icon, String amount, String price, Color color) => _panel(Row(children: [_iconTile(icon, color, 46), const SizedBox(width: 10), Expanded(child: Text(amount, style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 16))), _smallButton(price, () => message('Real-money checkout needs the store billing connection'))]));

  Widget _specialOffers() => Column(children: [
    _packBanner(), const SizedBox(height: 9),
    _panel(const Column(crossAxisAlignment: CrossAxisAlignment.start, children: [Text('WEEKLY ELITE OFFER', style: TextStyle(color: cyan, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: 1.2)), SizedBox(height: 6), Text('Limited time: Elite player + 1,000 gems', style: TextStyle(fontWeight: FontWeight.w900)), SizedBox(height: 5), Text('Offer refreshes in 18:42:07', style: TextStyle(color: muted, fontSize: 10))])),
    const SizedBox(height: 10), _actionButton('CLAIM OFFER · 1,000 ◆', Icons.redeem, () => _purchasePack(1000, title: 'Weekly elite offer')),
  ]);

  Widget _preMatch() => _page(
    eyebrow: enteredTournament == null
        ? 'LA LIGA  •  MATCHDAY 32'
        : '${enteredTournament!.toUpperCase()}  •  ROUND OF 16',
    title: enteredTournament == null ? 'El Clásico' : 'Tournament Match',
    children: [
      Container(height: 258, decoration: BoxDecoration(borderRadius: BorderRadius.circular(20), border: Border.all(color: line), gradient: const LinearGradient(begin: Alignment.topCenter, end: Alignment.bottomCenter, colors: [Color(0xFF174B70), Color(0xFF0A223A), Color(0xFF061321)])), child: Stack(children: [
        const Positioned.fill(child: CustomPaint(painter: StadiumGlowPainter())),
        Positioned(
          left: 0,
          bottom: 0,
          child: Image.asset(
            'assets/images/fictional_player.webp',
            width: 148,
            height: 220,
            fit: BoxFit.contain,
            alignment: Alignment.bottomCenter,
          ),
        ),
        Positioned(
          right: 0,
          bottom: 0,
          child: Image.asset(
            'assets/images/opponent_player.webp',
            width: 148,
            height: 220,
            fit: BoxFit.contain,
            alignment: Alignment.bottomCenter,
          ),
        ),
        const Center(child: Text('VS', style: TextStyle(fontSize: 36, fontWeight: FontWeight.w900, color: Colors.white, shadows: [Shadow(color: cyan, blurRadius: 22)]))),
        Positioned(left: 16, bottom: 14, child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [_crest('RMA', gold), const SizedBox(height: 5), const Text('REAL MADRID', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 11))])),
        Positioned(right: 16, bottom: 14, child: Column(crossAxisAlignment: CrossAxisAlignment.end, children: [_crest('FCB', const Color(0xFFFF667A)), const SizedBox(height: 5), const Text('BARCELONA', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 11))])),
      ])),
      const SizedBox(height: 10),
      Row(children: [Expanded(child: _pill(formation, cyan)), const SizedBox(width: 8), Expanded(child: _pill('24°C  •  CLEAR', cyan)), const SizedBox(width: 8), Expanded(child: _pill('62,000 FANS', cyan))]),
      const SizedBox(height: 9),
      _panel(Row(children: [const Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [Text('SANTIAGO BERNABÉU', style: TextStyle(fontWeight: FontWeight.w900, fontSize: 11)), SizedBox(height: 4), Text('Madrid, Spain  •  Referee: J. Hernández', style: TextStyle(color: muted, fontSize: 9))])), _crest('RMA', gold)])),
      const SizedBox(height: 12),
      Center(child: _actionButton('PLAY MATCH', Icons.play_arrow_rounded, () => go('hud'))),
    ],
  );

  Widget _matchHud() => LayoutBuilder(builder: (context, constraints) => Stack(children: [
    Positioned.fill(child: Container(decoration: const BoxDecoration(gradient: LinearGradient(begin: Alignment.topCenter, end: Alignment.bottomCenter, colors: [Color(0xFF287346), Color(0xFF0E592F), Color(0xFF145E34)])), child: const CustomPaint(painter: PitchPainter()))),
    Positioned.fill(child: IgnorePointer(child: CustomPaint(painter: MatchPlayersPainter(activePlayer: controlledPlayer, ballPosition: ballPosition)))),
    Positioned(left: 12, top: 12, child: _scoreboard()),
    Positioned(right: 12, top: 12, child: _iconButton(Icons.pause_rounded, () => go('pause'))),
    Positioned(left: 14, bottom: 22, child: _joystick()),
    Positioned(right: 16, bottom: 22, child: _matchControls()),
    Positioned(left: constraints.maxWidth * .37, right: constraints.maxWidth * .37, bottom: constraints.maxWidth < 500 ? 164 : 22, child: _miniMap()),
    if (matchMessage.isNotEmpty) Positioned(top: 76, left: 0, right: 0, child: Center(child: _pill(matchMessage, cyan))),
  ]));

  Widget _scoreboard() => Container(padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8), decoration: BoxDecoration(color: const Color(0xE8071727), borderRadius: BorderRadius.circular(12), border: Border.all(color: line)), child: Row(mainAxisSize: MainAxisSize.min, children: [const Text('RMA', style: TextStyle(fontSize: 10, fontWeight: FontWeight.w900)), const SizedBox(width: 7), Text('$homeGoals  -  $awayGoals', style: const TextStyle(color: cyan, fontSize: 13, fontWeight: FontWeight.w900)), const SizedBox(width: 7), const Text('FCB', style: TextStyle(fontSize: 10, fontWeight: FontWeight.w900)), const SizedBox(width: 9), Text('$matchMinute:${matchSecond.toString().padLeft(2, '0')}', style: const TextStyle(color: muted, fontSize: 10))]));

  Widget _joystick() => GestureDetector(
    onPanUpdate: _movePlayer,
    onTap: () => _movePlayerBy(const Offset(0, -22)),
    child: Container(width: 98, height: 98, decoration: BoxDecoration(shape: BoxShape.circle, color: const Color(0x663A6170), border: Border.all(color: Colors.white54)), child: Center(child: Container(width: 42, height: 42, decoration: BoxDecoration(shape: BoxShape.circle, color: const Color(0xFFE4EDF3), border: Border.all(color: Colors.white, width: 2), boxShadow: const [BoxShadow(color: Colors.black45, blurRadius: 9)])))),
  );

  Widget _matchControls() => Column(mainAxisSize: MainAxisSize.min, children: [
    _roundControl('SHOOT', 66, _shoot),
    const SizedBox(height: 8),
    Row(children: [
      _roundControl('PASS', 54, _autoPass),
      const SizedBox(width: 8),
      _roundControl('SPRINT', 54, _sprint),
    ]),
  ]);

  Widget _roundControl(String text, double size, VoidCallback onTap) => InkWell(onTap: onTap, borderRadius: BorderRadius.circular(size), child: Container(width: size, height: size, decoration: BoxDecoration(shape: BoxShape.circle, color: const Color(0xB90A2030), border: Border.all(color: cyan, width: 1.5), boxShadow: [BoxShadow(color: cyan.withValues(alpha: .24), blurRadius: 12)]), child: Center(child: Text(text, style: const TextStyle(fontSize: 10, fontWeight: FontWeight.w900)))));

  Widget _miniMap() => Container(height: 62, decoration: BoxDecoration(color: const Color(0xC807291E), border: Border.all(color: Colors.white70), borderRadius: BorderRadius.circular(8)), child: const CustomPaint(painter: MiniMapPainter(), child: SizedBox.expand()));

  Widget _pauseMenu() => Stack(children: [
    Positioned.fill(child: Container(color: const Color(0x9D020914), child: const CustomPaint(painter: StadiumGlowPainter()))),
    Center(child: SingleChildScrollView(padding: const EdgeInsets.all(24), child: Container(width: 340, padding: const EdgeInsets.all(18), decoration: BoxDecoration(color: const Color(0xF2071728), borderRadius: BorderRadius.circular(20), border: Border.all(color: cyan)), child: Column(mainAxisSize: MainAxisSize.min, children: [
      const Text('FC 2026', style: TextStyle(color: cyan, fontSize: 20, fontWeight: FontWeight.w900, fontStyle: FontStyle.italic)),
      const SizedBox(height: 10),
      Row(mainAxisAlignment: MainAxisAlignment.spaceAround, children: [_crest('RMA', gold), Text('$homeGoals  -  $awayGoals', style: const TextStyle(fontSize: 25, fontWeight: FontWeight.w900)), _crest('FCB', const Color(0xFFFF667A))]),
      const SizedBox(height: 4), Text('$matchMinute:${matchSecond.toString().padLeft(2, '0')}', style: const TextStyle(color: muted, fontSize: 11)),
      const SizedBox(height: 15),
      _menuButton('RESUME', () => go(previousScreen)),
      const SizedBox(height: 7), _menuButton('FORMATION', _editFormation),
      const SizedBox(height: 7), _menuButton('SETTINGS', _showSettings),
      const SizedBox(height: 7), _menuButton('QUIT MATCH', () { setState(() { matchMinute = 0; matchSecond = 0; homeGoals = 0; awayGoals = 0; }); go('home'); }),
    ])))),
  ]);

  Widget _menuButton(String label, VoidCallback onTap) => SizedBox(width: double.infinity, height: 46, child: OutlinedButton(onPressed: onTap, style: OutlinedButton.styleFrom(side: const BorderSide(color: cyan), foregroundColor: Colors.white, backgroundColor: const Color(0x66102134), shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10))), child: Text(label, style: const TextStyle(fontWeight: FontWeight.w900, letterSpacing: .5))));

  Widget _sectionTitle(String title, String action, VoidCallback onTap) => Row(children: [Expanded(child: Text(title, style: const TextStyle(fontSize: 11, fontWeight: FontWeight.w900, letterSpacing: .8))), TextButton(onPressed: onTap, child: Text(action, style: const TextStyle(color: cyan, fontSize: 10, fontWeight: FontWeight.w900)))]);

  Widget _panel(Widget child) => Container(padding: const EdgeInsets.all(13), decoration: _boxDecoration(), child: child);

  BoxDecoration _boxDecoration({double radius = 16}) => BoxDecoration(color: panel, borderRadius: BorderRadius.circular(radius), border: Border.all(color: line));

  Widget _pill(String label, Color color) => Container(padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 6), decoration: BoxDecoration(color: color.withValues(alpha: .12), border: Border.all(color: color.withValues(alpha: .5)), borderRadius: BorderRadius.circular(20)), child: Text(label, maxLines: 1, overflow: TextOverflow.ellipsis, style: TextStyle(color: color, fontSize: 10, fontWeight: FontWeight.w900, letterSpacing: .7)));

  Widget _actionButton(String label, IconData icon, VoidCallback onTap) => SizedBox(height: 46, child: FilledButton.icon(onPressed: onTap, icon: Icon(icon, size: 18), label: Text(label, style: const TextStyle(fontSize: 11, fontWeight: FontWeight.w900, letterSpacing: .7)), style: FilledButton.styleFrom(backgroundColor: cyan, foregroundColor: ink, padding: const EdgeInsets.symmetric(horizontal: 17), shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)), minimumSize: const Size(44, 44))));

  Widget _smallButton(String label, VoidCallback onTap) => SizedBox(height: 44, child: OutlinedButton(onPressed: onTap, style: OutlinedButton.styleFrom(foregroundColor: cyan, side: const BorderSide(color: line), padding: const EdgeInsets.symmetric(horizontal: 10), shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(9))), child: Text(label, style: const TextStyle(fontSize: 10, fontWeight: FontWeight.w900))));

  Widget _outlineButton(String label, IconData icon, VoidCallback onTap) => SizedBox(height: 46, child: OutlinedButton.icon(onPressed: onTap, icon: Icon(icon, size: 15), label: Text(label, maxLines: 1, overflow: TextOverflow.ellipsis, style: const TextStyle(fontSize: 10, fontWeight: FontWeight.w900)), style: OutlinedButton.styleFrom(foregroundColor: Colors.white, side: const BorderSide(color: line), padding: const EdgeInsets.symmetric(horizontal: 7), shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(11)))));

  Widget _segmented(List<String> labels, int selected, ValueChanged<int> onSelect) => Container(padding: const EdgeInsets.all(4), decoration: BoxDecoration(color: const Color(0xE8071726), border: Border.all(color: line), borderRadius: BorderRadius.circular(12)), child: Row(children: List.generate(labels.length, (i) => Expanded(child: InkWell(onTap: () => onSelect(i), borderRadius: BorderRadius.circular(9), child: Container(height: 36, decoration: BoxDecoration(color: selected == i ? const Color(0xFF0D597A) : Colors.transparent, border: selected == i ? Border.all(color: cyan) : null, borderRadius: BorderRadius.circular(9)), child: Center(child: Text(labels[i], maxLines: 1, overflow: TextOverflow.ellipsis, style: TextStyle(color: selected == i ? Colors.white : muted, fontSize: 10, fontWeight: FontWeight.w900))))))));

  Widget _iconTile(IconData icon, Color tint, double size) => Container(width: size, height: size, decoration: BoxDecoration(color: tint.withValues(alpha: .13), borderRadius: BorderRadius.circular(12), border: Border.all(color: tint.withValues(alpha: .34))), child: Icon(icon, size: size * .52, color: tint));

  Widget _avatar(String letters, Color color, double size) => Container(width: size, height: size, decoration: BoxDecoration(shape: BoxShape.circle, color: color.withValues(alpha: .13), border: Border.all(color: color.withValues(alpha: .65))), child: Center(child: Text(letters, style: TextStyle(color: color, fontWeight: FontWeight.w900, fontSize: size * .23))));

  Widget _crest(String letters, Color color) => Container(width: 42, height: 42, decoration: BoxDecoration(shape: BoxShape.circle, color: color.withValues(alpha: .15), border: Border.all(color: color, width: 1.5), boxShadow: [BoxShadow(color: color.withValues(alpha: .12), blurRadius: 10)]), child: Center(child: Text(letters, style: TextStyle(color: color, fontSize: 10, fontWeight: FontWeight.w900))));
}
