import 'package:flutter/material.dart';

const ink = Color(0xFF04101E);
const panel = Color(0xE9081B2D);
const cyan = Color(0xFF16D9FF);
const muted = Color(0xFF91A9BE);
const line = Color(0xFF174663);
const gold = Color(0xFFFFC94A);

ThemeData buildGameTheme() => ThemeData(
      brightness: Brightness.dark,
      useMaterial3: true,
      scaffoldBackgroundColor: ink,
      colorScheme: const ColorScheme.dark(
          primary: cyan, secondary: gold, surface: panel),
      textTheme: ThemeData.dark().textTheme.apply(
            bodyColor: Colors.white,
            displayColor: Colors.white,
          ),
    );
