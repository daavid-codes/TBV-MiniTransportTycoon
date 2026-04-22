# Mini Transport Tycoon

## A csapat:

- Brekovszki Márk
- Tóth Dávid
- Vida Zsombor Hunor

## Project rövid leírása

A projekt célja egy egyszerűsített, 2D felülnézetes közlekedési‑gazdasági szimulátor fejlesztése, amelyben a játékos városok és ipari létesítmények között szervez közúti áruszállítást és utasszállítást. A játék rácsalapú térképen zajlik, ahol a játékos utakat építhet, megállókat helyezhet el, járműveket vásárolhat és körút jellegű útvonalakat definiálhat. A járművek megállók között közlekednek, adott útvonalat követnek és rakodnak, miközben figyelembe veszik az úthálózat forgalmi szabályait.

A gazdasági rendszer bevételeket biztosít a sikeresen leszállított áruk és utasok után, miközben költséget jelent az útépítés, a járművek fenntartása és beszerzése. A játék legalább ötféle árutípust kezel, az ipari létesítmények termelnek és fogyasztanak, és legalább két ipari láncolat működik. A városok és üzemek fix pozícióval rendelkeznek, és több mezőt foglalnak el a térképen.

[Részletes leírás](https://canvas.elte.hu/courses/62433/files/4226766/download)

## Választott részfeladatok

- ### Erdők (0.5)
    Az üres mezőkön fák jelenhetnek meg. Egy mezőn 1–4 fa lehet. Idővel egy mezőn a fák száma nőhet
    (pl. 1 → 4), továbbá ha egy mezőn 3 vagy 4 fa van, akkor a szomszédos üres mezőkön is idővel egy új
    fa jelenhet meg. A játék kezdeti állapotában is legyenek erdős mezők.
    Erdős mezőkre is építhető út, de az magasabb költséggel járjon (irtás).

- ### Folyók és tavak (0.5)
    A térképen legyenek víz mezők is, amelyek kezdeti elhelyezésekor (akár előre rögzített a térkép, akár
    véletlen generált) tavakat és folyókat alkossanak. Legyen legalább 3 különböző híd típus a játékban,
    amelyek különböző költséggel, maximális áthidalási távolsággal és sebességkorlátozással
    rendelkeznek.

- ### Garázs (0.5)
    A játékos építhessen 1 vagy több garázst is, amelyet szintén az úthálózathoz szükséges kapcsolnia. A
    járműveket vásárolni és karbantartani a garázsban lehessen. A járművek karbantartásra megadott
    időközönként a (legközelebbi) garázsba automatikusan térjenek vissza, vagy a metenrendjükbe is
    legyen ütemezhető ez. Minél idősebb egy jármű, annál gyakoribb karbantartás legyen szükséges.
    Legyen lehetőség eladni a túlkoros járműveket, hogy ne okozzanak további üzemeltetési nehézséget
    és költséget.

- ### Minimap (0.5)
    A játékpálya legyen nagyobb a megjelenítettnél, a navigáláshoz azt X és Y dimenzióban lehessen
    görgetni. A könnyebb tájékozódáshoz a játékhoz tartozzon navigálható minimap.

- ### Perzisztencia (0.5)
    Egy adott játékállást legyen lehetőség elmenteni, majd később egy kiválasztott játékállást
    visszatölteni és a játékot folytatni. A visszatöltés után a mentéskor éppen mozgásban lévő járművek
    onnan folytatják az útjukat, ahol éppen tartózkodtak. Több mentés kezelésére is legyen lehetőség.

- ### Folyamatos mozgás (0.5)
    A járművek mozgása a játékpálya mezői között ne ugrásszerű, hanem folyamatos legyen. A járművek
    ettől még logikailag lehetnek mindig kizárólagosan egyetlen mezőn, de mozgatásuk a mezők között
    animált és folyamatos legyen.


## Technology Stack

<p align="center">
  <a href="https://skillicons.dev">
    <img src="https://skillicons.dev/icons?i=unity,cs" />
  </a>
</p>

- Unity
- C#

### CI Coverage (GitHub Actions)

Coverage is generated in CI by game-ci/unity-test-runner with:

- enableCodeCoverage: true
- coverageOptions: generateAdditionalMetrics;generateHtmlReport

The workflow uploads a `code-coverage` artifact containing the `CodeCoverage` folder.

### Run Coverage Locally (macOS)

You can generate EditMode test coverage in batch mode with:

```bash
"/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity" \
    -batchmode -nographics -quit \
    -projectPath "Mini Transport Tycoon" \
    -runTests -testPlatform editmode \
    -enableCodeCoverage \
    -coverageOptions "generateAdditionalMetrics;generateHtmlReport" \
    -testResults "TestResults/EditMode-results.xml" \
    -logFile "TestResults/editmode-coverage.log"
```

After the run, open the generated HTML report from the `CodeCoverage` output directory.
In CI this directory is uploaded as the `code-coverage` artifact.

## Hivatkozások

- [Használati eset diagram]()
- [Felhasználói-felület terv (wireframe / mockups)]()
- [Felhasználói történetek (a használati esetek alapján, azok kifejtése)]()
- [Osztálydiagram]()