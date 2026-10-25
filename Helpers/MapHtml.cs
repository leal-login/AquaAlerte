namespace AquaAlerte.Helpers;

// Page HTML de la carte (Leaflet + OpenStreetMap), affichée dans un WebView.
// Le C# parle à la carte avec setMarkers(), selectMarker(), setUser().
// La carte parle au C# en changeant l'adresse : aqua://marker/3, aqua://locate, aqua://reload.
public static class MapHtml
{
    // Fond de carte OpenStreetMap. Si les tuiles restent grises, voir la note « Si la carte reste grise ».
    private const string TileUrl = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";

    public static string Build() => Template.Replace("__TILE__", TileUrl);

    private const string Template = """
<!DOCTYPE html>
<html lang="fr">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css">
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
<style>
  html, body { height: 100%; margin: 0; padding: 0; background: #E6F5FA; font-family: sans-serif; }
  #map { position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: #E6F5FA; }

  .pin { width: 44px; height: 44px; box-sizing: border-box; border-radius: 50%; border: 3px solid #fff;
         box-shadow: 0 2px 6px rgba(0,0,0,.30); display: flex; align-items: center; justify-content: center;
         transition: transform .15s; }
  .pin.red { background: #D64545; }
  .pin.green { background: #1E9E6A; }
  .pin.sel { transform: scale(1.22); }

  .you { width: 18px; height: 18px; box-sizing: border-box; border-radius: 50%; background: #0B7A9B;
         border: 3px solid #fff; box-shadow: 0 0 0 12px rgba(11,122,155,.18); }

  .legend { background: rgba(255,255,255,.94); border-radius: 12px; padding: 8px 10px; font-size: 12px;
            color: #4A5A68; box-shadow: 0 1px 5px rgba(0,0,0,.2); line-height: 1.6; }
  .legend .row { display: flex; align-items: center; gap: 7px; }
  .legend .dot { width: 10px; height: 10px; border-radius: 50%; display: inline-block; }

  .ctl { display: flex; flex-direction: column; gap: 10px; }
  .btn { width: 44px; height: 44px; background: #fff; border-radius: 12px; box-shadow: 0 1px 5px rgba(0,0,0,.25);
         display: flex; align-items: center; justify-content: center; color: #0B7A9B; font-size: 28px;
         line-height: 1; -webkit-user-select: none; user-select: none; }

  #msg { display: none; position: absolute; top: 0; left: 0; right: 0; bottom: 0; z-index: 1000;
         background: #F4F7FA; color: #6B7785; flex-direction: column; align-items: center;
         justify-content: center; text-align: center; padding: 24px; font-size: 15px; gap: 14px; }
  #msg button { background: #0B7A9B; color: #fff; border: 0; border-radius: 12px; padding: 10px 18px;
                font-size: 15px; font-weight: bold; }
  .leaflet-control-attribution { font-size: 10px; }
</style>
</head>
<body>
<div id="map"></div>
<div id="msg">
  <div>Connexion Internet requise pour afficher la carte.</div>
  <button onclick="go('reload')">Réessayer</button>
</div>

<script>
var map = null, layer = null, you = null, markers = {};
window.mapReady = false;

var DROP_OFF = '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 3s-6 7-6 11a6 6 0 0 0 12 0c0-4-6-11-6-11z"/><line x1="4" y1="4" x2="20" y2="20"/></svg>';
var CHECK = '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="#fff" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="M5 12l5 5L20 7"/></svg>';
var TARGET = '<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#0B7A9B" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="7"/><circle cx="12" cy="12" r="2.5"/><path d="M12 1v4M12 19v4M1 12h4M19 12h4"/></svg>';

// La carte envoie un message au C# en changeant l'adresse de la page
function go(path) { window.location.href = 'aqua://' + path; }

function makeIcon(ongoing, sel) {
  return L.divIcon({
    className: '',
    html: '<div class="pin ' + (ongoing ? 'red' : 'green') + (sel ? ' sel' : '') + '">' + (ongoing ? DROP_OFF : CHECK) + '</div>',
    iconSize: [44, 44],
    iconAnchor: [22, 22]
  });
}

// Appelé par le C# : remplace tous les repères.
// mode : 'keep' (ne bouge pas la carte), 'all' (cadre tous les repères), 'selected' (centre sur le repère choisi)
function setMarkers(items, selId, mode) {
  if (!window.mapReady) return;
  layer.clearLayers();
  markers = {};
  var pts = [];

  items.forEach(function (it) {
    var m = L.marker([it.lat, it.lng], {
      icon: makeIcon(it.ongoing, it.id === selId),
      ongoing: it.ongoing,
      keyboard: false,
      zIndexOffset: it.id === selId ? 1000 : 0
    });
    m.on('click', function () { selectMarker(it.id); go('marker/' + it.id); });
    m.addTo(layer);
    markers[it.id] = m;
    pts.push([it.lat, it.lng]);
  });

  if (mode === 'selected' && markers[selId]) {
    map.setView(markers[selId].getLatLng(), 16);
  } else if (mode !== 'keep' && pts.length) {
    map.fitBounds(L.latLngBounds(pts), { padding: [50, 50], maxZoom: 16 });
  }
  setTimeout(function () { map.invalidateSize(); }, 150);
}

// Met en évidence un repère (plus grand, au premier plan) et le recentre
function selectMarker(id) {
  if (!window.mapReady) return;
  Object.keys(markers).forEach(function (k) {
    var m = markers[k], on = Number(k) === id;
    m.setIcon(makeIcon(m.options.ongoing, on));
    m.setZIndexOffset(on ? 1000 : 0);
  });
  if (markers[id]) map.panTo(markers[id].getLatLng());
}

// Point bleu « Vous »
function setUser(lat, lng) {
  if (!window.mapReady) return;
  if (you) {
    you.setLatLng([lat, lng]);
  } else {
    you = L.marker([lat, lng], {
      icon: L.divIcon({ className: '', html: '<div class="you"></div>', iconSize: [18, 18], iconAnchor: [9, 9] }),
      interactive: false,
      zIndexOffset: -500
    }).addTo(map);
  }
  map.setView([lat, lng], Math.max(map.getZoom(), 16));
}

function init() {
  // Leaflet n'a pas pu être téléchargé : pas d'Internet
  if (typeof L === 'undefined') {
    document.getElementById('msg').style.display = 'flex';
    return;
  }

  map = L.map('map', { zoomControl: false, attributionControl: true }).setView([4.0512, 9.6953], 15);

  L.tileLayer('__TILE__', {
    maxZoom: 19,
    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
    referrerPolicy: 'no-referrer-when-downgrade'
  }).addTo(map);

  map.attributionControl.setPrefix('Positions approximatives');
  map.attributionControl.setPosition('bottomleft');
  layer = L.layerGroup().addTo(map);

  // Légende en haut à gauche
  var legend = L.control({ position: 'topleft' });
  legend.onAdd = function () {
    var d = L.DomUtil.create('div', 'legend');
    d.innerHTML =
      '<div class="row"><span class="dot" style="background:#D64545"></span>En cours</div>' +
      '<div class="row"><span class="dot" style="background:#1E9E6A"></span>Eau revenue</div>' +
      '<div class="row"><span class="dot" style="background:#0B7A9B"></span>Vous</div>';
    return d;
  };
  legend.addTo(map);

  // Boutons zoom + / - et position, en bas à droite
  var ctl = L.control({ position: 'bottomright' });
  ctl.onAdd = function () {
    var box = L.DomUtil.create('div', 'ctl');
    var plus = L.DomUtil.create('div', 'btn', box);
    var minus = L.DomUtil.create('div', 'btn', box);
    var loc = L.DomUtil.create('div', 'btn', box);
    plus.innerHTML = '+';
    minus.innerHTML = '&minus;';
    loc.innerHTML = TARGET;
    L.DomEvent.disableClickPropagation(box);
    L.DomEvent.on(plus, 'click', function () { map.zoomIn(); });
    L.DomEvent.on(minus, 'click', function () { map.zoomOut(); });
    L.DomEvent.on(loc, 'click', function () { go('locate'); });
    return box;
  };
  ctl.addTo(map);

  window.mapReady = true;
  setTimeout(function () { map.invalidateSize(); }, 200);
}

init();
</script>
</body>
</html>
""";
}