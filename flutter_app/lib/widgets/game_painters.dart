import 'package:flutter/material.dart';

import '../theme/game_theme.dart';

class StadiumGlowPainter extends CustomPainter {
  const StadiumGlowPainter();

  @override
  void paint(Canvas canvas, Size size) {
    final glow = Paint()
      ..shader = RadialGradient(
        colors: [cyan.withValues(alpha: .19), Colors.transparent],
      ).createShader(Rect.fromCircle(
        center: Offset(size.width * .5, size.height * .06),
        radius: size.width * .75,
      ));
    canvas.drawRect(Offset.zero & size, glow);

    final light = Paint()..color = const Color(0xFFE9F8FF);
    for (var i = 0; i < 8; i++) {
      final point = Offset(size.width * (i / 7), size.height * .045);
      canvas.drawCircle(point, 1.7, light);
      canvas.drawCircle(
        point,
        size.width * .04,
        Paint()..color = cyan.withValues(alpha: .05),
      );
    }

    final vignette = Paint()
      ..shader = LinearGradient(
        begin: Alignment.topCenter,
        end: Alignment.bottomCenter,
        colors: [Colors.transparent, const Color(0x44040A13)],
      ).createShader(Offset.zero & size);
    canvas.drawRect(Offset.zero & size, vignette);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}

class PitchPainter extends CustomPainter {
  const PitchPainter();

  @override
  void paint(Canvas canvas, Size size) {
    final stripe = Paint()..color = const Color(0x18000000);
    for (var i = 0; i < 8; i += 2) {
      canvas.drawRect(
        Rect.fromLTWH(size.width * i / 8, 0, size.width / 8, size.height),
        stripe,
      );
    }

    final markings = Paint()
      ..color = const Color(0x99E5FFF2)
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1.1;
    canvas.drawRect(
        Rect.fromLTWH(7, 6, size.width - 14, size.height - 12), markings);
    canvas.drawLine(Offset(7, size.height / 2),
        Offset(size.width - 7, size.height / 2), markings);
    canvas.drawCircle(
        Offset(size.width / 2, size.height / 2), size.width * .15, markings);
    canvas.drawCircle(
        Offset(size.width / 2, size.height / 2),
        2,
        Paint()..color = Colors.white70);
    final boxWidth = size.width * .42;
    canvas.drawRect(
        Rect.fromLTWH((size.width - boxWidth) / 2, 6, boxWidth, size.height * .15),
        markings);
    canvas.drawRect(
        Rect.fromLTWH((size.width - boxWidth) / 2, size.height * .85 - 6,
            boxWidth, size.height * .15),
        markings);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}

class MatchPlayersPainter extends CustomPainter {
  final Offset activePlayer;
  final Offset ballPosition;

  const MatchPlayersPainter({
    required this.activePlayer,
    required this.ballPosition,
  });

  @override
  void paint(Canvas canvas, Size size) {
    const home = [
      Offset(.26, .34),
      Offset(.42, .28),
      Offset(.60, .36),
      Offset(.78, .45),
      Offset(.34, .53),
      Offset(.54, .52),
      Offset(.71, .62),
      Offset(.45, .73),
      Offset(.61, .68),
      Offset(.21, .58),
    ];
    const away = [
      Offset(.18, .19),
      Offset(.34, .21),
      Offset(.52, .18),
      Offset(.71, .21),
      Offset(.84, .25),
      Offset(.25, .39),
      Offset(.47, .40),
      Offset(.67, .43),
      Offset(.37, .51),
      Offset(.77, .57),
      Offset(.55, .61),
    ];

    final homeKit = Paint()..color = Colors.white;
    final awayKit = Paint()..color = const Color(0xFFFF755B);
    final activeRing = Paint()
      ..color = cyan
      ..style = PaintingStyle.stroke
      ..strokeWidth = 2;
    final shadow = Paint()..color = Colors.black26;

    for (var i = 0; i < home.length; i++) {
      final point = i == 5 ? activePlayer : _toPoint(home[i], size);
      canvas.drawOval(
        Rect.fromCenter(center: point.translate(0, 13), width: 17, height: 5),
        shadow,
      );
      canvas.drawCircle(point, 8, homeKit);
      if (i == 5) canvas.drawCircle(point, 15, activeRing);
    }

    for (final point in away) {
      canvas.drawOval(
        Rect.fromCenter(
            center: _toPoint(point, size).translate(0, 13),
            width: 17,
            height: 5),
        shadow,
      );
      canvas.drawCircle(_toPoint(point, size), 8, awayKit);
    }

    final ballCenter = _toPoint(ballPosition, size);
    canvas.drawCircle(ballCenter, 5, Paint()..color = Colors.white);
    canvas.drawCircle(
      ballCenter,
      5,
      Paint()
        ..color = const Color(0xFF101820)
        ..style = PaintingStyle.stroke
        ..strokeWidth = .8,
    );
  }

  Offset _toPoint(Offset normalized, Size size) =>
      Offset(normalized.dx * size.width, normalized.dy * size.height);

  @override
  bool shouldRepaint(covariant MatchPlayersPainter oldDelegate) =>
      activePlayer != oldDelegate.activePlayer ||
      ballPosition != oldDelegate.ballPosition;
}

class MiniMapPainter extends CustomPainter {
  const MiniMapPainter();

  @override
  void paint(Canvas canvas, Size size) {
    final markings = Paint()
      ..color = Colors.white54
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1;
    canvas.drawRect(Rect.fromLTWH(3, 3, size.width - 6, size.height - 6), markings);
    canvas.drawLine(Offset(size.width / 2, 3),
        Offset(size.width / 2, size.height - 3), markings);
    canvas.drawCircle(Offset(size.width / 2, size.height / 2), 9, markings);

    final home = Paint()..color = Colors.white;
    final away = Paint()..color = const Color(0xFFFF755B);
    for (var i = 0; i < 7; i++) {
      canvas.drawCircle(
          Offset(size.width * (.15 + (i % 3) * .16),
              size.height * (.22 + (i ~/ 3) * .2)),
          2.2,
          home);
      canvas.drawCircle(
          Offset(size.width * (.58 + (i % 3) * .12),
              size.height * (.18 + (i ~/ 3) * .21)),
          2.2,
          away);
    }
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}

class PlayerSilhouettePainter extends CustomPainter {
  final Color jerseyColor;
  final double opacity;

  const PlayerSilhouettePainter({required this.jerseyColor, this.opacity = .9});

  @override
  void paint(Canvas canvas, Size size) {
    final scale = size.shortestSide;
    final center = Offset(size.width / 2, size.height / 2);
    final jersey = Paint()..color = jerseyColor.withValues(alpha: opacity);
    final dark = Paint()..color = const Color(0xFF081421).withValues(alpha: opacity);
    final skin = Paint()..color = const Color(0xFFB77551).withValues(alpha: opacity);
    final glow = Paint()
      ..color = jerseyColor.withValues(alpha: opacity * .17)
      ..maskFilter = const MaskFilter.blur(BlurStyle.normal, 16);

    canvas.drawCircle(Offset(center.dx, size.height * .17), scale * .07, glow);
    canvas.drawCircle(Offset(center.dx, size.height * .17), scale * .055, skin);

    final torso = Path()
      ..moveTo(size.width * .40, size.height * .28)
      ..quadraticBezierTo(size.width * .50, size.height * .24,
          size.width * .60, size.height * .28)
      ..lineTo(size.width * .72, size.height * .55)
      ..lineTo(size.width * .61, size.height * .60)
      ..lineTo(size.width * .57, size.height * .48)
      ..lineTo(size.width * .58, size.height * .66)
      ..lineTo(size.width * .42, size.height * .66)
      ..lineTo(size.width * .42, size.height * .48)
      ..lineTo(size.width * .39, size.height * .60)
      ..lineTo(size.width * .28, size.height * .55)
      ..close();
    canvas.drawPath(torso, jersey);

    final armLeft = Path()
      ..moveTo(size.width * .32, size.height * .43)
      ..lineTo(size.width * .25, size.height * .59)
      ..lineTo(size.width * .30, size.height * .63)
      ..lineTo(size.width * .40, size.height * .50)
      ..close();
    final armRight = Path()
      ..moveTo(size.width * .68, size.height * .43)
      ..lineTo(size.width * .75, size.height * .59)
      ..lineTo(size.width * .70, size.height * .63)
      ..lineTo(size.width * .60, size.height * .50)
      ..close();
    canvas.drawPath(armLeft, skin);
    canvas.drawPath(armRight, skin);

    canvas.drawRect(
      Rect.fromLTRB(size.width * .43, size.height * .65,
          size.width * .57, size.height * .70),
      dark,
    );
    final leftLeg = Path()
      ..moveTo(size.width * .43, size.height * .69)
      ..lineTo(size.width * .50, size.height * .70)
      ..lineTo(size.width * .46, size.height * .91)
      ..lineTo(size.width * .37, size.height * .91)
      ..close();
    final rightLeg = Path()
      ..moveTo(size.width * .51, size.height * .70)
      ..lineTo(size.width * .58, size.height * .69)
      ..lineTo(size.width * .64, size.height * .91)
      ..lineTo(size.width * .55, size.height * .91)
      ..close();
    canvas.drawPath(leftLeg, dark);
    canvas.drawPath(rightLeg, dark);
    canvas.drawOval(
        Rect.fromLTWH(size.width * .34, size.height * .90, size.width * .16,
            size.height * .045),
        Paint()..color = Colors.white.withValues(alpha: opacity));
    canvas.drawOval(
        Rect.fromLTWH(size.width * .53, size.height * .90, size.width * .16,
            size.height * .045),
        Paint()..color = Colors.white.withValues(alpha: opacity));
  }

  @override
  bool shouldRepaint(covariant PlayerSilhouettePainter oldDelegate) =>
      jerseyColor != oldDelegate.jerseyColor || opacity != oldDelegate.opacity;
}

class JerseyPainter extends CustomPainter {
  final Color color;
  final bool stripes;

  const JerseyPainter({required this.color, this.stripes = false});

  @override
  void paint(Canvas canvas, Size size) {
    final shirt = Path()
      ..moveTo(size.width * .35, size.height * .13)
      ..lineTo(size.width * .43, size.height * .08)
      ..quadraticBezierTo(size.width * .50, size.height * .18,
          size.width * .57, size.height * .08)
      ..lineTo(size.width * .65, size.height * .13)
      ..lineTo(size.width * .92, size.height * .32)
      ..lineTo(size.width * .81, size.height * .48)
      ..lineTo(size.width * .70, size.height * .40)
      ..lineTo(size.width * .72, size.height * .91)
      ..lineTo(size.width * .28, size.height * .91)
      ..lineTo(size.width * .30, size.height * .40)
      ..lineTo(size.width * .19, size.height * .48)
      ..lineTo(size.width * .08, size.height * .32)
      ..close();
    canvas.drawShadow(shirt, Colors.black54, 12, true);
    canvas.drawPath(
      shirt,
      Paint()
        ..shader = LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [color, color.withValues(alpha: .72)],
        ).createShader(Offset.zero & size),
    );
    if (stripes) {
      final stripe = Paint()..color = Colors.white.withValues(alpha: .22);
      for (var i = 0; i < 4; i++) {
        final x = size.width * (.35 + i * .09);
        canvas.drawRect(
            Rect.fromLTWH(x, size.height * .22, size.width * .045,
                size.height * .64),
            stripe);
      }
    }
    final crest = Paint()..color = gold;
    canvas.drawCircle(Offset(size.width * .61, size.height * .33),
        size.shortestSide * .045, crest);
    final number = TextPainter(
      text: const TextSpan(
        text: '7',
        style: TextStyle(
            color: Colors.white, fontSize: 26, fontWeight: FontWeight.w900),
      ),
      textDirection: TextDirection.ltr,
    )..layout();
    number.paint(canvas, Offset(size.width * .5 - number.width / 2,
        size.height * .51 - number.height / 2));
  }

  @override
  bool shouldRepaint(covariant JerseyPainter oldDelegate) =>
      color != oldDelegate.color || stripes != oldDelegate.stripes;
}
