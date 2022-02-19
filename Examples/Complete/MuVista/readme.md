# MuVista - 3D Pointcloud and Pano Viewer
## Anleitung zur Steuerung von MuVista
### Panorama-Ansicht: 

Befindet man sich in der Panoramaansicht, so kann man sich durch klicken und ziehen der Maus umschauen. Passend dazu kann man mit dem Mausrad den Zoom benutzen. Alternativ geht das auch mit UI-Elementen, dem Plus und dem Minus in der rechten unteren Ecke des Fensters. Durch die beiden Reglern kann man zum einen die Punktgröße der Punktwolke anpassen und die Transparenz des Panoramabilds.
Durch klicken auf die grünen Pfeilen kann man sich von einem Panoramabild zum nächsten bewegen. Während den animierten Übergängen kann man sich weiterhin umschauen und kann durch die kurzzeitig eingeblendete Punktwolken die Bewegung beobachten.

<img src="https://github.com/FabianKowatsch/Fusee/blob/feature/MuVista2/Examples/Complete/MuVista/Core/Assets/panoramaAnsicht.png">


Um zwischen den beiden Ansichten schnell und einfach hin und her zu wechseln drückt man die ENTER-Taste. Beim wechseln von einem Panoramabild wird man an die gleiche Stelle gesetzt, welche man gerade inne hatte. Andersherum wird man zum nächsten Panoramabild teleportiert.
### Punktwolken-Ansicht:

<img src="https://github.com/FabianKowatsch/Fusee/blob/feature/MuVista2/Examples/Complete/MuVista/Core/Assets/punktwolkenAnsicht.png">


In der Punktwolken-Ansicht kann man sich mit den Tasten W,A,S,D, SPACE  und STRG bewegen. Außerdem kann man sich, wie in der Panorama-Ansicht, durch klicken und ziehen der Maus umschauen.
Hier kann man ebenfalls mit dem Regler an der linken Seite die Punktgröße einstellen.

### Map-Ansicht
<img src="https://github.com/FabianKowatsch/Fusee/blob/feature/MuVista2/Examples/Complete/MuVista/Core/Assets/mapAnsicht.png">
Durchs klicken auf die Minimap, oben links im Fenster, kommt man auf die Map-Ansicht. Dort sieht man die verfügbaren Panoramabildern an ihren Koordinaten, hier repräsentiert durch die grünen Kreise. Hier kann man zu den Panoramabildern springen indem man auf die jeweilige Kreise klickt.

---

## Dokumentation der einzelnen Klassen
### AppSetup

### GUI
Die GUI erbt von SceneContainer. In der Klasse MuVista bekommt sie dann noch eine eigene Kamera und einen Renderer.
#### Konstruktor
Im Konstruktor der GUI werden alle Komponenten welche später in der GUI zu sehen sind erstellt. Erstellt werden:
1.	Das Fusee Logo (TexturNode), welches auf die Website von fusee3D.org verlinkt
2.	Der Text (TextNode) in dem der Titel der Anwendung steht
3.	Der ZoomIn (TexturNode) und ZoomOut (TexturNode) Button der Brennweite der Kamera. Den beiden Nodes wird jeweils ein Component der Klasse GuiButton angehängt.
4.	Die Minimap (TexturNode) über welcher man zur Map-Ansicht der Anwendung wechseln kann. Dem Node wird ein Component der Klasse GuiButton angehängt.
5.	Die Panorama Alpha-Wert Handle (TexturNode) über welche man den Alphawert der Panoramen verändern kann. Die Handle hat weitere Childnodes unter welchen zwei Nodes ein Component der Klasse GuiButton angehängt werden.
6.	Die Point Size Handle (TexturNode) über welche man die Größe der Punktwolken einstellen kann. Die Handle hat weitere ChildNodes unter welchen zwei Nodes ein Component der Klasse GuiButton angehängt werden.

#### Besondere Funktionen
##### CreateHandle()
Die Funktion erstellt eine Handle die später in eine Canvas Node gesetzt werden kann. Damit die handle dann fnktioniert müssen noch 3 funktionen geschrieben werden (vgl. OnPointSizeUp, OnPointSizeDown und OnPointSizeStop) diese können dann in einer RenderAFrame Funktion ausgelöst werden. Die Handle besteht aus 5 Komponenten. Einem Background (TextureNode), Down (TextureNode) und Up (TextureNode) mit jeweils GuiButtons angehängt, Handle (TextureNode) und einem Titel (TextNode). 

### MuVista
#### Animation
Die Animation in MuVista wird von mehreren Funktionen geregelt. 

##### CreateAnimationToNextSphere()
Die Funktion startet die Animation wenn der User auf den Pfeil geklickt hat, der zur nächsten Sphere zeigt. Sie setzt einen Boolean Wert auf True der später pro Frame abgefragt wird ob die Animation noch läuft, setzt die Startzeit der Animation und blendet die Ziel-Sphere ein. 

##### CreateAnimationToPreviousSphere()
Die Funktion tut dasselbe wie die Funktion CreateAnimationToNextSphere(), jedoch schickt sie den User zu der vorherigen Sphere welche vor der jetzigen Sphere erstellt wurde.

##### AnimatePanoChange()
Die Funktion erhöht je durchlaufendem Frame den Alpha-Wert des Ziel-Panoramabildes und senkt den Alpha Wert des ursprünglichen Panoramabildes. Somit entsteht der Fade Effekt. Gleichzeitig wird die Position der Kamera zwischen den beiden Panoramabildern errechnet. 

#### Interaktion mit GUI
##### HndGuiButtonInput()
Die Gui-Button Elemente bekommen bei einer bestimmten Interaktion eine spezielle Funktion angehängt.

##### movePanoAlphaHandle()
Die Funktion bewegt den Cursor der Handle für den Alpha-Wert der Panoramabilder. Anhand von positionen des Cursors wird überprüft ob die Handle sich bewegen kann oder nicht. Gleichzeitig wenn der Cursor sich bewegen kann wird der Wert des Alphas der Panoramabilder auf den Wert gesetzt, an welchem sich der Cursor gerade Prozentual zwischen ganz unten und ganz oben von seinem möglichen Bewegungsradius befindet.

##### movePointSizeHandle()
Die Funktion funktioniert gleich wie die Funktion movePanoAlphaHandle() jedoch beeinflusst sie den Wert der Größe der Punkte der Punktwolke.

#### SwitchModes()
SwitchModes() toggelt zwischen der Punktwolkenansicht und der Panoramabild Ansicht. Falls der User sich in der Anwendung gerade im Punktwolkenmodus befindet und zur Panoramabildansicht wechselt wird die Kamera auf die Position der Current Sphere gesetzt und die Sphere wird ein- und die Punktwolken werden ausgeblendet.
Falls der Wechsel in die andere Richtung durchgeführt wird werden die Punkte eingeblendet und die Panoramabilder werden ausgeblendet. Die Kamera verändert aber nicht ihre Position.

