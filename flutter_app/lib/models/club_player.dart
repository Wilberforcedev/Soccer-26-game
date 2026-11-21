class ClubPlayer {
  final String name;
  final String shortName;
  final String role;
  final int rating;
  final int pace;
  final int shooting;
  final int passing;
  final int fitness;
  final int dribbling;
  final int defending;
  final int physical;
  final int age;
  final int heightCm;
  final String preferredFoot;

  const ClubPlayer(
    this.name,
    this.shortName,
    this.role,
    this.rating,
    this.pace,
    this.shooting,
    this.passing,
    this.fitness, {
    this.dribbling = 80,
    this.defending = 50,
    this.physical = 76,
    this.age = 26,
    this.heightCm = 180,
    this.preferredFoot = 'Right',
  });

  ClubPlayer copyWith({
    int? rating,
    int? pace,
    int? shooting,
    int? passing,
    int? fitness,
    int? dribbling,
    int? defending,
    int? physical,
  }) =>
      ClubPlayer(
        name,
        shortName,
        role,
        rating ?? this.rating,
        pace ?? this.pace,
        shooting ?? this.shooting,
        passing ?? this.passing,
        fitness ?? this.fitness,
        dribbling: dribbling ?? this.dribbling,
        defending: defending ?? this.defending,
        physical: physical ?? this.physical,
        age: age,
        heightCm: heightCm,
        preferredFoot: preferredFoot,
      );
}

const startingPlayers = <ClubPlayer>[
  ClubPlayer('Thibaut Courtois', 'COURTOIS', 'GK', 91, 62, 35, 78, 100,
      dribbling: 45, defending: 18, physical: 86, age: 33, heightCm: 200),
  ClubPlayer('Ferland Mendy', 'MENDY', 'LB', 86, 88, 58, 79, 97,
      dribbling: 81, defending: 84, physical: 82, age: 30, heightCm: 180),
  ClubPlayer('Antonio Rüdiger', 'RÜDIGER', 'CB', 88, 78, 54, 76, 94,
      dribbling: 63, defending: 89, physical: 90, age: 32, heightCm: 190),
  ClubPlayer('Éder Militão', 'MILITÃO', 'CB', 87, 84, 55, 78, 98,
      dribbling: 70, defending: 87, physical: 86, age: 27, heightCm: 186),
  ClubPlayer('Dani Carvajal', 'CARVAJAL', 'RB', 85, 77, 61, 82, 91,
      dribbling: 78, defending: 84, physical: 80, age: 33, heightCm: 173),
  ClubPlayer('Jude Bellingham', 'BELLINGHAM', 'CM', 91, 80, 86, 88, 100,
      dribbling: 91, defending: 78, physical: 86, age: 22, heightCm: 186),
  ClubPlayer('Federico Valverde', 'VALVERDE', 'CM', 89, 91, 82, 86, 96,
      dribbling: 86, defending: 80, physical: 88, age: 27, heightCm: 182),
  ClubPlayer('Aurélien Tchouaméni', 'TCHOUAMÉNI', 'CDM', 87, 72, 68, 82, 93,
      dribbling: 78, defending: 86, physical: 88, age: 25, heightCm: 188),
  ClubPlayer('Vinícius Júnior', 'VINI JR.', 'LW', 92, 97, 88, 84, 99,
      dribbling: 95, defending: 35, physical: 70, age: 25, heightCm: 176),
  ClubPlayer('Kylian Mbappé', 'MBAPPÉ', 'ST', 94, 97, 96, 82, 100,
      dribbling: 94, defending: 39, physical: 78, age: 26, heightCm: 178),
  ClubPlayer('Rodrygo', 'RODRYGO', 'RW', 89, 91, 86, 83, 95,
      dribbling: 91, defending: 42, physical: 68, age: 24, heightCm: 174),
];

const reservePlayers = <ClubPlayer>[
  ClubPlayer('Andriy Lunin', 'LUNIN', 'GK', 83, 72, 30, 71, 94,
      dribbling: 42, defending: 19, physical: 77, age: 26, heightCm: 191),
  ClubPlayer('Nacho Fernández', 'NACHO', 'CB', 82, 71, 48, 74, 92,
      dribbling: 65, defending: 83, physical: 78, age: 35, heightCm: 180),
  ClubPlayer('Lucas Vázquez', 'L. VÁZQUEZ', 'RB', 81, 79, 64, 78, 90,
      dribbling: 80, defending: 74, physical: 70, age: 34, heightCm: 173),
  ClubPlayer('Luka Modrić', 'MODRIĆ', 'CM', 87, 70, 74, 92, 88,
      dribbling: 90, defending: 65, physical: 63, age: 40, heightCm: 172),
  ClubPlayer('Brahim Díaz', 'BRAHIM', 'CAM', 84, 86, 77, 79, 96,
      dribbling: 88, defending: 40, physical: 66, age: 26, heightCm: 170),
];
