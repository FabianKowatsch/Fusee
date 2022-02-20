# MuVista - 3D Pointcloud and Pano Viewer

## Anleitung zur Steuerung von MuVista

### Panorama-Ansicht:

Befindet man sich in der Panoramaansicht, so kann man sich durch klicken und ziehen der Maus umschauen. Passend dazu kann man mit dem Mausrad den Zoom benutzen. Alternativ geht das auch mit UI-Elementen, dem Plus und dem Minus in der rechten unteren Ecke des Fensters. Durch die beiden Reglern kann man zum einen die Punktgröße der Punktwolke anpassen und die Transparenz des Panoramabilds.
Durch klicken auf die grünen Pfeilen kann man sich von einem Panoramabild zum nächsten bewegen. Während den animierten Übergängen kann man sich weiterhin umschauen und kann durch die kurzzeitig eingeblendete Punktwolken die Bewegung beobachten.

<img src="https://github.com/FabianKowatsch/Fusee/blob/feature/MuVista2/Examples/Complete/MuVista/Core/Assets/panoramaAnsicht.png">

Um zwischen den beiden Ansichten schnell und einfach hin und her zu wechseln drückt man die ENTER-Taste. Beim wechseln von einem Panoramabild wird man an die gleiche Stelle gesetzt, welche man gerade inne hatte. Andersherum wird man zum nächsten Panoramabild teleportiert.

### Punktwolken-Ansicht:

<img src="https://github.com/FabianKowatsch/Fusee/blob/feature/MuVista2/Examples/Complete/MuVista/Core/Assets/punktwolkenAnsicht.png">

In der Punktwolken-Ansicht kann man sich mit den Tasten W,A,S,D, SPACE und STRG bewegen. Außerdem kann man sich, wie in der Panorama-Ansicht, durch klicken und ziehen der Maus umschauen.
Hier kann man ebenfalls mit dem Regler an der linken Seite die Punktgröße einstellen.

### Map-Ansicht

<img src="https://github.com/FabianKowatsch/Fusee/blob/feature/MuVista2/Examples/Complete/MuVista/Core/Assets/mapAnsicht.png">
Durchs klicken auf die Minimap, oben links im Fenster, kommt man auf die Map-Ansicht. Dort sieht man die verfügbaren Panoramabildern an ihren Koordinaten, hier repräsentiert durch die grünen Kreise. Hier kann man zu den Panoramabildern springen indem man auf die jeweilige Kreise klickt.

---

# Dokumentation der einzelnen Klassen

## GUI

Die GUI erbt von SceneContainer. In der Klasse MuVista bekommt sie dann noch eine eigene Kamera und einen Renderer.

### Konstruktor

Im Konstruktor der GUI werden alle Komponenten welche später in der GUI zu sehen sind erstellt. Erstellt werden:

1. Das Fusee Logo (TexturNode), welches auf die Website von fusee3D.org verlinkt
2. Der Text (TextNode) in dem der Titel der Anwendung steht
3. Der ZoomIn (TexturNode) und ZoomOut (TexturNode) Button der Brennweite der Kamera. Den beiden Nodes wird jeweils ein Component der Klasse GuiButton angehängt.
4. Die Minimap (TexturNode) über welcher man zur Map-Ansicht der Anwendung wechseln kann. Dem Node wird ein Component der Klasse GuiButton angehängt.
5. Die Panorama Alpha-Wert Handle (TexturNode) über welche man den Alphawert der Panoramen verändern kann. Die Handle hat weitere Childnodes unter welchen zwei Nodes ein Component der Klasse GuiButton angehängt werden.
6. Die Point Size Handle (TexturNode) über welche man die Größe der Punktwolken einstellen kann. Die Handle hat weitere ChildNodes unter welchen zwei Nodes ein Component der Klasse GuiButton angehängt werden.

### Besondere Funktionen

#### CreateHandle()

Die Funktion erstellt eine Handle die später in eine Canvas Node gesetzt werden kann. Damit die handle dann fnktioniert müssen noch 3 funktionen geschrieben werden (vgl. OnPointSizeUp, OnPointSizeDown und OnPointSizeStop) diese können dann in einer RenderAFrame Funktion ausgelöst werden. Die Handle besteht aus 5 Komponenten. Einem Background (TextureNode), Down (TextureNode) und Up (TextureNode) mit jeweils GuiButtons angehängt, Handle (TextureNode) und einem Titel (TextNode).

## Waypoint

Die Klasse Waypoint leitet von SceneNode ab und erstellt eine Sphere auf der Minimap an der Position der Panosphere in der Minimap Szene und beinhaltet die PanoSphere zu welcher PanoSphere gewechselt werden soll, wenn der Waypoint angeklickt wird.

## PtRenderParams

Die Klasse PtRenderParams enthält alle Parameter, mit denen die Punktwolke verändert werden kann. Diese Klasse beinhaltet unter anderem den Pfad zur Punktwolke, die Farbe der Punkte sowie die Größe der Punkte in der Punktwolke.

## AppSetup

Diese Klasse wird in der Main Funktion verwendet, um die ganze Anwendung mit der benötigten Setup-Methode abhängig vom verwendeten Typ der Punktwolke zu starten.

## MuVista

### Init()

Zunächst werden die [PanoSpheres](#panosphere) mithilfe der [createPanoSpheres()](#cps) Methode der Klasse PanoSphereFactory erstellt. Desweiteren werden Kamera, GUI und Minimap samt Picking initialisiert. Außerdem wird die Punktwolke geladen und in die Szene gesetzt. Danach werden die Spheren platziert und in den Anfangszustand versetzt (deaktiviert)

### Animation

Die Animation in MuVista wird von mehreren Funktionen geregelt.

#### CreateAnimationToNextSphere()

Die Funktion startet die Animation wenn der User auf den Pfeil geklickt hat, der zur nächsten Sphere zeigt. Sie setzt einen Boolean Wert auf True der später pro Frame abgefragt wird ob die Animation noch läuft, setzt die Startzeit der Animation und blendet die Ziel-Sphere ein.

#### CreateAnimationToPreviousSphere()

Die Funktion tut dasselbe wie die Funktion CreateAnimationToNextSphere(), jedoch schickt sie den User zu der vorherigen Sphere welche vor der jetzigen Sphere erstellt wurde.

#### AnimatePanoChange()

Die Funktion erhöht je durchlaufendem Frame den Alpha-Wert des Ziel-Panoramabildes und senkt den Alpha Wert des ursprünglichen Panoramabildes. Somit entsteht der Fade Effekt. Gleichzeitig wird die Position der Kamera zwischen den beiden Panoramabildern errechnet.

### Interaktion mit GUI

#### HndGuiButtonInput()

Die Gui-Button Elemente bekommen bei einer bestimmten Interaktion eine spezielle Funktion angehängt.

#### movePanoAlphaHandle()

Die Funktion bewegt den Cursor der Handle für den Alpha-Wert der Panoramabilder. Anhand von positionen des Cursors wird überprüft ob die Handle sich bewegen kann oder nicht. Gleichzeitig wenn der Cursor sich bewegen kann wird der Wert des Alphas der Panoramabilder auf den Wert gesetzt, an welchem sich der Cursor gerade Prozentual zwischen ganz unten und ganz oben von seinem möglichen Bewegungsradius befindet.

#### movePointSizeHandle()

Die Funktion funktioniert gleich wie die Funktion movePanoAlphaHandle() jedoch beeinflusst sie den Wert der Größe der Punkte der Punktwolke.

### SwitchModes()

SwitchModes() toggelt zwischen der Punktwolkenansicht und der Panoramabild Ansicht. Falls der User sich in der Anwendung gerade im Punktwolkenmodus befindet und zur Panoramabildansicht wechselt wird die Kamera auf die Position der Current Sphere gesetzt und die Sphere wird ein- und die Punktwolken werden ausgeblendet.
Falls der Wechsel in die andere Richtung durchgeführt wird werden die Punkte eingeblendet und die Panoramabilder werden ausgeblendet. Die Kamera verändert aber nicht ihre Position.

### Map

Die Map besteht aus einer eigenen Szene. Die Szene besteht aus einer Plane, welche auf die Position der mittlersten PanoSphere positioniert wird und alle PanoSpheres werden auf die Plane als grüne Spheres in Form von Waypoints platziert.

#### SwitchCamViewport

In dieser Funktion wird der Viewport der Main Kamera und der Minimap getauscht. Dies wird ausgeführt, wenn F5 betätigt wurde oder oben rechts auf die Minimap geklickt wird.

#### DoPicking()

In dieser Funktion wird die Funktionalitäten des Pickings in der Anwendung ausgeführt. Einerseits soll, wenn gerade die Minimap offen ist überprüft werden, ob einer der Waypoints angeklickt wurde, um dann zur Panoramansicht dieses angeklickten Panoramabildes zu gelangen. Andererseits soll in der Panoramansicht überprüft werden, ob einer der Verbindungspfeile angeklickt wurde, um die Animtation zwischen den Panoramabildern zu starten.

## PanoSphereFactory

Die Klasse PanoSphereFactory dient der Erstellung der PanoShpheres und stellt die Verbindung zwischen Panoramabildern und den Spheren sowie deren Positionierung her. Dafür wird die Funktion createPanoSpheres() aufgerufen.

### <a name="cps">createPanoSpheres()

In der Funktion createPanoSpheres() werden zunächst die Metadaten aus der meta.json der Punktwolke eingelesen, um den Offset/Versatz der Punktwolke zu bestimmen. Dieser wird benötigt, um die Panoramabilder an die richtige Position zu setzen. Danach werden die Metadaten der Bilder eingelesen und eine Liste vom Typ PanoImage erstellt. Dann wird für jedes Bild in der Liste die Funktion createSphereWithShift() aufgerufen, welche für jedes Bild eine PanoSphere erstellt und diese korrekt rotiert und verschiebt. Nachdem diese wie unten beschrieben korrekt transformiert wurden, wird ihnen eine Pfeil-ChildNode angehängt, welcher
in Richtung der nächsten/vorherigen Sphere zeigt (createArrow()).

### createSphereWithShift()

Die PanoSpheres benötigen eine Textur, die korrekte Rotation und Translation. Die Textur erhalten sie durch den Dateinamen des Bildes, welcher an den Konstruktor der PanoSphere übergeben wird.

#### Translation

Die Translation ergibt sich aus dem Offset und den in den Metadaten der Bilder angegebenen Koordinaten, indem man den Offset von den Bildkoordinaten subtrahiert. Zu beachten ist hierbei, dass Z-Achse und Y-Achse nach dem subtrahieren vertauscht werden müssen

#### Rotation

Die Rotation erfolgt mithilfe der in den Bilddaten angegebenen Quaternionen. Dabei muss zunächst qz negiert werden. Beim erstellen der Rotationsmatrix aus den Quaternionen müssen dann qy und qz vertauscht werden.

> Weiter zu beachten ist, dass dies nur für Bilder der hinteren Kamera gilt. Bilder der Frontkameras müssen zusätzlich noch um 180 Grad an der Fusee-Y-Achse gedreht werden.

### createArrow()

Hier werden die Pfeile erstellt, welche in Richtung der angegebenen Sphere zeigen.

## PanoImage

Die PanoImage-Klasse dient als Datenstruktur, die der JSON-Struktur der Bildmetadaten gleicht. Wichtige Attribute sind hier der filename, XYZ Koordinaten sowie die Quaternionen.

## <a name="panosphere"></a>PanoSphere

Die Klasse PanoSphere erbt von der SceneNode-Klasse. In ihr stehen Attribute wie die Transform-Component, Radius, Textur sowie Verweise auf die vorherige und nächste Sphere.

<!-- Davor den restlichen Klassenstuff _______________________________________________________________________________________________________________ -->

# Daten für MuVista vorbereiten

Zum starten von MuVista werden verschiedene Daten benötigt, die in einem bestimmten Format vorliegen müssen:

- Die Punktwolke: muss mithilfe des [OocFileGenerator](https://github.com/FabianKowatsch/Fusee/blob/feature/MuVista2/src/PointCloud/Tools/Program.cs) aus einem .las/.laz-File erstellt worden sein und eine meta.json enthalten
- Die Panoramabilder: müssen im ...MuVista/Core/Assets/Panos-Verzeichnis von liegen
- Die data.json in ...MuVista/Core/Assets/Data , welche aus dem .log-File der Panoramadaten mithilfe der Tools [JSONReducer](https://github.com/FabianKowatsch/Fusee/blob/feature/MuVista2/Examples/Complete/JSONReducer/Program.cs) und [LogToJSON](https://github.com/FabianKowatsch/Fusee/blob/feature/MuVista2/Examples/Complete/JSONReducer/LogToJSON/LogToJSON.cs) erstellt wird.

### Die Logdatei umwandeln

Um die Logdaten in das JSON-Format umzuwandeln muss in der Klasse LogToJSON (siehe oben) der Pfad der .log-Datei sowie der Pfad zum output vorgegeben werden und anschließend das Programm ausgeführt werden.

### Die JSON-Datei anpassen

MuVista generiert die PanoSpheres basierend auf den Panoramabildern im Asset-Ordner und der zugehörigen data.json. Um die Startdauer von MuVista zu reduzieren, wird eine verkürzte Version der data.json verwendet, welche nur Informationen zu den Bildern im Asset-Ordner enthält. Um diese zu erstellen muss das Programm jSONReducer ausgeführt werden und die Pfade zu den Bildern, Input und Output korrekt in den Attributen angegeben werden.

### Die Punktwolkendaten generieren

Um die Punktwolke brauchbar für das Projekt MuVista zu machen, muss das Programm OocFileGenerator (siehe oben) ausgeführt werden. Dafür wird der Pfad zur .las/-laz-Datei benötigt, sowie der Pfad zum output. Dieser sollte vorzugsweise ... MuVista/Core/Assets/Data/ProcessedPointcloud sein. Außerdem muss die Anzahl der Punkte pro Oktant angegeben werden (empfohlen zwischen 3000 und 20000, je nach Größe und Performance) sowie der Indikator des Punkttyps als Integer (1 für einfarbige Punkte, 3 für mehrfarbige)
