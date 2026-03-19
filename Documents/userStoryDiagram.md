# Felhasználói történetek diagram - Mini Transport Tycoon


## 1. Út építése üres mezőre
|        | Leírás |
|--------|--------|
| **GIVEN** | A térképen van egy üres mező <br> A játékos rendelkezik elegendő pénzzel <br> A mező nem város vagy ipari terület |
| **WHEN**  | A játékos utat épít a mezőre |
| **THEN**  | Az út megjelenik a térképen <br> A játékos vagyona csökken az út költségével <br> Az építés engedélyezett |

## 2. Útépítés tiltott területre
|        | Leírás |
|--------|--------|
| **GIVEN** | A mező város vagy ipari létesítmény része |
| **WHEN**  | A játékos utat próbál építeni |
| **THEN**  | Az építés meghiúsul <br> A játékos vagyona nem változik <br> Hibaüzenet jelenik meg |

## 3. Jármű vásárlása
|        | Leírás |
|--------|--------|
| **GIVEN** | A játékos rendelkezik elegendő pénzzel <br> Van elérhető járműtípus |
| **WHEN**  | Járművet vásárol |
| **THEN**  | Az új jármű létrejön <br> A jármű a játékos tulajdonába kerül <br> A játékos vagyona csökken a jármű árával |

## 4. Megállók
|        | Leírás |
|--------|--------|
| **GIVEN** | Van útvonal és az mellett szabad hely |
| **WHEN**  | A játékos megállót helyez a szabad helyre |
| **THEN**  | A megálló megépül és elfoglalja a kijelölt helyet |

## 5. Útvonalak
|        | Leírás |
|--------|--------|
| **GIVEN** | Van útvonal, legalább 2 megálló, és legalább egy jármű |
| **WHEN**  | A járművekhez körút jellegű útvonalat rendel a játékos |
| **THEN**  | A járművek automatikusan ismétlik az útvonalukat |

## 6. Áru vagy utas szállítása
|        | Leírás |
|--------|--------|
| **GIVEN** | A jármű megállóhoz ér <br> A célmegálló fogadni tudja |
| **WHEN**  | Van elérhető áru vagy utas <br> A jármű megérkezik |
| **THEN**  | A jármű rakodik <br> Az áru/utas leszállításra kerül <br> A játékos bevételt kap |

## 7. Bevétel és költségek kezelése
|        | Leírás |
|--------|--------|
| **GIVEN** | A jármű sikeresen szállít |
| **WHEN**  | A szállítás befejeződik |
| **THEN**  | A játékos vagyona nő <br> Fenntartási költség levonásra kerül |

## 8. Csőd állapot
|        | Leírás |
|--------|--------|
| **GIVEN** | A játékos vagyona 0 vagy negatív |
| **WHEN**  | Gazdasági frissítés történik |
| **THEN**  | A játék véget ér <br> „Csőd” állapot jelenik meg <br> A játékos nem végezhet további műveleteket |


## 9. Gazdaság - Kezdőtőke
|        | Leírás |
|--------|--------|
| **GIVEN** | A játékos új játékot kezd <br> A kezdőtőke meghatározott érték |
| **WHEN**  | A játék elindul |
| **THEN**  | A játékos vagyona a kezdőtőkével lesz megegyező |

## 10. Gazdaság - Bevétel szerzése
|        | Leírás |
|--------|--------|
| **GIVEN** | A játékos sikeresen szállít árut vagy utast |
| **WHEN**  | Az áru vagy utas leszállításra kerül |
| **THEN**  | A játékos vagyona nő a bevétel összegével |

## 11. Gazdaság - Költségek
|        | Leírás |
|--------|--------|
| **GIVEN** | A játékos utat épít, járművet vásárol vagy fenntartási folyamatot végez |
| **WHEN**  | Út építése, jármű vásárlása vagy fenntartása történik |
| **THEN**  | A játékos vagyona csökken a költségek összegével |